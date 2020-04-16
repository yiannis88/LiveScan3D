/**
* This class has been created to save the frames received from the clients (i.e. sensors). 
* One buffer per client socket, holding up to 255 frames per client. 
* Operation: 
* The server first checks the size of the buffer, if
* 
*  - the buffer is empty --> the server requests for any frames produced by the client (note that could be 0, 1, or more).
*                            The first frame received, is added to the lists (ready for display, unless multiple clients are used
*                            which means that rendering takes place), whereas the following frames (if any) are saved at the buffer
*                            
*  - the buffer is not empty but less than a threshold --> the server adds to the lists the first available frame from the buffer and
*                                                          then requests for any frames produced by the client, in order to fill buffer
*                                                          
*  - the buffer is above the threshold --> the server adds to the lists the first available frame from the buffer and does not request 
*                                          for any frames from the clients.
*                                          
* * Known issues: 
*   - Ordering issues may occur, when multiple parallel connections
*     as frames delivered in different ports may experience different 
*     latency due to different path (https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-bounding-and-blocking?redirectedfrom=MSDN).
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

using KinectServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class BufferRxAlgorithm
{
    /* declaration of variables */
    const int MaxBufferSize = 256;
    string clientId_str = null;
    int lastDequeuedTs = 0;
    int lastEnqueuedTs = 0;
    string bufferStatsL = null;

    public readonly ConcurrentQueue<KinectServerBufferObject> bufferedObjects = new ConcurrentQueue<KinectServerBufferObject>();
    LoggingInformation logInfo = new LoggingInformation();
    object stringOperationLock = new object();

    enum SyncType
    {
        DEQUEUE_FRAME,
        OLD_FRAME,
        NEW_FRAME
    };

    public void SetClientIdPath(string cl_id)
    {
        clientId_str = cl_id;

        Thread bufferStats = new Thread(this.BufferStats);
        bufferStats.IsBackground = true;
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
            if (clientId_str != null)
            {
                logInfo.CreateDirectory();
                string directory = logInfo.GetDirectory();
                if (directory == null)
                    throw new ArgumentNullException("directory");
                string myFilePath;
                myFilePath = directory + @"\OutputFileRxBufferMetrics_" + clientId_str + "_" + sysData + "_" + sysTime + ".txt";
                logInfo.SetFilePath(myFilePath);
                string str = "DelayToEnqueue\tDeltaEnqueue\tQueueDelay\tDeltaDequeue\tFrameSize\tBuffer\tRemotePort\tLocalPort";
                logInfo.RedirectOutput(str);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Process of path in BufferLiveShowAlgorithm for live show buffer metrics failed: {0}", e.ToString());
        }
    }

    //this Enqueue function is used for the rx buffer (clients to server)
    public void Enqueue(List<byte> lFrameRGBIn, List<Single> lFrameVertsIn, List<Body> lBodiesIn, int timestamp, int _totalBytes, int remPort, int locPort, int tsOffsetFromUtcTime, int sourceID)
    {
        if (bufferedObjects.Count < MaxBufferSize)
        {
            int tsEnqueued = GetTimestamp(tsOffsetFromUtcTime);
            int delta = deltaCorrection(tsEnqueued - timestamp);
            int deltaLastEnq = (lastEnqueuedTs > 0) ? deltaCorrection(tsEnqueued - lastEnqueuedTs) : 0;
            lastEnqueuedTs = tsEnqueued;

            bufferedObjects.Enqueue(new KinectServerBufferObject(lFrameRGBIn, lFrameVertsIn, lBodiesIn, timestamp, tsEnqueued, delta, deltaLastEnq, _totalBytes, remPort, locPort, sourceID));
        }
    }

    public (List<byte> lFrameRGBOut, List<Single> lFrameVertsOut, List<Body> lBodiesOut, int timeStampOut, int totalBytesOut, int sourceID) Dequeue(int syncTimestamp, int tsOffsetFromUtcTime, int rxBufferHoldPktsThreshold)
    {
        List <Single> lFrameVertsOut = new List<Single>();
        List<byte> lFrameRGBOut = new List<byte>();
        List<Body> lBodiesOut = new List<Body>();
        int sourceID = 0;
        int timeStampOut = 0;
        int dequeuedTimeUtc = 0;
        int totalBytesOut = 0;

        bool _flag = true;
        if (lastDequeuedTs == 0)
        {
            _flag = bufferedObjects.Count >= rxBufferHoldPktsThreshold ? true : false;
        }

        if (!bufferedObjects.IsEmpty && _flag)
        {
            bufferedObjects.OrderBy(x => x.Timestamp);
            KinectServerBufferObject item;
            if (bufferedObjects.TryPeek(out item))
            {
                int _timestampToCheck = item.Timestamp;
                int optionDequeue = IsFrameEligibleForDequeueing(syncTimestamp, _timestampToCheck);
                if (optionDequeue == 0)//dequeue
                {
                    int _buffSize = bufferedObjects.Count - 1;
                    bufferedObjects.TryDequeue(out item);

                    lFrameRGBOut = item.LFrameRGB; //return
                    lFrameVertsOut = item.LFrameVerts; //return
                    lBodiesOut = item.LBodies; //return
                    sourceID = item.SourceID; //return
                    timeStampOut = item.Timestamp; //return

                    /* stats */
                    totalBytesOut = item.TotalBytes;
                    int _totalSize = totalBytesOut + 8; //4 is the 4-bytes for the timestamp + 4 bytes for the hdr
                    int tsEnqueued = item.TimestampEnqueued;
                    dequeuedTimeUtc = GetTimestamp(tsOffsetFromUtcTime);
                    int origLocPort = item.LocalPort;
                    int origRemPort = item.RemotePort;
                    int _delayToEnqueue = item.DelayToEnqueue;
                    int _deltaEnqueue = item.DeltaToEnqueue;
                    int _lastDequeuedDelta = (lastDequeuedTs > 0) ? deltaCorrection(dequeuedTimeUtc - lastDequeuedTs) : 0;
                    lastDequeuedTs = dequeuedTimeUtc;                 
                    int _delQueue = deltaCorrection(dequeuedTimeUtc - tsEnqueued);
                    lock(stringOperationLock)
                    {
                        bufferStatsL += _delayToEnqueue + "\t" + _deltaEnqueue + "\t" + _delQueue + "\t" + _lastDequeuedDelta + "\t" + _totalSize + "\t" + _buffSize + "\t" + origRemPort + "\t" + origLocPort + "\n";
                    }
                }
                else if (optionDequeue == 1)//old frame - drop it no longer needed
                {
                    bufferedObjects.TryDequeue(out item);
                }
                else
                {
                    Console.WriteLine("New frame do nothing"); //if optionDequeue == 2 --> new frame, hence do nothing!!! wait for the next dequeue signal
                }
            }
        }
        return (lFrameRGBOut, lFrameVertsOut, lBodiesOut, timeStampOut, totalBytesOut, sourceID);
    }

    public bool CheckStoredFrames(int rxBufferHoldPktsThreshold)
    {
        bool reqFrames;
        reqFrames = bufferedObjects.Count < rxBufferHoldPktsThreshold ? true : false;       
        return reqFrames;
    }

    private int IsFrameEligibleForDequeueing(int timestamp, int timestampToCheck)
    {
        int dequeueType = 0;
        int syncInterval = 20; // this comes from the fact that kinect v2 produces frames with a rate of 30Hz (30 FPS --> every 33ms)
        int timestampL = timestamp - syncInterval;
        int timestampU = timestamp + syncInterval;

        if (timestamp == 0 || (timestampToCheck >= timestampL && timestampToCheck <= timestampU))
            dequeueType = (int)SyncType.DEQUEUE_FRAME;
        else if (timestampToCheck < timestampL)
            dequeueType = (int)SyncType.OLD_FRAME;
        else if (timestampToCheck > timestampU)
            dequeueType = (int)SyncType.NEW_FRAME;

        return dequeueType;
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