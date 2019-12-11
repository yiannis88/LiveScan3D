
using System;
using System.Collections.Generic;

namespace KinectServer
{
    public class DisplayFrameTransformer{
    
        public DisplayFrameTransformer(int clientCount){
            ClientCount = clientCount;
        }

        private int clientCount;
        public int ClientCount { get{return clientCount;} set{clientCount = value;} }

        private List<AffineTransform> transforms = new List<AffineTransform>();
        public List<AffineTransform> Transforms {
            get {
                if(transforms.Count != ClientCount)
                {
                    var newTransforms = new List<AffineTransform>();
                    for (int clientNumber = 0; clientNumber < ClientCount; clientNumber++)
                    {
                        newTransforms.Add(GenerateClientTransform(clientNumber));
                    }
                    transforms = newTransforms;
                    return transforms;
                }
                else
                {
                    return transforms;
                }   
            }
            set
            {
                transforms = value;
            }
        }

        public AffineTransform GenerateClientTransform(int clientNumber){
            if (clientNumber >= ClientCount) {
                throw new IndexOutOfRangeException($"{clientNumber} greater than client count of {ClientCount}");
            }
            if (clientNumber < 0) {
                throw new IndexOutOfRangeException($"{clientNumber} less than 0");
            }

            float rotationDegrees = ((float)clientNumber / (float)ClientCount) * 360;

            return Transformer.GetYRotationTransform(rotationDegrees);
        }

        public AffineTransform GetClientTransform(int clientNumber)
        {
            if (clientNumber >= ClientCount)
            {
                throw new IndexOutOfRangeException($"{clientNumber} greater than client count of {ClientCount}");
            }
            if (clientNumber < 0)
            {
                throw new IndexOutOfRangeException($"{clientNumber} less than 0");
            }

            var transformCopy = Transforms;
            return Transforms[clientNumber];
        }
    }

}