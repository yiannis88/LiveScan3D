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
    public class DisplayFrameTransformer{

        public DisplayFrameTransformer(){}
        public DisplayFrameTransformer(ConcurrentDictionary<int, Frame> sourceFrames){
            this.sourceFrames = sourceFrames;
        }

        // shared variable with OpenGL window containing live client frames indexed by clientNumber 
        private ConcurrentDictionary<int, Frame> sourceFrames { get; set; }
        // also indexed by clientNumber
        private Dictionary<int, SourcePosition> SourceOverrides = new Dictionary<int, SourcePosition>();

        public void setSourceFrameDict(ConcurrentDictionary<int, Frame> _sourceFrames)
        {
            sourceFrames = _sourceFrames;
        }

        // degrees about origin for arbitrary client number, defines DEFAULT BEHAVIOUR prior to override
        private float GetDefaultRotationDegrees(int sourceID)
        {
            var list = new List<int>(sourceFrames.Keys);
            list.Sort();

            return (list.IndexOf(sourceID) / sourceFrames.Count) * 360;
        }

        // generate rotation transformation for arbitrary client using default rotation DEFAULT BEHAVIOUR
        public AffineTransform GetRotationMatrix(int sourceID){
            if (sourceID < 0)
            {
                throw new IndexOutOfRangeException($"{sourceID} less than 0");
            }

            return Transformer.GetYRotationTransform(GetDefaultRotationDegrees(sourceID));
        }

        // get display ready transform for given client, generates either DEFAULT or OVERRIDE transform matrix
        public AffineTransform GetSourceTransform(int sourceID)
        {
            if (!sourceFrames.ContainsKey(sourceID)) {
                throw new IndexOutOfRangeException($"client {sourceID} not found in frame storage");
            }

            if (!SourceOverrides.ContainsKey(sourceID))
            {
                return GetRotationMatrix(sourceID);
            }
            else
            {
                var clientOverride = SourceOverrides[sourceID];
                var transform = Transformer.GetYRotationTransform(clientOverride.rotationY);
                transform.t = new float[] { clientOverride.positionX, clientOverride.positionY, clientOverride.positionZ };
                return transform;
            }
        }

        // used during OVERRIDE update
        private SourcePosition GetOverride(int sourceID)
        {
            if (!SourceOverrides.ContainsKey(sourceID))
            {
                SourceOverrides[sourceID] = new SourcePosition(sourceID, 0, GetDefaultRotationDegrees(sourceID), 0, 0, 0, 0);
            }
            return SourceOverrides[sourceID];
        }

        public void RotateSource(int sourceID, float x, float y, float z)
        {
            var tran = GetOverride(sourceID);
            tran.rotationX += x;
            tran.rotationY += y;
            tran.rotationZ += z;
        }

        public void TranslateSource(int sourceID, float x, float y, float z)
        {
            var clientOverride = GetOverride(sourceID);
            clientOverride.positionX += x;
            clientOverride.positionY += y;
            clientOverride.positionZ += z;
        }

        public void ResetSource(int sourceID)
        {
            SourceOverrides.Remove(sourceID);
        }

        public void ResetAllSources()
        {
            SourceOverrides.Clear();
        }

    }

// Position and rotation of source in 3D, used to define a source override
    public class SourcePosition
    {
        public int SourceID;
        public float rotationX;
        public float rotationY;
        public float rotationZ;

        public float positionX;
        public float positionY;
        public float positionZ;

        public SourcePosition(int sourceID)
        {
            this.SourceID = sourceID;

            rotationX = 0;
            rotationY = 0;
            rotationZ = 0;

            positionX = 0;
            positionY = 0;
            positionZ = 0;
        }

        public SourcePosition(int sourceID, float rotationX, float rotationY, float rotationZ, float positionX, float positionY, float positionZ)
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
