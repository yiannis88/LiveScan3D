
using System;
using System.Collections.Generic;

namespace KinectServer
{
    public class DisplayFrameTransformer{

        public DisplayFrameTransformer(){}
        public DisplayFrameTransformer(Dictionary<int, Frame> clientFrames){
            ClientFrames = clientFrames;
        }

        public Dictionary<int, Frame> ClientFrames;
        private Dictionary<int, ClientPosition> ClientOverrides = new Dictionary<int, ClientPosition>();

        public int ClientCount { get { return ClientFrames.Count; } }
        public List<int> ClientIDs { get { return new List<int>(ClientFrames.Keys); } }

        private float GetDefaultRotationDegrees(int clientNumber)
        {
            var list = new List<int>(ClientFrames.Keys);
            list.Sort();

            return ((float)list.IndexOf(clientNumber) / (float)ClientCount) * 360;
        }

        public AffineTransform GetRotationMatrix(int clientNumber){
            if (clientNumber >= ClientCount)
            {
                throw new IndexOutOfRangeException($"{clientNumber} greater than client count of {ClientCount}");
            }
            if (clientNumber < 0)
            {
                throw new IndexOutOfRangeException($"{clientNumber} less than 0");
            }

            return Transformer.GetYRotationTransform(GetDefaultRotationDegrees(clientNumber));
        }

        public AffineTransform GetClientTransform(int clientNumber)
        {
            if (!ClientFrames.ContainsKey(clientNumber)) {
                throw new IndexOutOfRangeException($"client {clientNumber} not found in frame storage");
            }

            if (!ClientOverrides.ContainsKey(clientNumber))
            {
                return GetRotationMatrix(clientNumber);
            }
            else
            {
                var clientOverride = ClientOverrides[clientNumber];
                var transform = Transformer.GetYRotationTransform(clientOverride.rotationY);
                transform.t = new float[] { clientOverride.positionX, clientOverride.positionY, clientOverride.positionZ };
                return transform;
            }
        }

        private ClientPosition GetOverride(int clientNumber)
        {
            if (!ClientOverrides.ContainsKey(clientNumber))
            {
                ClientOverrides[clientNumber] = new ClientPosition(clientNumber, 0, GetDefaultRotationDegrees(clientNumber), 0, 0, 0, 0);
            }
            return ClientOverrides[clientNumber];
        }

        public void RotateClient(int clientNumber, float degrees)
        {
            GetOverride(clientNumber).rotationY += degrees;
        }

        public void TranslateClient(int clientNumber, float x, float y, float z)
        {
            var clientOverride = GetOverride(clientNumber);
            clientOverride.positionX += x;
            clientOverride.positionY += y;
            clientOverride.positionZ += z;
        }

        public void ResetClient(int clientNumber)
        {
            ClientOverrides.Remove(clientNumber);
        }

        public void ResetAllClients()
        {
            ClientOverrides.Clear();
        }

    }

    public class ClientPosition
    {
        public int ClientID;
        public float rotationX;
        public float rotationY;
        public float rotationZ;

        public float positionX;
        public float positionY;
        public float positionZ;

        public ClientPosition(int ClientID)
        {
            this.ClientID = ClientID;

            rotationX = 0;
            rotationY = 0;
            rotationZ = 0;

            positionX = 0;
            positionY = 0;
            positionZ = 0;
        }

        public ClientPosition(int ClientID, float rotationX, float rotationY, float rotationZ, float positionX, float positionY, float positionZ)
        {
            this.ClientID = ClientID;
            this.rotationX = rotationX;
            this.rotationY = rotationY;
            this.rotationZ = rotationZ;

            this.positionX = positionX;
            this.positionY = positionY;
            this.positionZ = positionZ;
        }
    }
}
