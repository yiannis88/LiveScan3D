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
//
//  The initial code has been significantly modified in order to support multiple TCP connections, decoupled functions, buffers, sync,
//  and other functionalities to correctly request/receive/display the frames.
//  Comments or modifications (major) are made by Ioannis Selinis (5GIC University of Surrey, 2019)
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics; // I used this for the stopwatch () --- Exec Time
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace KinectServer
{
    public delegate void SocketListChangedHandler(List<KinectSocket> list);   

    struct ClientSocketInfo
    {
        public List<KinectServerSocketInfoObject> lSocketsInfo; // holds a list with the sockets to the clients;
        public BufferRxAlgorithm bufferRxAlgorithmPtr;

        /**
         * Structure that holds information about:
         * - the socket per port (for multiple TCP connections),
         * - a reference to the BufferRx class in order to keep one buffer per client IP and not per socket
         */
        public ClientSocketInfo(List<KinectServerSocketInfoObject> listSocketsObj, BufferRxAlgorithm bufferRx)
        {
            lSocketsInfo = listSocketsObj;
            bufferRxAlgorithmPtr = bufferRx;
        }
    }

    public class State
    {
        public int index = 0;
        public int port;
        public int nToReadBytes = 0;
        public int nBytesRead = 0;
        public bool nToReadBytesFlag = false;
        public const int BufferSize = 1024;         
        public byte[] buffer = new byte[BufferSize];     // Receive buffer.     
    }

    public class KinectServer
    {
        List<Socket> oServerSocket = new List<Socket>();
        List<int> oPortsPool = new List<int>(6) {48000, 48001, 48002, 48003, 48004, 48005 };
        int tcpConnections = 1;
        ConcurrentDictionary<IPAddress, ClientSocketInfo> ipAddressBufferMap = new ConcurrentDictionary<IPAddress, ClientSocketInfo>();
        List<KinectSocket> oTotalClientSockets = new List<KinectSocket>();

        bool bServerRunning = false;
        bool sntpFlagConnected = false;

        KinectSettings oSettings;

        public event SocketListChangedHandler eSocketListChanged;
        LoggingInformation logOutputProfiling = new LoggingInformation();
        LoggingInformation logOutputStatsAlwaysOn = new LoggingInformation();

        public string myFilePathProfiling;
        public string myFilePathStatsAlwaysOn; //This is always on, to capture info regarding the server

        DateTime statsLastTime;
        public int localOffsetTs = 0;

        private readonly object acceptLock = new object();

        private int reqDelayClient = 15; // the min request delay from the clients
        private bool debugFlag = false;
        private int socketBuffer = 4194304;
        int rxBufferHoldPktsThreshold = 1; // the threshold for the rx-buffer; if the stored packets are less than this threshold, the server requests from the client
        private int clientId = 0; // this is the id per client (not per port)

        BufferLiveShowAlgorithm.BufferLiveShowAlgorithm oLiveShowBuffer;
        const string ntpServer = "pool.ntp.org";// "ntp1a.versadns.com";//"pool.ntp.org";//"0.uk.pool.ntp.org";// "time.windows.com"; //2.jp.pool.ntp.org
        InternetTime.SNTPClient sntpClient = new InternetTime.SNTPClient(ntpServer);

        //MainWindowForm oMainWindowForm;

        private double bandwidth;
        public double Bandwidth
        {
            get
            {
                return bandwidth;
            }
        }

        /**
         * This function is called in MainWindowForm::btRecord_Click () where at least one
         * client needs to be connected to tstart recording and in the MainWindowForm::btRefineCalib_Click()
         * where at least 2 clients are required for calibration!
         */
        public int nClientCount
        {
            get
            {
                int nClients = ipAddressBufferMap.Count;
                Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " KinectServer::nClientCount RELEASE " + Thread.CurrentThread.ManagedThreadId);
                return nClients;
            }
        }

        /*
        public void SetMainWindowForm(MainWindowForm _mwf)
        {
            oMainWindowForm = _mwf;
        }
        */

        public void SetLiveShowBuffer(BufferLiveShowAlgorithm.BufferLiveShowAlgorithm oLsBfr)
        {
            oLiveShowBuffer = oLsBfr;
            oLiveShowBuffer.StartThread();
        }

        public void SetRxBufferHoldScheme(int holdNum)
        {
            rxBufferHoldPktsThreshold = holdNum;
        }

        public void SetTcpConnections(int tcpNum)
        {
            if (!bServerRunning) //only before starting the server, we are allowing to use more TCP connections (this is to avoid complexity and overhead when single TCP is sufficient)
                tcpConnections = Math.Max(Math.Min(tcpNum, oPortsPool.Count), 1);
        }

        public void SetReqDelayClient(int delay)
        {
            reqDelayClient = delay;
        }

        public List<AffineTransform> lCameraPoses
        {
            get
            {
                List<AffineTransform> cameraPoses = new List<AffineTransform>();
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        cameraPoses.Add(_csInfo.lSocketsInfo[0].LClientSocket.oCameraPose);
                    }                    
                }
                return cameraPoses;
            }
            set
            {
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        int i = Array.IndexOf(ipAddressBufferMap.Keys.ToArray(), _var.Key);
                        for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++)
                        {
                            ClientSocketInfo _csInfoNew = _csInfo;
                            _csInfoNew.lSocketsInfo[jj].LClientSocket.oCameraPose = value[i];
                            ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                        }
                    }
                }
            }
        }

        public List<AffineTransform> lWorldTransforms
        {
            get
            {
                List<AffineTransform> worldTransforms = new List<AffineTransform>();
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        worldTransforms.Add(_csInfo.lSocketsInfo[0].LClientSocket.oWorldTransform);
                    }
                }
                return worldTransforms;
            }

            set
            {
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        int i = Array.IndexOf(ipAddressBufferMap.Keys.ToArray(), _var.Key);
                        for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++)
                        {
                            ClientSocketInfo _csInfoNew = _csInfo;
                            _csInfoNew.lSocketsInfo[jj].LClientSocket.oWorldTransform = value[i];
                            ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                        }
                    }
                }
            }
        }

        public bool bAllCalibrated
        {
            get
            {
                bool allCalibrated = true;
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        int i = Array.IndexOf(ipAddressBufferMap.Keys.ToArray(), _var.Key);
                        if (!_csInfo.lSocketsInfo[i].LClientSocket.bCalibrated) // calibration happens to one of the tcp connections but updates to all ports client instances
                        {
                            allCalibrated = false;
                            break;
                        }
                    }
                }
                return allCalibrated;
            }
        }

        public KinectServer(KinectSettings settings)
        {
            this.oSettings = settings;
        }

        private void SocketListChanged()
        {
            eSocketListChanged?.Invoke(oTotalClientSockets);
        }

        public void StartServer()
        {
            if (!bServerRunning)
            {
                bServerRunning = true;
                string sysTime = DateTime.Now.ToString("hhmmss");
                string sysData = DateTime.Now.ToString("ddMMyy");

                try
                {
                    logOutputStatsAlwaysOn.CreateDirectory();
                    string directory = logOutputStatsAlwaysOn.GetDirectory();
                    if (directory == null)
                        throw new ArgumentNullException("directory");

                    myFilePathStatsAlwaysOn = directory + @"\OutputFileServerMetrics_" + sysData + "_" + sysTime + ".txt";
                    logOutputStatsAlwaysOn.SetFilePath(myFilePathStatsAlwaysOn);
                    string str = "FPS_Client\tMbps_Client\tFPS_Total\tFPS_Display\tMbps_Total\tUTC\tOffset\n ";
                    logOutputStatsAlwaysOn.RedirectOutput(str);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Process of path in StartServer() for Server Metrics failed: {0}", e.ToString());
                }

                for (int ii = 0; ii < tcpConnections; ii++)
                {
                    Thread.Sleep(100);
                    Socket _tempS = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    resetTimer();
                    IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, oPortsPool[ii]);
                    try
                    {
                        _tempS.Bind(endPoint); // Associates a Socket with a local endpoint.
                        _tempS.Listen(10); // Listen causes a connection-oriented Socket to listen for incoming connection attempts (default value in app was 10). 
                        State state = new State();
                        state.index = ii;
                        state.port = oPortsPool[ii];
                        // Start an asynchronous socket to listen for connections.  
                        InitialListenerSetup(_tempS, state);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    if (debugFlag)
                        Console.WriteLine("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " KinectServer::StartServer() now bServerRunning " + bServerRunning + " socket Type " + oServerSocket[ii].SocketType + " endPoint Address " + endPoint.Address + " endPoint Port " + endPoint.Port + " __ " + oPortsPool[ii]);
                }
                Thread ntpSynchronisation = new Thread(this.NtpSynchronisation);
                ntpSynchronisation.IsBackground = true;
                ntpSynchronisation.Name = "RxNtpSynchronisation";
                ntpSynchronisation.Start();
                Thread receivingThread = new Thread(this.ReceivingWorker);
                receivingThread.IsBackground = true;
                receivingThread.Name = "RxReceiving";
                receivingThread.Start();
                Thread calculateMetrics = new Thread(this.CheckStatistics);
                calculateMetrics.IsBackground = true;
                calculateMetrics.Name = "RxServerMetrics";
                calculateMetrics.Start();
                Thread receivingLatestFrame = new Thread(this.GetLatestFrame);
                receivingLatestFrame.IsBackground = true;
                receivingLatestFrame.Name = "RxReceivingLatestFrame";
                receivingLatestFrame.Start();
                Thread reqLatestFrame = new Thread(this.RequestForLatestFrame);
                reqLatestFrame.IsBackground = true;
                reqLatestFrame.Name = "RxReqLatestFrame";
                reqLatestFrame.Start();
            }
        }

        private async void InitialListenerSetup(Socket _tempS, State state)
        {
            try
            {
                Socket readTask = await Task.Factory.FromAsync((Func<AsyncCallback, object, IAsyncResult>)_tempS.BeginAccept,
                                                (Func<IAsyncResult, Socket>)_tempS.EndAccept, state);

                oServerSocket.Add(readTask);
                bool _flag = AcceptCallback(readTask, state);
                if (_flag)
                {
                    _ = oTotalClientSockets[state.index].ReceiveFrame(localOffsetTs).ContinueWith(t => Console.WriteLine(t.Exception),
                      TaskContinuationOptions.OnlyOnFaulted);
                    InitialListenerSetup(_tempS, state);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " AcceptCallback error: " + state.index + " msg: " + ex.ToString());
            }
        }

        private bool AcceptCallback(Socket _newClient, State state)
        {
            var port = state.port;
            bool _flag = false;
            try
            {
                _newClient.SendBufferSize = socketBuffer;
                _newClient.ReceiveBufferSize = socketBuffer;
                IPEndPoint _remoteIpEndPoint = _newClient.RemoteEndPoint as IPEndPoint;
                IPEndPoint _localIpEndPoint = _newClient.LocalEndPoint as IPEndPoint;
                List<KinectServerSocketInfoObject> _listTemp = new List<KinectServerSocketInfoObject>
                {
                    new KinectServerSocketInfoObject(new KinectSocket(_newClient), _localIpEndPoint.Port, _remoteIpEndPoint.Port)
                };
                BufferRxAlgorithm _bufferTemp = new BufferRxAlgorithm();
                ClientSocketInfo _csInfo = new ClientSocketInfo(_listTemp, _bufferTemp);

                //continue with the initialisation of the rest functions/parameters (settings sent to all ports, same client... needs to be fixed)                                   
                _listTemp[0].LClientSocket.SendSettings(oSettings);

                string valAddr = null;
                valAddr += _remoteIpEndPoint.Address;
                // Split on one or more non-digit characters.
                string[] numbers = Regex.Split(valAddr, @"\D+");
                string finalIp = null;
                foreach (string value in numbers)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        int i = int.Parse(value);
                        finalIp += i;
                    }
                }

                lock (acceptLock)
                {
                    if (ipAddressBufferMap.IsEmpty || !ipAddressBufferMap.ContainsKey(_remoteIpEndPoint.Address))
                    {
                        string clientIdStr = "Client_" + clientId + "_IP_" + finalIp;
                        _listTemp[0].LClientSocket.SetBufferRx(_bufferTemp);
                        _listTemp[0].LClientSocket.SetClientId(clientIdStr, clientId);
                        _listTemp[0].LClientSocket.SetSocketStatus();
                        oTotalClientSockets.Add(_listTemp[0].LClientSocket);
                        _listTemp[0].LClientSocket.eChanged += new SocketChangedHandler(SocketListChanged);
                        eSocketListChanged?.Invoke(oTotalClientSockets);
                        clientId++;
                    }

                    ipAddressBufferMap.AddOrUpdate(_remoteIpEndPoint.Address, _csInfo,
                    (key, existingVal) =>
                    {
                        // If this delegate is invoked, then the key already exists.
                        KinectSocket _kinSock = new KinectSocket(_newClient);
                        oTotalClientSockets.Add(_kinSock);
                        _kinSock.SetBufferRx(existingVal.bufferRxAlgorithmPtr);
                        _kinSock.SetSocketStatus();
                        _kinSock.eChanged += new SocketChangedHandler(SocketListChanged);
                        eSocketListChanged?.Invoke(oTotalClientSockets);
                        existingVal.lSocketsInfo.Add(new KinectServerSocketInfoObject(_kinSock, _localIpEndPoint.Port, _remoteIpEndPoint.Port));
                        return existingVal;
                    });
                }
                _flag = true;
                state.index = oTotalClientSockets.Count - 1;
            }
            catch (ObjectDisposedException)
            {
                Debugger.Log(0, "1", "\n AcceptCallback: Socket has been closed\n");
            }
            return _flag;
        }

        public void StopServer()
        {
            if (bServerRunning)
            {
                bServerRunning = false;
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        int i = Array.IndexOf(ipAddressBufferMap.Keys.ToArray(), _var.Key);
                        ClientSocketInfo _csInfoNew = _csInfo;
                        _csInfoNew.lSocketsInfo[i].LClientSocket.sSocketState = " ";
                        ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                    }
                }

                ipAddressBufferMap.Clear();
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
                SocketListChanged();
            }
        }

        public void CaptureSynchronizedFrame()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    ClientSocketInfo _csInfoNew = _csInfo;
                    _csInfoNew.lSocketsInfo[0].LClientSocket.CaptureFrame();
                    ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                }
            }

            //Wait till frames captured
            bool allGathered = false;
            while (!allGathered)
            {
                allGathered = true;
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++) //shall we check whether at least in one of them arrived or should we update all ports for a client upon arrival?
                        {
                            if (!_csInfo.lSocketsInfo[jj].LClientSocket.bFrameCaptured)
                            {
                                allGathered = false;
                                break;
                            }
                        }
                    }
                }
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTimeMs = string.Format("{0}", ts.Milliseconds); //todo: add a few traces on Clients/UE/Server exec time and when they req/rec frames (save them on separate files) -- once this finishes then save multiple frames until request and then add header
            if (!string.IsNullOrEmpty(myFilePathProfiling))
                logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tCaptureSynchronizedFrame\t" + elapsedTimeMs);
        }

        public void Calibrate()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    ClientSocketInfo _csInfoNew = _csInfo;
                    _csInfoNew.lSocketsInfo[0].LClientSocket.Calibrate(); // here we call KinectSocket::Calibrate() just for single socket and not in all ports!!!
                    ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
            if (!string.IsNullOrEmpty(myFilePathProfiling))
                logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tCalibrate\t" + elapsedTimeMs);
        }

        public void DebugFlagOn()
        {
            //Set the flag for keeping log and redirect it to a file (Note, that this feature puts an extra burden on the machine)
            debugFlag = true;
            string sysTime = DateTime.Now.ToString("hhmmss");
            string sysData = DateTime.Now.ToString("ddMMyy");
            try
            {
                logOutputProfiling.CreateDirectory(); // we don't have to call it twice for logInformationRef and logOutputProfiling
                string directory = logOutputProfiling.GetDirectory();
                if (directory == null)
                    throw new ArgumentNullException("directory");
                myFilePathProfiling = directory + @"\OutputFileProfilingServer_" + sysData + "_" + sysTime + ".txt";
                logOutputProfiling.SetFilePath(myFilePathProfiling);
                logOutputProfiling.RedirectOutput("LocalTime\tFunction\tms");
            }
            catch (Exception e)
            {
                Console.WriteLine("Process of path in DebugFlagOn() failed: {0}", e.ToString());
            }
        }

        public void DebugFlagOff()
        {
            //Disables the Logging (only the FPS, Mbps, NoOfPkts are saved)
            debugFlag = false;
            logOutputProfiling.ResetFlag();
            myFilePathProfiling = string.Empty;
        }

        public void SendSettings()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    ClientSocketInfo _csInfoNew = _csInfo;
                    _csInfoNew.lSocketsInfo[0].LClientSocket.SendSettings(oSettings); // here we call KinectSocket::SendSettings() just for single socket and not in all ports!!!
                    ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
            if (!string.IsNullOrEmpty(myFilePathProfiling))
                logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tSendSettings\t" + elapsedTimeMs);
        }

        public void SendCalibrationData()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    ClientSocketInfo _csInfoNew = _csInfo;
                    _csInfoNew.lSocketsInfo[0].LClientSocket.SendCalibrationData(); // here we call KinectSocket::SendCalibrationData() just for single socket and not in all ports!!!
                    ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
            if (!string.IsNullOrEmpty(myFilePathProfiling))
                logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tSendCalibrationData\t" + elapsedTimeMs);
        }

        public bool GetStoredFrame(List<List<byte>> lFramesRGB, List<List<Single>> lFramesVerts)
        {
            bool bNoMoreStoredFrames;
            lFramesRGB.Clear();
            lFramesVerts.Clear();

            //Request frames
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    ClientSocketInfo _csInfoNew = _csInfo;
                    _csInfoNew.lSocketsInfo[0].LClientSocket.RequestStoredFrame(); // here we call KinectSocket::RequestStoredFrame() just for single socket and not in all ports!!!
                    ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                }
            }
            //Wait till frames received
            bool allGathered = false;
            bNoMoreStoredFrames = false;
            List<int> temporList = new List<int>();
            while (!allGathered)
            {
                allGathered = true;
                temporList.Clear();

                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++)
                        {
                            if (!_csInfo.lSocketsInfo[jj].LClientSocket.bStoredFrameReceived)
                            {
                                allGathered = false;
                                break;
                            }
                            if (_csInfo.lSocketsInfo[jj].LClientSocket.bNoMoreStoredFrames)
                            {
                                bNoMoreStoredFrames = true;
                            }
                        }
                        if (!allGathered)
                            temporList.Add(1);
                    }
                }
            }

            //Store received frames
            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++)
                    {
                        if (_csInfo.lSocketsInfo[jj].LClientSocket.lFrameRGB.Count > 0)
                        {
                            lFramesRGB.Add(new List<byte>(_csInfo.lSocketsInfo[jj].LClientSocket.lFrameRGB));
                            lFramesVerts.Add(new List<Single>(_csInfo.lSocketsInfo[jj].LClientSocket.lFrameVerts));
                            break; //for now I do not care about the stored frames, hence I just get what is in one of the ports (from the same client)
                        }
                    }
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
            if (!string.IsNullOrEmpty(myFilePathProfiling))
                logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tGetStoredFrame\t" + elapsedTimeMs);
            
            if (bNoMoreStoredFrames)
                return false;
            else
                return true;
        }

        public void RequestForLatestFrame()
        {
            while (bServerRunning)
            {
                Thread.Sleep(reqDelayClient);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        if (_csInfo.lSocketsInfo.Count >= tcpConnections)
                        {                            
                            try
                            {
                                ClientSocketInfo _csInfoNew = _csInfo;
                                _csInfoNew.lSocketsInfo[0].LClientSocket.CheckIfRequestFrameIsRequired(rxBufferHoldPktsThreshold);
                                ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("KinectServer::RequestForLatestFrame Exception Message: " + ex.ToString());
                            }
                        }
                    }
                }
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
                if (!string.IsNullOrEmpty(myFilePathProfiling))
                    logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tRequestForLatestFrame\t" + elapsedTimeMs);
            }
        }

        public void GetLatestFrame()
        {
            while (bServerRunning)
            {
                Thread.Sleep(5);
                List<List<byte>> lFramesRGB = new List<List<byte>>();
                List<List<float>> lFramesVerts = new List<List<float>>();
                List<List<Body>> lFramesBody = new List<List<Body>>();
                int sourceID = 0; // should this be initialised to an unused ID? for error checking?
                int minTs = int.MaxValue;
                int totalSizeNoHdr = 0;

                //here we check whether there is any packet in the rx-buffer before we request from the client (per client basis)
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                int syncTimestamp = 0;
                bool refTsFlag = false;

                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        try
                        {
                            var (lFrameRGBOut, lFrameVertsOut, lBodiesOut, timestampOut, totalBytesOut, _sourceID) = _csInfo.lSocketsInfo[0].LClientSocket.CheckRxBufferStatus(syncTimestamp, localOffsetTs, rxBufferHoldPktsThreshold);
                            
                            sourceID = _sourceID; // should this be moved to lFrameRGBOut if statement?
                            
                            if (!refTsFlag && timestampOut > 0)
                            {
                                syncTimestamp = timestampOut;
                                refTsFlag = true; // we found the reference frame for Timestamp and sync operations!!!
                            }
                            if (lFrameRGBOut.Count > 0) // if there weren't any available frames for the client to transmit, no need to add null lists
                            {
                                totalSizeNoHdr += totalBytesOut;
                                lFramesRGB.Add(lFrameRGBOut);
                                lFramesVerts.Add(lFrameVertsOut);
                                lFramesBody.Add(lBodiesOut);
                                if (timestampOut < minTs)
                                    minTs = timestampOut;
                            }
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine("KinectServer::GetLatestFrame Exception Message: " + ex.ToString());
                        }                        
                    }
                }

                if (lFramesRGB.Count > 0)
                {
                    oLiveShowBuffer.Enqueue(lFramesRGB, lFramesVerts, lFramesBody, minTs, localOffsetTs, rxBufferHoldPktsThreshold, totalSizeNoHdr, sourceID);
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
                if (!string.IsNullOrEmpty(myFilePathProfiling))
                    logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tGetLatestFrame\t" + elapsedTimeMs + "\t" + lFramesRGB.Count);
            }
        }

        public void ClearStoredFrames()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (var _var in ipAddressBufferMap)
            {
                ClientSocketInfo _csInfo;
                if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                {
                    for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++)
                    {
                        ClientSocketInfo _csInfoNew = _csInfo;
                        _csInfoNew.lSocketsInfo[jj].LClientSocket.ClearStoredFrames();
                        ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                    }
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTimeMs = string.Format("{0}", ts.Milliseconds);
            if (!string.IsNullOrEmpty(myFilePathProfiling))
                logOutputProfiling.RedirectOutput(DateTime.Now.ToString("hh.mm.ss.fff") + "\tClearStoredFrames\t" + elapsedTimeMs);
        }

        /**
         * The ReceivingWorker function checks what clients are still connected and triggers the reception of a frame based on the information transmitted by the client
         */
        private void ReceivingWorker()
        {
            System.Timers.Timer checkConnectionTimer = new System.Timers.Timer();
            checkConnectionTimer.Interval = 1000;

            checkConnectionTimer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                foreach (var _var in ipAddressBufferMap)
                {
                    ClientSocketInfo _csInfo;
                    if (ipAddressBufferMap.TryGetValue(_var.Key, out _csInfo))
                    {
                        try
                        {
                            IPAddress _ipAddress = _var.Key;                            
                            for (int jj = 0; jj < _csInfo.lSocketsInfo.Count; jj++)
                            {
                                if (!_csInfo.lSocketsInfo[jj].LClientSocket.SocketConnected())
                                {
                                    _csInfo.lSocketsInfo.RemoveAt(jj);
                                    int i = Array.IndexOf(ipAddressBufferMap.Keys.ToArray(), _var.Key);
                                    oTotalClientSockets.RemoveAt(i + jj); // assuming that first we connect clients serially 
                                    eSocketListChanged?.Invoke(oTotalClientSockets);
                                    if (_csInfo.lSocketsInfo.Count > 0)
                                    {
                                        ClientSocketInfo _csInfoNew = _csInfo;
                                        ipAddressBufferMap.TryUpdate(_var.Key, _csInfoNew, _csInfo);
                                    }
                                    else
                                    {
                                        ipAddressBufferMap.TryRemove(_ipAddress, out _csInfo);
                                    }                                        
                                    continue;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " KinectServer::ReceivingWorker/Delegate Exception MSG " + ex.ToString());
                        }
                    }
                }
            };
        }

        /**
         * Scheduler (following C++) could be as:
         *   i) simply use a thread sleep every second in an endless while loop
         *  ii) use of System.Windows.Forms.Timer and make use of an eventhandler
         * iii) use of a TimerCallback: (private void CheckStatistics (Object stateInfo){} and AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;)
         * 
         * 
         * Option iii) tested but failed to smoothly work when the live show window opens!
         * This is due to the fact that other thread signals and the timer terminates (after two loops)
         */
        private void CheckStatistics()
        {
            // Here we calculate FPS, Mbps, and other metrics before we save them in a file ... 

            while (bServerRunning)
            {
                Thread.Sleep(1000);

                int _numSockets = oTotalClientSockets.Count;
                int totalFrames = 0;
                double totalMbps = 0;
                bool _flagEarly = false;

                string result = "";
                for (int ii = 0; ii < _numSockets; ++ii)
                {
                    int _framesClient = oTotalClientSockets[ii].lastFrameCtr + oTotalClientSockets[ii].storedFrameCtr;
                    double _fpsClient = Math.Round(GetFps(_framesClient), 2);
                    if (_fpsClient == -1.0)
                    {
                        _flagEarly = true;
                        break;
                    }
                    int _rxBytesClient = oTotalClientSockets[ii].lastFrameRxBytes + oTotalClientSockets[ii].storedFrameRxBytes;
                    double _mbpsClient = Math.Round(GetMbps(_rxBytesClient), 2);

                    result += _fpsClient + "\t" + _mbpsClient + "\t";
                    totalFrames += _framesClient;
                    totalMbps += _mbpsClient;
                }

                if (!_flagEarly)
                {
                    bandwidth = totalMbps;
                    double _totalFps = Math.Round(GetFps(totalFrames), 2);
                    //double _displayedFps = Math.Round(GetFps(oMainWindowForm.GetDisplayedFrameCounter()), 2);
                    result += _totalFps + "\t" + 
                        //_displayedFps + 
                        "\t" + totalMbps + "\t" + DateTime.Now.ToString("hh.mm.ss.fff") + "\t" + localOffsetTs;
                    logOutputStatsAlwaysOn.RedirectOutput(result);
                    resetStats();
                    resetTimer();
                }
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

        private double GetFps(int frameCtr)
        {
            double fps = -1.0;
            uint interval = 1000; //[ms] consider interval of 1 sec for now...
            DateTime fpsTimeNow = DateTime.UtcNow;
            TimeSpan timeDiff = fpsTimeNow - statsLastTime;
            Int32 _elapsedMs = Convert.ToInt32(timeDiff.TotalMilliseconds);

            if (_elapsedMs >= interval)
            {
                fps = 1000.0 * ((double)frameCtr / (_elapsedMs)); // convert ms to sec and calculate fps    
            }

            return fps;
        }

        private void resetTimer()
        {
            statsLastTime = DateTime.UtcNow;
        }

        private void resetStats()
        {
            int _numSockets = oTotalClientSockets.Count;
            for (int ii = 0; ii < _numSockets; ++ii)
            {
                oTotalClientSockets[ii].lastFrameCtr = 0;
                oTotalClientSockets[ii].storedFrameCtr = 0;
                oTotalClientSockets[ii].lastFrameRxBytes = 0;
                oTotalClientSockets[ii].storedFrameRxBytes = 0;
            }
        }

        private void NtpSynchronisation()
        {
            while (bServerRunning)
            {
                sntpFlagConnected = sntpClient.Connect(true);
                localOffsetTs = 0;
                if (sntpFlagConnected)
                {
                    localOffsetTs = sntpClient.LocalClockOffset;
                    Console.WriteLine(sntpClient.ToString());
                }                  
                
                Thread.Sleep(600000); // 10 minutes request ntp sync
            }
        }
    }
}
