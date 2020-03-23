/**
* This class acts as the controller of source placement within the OpenGL window.
*
* The OpenGL window uses an instance of this class during rendering to retrieve the required transformation
* to apply to each point cloud before composition for display
*
* By default, sources are arranged in a circle around the origin on a horizontal plane.
*
* Using keyboard controls the OpenGL window can pass commands to rotate and translate a particular client
* Rotation and translation functions generate an override if this is the first transformation of the source
*
* An override defines the rotation and translation of a source in 3D and represents a user override of a sources display
*
* When the OpenGL window requests a sources transformation, a present override will be prioritised over the default method 
*                                          
* Andy Pack 2019 (5GIC, University of Surrey)                                           
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace KinectServer
{
    // ClientID === clientNumber
    public class DisplayFrameTransformer{

        public DisplayFrameTransformer(){}
        public DisplayFrameTransformer(ConcurrentDictionary<int, Frame> clientFrames){
            ClientFrames = clientFrames;
        }

        // shared variable with OpenGL window containing live client frames indexed by clientNumber 
        public ConcurrentDictionary<int, Frame> ClientFrames { get; set; }
        // also indexed by clientNumber
        private Dictionary<int, ClientPosition> ClientOverrides = new Dictionary<int, ClientPosition>();

        public int ClientCount { get { return ClientFrames.Count; } }
        public List<int> ClientIDs { get { return new List<int>(ClientFrames.Keys); } }

        // degrees about origin for arbitrary client number, defines DEFAULT BEHAVIOUR prior to override
        private float GetDefaultRotationDegrees(int sourceID)
        {
            var list = new List<int>(ClientFrames.Keys);
            list.Sort();

            return ((float)list.IndexOf(sourceID) / (float)ClientCount) * 360;
        }

        // generate rotation transformation for arbitrary client using default rotation DEFAULT BEHAVIOUR
        public AffineTransform GetRotationMatrix(int sourceID){
            /*
            if (sourceID >= ClientCount)
            {
                throw new IndexOutOfRangeException($"{sourceID} greater than client count of {ClientCount}");
            }*/
            if (sourceID < 0)
            {
                throw new IndexOutOfRangeException($"{sourceID} less than 0");
            }

            return Transformer.GetYRotationTransform(GetDefaultRotationDegrees(sourceID));
        }

        // get display ready transform for given client, generates either DEFAULT or OVERRIDE transform matrix
        public AffineTransform GetClientTransform(int sourceID)
        {
            if (!ClientFrames.ContainsKey(sourceID)) {
                throw new IndexOutOfRangeException($"client {sourceID} not found in frame storage");
            }

            if (!ClientOverrides.ContainsKey(sourceID))
            {
                return GetRotationMatrix(sourceID);
            }
            else
            {
                var clientOverride = ClientOverrides[sourceID];
                var transform = Transformer.GetYRotationTransform(clientOverride.rotationY);
                transform.t = new float[] { clientOverride.positionX, clientOverride.positionY, clientOverride.positionZ };
                return transform;
            }
        }

        // used during OVERRIDE update
        private ClientPosition GetOverride(int sourceID)
        {
            if (!ClientOverrides.ContainsKey(sourceID))
            {
                ClientOverrides[sourceID] = new ClientPosition(sourceID, 0, GetDefaultRotationDegrees(sourceID), 0, 0, 0, 0);
            }
            return ClientOverrides[sourceID];
        }

        public void RotateClient(int sourceID, float x, float y, float z)
        {
            var tran = GetOverride(sourceID);
            tran.rotationX += x;
            tran.rotationY += y;
            tran.rotationZ += z;
        }

        public void TranslateClient(int sourceID, float x, float y, float z)
        {
            var clientOverride = GetOverride(sourceID);
            clientOverride.positionX += x;
            clientOverride.positionY += y;
            clientOverride.positionZ += z;
        }

        public void ResetClient(int sourceID)
        {
            ClientOverrides.Remove(sourceID);
        }

        public void ResetAllClients()
        {
            ClientOverrides.Clear();
        }

    }

// Position and rotation of source in 3D, used to define a source override
    public class ClientPosition
    {
        public int SourceID;
        public float rotationX;
        public float rotationY;
        public float rotationZ;

        public float positionX;
        public float positionY;
        public float positionZ;

        public ClientPosition(int sourceID)
        {
            this.SourceID = sourceID;

            rotationX = 0;
            rotationY = 0;
            rotationZ = 0;

            positionX = 0;
            positionY = 0;
            positionZ = 0;
        }

        public ClientPosition(int sourceID, float rotationX, float rotationY, float rotationZ, float positionX, float positionY, float positionZ)
        {
            this.SourceID = sourceID;
            this.rotationX = rotationX;
            this.rotationY = rotationY;
            this.rotationZ = rotationZ;

            this.positionX = positionX;
            this.positionY = positionY;
            this.positionZ = positionZ;
        }
    }
}
