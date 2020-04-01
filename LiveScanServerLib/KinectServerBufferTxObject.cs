/**
* This class has been created to hold all the information stored in the TX buffer at the server. 
* It creates a list of objects when enqueueing; the frame ready for transmission and the enqueued timestamp
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

public class KinectServerBufferTxObject
{
    // Auto-implemented properties for trivial get and set
    public byte[] LFrameForTx { get; set; }
    public int TimestampEnqueued { get; set; }
    public int DeltaCreationEnq { get; set; }
    public int DeltaEnq { get; set; }
    public int SourceID { get; set; }

    public KinectServerBufferTxObject(byte[] lFrameForTx, int timestampEnqueued, int deltaCreationEnq, int deltaEnq, int sourceID)
    {
        LFrameForTx = lFrameForTx;
        TimestampEnqueued = timestampEnqueued;
        DeltaCreationEnq = deltaCreationEnq;
        DeltaEnq = deltaEnq;
        SourceID = sourceID;
    }
}