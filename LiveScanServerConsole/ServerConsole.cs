using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace KinectServer
{
    class ServerConsole
    {
        KinectServer oServer;
        TransferServer oTransferServer;
        BufferTxAlgorithm oBufferAlgorithm; // this is used to store the frames produced by the live show and send them to the UEs!!!
        BufferLiveShowAlgorithm.BufferLiveShowAlgorithm oBufferLiveShowAlgorithm; // this is used to store the frames produced for the live show

        KinectSettings oSettings = new KinectSettings();

        //Sensor poses from all of the sensors
        List<AffineTransform> lAllCameraPoses = new List<AffineTransform>();

        bool bServerRunning = false;
        bool bDebugOption = false;
        bool bRecording = false;
        bool bSaving = false;

        ConcurrentDictionary<int, Frame> sourceFrames = new ConcurrentDictionary<int, Frame>();

        private string input = "";
        private string[] exitCommands = { "exit", "ex" };

        public ServerConsole()
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
            oTransferServer = new TransferServer();

            oBufferAlgorithm = new BufferTxAlgorithm();
            oBufferLiveShowAlgorithm = new BufferLiveShowAlgorithm.BufferLiveShowAlgorithm();

            oTransferServer.SetBufferClass(oBufferAlgorithm);
            oServer.SetLiveShowBuffer(oBufferLiveShowAlgorithm);
        }

        ~ServerConsole() {
            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            Stream stream = new FileStream("settings.bin", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, oSettings);
            stream.Close();

            oServer.StopServer();
            oTransferServer.StopServer();
        }

        public void Start()
        {
            Console.WriteLine("  LiveScan3D  ");
            Console.WriteLine("==============");
            Console.WriteLine("");

            while(Array.IndexOf(exitCommands, input) == -1)
            {
                NextInput();
            }
            
        }

        private void NextInput()
        {
            Console.Write("> ");
            input = Console.ReadLine().Trim().ToLower();

            switch (input)
            {
                case "start":
                    StartServer();
                    break;
                case "stop":
                    StopServer();
                    break;
                default:
                    if(Array.IndexOf(exitCommands, input) == -1)
                        Console.WriteLine($"{input} not a recognised command");
                    break;
            }
        }

        private void StartServer()
        {
            if (!bServerRunning)
            {
                oServer.StartServer();
                oTransferServer.StartServer();
                Console.WriteLine("Server started");
            }
            else
            {
                Console.WriteLine("Server already running");
            }
        }

        private void StopServer()
        {
            if (!bServerRunning)
            {
                oServer.StartServer();
                oTransferServer.StartServer();
                Console.WriteLine("Server stopped");
            }
            else
            {
                Console.WriteLine("Server not running");
            }
        }
    }
}
