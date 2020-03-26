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
//  The initial code has been significantly modified in order to support multiple TCP connections, decoupled functions, buffers,
//  and other functionalities to correctly request/receive/display the frames at the server.
//  Comments or modifications (major) are made by Ioannis Selinis (5GIC University of Surrey, 2019)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KinectServer
{
    public partial class MainWindowForm : Form
    {
        [DllImport("ICP.dll")]
        static extern float ICP(IntPtr verts1, IntPtr verts2, int nVerts1, int nVerts2, float[] R, float[] t, int maxIter = 200);

        KinectServer oServer;
        TransferServer oTransferServer;
        BufferTxAlgorithm oBufferAlgorithm; // this is used to store the frames produced by the live show and send them to the UEs!!!
        BufferLiveShowAlgorithm.BufferLiveShowAlgorithm oBufferLiveShowAlgorithm; // this is used to store the frames produced for the live show

        //Sensor poses from all of the sensors
        List<AffineTransform> lAllCameraPoses = new List<AffineTransform>();

        bool bServerRunning = false;
        bool bDebugOption = false;
        bool bRecording = false;
        bool bSaving = false;

        int reqDelayClient = 15; // the min interval when server will be requesting for the latest captured frame from the sensors
        int showLiveDelay = 20; // the min interval when server will be displaying the hologram on live show 
        int reqDelayUe = 10; // the min interval when server processes the requests for the UEs
        int rxBufferHoldPkts = 1; // by default, the threshold is set to 1 (the stored pkts are less than rxBufferHoldPkts, then server requests) 
        int tcpConnectionsNum = 1; // by default the number of TCP connections is set to 1
        int tcpConnectionsNumUe = 1; // by default the number of TCP connections is set to 1 (for UEs)
        int displayedFramesCtr = 0;
        object displayedFramesLock = new object();

        LoggingInformation logInformationPtr = new LoggingInformation();

        public string strfilePath;

        //Live view open or not
        bool bLiveViewRunning = false;

        System.Timers.Timer oStatusBarTimer = new System.Timers.Timer();

        KinectSettings oSettings = new KinectSettings();
        //The live view window class
        OpenGLWindow oOpenGLWindow;
        ConcurrentDictionary<int, Frame> sourceFrames = new ConcurrentDictionary<int, Frame>();
        List<Frame> LocalFrames = new List<Frame>();

        public MainWindowForm()
        {
            //This tries to read the settings from "settings.bin", if it fails the settings stay at default values.
            try
            {
                IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Stream stream = new FileStream("settings.bin", FileMode.Open, FileAccess.Read);
                oSettings = (KinectSettings)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read the settings.bin " + ex.ToString());
            }

            oServer = new KinectServer(oSettings);
            oServer.eSocketListChanged += new SocketListChangedHandler(UpdateListView);
            oTransferServer = new TransferServer();

            oBufferAlgorithm = new BufferTxAlgorithm();
            oBufferLiveShowAlgorithm = new BufferLiveShowAlgorithm.BufferLiveShowAlgorithm();

            oTransferServer.SetBufferClass(oBufferAlgorithm);
            oServer.SetLiveShowBuffer(oBufferLiveShowAlgorithm);
            oServer.SetMainWindowForm(this);

            InitializeComponent();
        }

        public int GetDisplayedFrameCounter()
        {
            int _out = displayedFramesCtr;
            lock (displayedFramesLock)
            {                
                displayedFramesCtr = 0;
            }
            return _out;            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //The current settings are saved to a files.
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            Stream stream = new FileStream("settings.bin", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, oSettings);
            stream.Close();

            oServer.StopServer();
            oTransferServer.StopServer();
        }

        //Starts the server
        private void btStart_Click(object sender, EventArgs e)
        {
            bServerRunning = !bServerRunning;
            if (bServerRunning)
            {
                oServer.StartServer();
                oTransferServer.StartServer();

                btStart.Text = "Stop server";
                TCPPicker.Enabled = false;
                ueTCPPicker.Enabled = false;
                btShowLive.Enabled = true;

                if (!updateWorker.IsBusy)
                    updateWorker.RunWorkerAsync();
                if (!statsWorker.IsBusy)
                    statsWorker.RunWorkerAsync();

                // FOR TESTING ONLY
                frameCounter = 0;
                LocalFrames = LoadFrames();
            }
            else
            {
                oServer.StopServer();
                oTransferServer.StopServer();
                
                btStart.Text = "Start server";
                TCPPicker.Enabled = true;
                ueTCPPicker.Enabled = true;
                btShowLive.Enabled = false;

                updateWorker.CancelAsync();
                statsWorker.CancelAsync();

                LocalFrames = new List<Frame>();
            }
        }

        //Opens the settings form
        private void btSettings_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.oSettings = oSettings;
            form.oServer = oServer;
            form.Show();
        }

        //Performs recording which is synchronized frame capture.
        //The frames are downloaded from the clients and saved once recording is finished.
        private void recordingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            oServer.ClearStoredFrames();

            int nCaptured = 0;
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending) // BackgroundWorker.CancellationPending Property: Gets a value indicating whether the application has requested cancellation of a background operation ... yanis
            {
                oServer.CaptureSynchronizedFrame();

                nCaptured++;
                SetStatusBarOnTimer("Captured frame " + (nCaptured).ToString() + ".", 5000);
            }
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " recordingWorker_DoWork with nCaptured: " + nCaptured + " updateWorker.IsBusy: " + updateWorker.IsBusy + " oOpenGLWindow " + oOpenGLWindow);
        }

        private void recordingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " recordingWorker_RunWorkerCompleted recording has been terminated, begin now to save the frames");

            //After recording has been terminated it is time to begin saving the frames.
            //Saving is downloading the frames from clients and saving them locally.
            bSaving = true;

            btRecord.Text = "Stop saving";
            btRecord.Enabled = true;

            savingWorker.RunWorkerAsync();
        }

        //Opens the live view window
        private void OpenGLWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " OpenGLWorker_DoWork, the live view window is open");

            bLiveViewRunning = true;
            oOpenGLWindow = new OpenGLWindow();
            oOpenGLWindow.setSourceFrameDict(sourceFrames);
            oOpenGLWindow.Run();
        }

        private void OpenGLWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bLiveViewRunning = false;
            //updateWorker.CancelAsync();
        }

        private void savingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " savingWorker_DoWork, use the btRecord button (to cancel) or the loop will break once there are no more stored frames " + " updateWorker.IsBusy: " + updateWorker.IsBusy + " oOpenGLWindow " + oOpenGLWindow);

            int nFrames = 0;

            string outDir = "out" + "\\" + txtSeqName.Text + "\\";
            DirectoryInfo di = Directory.CreateDirectory(outDir);

            BackgroundWorker worker = (BackgroundWorker)sender;
            //This loop is running till it is either cancelled (using the btRecord button), or till there are no more stored frames.
            while (!worker.CancellationPending)
            {
                List<List<byte>> lFrameRGBAllDevices = new List<List<byte>>();
                List<List<float>> lFrameVertsAllDevices = new List<List<float>>();

                bool success = oServer.GetStoredFrame(lFrameRGBAllDevices, lFrameVertsAllDevices);

                //This indicates that there are no more stored frames.
                if (!success)
                    break;

                nFrames++;
                int nVerticesTotal = 0;
                for (int i = 0; i < lFrameRGBAllDevices.Count; i++)
                {
                    nVerticesTotal += lFrameVertsAllDevices[i].Count;
                }

                List<byte> lFrameRGB = new List<byte>();
                List<Single> lFrameVerts = new List<Single>();

                SetStatusBarOnTimer("Saving frame " + (nFrames).ToString() + ".", 5000);
                for (int i = 0; i < lFrameRGBAllDevices.Count; i++)
                {
                    lFrameRGB.AddRange(lFrameRGBAllDevices[i]);
                    lFrameVerts.AddRange(lFrameVertsAllDevices[i]);

                    //This is ran if the frames from each client are to be placed in separate files.
                    if (!oSettings.bMergeScansForSave)
                    {
                        string outputFilename = outDir + "\\" + nFrames.ToString().PadLeft(5, '0') + i.ToString() + ".ply";
                        Utils.saveToPly(outputFilename, lFrameVertsAllDevices[i], lFrameRGBAllDevices[i], oSettings.bSaveAsBinaryPLY);
                    }
                }

                //This is ran if the frames from all clients are to be placed in a single file.
                if (oSettings.bMergeScansForSave)
                {
                    string outputFilename = outDir + "\\" + nFrames.ToString().PadLeft(5, '0') + ".ply";
                    Utils.saveToPly(outputFilename, lFrameVerts, lFrameRGB, oSettings.bSaveAsBinaryPLY);
                }
            }
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " savingWorker_DoWork, saved frames: " + nFrames);
        }

        private void savingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            oServer.ClearStoredFrames();
            bSaving = false;

            //If the live view window was open, we need to restart the UpdateWorker.
            if (bLiveViewRunning)
                RestartUpdateWorker();

            btRecord.Enabled = true;
            btRecord.Text = "Start recording";
            btRefineCalib.Enabled = true;
            btCalibrate.Enabled = true;
        }

        private int frameCounter = 0;
        //Continually requests frames that will be displayed in the live view window.
        private void updateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                Thread.Sleep(showLiveDelay);
                int _tsOffsetFromUtcTime = oServer.localOffsetTs;
                var (lFramesRGB, lFramesVerts, lFramesBody, _outMinTimestamp, _sourceID) = oBufferLiveShowAlgorithm.Dequeue(_tsOffsetFromUtcTime);
                if (lFramesRGB.Count > 0)
                {
                    var liveFrame = new Frame(new List<Single>(), new List<byte>(), new List<Body>(), _sourceID);
                    for (int i = 0; i < lFramesRGB.Count; i++)
                    {
                        liveFrame.Vertices.AddRange(lFramesVerts[i]);
                        liveFrame.RGB.AddRange(lFramesRGB[i]);
                        liveFrame.Bodies.AddRange(lFramesBody[i]);
                    }
                    lAllCameraPoses.AddRange(oServer.lCameraPoses);

                    sourceFrames[liveFrame.SourceID] = liveFrame;
                    
                    //TODO add local frames to UE
                    if (oTransferServer.UesCurrentlyConnected())
                        // TODO get real source ID's
                        oBufferAlgorithm.BufferedFrames(
                                Transformer.Apply3DTransform(liveFrame.Vertices.ToList(), Transformer.GetYRotationTransform(180)), 
                                liveFrame.RGB.ToList(), 
                                _outMinTimestamp, 
                                _tsOffsetFromUtcTime, 
                                _sourceID
                            );
                            
                    // LOCAL FRAMES FOR DEBUGGING
                    if (LocalFrames.Count > 0)
                    {
                        var localFrame = LocalFrames[frameCounter % LocalFrames.Count];
                        if (oOpenGLWindow != null) oOpenGLWindow.AddClientFrame(localFrame);
                        
                        //TODO add local frames to UE
                        if (oTransferServer.UesCurrentlyConnected())
                            // TODO get real source ID's
                            oBufferAlgorithm.BufferedFrames(
                                    Transformer.Apply3DTransform(localFrame.Vertices.ToList(), Transformer.GetYRotationTransform(180)), 
                                    localFrame.RGB.ToList(), 
                                    _outMinTimestamp, 
                                    _tsOffsetFromUtcTime, 
                                    100 // dummy source ID
                                );

                        frameCounter++;
                    }
                    // LOCAL FRAMES FOR END DEBUGGING

                    //Note the fact that a new frame was downloaded, this is used to estimate the FPS.
                    if (oOpenGLWindow != null && lFramesRGB.Count > 0)
                    {
                        //oOpenGLWindow.AddClientFrame(liveFrame);
                        displayedFramesCtr++;
                        oOpenGLWindow.CloudUpdateTick();
                    }                        
                }
            }
        }

        private void ExportFrame(List<Single> lFramesVerts, List<byte> lFramesRGB, List<Body> lFramesBody)//, List<AffineTransform> lCameraPoses)
        {

            var frame = new Frame(lFramesVerts, lFramesRGB, lFramesBody, 1);//, lCameraPoses);

            XmlSerializer serializer = new XmlSerializer(typeof(Frame));

            using(TextWriter file = new StreamWriter($"frames\\{frameCounter}.xml")){ 
                serializer.Serialize(file, frame);
            }
        }

        private List<Frame> LoadFrames(){

            List<Frame> frames = new List<Frame>();

            foreach (string fileName in Directory.EnumerateFiles("frames", "*.xml"))
            {
                Frame frame; 
                XmlSerializer serializer = new XmlSerializer(typeof(Frame));
            
                using(StreamReader file = new StreamReader(fileName)){ 
                    frame = (Frame)serializer.Deserialize(file);
                }  

                frames.Add(frame);
            }

            return frames;
                      
        }
        
        //Performs the ICP based pose refinement.
        private void refineWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (oServer.bAllCalibrated == false)
            {
                SetStatusBarOnTimer("Not all of the devices are calibrated.", 5000);
                return;
            }

            //Download a frame from each client.
            var (lAllFrameColors, lAllFrameVertices, lAllFrameBody, _outMinTimestamp, _sourceID) = oBufferLiveShowAlgorithm.Dequeue(oServer.localOffsetTs);

            //Initialize containers for the poses.
            List<float[]> Rs = new List<float[]>();
            List<float[]> Ts = new List<float[]>();
            for (int i = 0; i < lAllFrameVertices.Count; i++)
            {
                float[] tempR = new float[9];
                float[] tempT = new float[3];
                for (int j = 0; j < 3; j++)
                {
                    tempT[j] = 0;
                    tempR[j + j * 3] = 1;
                }

                Rs.Add(tempR);
                Ts.Add(tempT);
            }

            //Use ICP to refine the sensor poses.
            //This part is explained in more detail in our article (name on top of this file).            
            for (int refineIter = 0; refineIter < oSettings.nNumRefineIters; refineIter++)
            {
                for (int i = 0; i < lAllFrameVertices.Count; i++)
                {
                    List<float> otherFramesVertices = new List<float>();
                    for (int j = 0; j < lAllFrameVertices.Count; j++)
                    {
                        if (j == i)
                            continue;
                        otherFramesVertices.AddRange(lAllFrameVertices[j]);
                    }

                    float[] verts1 = otherFramesVertices.ToArray();
                    float[] verts2 = lAllFrameVertices[i].ToArray();

                    IntPtr pVerts1 = Marshal.AllocHGlobal(otherFramesVertices.Count * sizeof(float));
                    IntPtr pVerts2 = Marshal.AllocHGlobal(lAllFrameVertices[i].Count * sizeof(float));

                    Marshal.Copy(verts1, 0, pVerts1, verts1.Length);
                    Marshal.Copy(verts2, 0, pVerts2, verts2.Length);

                    ICP(pVerts1, pVerts2, otherFramesVertices.Count / 3, lAllFrameVertices[i].Count / 3, Rs[i], Ts[i], oSettings.nNumICPIterations);

                    Marshal.Copy(pVerts2, verts2, 0, verts2.Length);
                    lAllFrameVertices[i].Clear();
                    lAllFrameVertices[i].AddRange(verts2);
                }
            }

            //Update the calibration data in client machines.
            List<AffineTransform> worldTransforms = oServer.lWorldTransforms;
            List<AffineTransform> cameraPoses = oServer.lCameraPoses;

            for (int i = 0; i < worldTransforms.Count; i++)
            {
                float[] tempT = new float[3];
                float[,] tempR = new float[3, 3];
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        tempT[j] += Ts[i][k] * worldTransforms[i].R[k, j];
                    }

                    worldTransforms[i].t[j] += tempT[j];
                    cameraPoses[i].t[j] += Ts[i][j];
                }

                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        for (int l = 0; l < 3; l++)
                        {
                            tempR[j, k] += Rs[i][l * 3 + j] * worldTransforms[i].R[l, k];
                        }

                        worldTransforms[i].R[j, k] = tempR[j, k];
                        cameraPoses[i].R[j, k] = tempR[j, k];
                    }
                }
            }

            oServer.lWorldTransforms = worldTransforms;
            oServer.lCameraPoses = cameraPoses;

            oServer.SendCalibrationData();
        }

        private void refineWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " refineWorker_RunWorkerCompleted, Re-enable all of the buttons after refinement");

            //Re-enable all of the buttons after refinement.
            btRefineCalib.Enabled = true;
            btCalibrate.Enabled = true;
            btRecord.Enabled = true;
        }

        //This is used for: starting/stopping the recording worker, stopping the saving worker
        private void btRecord_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " btRecord_Click, tarting/stopping the recording worker, stopping the saving worker. No of clients: " + oServer.nClientCount + " bSaving " + bSaving);

            if (oServer.nClientCount < 1)
            {
                SetStatusBarOnTimer("At least one client needs to be connected for recording.", 5000);
                return;
            }

            //If we are saving frames right now, this button stops saving.
            if (bSaving)
            {
                btRecord.Enabled = false;
                savingWorker.CancelAsync();
                return;
            }

            bRecording = !bRecording;

            if (bRecording)
            {
                //Stop the update worker to reduce the network usage (provides better synchronization).
                updateWorker.CancelAsync();

                recordingWorker.RunWorkerAsync();
                btRecord.Text = "Stop recording";
                btRefineCalib.Enabled = false;
                btCalibrate.Enabled = false;
            }
            else
            {
                btRecord.Enabled = false;
                recordingWorker.CancelAsync();
            }
        }

        private void requestLatencyPicker_ValueChanged(object sender, EventArgs e)
        {
            reqDelayClient = (int)requestLatencyPicker.Value;
            oServer.SetReqDelayClient(reqDelayClient);
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " request interval changed and set to: " + reqDelayClient);
            updateRxFrequency();
        }

        private void liveLatencyPicker_ValueChanged(object sender, EventArgs e)
        {
            showLiveDelay = (int)liveLatencyPicker.Value;
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " show live interval changed and set to: " + showLiveDelay);
            updateLiveFrequency();
        }

        private void rxBufferHoldPicker_ValueChanged(object sender, EventArgs e)
        {
            rxBufferHoldPkts = (int)rxBufferHoldPicker.Value;
            oServer.SetRxBufferHoldScheme(rxBufferHoldPkts);

            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " the rx-buffer threshold (hold) changed to: " + rxBufferHoldPkts);
        }

        private void TCPPicker_ValueChanged(object sender, EventArgs e)
        {
            tcpConnectionsNum = (int)TCPPicker.Value;
            oServer.SetTcpConnections(tcpConnectionsNum);

            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " Number of Rx TCP connections is set to: " + tcpConnectionsNum);
        }

        private void ueTCPPicker_ValueChanged(object sender, EventArgs e)
        {
            tcpConnectionsNumUe = (int) ueTCPPicker.Value;
            oTransferServer.SetUeTcpConnections(tcpConnectionsNumUe);

            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " Number of TCP connections (UE) is set to: " + tcpConnectionsNumUe);
        }

        private void btDebug_Click(object sender, EventArgs e)
        {
            bDebugOption = !bDebugOption;
            if (bDebugOption)
            {
                oServer.DebugFlagOn();
                btDebug.Text = "Stop Log";

                string sysTime = DateTime.Now.ToString("hhmmss");
                string sysData = DateTime.Now.ToString("ddMMyy");
                try
                {
                    logInformationPtr.CreateDirectory();
                    string directory = logInformationPtr.GetDirectory();
                    if (directory == null)
                        throw new ArgumentNullException("directory");
                    strfilePath = directory + @"\OutputFileServerMWF_" + sysData + "_" + sysTime + ".txt";
                    logInformationPtr.SetFilePath(strfilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("MainWindowForm directory not found " + ex.Message);
                }
            }
            else
            {
                oServer.DebugFlagOff();
                btDebug.Text = "Start Log";
                strfilePath = string.Empty;
            }
        }

        private void btCalibrate_Click(object sender, EventArgs e)
        {
            oServer.Calibrate();
        }

        private void btRefineCalib_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " btRefineCalib_Click pressed, clients: " + oServer.nClientCount + " updateWorker.IsBusy: " + updateWorker.IsBusy + " oOpenGLWindow " + oOpenGLWindow);

            if (oServer.nClientCount < 2)
            {
                SetStatusBarOnTimer("To refine calibration you need at least 2 connected devices.", 5000);
                return;
            }

            btRefineCalib.Enabled = false;
            btCalibrate.Enabled = false;
            btRecord.Enabled = false;

            refineWorker.RunWorkerAsync();
        }

        void RestartUpdateWorker()
        {
            if (!updateWorker.IsBusy)
                updateWorker.RunWorkerAsync();
        }

        private void btShowLive_Click(object sender, EventArgs e)
        {
            RestartUpdateWorker();

            //Opens the live view window if it is not open yet.
            if (!OpenGLWorker.IsBusy)
                OpenGLWorker.RunWorkerAsync();
        }

        private void SetStatusBarOnTimer(string message, int milliseconds)
        {
            statusLabel.Text = message;

            oStatusBarTimer.Stop();
            oStatusBarTimer = new System.Timers.Timer();

            oStatusBarTimer.Interval = milliseconds;
            oStatusBarTimer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                oStatusBarTimer.Stop();
                statusLabel.Text = "";
            };
            oStatusBarTimer.Start();
        }

        //Updates the ListBox containing the connected clients, called by events inside KinectServer.
        private void UpdateListView(List<KinectSocket> socketList)
        {
            if (!string.IsNullOrEmpty(strfilePath))
                logInformationPtr.RedirectOutput("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " UpdateListView");

            List<string> listBoxItems = new List<string>();
            for (int i = 0; i < socketList.Count; i++)
            {
                listBoxItems.Add(socketList[i].sSocketState);
            }

            lClientListBox.DataSource = listBoxItems;
        }

        private void MainWindowForm_Load(object sender, EventArgs e)
        {
            updateRxFrequency();
            updateLiveFrequency();
        }

        private void bufferStats_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;

            while (bServerRunning && !worker.CancellationPending)
            {
                Thread.Sleep(200);

                rxBandwidthLabel.Invoke((MethodInvoker)delegate {
                    rxBandwidthLabel.Text = $"{oServer.Bandwidth} MB/s";
                });
                txBandwidthLabel.Invoke((MethodInvoker)delegate {
                    txBandwidthLabel.Text = $"{oTransferServer.Bandwidth} MB/s";
                });

                liveBufferLabel.Invoke((MethodInvoker)delegate {
                    liveBufferLabel.Text = oBufferLiveShowAlgorithm.Count.ToString();
                });
                txBufferLabel.Invoke((MethodInvoker)delegate {
                    txBufferLabel.Text = oBufferAlgorithm.Count.ToString();
                });

                sourceTotalLabel.Invoke((MethodInvoker)delegate {
                    sourceTotalLabel.Text = sourceFrames.Count.ToString();
                });

                var keys = new List<int>(sourceFrames.Keys);
                keys.Sort();
                var sourceIDString = String.Join(", ", keys);
                sourceListLabel.Invoke((MethodInvoker)delegate {
                    sourceListLabel.Text = $"Source IDs: ( {sourceIDString} )";
                });

                ueConnectedLabel.Invoke((MethodInvoker)delegate {
                    ueConnectedLabel.Text = oTransferServer.UesCurrentlyConnected().ToString();
                });
            }

            rxBandwidthLabel.Invoke((MethodInvoker)delegate {
                rxBandwidthLabel.Text = "0 MB/s";
            });
            txBandwidthLabel.Invoke((MethodInvoker)delegate {
                txBandwidthLabel.Text = "0 MB/s";
            });

            liveBufferLabel.Invoke((MethodInvoker)delegate {
                liveBufferLabel.Text = "0";
            });
            txBufferLabel.Invoke((MethodInvoker)delegate {
                txBufferLabel.Text = "0";
            });

            sourceTotalLabel.Invoke((MethodInvoker)delegate {
                sourceTotalLabel.Text = "0";
            });
            sourceListLabel.Invoke((MethodInvoker)delegate {
                sourceListLabel.Text = "";
            });

            ueConnectedLabel.Invoke((MethodInvoker)delegate {
                ueConnectedLabel.Text = "False";
            });
        }

        private void updateRxFrequency()
        {
            double rxFreq = Math.Round(1/ (((double) reqDelayClient) / 1000), 2);
            rxFrequencyLabel.Text = $"{rxFreq}Hz";
        }

        private void updateLiveFrequency()
        {
            double liveFreq = Math.Round(1 / (((double) showLiveDelay) / 1000), 2);
            liveFrequencyLabel.Text = $"{liveFreq}Hz";
        }
    }
}
