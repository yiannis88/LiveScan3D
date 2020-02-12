/**
* This class has been created to hold all the information stored in the Live show buffer at the server. 
* It creates a list of objects (lFrameRGB, lFrameVerts, lBodies, tsArray) when enqueueing
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

using KinectServer;
using System;
using System.Collections.Generic;

public class KinectServerBufferLiveShowObject
{
    // Auto-implemented properties for trivial get and set
    public List<List<byte>> LFrameRGBLs { get; set; }
    public List<List<Single>> LFrameVertsLs { get; set; }
    public List<List<Body>> LBodiesLs { get; set; }
    public int Timestamp { get; set; }
    public int TimestampEnqueued { get; set; }
    public int DelayToEnqueueDisplay { get; set; }
    public int DeltaToEnqueueDisplay { get; set; }
    public int TotalBytesNoHdr { get; set; }

    public KinectServerBufferLiveShowObject(List<List<byte>> lFrameRGB, List<List<Single>> lFrameVerts, List<List<Body>> lBodies, int timestamp, int timestampEnqueued, int delayToEnqueueDisplay, int deltaToEnqueueDisplay, int totalBytesNoHdr)
    {
        LFrameRGBLs = lFrameRGB;
        LFrameVertsLs = lFrameVerts;
        LBodiesLs = lBodies;
        Timestamp = timestamp;
        TimestampEnqueued = timestampEnqueued;
        DelayToEnqueueDisplay = delayToEnqueueDisplay;
        DeltaToEnqueueDisplay = deltaToEnqueueDisplay;
        TotalBytesNoHdr = totalBytesNoHdr;
    }
}