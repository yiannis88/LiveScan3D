/**
 * This class is used for the buffering of the packets. When enabled,
 * the server buffers the packet that are to be transmitted to the UE.
 * Note that those packets that have already been transmitted to the UE
 * are discarded from the buffer. 
 *  TODO: Discard policies (e.g. if buffer is full, then drop the oldest or the 
 *        newest packet (?))
 *        
 * Ioannis Selinis 2019 (5GIC, University of Surrey)
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KinectServer
{
    public class BufferTxAlgorithm
    {
        /* declaration of variables */
        const int MaxBufferSize = 256;
        int lastEnqueuedTs = 0;
        int lastDequeuedTs = 0;
        int OffsetFromUtcTS = 0;
        int minTsDelete = 0;
        List<int> lTimestampsUes = new List<int>();

        ConcurrentQueue<KinectServerBufferTxObject> bufferedObjects = new ConcurrentQueue<KinectServerBufferTxObject>();
        LoggingInformation logInfo = new LoggingInformation();
        object stringOperationLock = new object();

        Thread cleaner;

        public int Count { get
            {
                return bufferedObjects.Count;
            } 
        }

        private void StartThread()
        {
            cleaner = new Thread(this.CleanBuffer);
            cleaner.IsBackground = true;
            cleaner.Start();
        }

        public void StopCleaner()
        {
            if (cleaner != null)
            {
                cleaner.Abort();
            }
        }

        public void AddToTimestampUeList(int index) // this should be called when a new UE connects (irrespectively from the number of parallel connections)
        {
            //add the new UE's ID in the list
            lTimestampsUes.Add(0);
            Console.WriteLine("AddToTimestampUeList with " + lTimestampsUes.Count);
        }

        public void RemoveItemFromTimestampUeList(int index)
        {
            try
            {
                lTimestampsUes.RemoveAt(index - 1);
                if (lTimestampsUes.Count < 1)
                    CleanAllBuffers();
            }
            catch (Exception ex)
            {
                Console.WriteLine("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " Exception error: item at " + (index - 1) + " is removed from the list, new size: " + lTimestampsUes.Count + " msg: " + ex.ToString());
            }
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
                myFilePath = directory + @"\OutputFileTxBufferMetrics_" + sysData + "_" + sysTime + ".txt";
                logInfo.SetFilePath(myFilePath);
                string str = "Buffer\tDelayToEnqueue\tDeltaEnqueue\tQueueDelay\tDeltaDequeue";
                logInfo.RedirectOutput(str);
                //Console.WriteLine("At " + DateTime.Now.ToString("hh.mm.ss.fff") + " SetDirectory() for Tx Buffer");
            }
            catch (Exception e)
            {
                Console.WriteLine("Process of path in BufferLiveShowAlgorithm for live show buffer metrics failed: {0}", e.ToString());
            }
        }

        private byte[] CreateFrameForTransmission(List<float> lVerticesFrame, List<byte> lAllColor, int timestampFrame, int sourceID)
        {
            int nVerticesToSend = lVerticesFrame.Count;
            int size = (sizeof(short) * nVerticesToSend) + (sizeof(byte) * nVerticesToSend);
            int hdr_indicator = 1, hdr_source = 1, hdr_size = 4, hdr_compr = 4, hdr_ts = 4;
            int hdrSize = hdr_indicator + hdr_source + hdr_size + hdr_compr + hdr_ts; //1 byte for indicating the frame type, 1 byte for source ID, 4 bytes for size, 4 bytes for compression, and 4 bytes for timestamp (UTC)
            int sizeBuffer = size + hdrSize; // whole packet size
            byte[] buffer = new byte[sizeBuffer];
            int compression = 0; //not really used
            try
            {
                short[] sVertices = Array.ConvertAll(lVerticesFrame.ToArray(), x => (short)(x * 1000));
                buffer[0] = (int)MessageUtils.SIGNAL_MESSAGE_TYPE.MSG_SEND_LAST_FRAME; // MESSAGE TYPE
                buffer[1] = (byte)sourceID; // SOURCE
                Buffer.BlockCopy(BitConverter.GetBytes(sizeBuffer - hdrSize), 0, buffer, hdr_indicator + hdr_source, hdr_size); // SIZE
                Buffer.BlockCopy(BitConverter.GetBytes(compression), 0, buffer, hdr_indicator + hdr_source + hdr_size, hdr_compr); // COMPRESSION
                Buffer.BlockCopy(BitConverter.GetBytes(timestampFrame), 0, buffer, hdr_indicator + hdr_source + hdr_size + hdr_compr, hdr_ts); // TS
                Buffer.BlockCopy(sVertices, 0, buffer, hdrSize, sVertices.Length * 2);
                Buffer.BlockCopy(lAllColor.ToArray(), 0, buffer, (sVertices.Length * 2) + hdrSize, lAllColor.ToArray().Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreateFrameForTransmission buffer error: " + ex.ToString());
            }

            return buffer;
        }

        //here we save the latest received frames, ready for transmission to the UEs (e.g. hololens)
        public void BufferedFrames(List<float> lVerticesFrame, List<byte> lAllColor, int timestampFrameOrig, int tsOffsetFromUtcTime, int sourceID)
        {
            if (OffsetFromUtcTS == 0)
            {
                StartThread();
            }
            OffsetFromUtcTS = tsOffsetFromUtcTime;
            if (bufferedObjects.Count < MaxBufferSize)
            {
                byte[] _enqBuffer = CreateFrameForTransmission(lVerticesFrame, lAllColor, timestampFrameOrig, sourceID); //create the packet ready for tx
                

                int _enqueuedTs = GetTimestamp(tsOffsetFromUtcTime);
                int _deltaCreationTs = deltaCorrection(_enqueuedTs - timestampFrameOrig);
                int _deltaLastEnq = (lastEnqueuedTs > 0) ? deltaCorrection(_enqueuedTs - lastEnqueuedTs) : 0;
                lastEnqueuedTs = _enqueuedTs;
                lock (stringOperationLock)
                {
                    //Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " BufferTxAlgorithm::Enqueue ___ timestampFrameOrig: " + timestampFrameOrig + " _enqueuedTs " + _enqueuedTs + " _frameSize: " + _enqBuffer.Length + " Buffer: " + bufferedObjects.Count);
                    bufferedObjects.Enqueue(new KinectServerBufferTxObject(_enqBuffer, _enqueuedTs, _deltaCreationTs, _deltaLastEnq, sourceID));
                }
            }
        }

        //here, we retrieve the lists for transmission to a specific UE
        public List<byte[]> GetBufferedFrames(int UeId)
        {
            List<byte[]> _LFramesForTransmission = new List<byte[]>();
            try
            {
                int UelastFrameTs = lTimestampsUes[UeId];
                int tsUpdated = lTimestampsUes[UeId];

                //Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " BufferTxAlgorithm::TryDequeue for UeId " + UeId + " with " + lTimestampsUes.Count + " lTimestampsUes[UeId] " + lTimestampsUes[UeId] + " UelastFrameTs " + UelastFrameTs + " tsUpdated " + tsUpdated + " Buffer: " + bufferedObjects.Count);

                if (!bufferedObjects.IsEmpty)
                {
                    lock (stringOperationLock)
                    {
                        foreach (var item in bufferedObjects)
                        {
                            if (item.TimestampEnqueued > UelastFrameTs)
                            {
                                _LFramesForTransmission.Add(item.LFrameForTx);
                                tsUpdated = item.TimestampEnqueued;
                            }
                            else
                                break; // buffer is ordered due to display (it was ordered in buffer rx)
                        }
                    }
                }
                if (_LFramesForTransmission.Count > 0)
                {
                    lTimestampsUes[UeId] = tsUpdated;
                    minTsDelete = lTimestampsUes.Min();
                    //Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " BufferTxAlgorithm::Dequeue  frames: " + _LFramesForTransmission.Count + " Buffer: " + bufferedObjects.Count + " LastTsDequeued: " + lTimestampsUes[UeId] + " minTsDelete " + minTsDelete);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("BufferTxAlgorithm::GetBufferedFrames error " + UeId + " msg: " + ex.ToString());
            }
            return _LFramesForTransmission;
        }

        private void CleanBuffer()
        {
            SetDirectory();
            while (true)
            {
                Thread.Sleep(20); // every 20 milliseconds clean queue
                while (!bufferedObjects.IsEmpty)
                {
                    KinectServerBufferTxObject item;
                    string bufferStatsL = null;
                    //Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " BufferTxAlgorithm::CleanBuffer TryClean buffer of size: " + bufferedObjects.Count);
                    if (bufferedObjects.TryPeek(out item))
                    {
                        //Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " Object with EnqueuedTs: " + item.TimestampEnqueued + " TsToBeDeleted (upto): " + minTsDelete);
                        if (item.TimestampEnqueued <= minTsDelete)
                        {
                            if (bufferedObjects.TryDequeue(out item))
                            {
                                int _queueSize = bufferedObjects.Count();
                                int _delayToEnqueue = item.DeltaCreationEnq;
                                int _deltaEnqueue = item.DeltaEnq;
                                int _dequeuedTs = GetTimestamp(OffsetFromUtcTS);
                                int _deltaStayedQueue = deltaCorrection(_dequeuedTs - item.TimestampEnqueued);
                                int deltaLastDeq = (lastDequeuedTs > 0) ? deltaCorrection(_dequeuedTs - lastDequeuedTs) : 0;
                                lastDequeuedTs = _dequeuedTs;

                                bufferStatsL += _queueSize + "\t" + _delayToEnqueue + "\t" + _deltaEnqueue + "\t" + _deltaStayedQueue + "\t" + deltaLastDeq + "\n";
                                //Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " BufferTxAlgorithm::CleanBuffer Removed Ts: " + item.TimestampEnqueued + " Stayed in Queue [ms]: " + _deltaStayedQueue + " Buffer: " + bufferedObjects.Count);
                            }else
                            {
                                Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " BufferTxAlgorithm::CleanBuffer Item Cleared Before Removal, Buffer: " + bufferedObjects.Count);
                            }
                        }
                        else
                            break; // break the loop and check again the queue in 20ms (Note that the queue is based on FIFO --> ascending order)
                    }

                    if (bufferStatsL != null)
                    {
                        logInfo.RedirectOutput(bufferStatsL);
                    }
                }
            }
        }

        public void CleanAllBuffers()
        {
            lock (stringOperationLock)
            {
                while (!bufferedObjects.IsEmpty)
                {
                    KinectServerBufferTxObject item;
                    bufferedObjects.TryDequeue(out item);
                }
                Console.WriteLine(DateTime.Now.ToString("hh.mm.ss.fff") + " CleanAllBuffers, size should be zero: " + bufferedObjects.Count);
            }
        }
    }
}
