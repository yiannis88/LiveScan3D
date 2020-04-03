/**
* This class has been created to save the frames after being processed by the server and are ready for the live show.
* The rationale for creating a different class for storing the frames and not adding the enqueue and dequeue functions
* in another file/class (e.g. BufferRxAlgorithm.cs) is for memory efficiency. We have one BufferRxAlgorithm.cs per client
* whereas we only need one for the storing the frames for the live show! This means that for each instance of the BufferRxAlgorithm.cs
* we would have 3 arrays of fixed size initialised, that we would not use in the BufferRxAlgorithm (storing frames from client to server)
* 
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

using KinectServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BufferLiveShowAlgorithm
{
    public class BufferLiveShowAlgorithm
    {
        /* declaration of variables */
        const int MaxBufferSize = 256;
        int lastEnqueuedTs = 0;
        int lastDequeuedTs = 0;
        private bool deqAllowed = false;
        string bufferStatsL = null;

        ConcurrentQueue<KinectServerBufferLiveShowObject> bufferedObjects = new ConcurrentQueue<KinectServerBufferLiveShowObject>();
        LoggingInformation logInfo = new LoggingInformation();
        object stringOperationLock = new object();

        public int Count { get
            {
                return bufferedObjects.Count;
            } 
        }

        public void StartThread()
        {
            Thread bufferStats = new Thread(this.BufferStats);
            bufferStats.IsBackground = true;
            bufferStats.Name = "LiveBufferStats";
            bufferStats.Start();
        }

        private int deltaCorrection(int delta)
        {
            if (delta > 4000000)
                delta -= 4040000; //e.g. 190000100 - 185959800 should give 300ms and not 4040300
            else if (delta > 30000)
                delta -= 40000; // e.g. 184000100 - 183959800 should give 300ms and not 40300

            return delta;
        }

        private int GetTimestamp(int offset)
        {
            Int32 _unixTimestampSec = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            int _unixTimestampFrac = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).Milliseconds;

            _unixTimestampSec += offset / 1000;
            _unixTimestampFrac += (offset % 1000);

            if (_unixTimestampFrac < 0)
            {
                int _redFactor = _unixTimestampFrac / 1000;
                _unixTimestampSec = _unixTimestampSec + (_redFactor - 1);
                _unixTimestampFrac += 1000;
            }
            if (_unixTimestampFrac >= 1000)
            {
                int _incFactor = _unixTimestampFrac / 1000;
                _unixTimestampSec += _incFactor;
                _unixTimestampFrac -= 1000;
            }

            int hour = (_unixTimestampSec % 86400) / 3600;
            int minute = (_unixTimestampSec % 3600) / 60;
            int second = (_unixTimestampSec % 60);
            int millisecond = _unixTimestampFrac;

            return hour * 10000000 + minute * 100000 + second * 1000 + millisecond;
        }

        private void SetDirectory()
        {
            string sysTime = DateTime.Now.ToString("hhmmss");
            string sysData = DateTime.Now.ToString("ddMMyy");
            try
            {
                logInfo.CreateDirectory();
                string directory = logInfo.GetDirectory();
                if (directory == null)
                    throw new ArgumentNullException("directory");
                string myFilePath;
                myFilePath = directory + @"\OutputFileLiveShowBufferMetrics_" + sysData + "_" + sysTime + ".txt";
                logInfo.SetFilePath(myFilePath);
                string str = "DelayToEnqueue\tDeltaEnqueue\tQueueDelay\tDeltaDequeue\tFrameSize\tBuffer";
                logInfo.RedirectOutput(str);
            }
            catch (Exception e)
            {
                Console.WriteLine("Process of path in BufferLiveShowAlgorithm for live show buffer metrics failed: {0}", e.ToString());
            }
        }

        public void Enqueue(List<List<byte>> lFrameRGBIn, List<List<float>> lFrameVertsIn, List<List<Body>> lBodiesIn, int minTs, int tsOffsetFromUtcTime, int rxBufferHoldPktsThreshold, int totalSizeNoHdr, int sourceID)
        {
            if (lastEnqueuedTs > 0 && bufferedObjects.Count >= rxBufferHoldPktsThreshold)
                deqAllowed = true;

            if (bufferedObjects.Count < MaxBufferSize)
            {
                int _enqueuedTs = GetTimestamp(tsOffsetFromUtcTime);
                int _deltaCreationTs = deltaCorrection(_enqueuedTs - minTs);
                int _deltaLastEnq = (lastEnqueuedTs > 0) ? deltaCorrection(_enqueuedTs - lastEnqueuedTs) : 0;
                lastEnqueuedTs = _enqueuedTs;
                bufferedObjects.Enqueue(new KinectServerBufferLiveShowObject(lFrameRGBIn, lFrameVertsIn, lBodiesIn, minTs, _enqueuedTs, _deltaCreationTs, _deltaLastEnq, totalSizeNoHdr, sourceID));                
            }
        }

        public (List<List<byte>> lFrameRGBOut, List<List<float>> lFrameVertsOut, List<List<Body>> lBodiesOut, int _outMinTimestamp, int sourceID) Dequeue(int tsOffsetFromUtcTime)
        {
            List<List<float>> lFrameVertsOut = new List<List<float>>();
            List<List<byte>> lFrameRGBOut = new List<List<byte>>();
            List<List<Body>> lBodiesOut = new List<List<Body>>();
            int sourceID = 0;
            int _outMinTimestamp = 0;

            if (deqAllowed)
            {
                if (!bufferedObjects.IsEmpty)
                {
                    KinectServerBufferLiveShowObject item;
                    if (bufferedObjects.TryDequeue(out item))
                    {
                        try
                        {                            
                            int _buffSize = bufferedObjects.Count;
                            lFrameVertsOut = item.LFrameVertsLs; //return
                            lFrameRGBOut = item.LFrameRGBLs; //return
                            lBodiesOut = item.LBodiesLs; //return
                            sourceID = item.SourceID; //return
                            _outMinTimestamp = item.Timestamp; //return    

                            /* stats */
                            int _enqSize = item.TotalBytesNoHdr;
                            int _delayToEnqueue = item.DelayToEnqueueDisplay;
                            int _deltaEnqueue = item.DeltaToEnqueueDisplay;
                            int _dequeuedTs = GetTimestamp(tsOffsetFromUtcTime);
                            int _deltaStayedQueue = deltaCorrection(_dequeuedTs - item.TimestampEnqueued);   
                            int deltaLastDeq = (lastDequeuedTs > 0) ? deltaCorrection(_dequeuedTs - lastDequeuedTs) : 0;
                            lastDequeuedTs = _dequeuedTs;
                            lock (stringOperationLock)
                            {
                                bufferStatsL += _delayToEnqueue + "\t" + _deltaEnqueue + "\t" + _deltaStayedQueue + "\t" + deltaLastDeq + "\t" + _enqSize + "\t" + _buffSize + "\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Buffer Live-Show dequeue error: {0}", ex.ToString());
                        }
                    }
                }
            }

            return (lFrameRGBOut, lFrameVertsOut, lBodiesOut, _outMinTimestamp, sourceID);
        }

        private void BufferStats()
        {
            SetDirectory();
            int interval = 1000; //[ms] consider interval of 1 sec for now...
            int _lastTime = Convert.ToInt32(DateTime.Now.ToString("hhmmssfff")); //time and interval in ms

            while (true)
            {
                Thread.Sleep(interval);
                int _TimeNow = Convert.ToInt32(DateTime.Now.ToString("hhmmssfff")); //time and interval in ms
                if (_TimeNow > _lastTime + interval)
                {
                    lock (stringOperationLock)
                    {
                        if (bufferStatsL != null)
                        {
                            logInfo.RedirectOutput(bufferStatsL);
                            bufferStatsL = null;
                        }
                    }
                    _lastTime = _TimeNow;
                }
            }
        }
    }
}