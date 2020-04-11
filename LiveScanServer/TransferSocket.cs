using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace KinectServer
{
    public class TransferSocket
    {
        Socket oSocket;
        int id = -1;
        BufferTxAlgorithm oBufferTxAlg;
        List<byte[]> framesToTx = new List<byte[]>();
        List<Socket> lSocketsPerUe = new List<Socket>();

        public int lastFrameRxBytes = 0;

        public TransferSocket(Socket ueSocket, int ueId, BufferTxAlgorithm oBufferAlg)
        {
            oSocket = ueSocket;
            id = ueId;
            oBufferTxAlg = oBufferAlg;
            lSocketsPerUe.Add(oSocket);
        }

        public void CloseSocketAndRelease()
        {
            try
            {
                oSocket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                oSocket.Close();
            }
        }

        public void UpdateSocketFromUeList (List<Socket> lSockets)
        {
            Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " UpdateSocketFromUeList  with [currentSize, newSize]: [" + lSocketsPerUe.Count  + ", "+ lSockets.Count + "]");
            lSocketsPerUe.Clear();
            if (lSockets.Count > 0)
                lSocketsPerUe = new List<Socket>(lSockets);
            else
            {
                CloseSocketAndRelease();
            }                
        }

        public Socket GetSocket()
        {
            return oSocket;
        }

        public async Task ReceiveFrame()
        {
            try
            {
                NetworkStream _stream = new NetworkStream(oSocket);
                int hdr_size = 1; //1 byte for the time being is received by the UEs (i.e. requests by UEs)
                while (SocketConnected())
                {
                    int _bytesRead = 0;
                    byte[] buffer = new byte[hdr_size];

                    IPEndPoint _remoteIpEndPoint = oSocket.RemoteEndPoint as IPEndPoint;

                    while (_bytesRead < hdr_size)
                    {
                        int bytesRead = await _stream.ReadAsync(buffer, _bytesRead, hdr_size - _bytesRead);

                        if (bytesRead == 0) throw new InvalidDataException("unexpected end-of-stream at header (TransferSocket)");
                        _bytesRead += bytesRead;
                    }

                    int _hdrIndicator = buffer[0];
                    if (_hdrIndicator == (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_REQUEST_LAST_FRAME)
                    {
                        //Request for last frame has been received by a UE                        
                        framesToTx = oBufferTxAlg.GetBufferedFrames(id);      // I want to check the sockets that are ready for writing (based on the ports) before sending the data...
                        List<Socket> _tempListSocket = new List<Socket>(lSocketsPerUe);
                        Socket.Select(null, _tempListSocket, null, 1000); // check for writability for 1 ms
                        int jj = 0;
                        if (framesToTx.Count > 0 && _tempListSocket.Count > 0)
                        {
                            for (int ii = 0; ii < framesToTx.Count; ii++) //todo: in the future, concatenate all frames into one and send them in one go... of course, we need to modify header
                            {
                                jj = Math.Min(jj, _tempListSocket.Count - 1);
                                //Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " Frames to Send: " + framesToTx.Count + " _tempListSocket.Count " + _tempListSocket.Count + " start with jj " + jj + " and ii: " + ii + " size: " + framesToTx[ii].Length + " send signal: " + (int)(framesToTx[ii])[0] + " ts: " + BitConverter.ToInt32(framesToTx[ii], 9));

                                NetworkStream _streamT = new NetworkStream(_tempListSocket[jj]);
                                int _length = framesToTx[ii].Length;
                                lastFrameRxBytes += _length;

                                await _streamT.WriteAsync(framesToTx[ii], 0, _length);
                                jj = (jj < _tempListSocket.Count - 1) ? ++jj : jj = 0;
                            }
                        }
                        else if (framesToTx.Count == 0)
                        {
                            int _hdrSizeTx = 14;
                            byte[] _bufferSend = new byte[_hdrSizeTx]; //header is always 14 bytes; 1 for indicator, 1 for source ID, 4 for length, 4 for compression, and 4 for timestamp
                            _bufferSend[0] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_NO_FRAME; 
                            NetworkStream _streamT = new NetworkStream(oSocket);
                            lastFrameRxBytes += _hdrSizeTx;

                            //Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " No frame in queue send signal: " + (int)_bufferSend[0]);
                            await _streamT.WriteAsync(_bufferSend, 0, _hdrSizeTx);
                        }
                        else
                        {
                            // we are here as all frames couldn't really fit into the socket on select, hence round robin through available sockets in synchronous manner
                            int ii = 0;                            
                            for (int kk = 0; kk < framesToTx.Count; kk++)
                            {
                                bool _flag = true;
                                while (_flag)
                                {
                                    NetworkStream _streamT = new NetworkStream(lSocketsPerUe[ii]);
                                    int _length = framesToTx[kk].Length;
                                    lastFrameRxBytes += _length;

                                    _streamT.Write(framesToTx[kk], 0, _length);
                                    _flag = false;
                                    //Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " Frames to Send (synchronous): " + framesToTx.Count + " frameNum: " + kk + " socket id: " + ii);
                                    ii = (ii < lSocketsPerUe.Count - 1) ? ++ii : ii = 0;
                                }
                            }
                        }
                    }
                    else
                        throw new InvalidDataException("TransferSocket Rx error with _hdrIndicator number " + buffer[0]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " Async TransferSocket Rx exception msg: " + ex.ToString());
            }
        }

        public bool SocketConnected()
        {
            return oSocket.Connected;
        }
    }
}
