/**
* This class has been created to hold all the information stored in the buffer from each client. 
* It creates a list of objects (lFrameRGB, lFrameVerts, lBodies, tsArray). This will help us to 
* sort the list everytime a new frame arrives (or upon request) to avoid synchronisation issues...
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

using KinectServer;
using System;
using System.Collections.Generic;

public class KinectServerBufferObject
{
    // Auto-implemented properties for trivial get and set
    public List<byte> LFrameRGB { get; set; }
    public List<Single> LFrameVerts { get; set; }
    public List<Body> LBodies { get; set; }
    public int Timestamp { get; set; }
    public int TimestampEnqueued { get; set; }
    public int DelayToEnqueue { get; set; }
    public int DeltaToEnqueue { get; set; }
    public int TotalBytes { get; set; }
    public int LocalPort { get; set; }
    public int RemotePort { get; set; }
    public int SourceID { get; set; }

    public KinectServerBufferObject(List<byte> lFrameRGB, List<Single> lFrameVerts, List<Body> lBodies, int timestamp, int timestampEnqueued, int delayToEnqueue, int deltaToEnqueue, int totalBytes, int remPort, int locPort, int sourceID)
    {
        LFrameRGB = lFrameRGB;
        LFrameVerts = lFrameVerts;
        LBodies = lBodies;
        Timestamp = timestamp;
        TimestampEnqueued = timestampEnqueued;
        DelayToEnqueue = delayToEnqueue;
        DeltaToEnqueue = deltaToEnqueue;
        TotalBytes = totalBytes;
        LocalPort = locPort;
        RemotePort = remPort;
        SourceID = sourceID;
    }
}