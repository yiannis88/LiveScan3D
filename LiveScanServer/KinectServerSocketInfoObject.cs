/**
* This class has been created to hold all information with respect to the sockets created with the clients. 
* It creates a list of objects (lClientSockets, localPort, remotePort).
*                                          
* Ioannis Selinis 2019 (5GIC, University of Surrey)                                           
*/

using KinectServer;

public class KinectServerSocketInfoObject
{
    // Auto-implemented properties for trivial get and set
    public KinectSocket LClientSocket { get; set; }
    public int LLocalPort { get; set; }
    public int LRemotePort { get; set; }

    public KinectServerSocketInfoObject(KinectSocket lClientSocket, int lLocalPort, int lRemotePort)
    {
        LClientSocket = lClientSocket;
        LLocalPort = lLocalPort;
        LRemotePort = lRemotePort;
    }
}