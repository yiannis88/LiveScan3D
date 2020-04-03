/**
 * This file is used for the communication between the server and a hololens or a UE.
 * The port listening the server for incoming connections differs from the one for the 
 * clients used for the sensors.
 * 
 *       For now, we transmit the buffered packets every x milliseconds, but we do not 
 *       address the re-ordering issue, which it ay severely impact the performance when
 *       multipath or high latency paths involved.
 * 
 * TODO: We need to create a protocol for the correct transmission and packets' sequence
 *       in order the UE to be able to correctly re-order the packets received when in 
 *       long distance. A buffer should also be applied at the UE when receiving multiple
 *       frames. This information should be carried on the header...
 */

/**
 * Some extra info wrt the lock:
 *   a)
 *      bool gotMonitor = false;                    
 *      try
 *       {
 *          Monitor.TryEnter(bufferOperationsLock, 2, ref gotMonitor);
 *          if (!gotMonitor)
 *          {
 *              throw new Exception("Couldn't enter !");
 *          }
 *          // do your stuff
 *       }
 *      finally
 *        {
 *           if (gotMonitor)
 *              Monitor.Exit(bufferOperationsLock);
 *        }
 *
 *   b)  However since there is an array, I tried with lock (bufferedFramesLVertices.SyncRoot) lock (bufferedFramesLColors.SyncRoot)
 *   
 *   c) ReaderWriterLock rwl = new ReaderWriterLock();
 *   
 *   haven't managed to make it work.... (I created a simple flag :( )
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace KinectServer
{
    public class StateUe
    {
        public int index = 0;
        public int port;
        public int nToReadBytes = 0;
        public int nBytesRead = 0;
        public bool nToReadBytesFlag = false;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];     // Receive buffer.     
    }
    public class TransferServer
    {
        private struct UeStruct
        {
            public int ue_id;   //this is the id of the UE based on the oClientSockets
            public int ue_ts;   //this is the timestamp of the latest tx frame to that UE
            public List<int> ue_port; //this is a list with the ports of the UE...
            public List<TransferSocket> ue_transferSocket;
            public List<Socket> ue_socket; // this is a list with the sockets per UE ...
        };

        List<Socket> oServerSocket = new List<Socket>();
        List<TransferSocket> lClientSockets = new List<TransferSocket>();
        private List<int> oPortsPool = new List<int>(10) { 48006, 48007, 48008, 48009, 48010, 48011, 48012, 48013, 48014, 48015 };
        private int tcpConnections = 1;

        BufferTxAlgorithm oBufferAlgorithm;

        DateTime statsLastTime;

        bool bServerRunning = false;
        private readonly object acceptLock = new object();

        public long timestampLastFrame = new long();
        private int socketBuffer = 1000000;
        private int ueId = 0;

        private ConcurrentDictionary<IPAddress, UeStruct> ipAddressMap = new ConcurrentDictionary<IPAddress, UeStruct>();

        private double bandwidth;
        public double Bandwidth
        {
            get
            {
                return bandwidth;
            }
        }

        ~TransferServer()
        {
            StopServer();
        }

        public void SetBufferClass(BufferTxAlgorithm tmp)
        {
            oBufferAlgorithm = tmp;
        }

        public void SetUeTcpConnections(int tcpNum)
        {
            if (!bServerRunning) //only before starting the server, we are allowing to use more TCP connections for UEs
                tcpConnections = Math.Max(Math.Min(tcpNum, oPortsPool.Count), 1);
        }

        public bool UesCurrentlyConnected()
        {
            return (lClientSockets.Count > 0) ? true : false;
        }        

        public void StartServer()
        {
            if (!bServerRunning)
            {
                //TcpListener or Socket as the KinectServer ?
                //Go for the first one, if TCP is only used or the second one, in case UDP might be used at some point...
                for (int ii = 0; ii < tcpConnections; ii++)
                {
                    Thread.Sleep(100);
                    Socket _tempS = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    //resetTimer();
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, oPortsPool[ii]);
                    try
                    {
                        _tempS.Bind(endPoint); // Associates a Socket with a local endpoint.
                        _tempS.Listen(10); // Listen causes a connection-oriented Socket to listen for incoming connection attempts (default value in app was 10). 
                        StateUe state = new StateUe();
                        state.index = ii;
                        state.port = oPortsPool[ii];
                        // Start an asynchronous socket to listen for connections.  
                        InitialListenerSetup(_tempS, state);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                bServerRunning = true;
                Thread receivingThread = new Thread(this.ReceivingWorker);
                receivingThread.Name = "TxReceiving";
                receivingThread.Start();

                Thread calculateMetrics = new Thread(this.CheckStatistics);
                calculateMetrics.IsBackground = true;
                calculateMetrics.Name = "TxServerMetrics";
                calculateMetrics.Start();
            }
        }

        public void StopServer()
        {
            if (bServerRunning)
            {
                bServerRunning = false;

                ipAddressMap.Clear();

                foreach (Socket socket in oServerSocket)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    finally 
                    { 
                        socket.Close(); 
                    }
                }

                oServerSocket.Clear();
                lClientSockets.Clear();
                oBufferAlgorithm.StopCleaner();
                oBufferAlgorithm.CleanAllBuffers();
            }
        }

        private async void InitialListenerSetup(Socket _tempS, StateUe state)
        {
            try
            {
                Socket readTask = await Task.Factory.FromAsync((Func<AsyncCallback, object, IAsyncResult>)_tempS.BeginAccept,
                                                (Func<IAsyncResult, Socket>)_tempS.EndAccept, state);
                
                bool _flag = AcceptCallback(readTask, state);
                if (_flag)
                {
                    oServerSocket.Add(readTask);
                    _ = lClientSockets[state.index].ReceiveFrame().ContinueWith(t => Console.WriteLine(t.Exception),
                      TaskContinuationOptions.OnlyOnFaulted);
                    InitialListenerSetup(_tempS, state);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " AcceptCallback error: " + ex.ToString());
            }
        }

        private bool AcceptCallback(Socket _newClient, StateUe state)
        {
            var port = state.port;
            bool _flag = false;
            try
            {
                _newClient.SendBufferSize = socketBuffer;
                _newClient.ReceiveBufferSize = socketBuffer;
                IPEndPoint _remoteIpEndPoint = _newClient.RemoteEndPoint as IPEndPoint;
                IPEndPoint _localIpEndPoint = _newClient.LocalEndPoint as IPEndPoint;

                lock (acceptLock)
                {                    
                    if (ipAddressMap.IsEmpty || !ipAddressMap.ContainsKey(_remoteIpEndPoint.Address))
                    {
                        Console.WriteLine("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " UE[id]: " + ipAddressMap.Count + " IP[new]: " + _remoteIpEndPoint.Address + " Port [Local, Remote]: [" + _localIpEndPoint.Port + ", " + _remoteIpEndPoint.Port + "]");
                        TransferSocket _transferSock = new TransferSocket(_newClient, ipAddressMap.Count, oBufferAlgorithm);
                        lClientSockets.Add(_transferSock);
                        oBufferAlgorithm.AddToTimestampUeList(ipAddressMap.Count);
                        UeStruct ueStr;
                        ueStr.ue_id = ueId;
                        ueStr.ue_ts = 0;
                        ueStr.ue_port = new List<int>();
                        ueStr.ue_port.Add(_remoteIpEndPoint.Port);
                        ueStr.ue_transferSocket = new List<TransferSocket>();
                        ueStr.ue_transferSocket.Add(_transferSock);
                        ueStr.ue_socket = new List<Socket>();
                        ueStr.ue_socket.Add(_newClient);
                        ipAddressMap.TryAdd(_remoteIpEndPoint.Address, ueStr);
                        ueId++;
                    }
                    else
                    {
                        // it means that the dictionary contains the key
                        UeStruct ueStr;
                        bool isItemExists = ipAddressMap.TryGetValue(_remoteIpEndPoint.Address, out ueStr);

                        Console.WriteLine("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " start existing UE[id]: " + ipAddressMap.Count + " IP[ex]: " + _remoteIpEndPoint.Address + " Port [Local, Remote]: [" + _localIpEndPoint.Port + ", " + _remoteIpEndPoint.Port + "]");

                        if (!isItemExists)
                            throw new InvalidDataException("Error msg: Transfer Server Dictionary contains the Key, hence true should be returned!");
                        //check if the socket disconnected or not by checking the port. If yes, then do nothing, update socket list otherwise (Add the new port)
                        if (!ueStr.ue_port.Exists(x => x == _remoteIpEndPoint.Port))
                        {
                            //new port, hence add port and update dictionary
                            TransferSocket _transSock = new TransferSocket(_newClient, ueStr.ue_id, oBufferAlgorithm);
                            lClientSockets.Add(_transSock);
                            UeStruct _newUeStr;
                            _newUeStr = ueStr;
                            _newUeStr.ue_port.Add(_remoteIpEndPoint.Port);
                            _newUeStr.ue_transferSocket.Add(_transSock);
                            _newUeStr.ue_socket.Add(_newClient);
                            ipAddressMap.TryUpdate(_remoteIpEndPoint.Address, _newUeStr, ueStr);
                            for (int ii = 0; ii < _newUeStr.ue_transferSocket.Count; ii++)
                            {
                                TransferSocket _initialTransferSocket = _newUeStr.ue_transferSocket[ii];// update the list of sockets per UE in TransferSocket now, in order to use the multiple parallel connections
                                _initialTransferSocket.UpdateSocketFromUeList(_newUeStr.ue_socket);
                            }

                            Console.WriteLine("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " existing UE[id]: " + ipAddressMap.Count + " IP[ex]: " + _remoteIpEndPoint.Address + " Port [Local, Remote]: [" + _localIpEndPoint.Port + ", " + _remoteIpEndPoint.Port + "]");
                        }
                    }
                }
                _flag = true;
                state.index = lClientSockets.Count - 1;
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\n AcceptCallback: Transfer Socket has been closed\n");
            }
            return _flag;
        }

         private void ReceivingWorker()
        {
            System.Timers.Timer checkConnectionTimer = new System.Timers.Timer();
            checkConnectionTimer.Interval = 1000;
            checkConnectionTimer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                for (int ii = 0; ii < lClientSockets.Count; ii++)
                {

                    if(!lClientSockets[ii].SocketConnected())
                    {
                        IPEndPoint _remoteIpEndPoint = lClientSockets[ii].GetSocket().RemoteEndPoint as IPEndPoint;
                        int _portToBeRemoved = _remoteIpEndPoint.Port;
                        UeStruct _ueStruct;
                        ipAddressMap.TryGetValue(_remoteIpEndPoint.Address, out _ueStruct);
                        List<int> _ports = new List<int>(_ueStruct.ue_port);
                        _ports.Remove(_portToBeRemoved);
                        List<Socket> _sockets = new List<Socket>(_ueStruct.ue_socket);
                        _sockets.Remove(lClientSockets[ii].GetSocket());
                        if (_ports.Count < 1)
                        {
                            ipAddressMap.TryRemove(_remoteIpEndPoint.Address, out _ueStruct); // remove all registry since no ports are left for this IP
                            oBufferAlgorithm.RemoveItemFromTimestampUeList(_ueStruct.ue_id);

                            for (int jj = 0; jj < _ueStruct.ue_transferSocket.Count; jj++)
                            {
                                TransferSocket _initialTransferSocket = _ueStruct.ue_transferSocket[jj];// update the list of sockets per UE in TransferSocket now, in order to use the multiple parallel connections
                                Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " Socket is not connected UpdateSocketFromUeList  with newsize [0]: " + _ueStruct.ue_socket.Count);
                                _initialTransferSocket.UpdateSocketFromUeList(_ueStruct.ue_socket);
                            }
                        }
                        else
                        {
                            UeStruct _newUeStruct = _ueStruct;
                            _newUeStruct.ue_port = _ports;
                            _newUeStruct.ue_socket = _sockets;
                            ipAddressMap.TryUpdate(_remoteIpEndPoint.Address, _newUeStruct, _ueStruct);

                            for (int jj = 0; jj < _newUeStruct.ue_transferSocket.Count; jj++)
                            {
                                TransferSocket _initialTransferSocket = _newUeStruct.ue_transferSocket[jj];// update the list of sockets per UE in TransferSocket now, in order to use the multiple parallel connections
                                Console.WriteLine(DateTime.Now.ToString("HH.mm.ss.fff") + " Socket is not connected UpdateSocketFromUeList  with newsize: " + _ueStruct.ue_socket.Count);
                                _initialTransferSocket.UpdateSocketFromUeList(_newUeStruct.ue_socket);
                            }
                        }                       
                        lClientSockets.RemoveAt(ii);                        
                    }
                }                
            };
        }

        private void CheckStatistics()
        {
            // Here we calculate FPS, Mbps, and other metrics before we save them in a file ... 

            while (bServerRunning)
            {
                Thread.Sleep(1000);

                int _numSockets = lClientSockets.Count;
                double totalMbps = 0;

                //string result = "";
                for (int ii = 0; ii < _numSockets; ++ii)
                {
                    int _rxBytesClient = lClientSockets[ii].lastFrameRxBytes;
                    double _mbpsClient = Math.Round(GetMbps(_rxBytesClient), 2);

                    totalMbps += _mbpsClient;
                }

                bandwidth = totalMbps;
                resetStats();
                resetTimer();
            }
        }
        private double GetMbps(int bytes)
        {
            double mbps = 0.0;
            if (bytes > 0)
            {
                uint interval = 1000; //[ms] consider interval of 1 sec for now...
                DateTime mbpsTimeNow = DateTime.UtcNow;
                TimeSpan timeDiff = mbpsTimeNow - statsLastTime;
                Int32 _elapsedMs = Convert.ToInt32(timeDiff.TotalMilliseconds);

                if (_elapsedMs >= interval)
                {
                    mbps = 1000.0 * ((bytes * 8.0) / 1e6) / (_elapsedMs);
                }
            }

            return mbps;
        }

        private void resetTimer()
        {
            statsLastTime = DateTime.UtcNow;
        }

        private void resetStats()
        {
            int _numSockets = lClientSockets.Count;
            for (int ii = 0; ii < _numSockets; ++ii)
            {
                lClientSockets[ii].lastFrameRxBytes = 0;
            }
        }

    }
}
