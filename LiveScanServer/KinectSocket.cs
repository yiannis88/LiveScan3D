//   Copyright (C) 2015  Marek Kowalski (M.Kowalski@ire.pw.edu.pl), Jacek Naruniec (J.Naruniec@ire.pw.edu.pl)
//   License: MIT Software License   See LICENSE.txt for the full license.

//   If you use this software in your research, then please use the following citation:

//    Kowalski, M.; Naruniec, J.; Daniluk, M.: "LiveScan3D: A Fast and Inexpensive 3D Data
//    Acquisition System for Multiple Kinect v2 Sensors". in 3D Vision (3DV), 2015 International Conference on, Lyon, France, 2015

//    @INPROCEEDINGS{Kowalski15,
//        author={Kowalski, M. and Naruniec, J. and Daniluk, M.},
//        booktitle={3D Vision (3DV), 2015 International Conference on},
//        title={LiveScan3D: A Fast and Inexpensive 3D Data Acquisition System for Multiple Kinect v2 Sensors},
//        year={2015},
//    }
//  The initial code has been significantly modified in order to support a buffer per client and correctly receive the frames.
//  Comments or modifications (major) are made by Ioannis Selinis (5GIC University of Surrey, 2019)

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace KinectServer
{
    public delegate void SocketChangedHandler();

    public class KinectSocket
    {
        Socket oSocket;
        public BufferRxAlgorithm oBufferRxAlgorithm;

        byte[] byteToSend = new byte[1];
        public bool bFrameCaptured = false;
        public bool bStoredFrameReceived = false;
        public bool bNoMoreStoredFrames = true;
        public bool bCalibrated = false;
        //The pose of the sensor in the scene (used by the OpenGLWindow to show the sensor)
        public AffineTransform oCameraPose = new AffineTransform();
        //The transform that maps the vertices in the sensor coordinate system to the world corrdinate system.
        public AffineTransform oWorldTransform = new AffineTransform();

        public string sSocketState;

        public List<byte> lFrameRGB = new List<byte>();
        public List<Single> lFrameVerts = new List<Single>();
        public List<Body> lBodies = new List<Body>();

        public int SourceID { get; private set; } = -1; // for use in stats calculation, refers to last received ID from frame transmission, -1 for not set yet

        public event SocketChangedHandler eChanged;
        private bool debugFlag = true;

        private int localPort = 0;
        private int remotePort = 0;
        private int clientIdSok = -1;

        /* Public variables for stats */
        public int storedFrameRxBytes = 0;
        public int storedFrameCtr = 0;
        public int lastFrameRxBytes = 0;
        public int lastFrameCtr = 0;

        public KinectSocket(Socket clientSocket)
        {
            oSocket = clientSocket;

            IPEndPoint _localIpEndPoint = oSocket.LocalEndPoint as IPEndPoint;
            IPEndPoint _remoteIpEndPoint = oSocket.RemoteEndPoint as IPEndPoint;
            localPort = _localIpEndPoint.Port;
            remotePort = _remoteIpEndPoint.Port;
        }

        public void SetClientId(string clId, int clientId)
        {
            clientIdSok = clientId;
            oBufferRxAlgorithm.SetClientIdPath(clId);
        }

        public void SetSocketStatus()
        {
            IPEndPoint _remoteIpEndPoint = oSocket.RemoteEndPoint as IPEndPoint;
            if (clientIdSok > -1)
                sSocketState = "Client: " + clientIdSok + " Source: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = false";
            else
                sSocketState = "\tSource: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = false";

            UpdateSocketState();
        }

        public void SetBufferRx(BufferRxAlgorithm bufferPtr)
        {
            oBufferRxAlgorithm = bufferPtr;
        }

        public void CaptureFrame()
        {
            bFrameCaptured = false;
            byteToSend[0] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_CAPTURE_FRAME;
            SendByte(byteToSend);
        }

        public void Calibrate()
        {
            Console.WriteLine("KinectSocket::Calibrate [1]: " + (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_CALIBRATE);
            bCalibrated = false;
            IPEndPoint _remoteIpEndPoint = oSocket.RemoteEndPoint as IPEndPoint;
            if (clientIdSok > -1)
                sSocketState = "Client: " + clientIdSok + " Source: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = false";
            else
                sSocketState = "\tSource: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = false";

            byteToSend[0] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_CALIBRATE;
            SendByte(byteToSend);

            UpdateSocketState();
        }

        public void SendSettings(KinectSettings settings)
        {
            Console.WriteLine("KinectSocket::SendSettings [2]: " + (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_RECEIVE_SETTINGS + " " + settings.ToByteList().Count);
            List<byte> lData = settings.ToByteList();

            byte[] bTemp = BitConverter.GetBytes(lData.Count);
            lData.InsertRange(0, bTemp);
            lData.Insert(0, (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_RECEIVE_SETTINGS);

            if (SocketConnected())
                SendByte(lData.ToArray());
        }

        public void RequestStoredFrame()
        {
            try
            {
                byteToSend[0] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_REQUEST_STORED_FRAME;
                SendByte(byteToSend);
                bNoMoreStoredFrames = false;
                bStoredFrameReceived = false;
            }
            catch (SocketException ex)
            {
                Console.WriteLine("KinectSocket::RequestStoredFrame Exception Message: " + ex.ToString());
            }
        }

        public void RequestLastFrame(float stepAlgorithm)
        {
            try
            {
                byte[] _byteNew = new byte[5]; // 1 is for the signal & 4 to carry the information for step algorithm
                Buffer.BlockCopy(BitConverter.GetBytes((int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_REQUEST_LAST_FRAME), 0, _byteNew, 0, 1);
                Buffer.BlockCopy(BitConverter.GetBytes(stepAlgorithm), 0, _byteNew, 1, 4);
                Console.WriteLine("KinectSocket::RequestLastFrame " + stepAlgorithm + " " + _byteNew[0] + " ____ " + (System.BitConverter.ToSingle(_byteNew, 1)).ToString());
                SendByte(_byteNew);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("KinectSocket::RequestLastFrame Exception Message: " + ex.ToString());
            }
        }

        public void SendCalibrationData()
        {
            int size = 1 + (9 + 3) * sizeof(float);
            byte[] data = new byte[size];
            int i = 0;

            data[i] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_RECEIVE_CALIBRATION;
            i++;

            Buffer.BlockCopy(oWorldTransform.R, 0, data, i, 9 * sizeof(float));
            i += 9 * sizeof(float);
            Buffer.BlockCopy(oWorldTransform.t, 0, data, i, 3 * sizeof(float));
            i += 3 * sizeof(float);

            Console.WriteLine("KinectSocket::SendCalibrationData [4]: " + (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_RECEIVE_CALIBRATION);

            if (SocketConnected())
                SendByte(data);
        }

        public void ClearStoredFrames()
        {
            byteToSend[0] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_CLEAR_STORED_FRAMES;
            SendByte(byteToSend);
        }

        public void ReceiveCalibrationData()
        {
            byte[] buffer = Receive(sizeof(int) * 1);
            //currently not used
            int markerId = BitConverter.ToInt32(buffer, 0);

            buffer = Receive(sizeof(float) * 9);
            Buffer.BlockCopy(buffer, 0, oWorldTransform.R, 0, sizeof(float) * 9);

            buffer = Receive(sizeof(float) * 3);
            Buffer.BlockCopy(buffer, 0, oWorldTransform.t, 0, sizeof(float) * 3);

            oCameraPose.R = oWorldTransform.R;
            for (int i = 0; i < 3; i++)
            {
                oCameraPose.t[i] = 0.0f;
                for (int j = 0; j < 3; j++)
                {
                    oCameraPose.t[i] += oWorldTransform.t[j] * oWorldTransform.R[i, j];
                }
            }
        }

        public async Task ReceiveFrame(int tsOffsetFromUtcTime)
        {
            try
            {
                NetworkStream _stream = new NetworkStream(oSocket);
                int hdr_indicator = 1, hdr_size = 4, hdr_compr = 4, hdr_step = 4, hdr_ts = 4;
                int hdrSize = hdr_indicator + hdr_size + hdr_compr + hdr_step + hdr_ts; //1 byte for indicating the frame type, 4 bytes for size, 4 bytes for compression, 4 bytes for stepAlgorithm, and 4 bytes for timestamp (UTC)
                //TODO: I could also merge some information into 1 byte (e.g. hdr_indicator & compression given that hdr_indicator spans up to 16 
                //      and then compression again up to 16, see SNTP or my ns-3 work with left/right shift operators). For now, this is not huge overhead
                while (SocketConnected())
                {
                    lFrameRGB.Clear();
                    lFrameVerts.Clear();
                    lBodies.Clear();
                    int _bytesRead = 0;
                    byte[] buffer = new byte[hdrSize];

                    while (_bytesRead < hdrSize)
                    {
                        int bytesRead = await _stream.ReadAsync(buffer, _bytesRead, hdrSize - _bytesRead);

                        if (bytesRead == 0) throw new InvalidDataException("unexpected end-of-stream at header");
                        _bytesRead += bytesRead;
                    }

                    int _hdrIndicator = buffer[0];
                    int _dataLength = BitConverter.ToInt32(buffer, hdr_indicator);
                    int iCompressed = BitConverter.ToInt32(buffer, hdr_indicator + hdr_size);
                    float stepAlg = BitConverter.ToInt32(buffer, hdr_indicator + hdr_size + hdr_compr); // For the time being we don't use this information from the client!
                    int timestamp = BitConverter.ToInt32(buffer, hdr_indicator + hdr_size + hdr_compr + hdr_step);                    

                    if (_hdrIndicator == (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_CONFIRM_CAPTURED)
                    {
                        bFrameCaptured = true;
                        continue;
                    }
                    else if (_hdrIndicator == (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_CONFIRM_CALIBRATED)
                    {
                        bCalibrated = true;
                        ReceiveCalibrationData();
                        UpdateSocketState();
                        continue;
                    }
                    //stored frame
                    else if (_hdrIndicator == (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_SEND_STORED_FRAME)
                    {
                        bStoredFrameReceived = true;
                        storedFrameRxBytes += _dataLength;
                        storedFrameCtr++;
                    }
                    //last frame
                    else if (_hdrIndicator == (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_SEND_LAST_FRAME)
                    {
                        lastFrameRxBytes += _dataLength;
                        lastFrameCtr++;

                    }
                    //No frames available at the client
                    else if (_hdrIndicator == (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_NO_FRAME)
                    {
                        //no frames available at the client --> do nothing (at least do not wait for frames dude)
                        continue;
                    }
                    else
                        throw new InvalidDataException("Buffer number " + buffer[0]);


                    buffer = new byte[_dataLength];
                    _bytesRead = 0;
                    while (_bytesRead < _dataLength)
                    {
                        int bytesRead = await _stream.ReadAsync(buffer, _bytesRead, _dataLength - _bytesRead);

                        if (bytesRead == 0) throw new InvalidDataException("unexpected end-of-stream");
                        _bytesRead += bytesRead;
                    }

                    if (iCompressed == 1)
                        buffer = ZSTDDecompressor.Decompress(buffer);

                    List<byte> _lFrameRGB = new List<byte>();
                    List<float> _lFrameVerts = new List<float>();
                    List<Body> _lBodies = new List<Body>();

                    //Receive depth and color data
                    int startIdx = 0;

                    SourceID = buffer[startIdx];
                    startIdx += 1;

                    int n_vertices = BitConverter.ToInt32(buffer, startIdx);
                    startIdx += 4;
                    //In total, we have 9 bytes per cloud point; with 3 bytes used for the color and 6 used for the position
                    //The loops below read this information 
                    for (int i = 0; i < n_vertices; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            _lFrameRGB.Add(buffer[startIdx++]);  //1-byte for color   ==> 3 iterations   ==> 3 bytes for color per point cloud           
                        }
                        for (int j = 0; j < 3; j++)
                        {
                            float val = BitConverter.ToInt16(buffer, startIdx);
                            //converting from milimeters to meters
                            val /= 1000.0f;
                            _lFrameVerts.Add(val);
                            startIdx += 2;     //2 bytes for position   ==> 3 iterations  ==> 6 bytes for positions per point cloud       
                        }
                    }

                    //Receive body data
                    //todo: inform client based on the settings if we want or not skeletons in live show!!! 
                    //      If not, then do not transmit this information (reduce overhead)
                   int nBodies = BitConverter.ToInt32(buffer, startIdx); // the remaining bytes (713B) are used for the skeleton information
                   startIdx += 4;

                    for (int i = 0; i < nBodies; i++)
                    {
                        Body tempBody = new Body();
                        tempBody.bTracked = BitConverter.ToBoolean(buffer, startIdx++);
                        int nJoints = BitConverter.ToInt32(buffer, startIdx);
                        startIdx += 4;

                        tempBody.lJoints = new List<Joint>(nJoints);
                        tempBody.lJointsInColorSpace = new List<Point2f>(nJoints);
                        for (int j = 0; j < nJoints; j++)
                        {
                            Joint tempJoint = new Joint();
                            Point2f tempPoint = new Point2f();

                            tempJoint.jointType = (JointType)BitConverter.ToInt32(buffer, startIdx);
                            startIdx += 4;
                            tempJoint.trackingState = (TrackingState)BitConverter.ToInt32(buffer, startIdx);
                            startIdx += 4;
                            tempJoint.position.X = BitConverter.ToSingle(buffer, startIdx);
                            startIdx += 4;
                            tempJoint.position.Y = BitConverter.ToSingle(buffer, startIdx);
                            startIdx += 4;
                            tempJoint.position.Z = BitConverter.ToSingle(buffer, startIdx);
                            startIdx += 4;

                            tempPoint.X = BitConverter.ToSingle(buffer, startIdx);
                            startIdx += 4;
                            tempPoint.Y = BitConverter.ToSingle(buffer, startIdx);
                            startIdx += 4;

                            tempBody.lJoints.Add(tempJoint);
                            tempBody.lJointsInColorSpace.Add(tempPoint);
                        }
                        _lBodies.Add(tempBody);
                    }

                    //here we need to queue packets 
                    if (_lFrameRGB.Count > 0)
                    {
                        oBufferRxAlgorithm.Enqueue(_lFrameRGB, _lFrameVerts, _lBodies, timestamp, _dataLength, remotePort, localPort, tsOffsetFromUtcTime, SourceID);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " Async Rx exception msg: " + ex.ToString());
            }
        }

        public byte[] Receive(int nBytes)
        {
            if (debugFlag)
                Console.WriteLine("KinectSocket::Receive() " + nBytes);
            byte[] buffer;
            if (oSocket.Available != 0)
            {
                buffer = new byte[Math.Min(nBytes, oSocket.Available)];
                oSocket.Receive(buffer, nBytes, SocketFlags.None);
            }
            else
                buffer = new byte[0];

            return buffer;
        }

        public bool SocketConnected()
        {
            return oSocket.Connected;
        }

        /**
         * For the time being the request or any frame other than the actual data is transmitted using a 1-byte header.
         * Todo: A unified hdr needs to be applied for all signals/data transmitted (i.e. 13 bytes of header, with the 
         * signal being the 1st byte and the rest null for the server)
         */ 
        private void SendByte(byte[] sendBuffer)
        {
              try
              {
                  if (SocketConnected())
                  {
                      // Begin sending the data to the remote device.  
                      oSocket.BeginSend(sendBuffer, 0, sendBuffer.Length, 0,
                                  new AsyncCallback(SendCallback), oSocket);
                  }
              }
              catch(SocketException ex)
              {
                  Console.WriteLine("KinectSocket::SendByte Exception Message: " + ex.ToString());
              }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine("SendCallback Catch Error Msg: " + e.ToString());
            }
        }

        public void UpdateSocketState()
        {
            if (bCalibrated)
            {
                IPEndPoint _remoteIpEndPoint = oSocket.RemoteEndPoint as IPEndPoint;
                if (clientIdSok > -1)
                    sSocketState = "Client: " + clientIdSok + " Source: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = true";
                else
                    sSocketState = "\tSource: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = true";
            }
            else
            {
                IPEndPoint _remoteIpEndPoint = oSocket.RemoteEndPoint as IPEndPoint;
                if (clientIdSok > -1)
                    sSocketState = "Client: " + clientIdSok + " Source: " + SourceID + " IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = false";
                else
                    sSocketState = "\tSource: " + SourceID + "IP: " + _remoteIpEndPoint.Address + " LocalPort: " + localPort + " RemotePort: " + remotePort + " Calibrated = false";
            }

            eChanged?.Invoke();
        }

        public (List<byte> lFrameRGBOut, List<Single> lFrameVertsOut, List<Body> lBodiesOut, int timestampOut, int totalBytesOut, int sourceID) CheckRxBufferStatus(int syncTimestamp, int tsOffsetFromUtcTime, int rxBufferHoldPktsThreshold)
        {
            List <Single> lFrameVertsOut = new List<Single>();
            List<byte> lFrameRGBOut = new List<byte>();
            List<Body> lBodiesOut = new List<Body>();
            int sourceID = 0;
            int timestampOut = 0;
            int totalBytesOut = 0;

            var (_lFrameRGBOut, _lFrameVertsOut, _lBodiesOut, _timestampOut, _totalBytesOut, _sourceID) = oBufferRxAlgorithm.Dequeue(syncTimestamp, tsOffsetFromUtcTime, rxBufferHoldPktsThreshold);
            lFrameVertsOut = _lFrameVertsOut;
            lFrameRGBOut = _lFrameRGBOut;
            lBodiesOut = _lBodiesOut;
            sourceID = _sourceID;
            timestampOut = _timestampOut;
            totalBytesOut = _totalBytesOut;

            return (lFrameRGBOut, lFrameVertsOut, lBodiesOut, timestampOut, totalBytesOut, sourceID);
        }

        public void CheckIfRequestFrameIsRequired(int rxBufferHoldPktsThreshold, float stepAlgorithm)
        {
            bool reqFrame = oBufferRxAlgorithm.CheckStoredFrames(rxBufferHoldPktsThreshold);
            if (reqFrame)
                RequestLastFrame(stepAlgorithm);
        }        
    }
}
