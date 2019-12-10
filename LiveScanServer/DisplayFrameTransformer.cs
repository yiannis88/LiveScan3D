
using System;

namespace KinectServer
{
    public class DisplayFrameTransformer(){
    
        public DisplayFrameTransformer(int clientCount){
            ClientCount = clientCount;
        }

        private int clientCount;
        public int ClientCount { get{return clientCount;} set{clientCount = value;} }

        public List<AffineTransform> Transforms {
            get {
                var transforms = new List<AffineTransform>();
                foreach(int clientNumber in ClientCount){
                    transforms.Add(GetClientTransform(clientNumber));
                }
                return transforms;
            }
        }

        public AffineTransform GetClientTransform(int clientNumber){
            if (clientNumber >= ClientCount) {
                thrown new IndexOutOfRangeException($"{clientNumber} greater than client count of {ClientCount}");
            }
            if (clientNumber < 0) {
                thrown new IndexOutOfRangeException($"{clientNumber} less than 0");
            }

            float rotationDegrees = (clientNumber / ClientCount) * 360;

            return Transformer.GetYRotationTransform(rotationDegrees);
        }
    }

}