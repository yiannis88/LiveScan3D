//   Copyright (C) 2015  Marek Kowalski (M.Kowalski@ire.pw.edu.pl), Jacek Naruniec (J.Naruniec@ire.pw.edu.pl)
//   License: MIT Software License   See LICENSE.txt for the full license.

//   If you use this software in your research, then please use the following citation:

//    Kowalski, M.; Naruniec, J.; Daniluk, M.: "LiveScan3D: A Fast and Inexpensive 3D Data
//    Acquisition System for Multiple Kinect v2 Sensors". in 3D Vision (3DV), 2015 International Conference on, Lyon, France, 2015

//    @INPROCEEDINGS{Kowalski15,
//        author={Kowalski, M. and Naruniec, J. and Daniluk, M.},
//        booktitle={3D Vision (3DV), 2015 International Conference on},
//        title={LiveScan3D: A Fast and Inexpensive 3D Data Acquisition System for Multiple Kinect v2 Sensors},
//        year={2015},
//    }
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace KinectServer
{

    //Basic structure to hold vertices, rgb info and bodies
    public struct Frame
    {
        public List<Single> Vertices;
        public List<byte> RGB;
        public List<Body> Bodies;
        public int ClientID;
        //public List<AffineTransform> CameraPoses;

        public Frame(List<Single> vertsin, List<byte> rgbin, List<Body> bodiesin, int clientID)//, List<AffineTransform> cameraPosesin)
        {
            Vertices = vertsin;
            RGB = rgbin;
            Bodies = bodiesin;
            ClientID = clientID;
            //CameraPoses = cameraPosesin;
            // TODO fix serialization to allow back in
        }
    }
    public struct Point2f
    {
        public float X;
        public float Y;
    }

    public struct Point3f
    {
        public float X;
        public float Y;
        public float Z;
    }

    [Serializable]
    public class AffineTransform
    {
        public float[,] R = new float[3, 3];
        public float[] t = new float[3];

        public AffineTransform()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == j)
                        R[i, j] = 1;
                    else
                        R[i, j] = 0;
                }
                t[i] = 0;
            }
        }
    }

    //For performing AffineTransforms on point3fs
    public class Transformer
    {
        public static List<Point3f> Apply3DTransform(List<Point3f> points, AffineTransform transform)
        {
            List<Point3f> newPoints = new List<Point3f>();

            foreach (Point3f point in points)
            {
                newPoints.Add(Apply3DTransform(point, transform));
            }

            return newPoints;
        }

        public static Point3f Apply3DTransform(Point3f point, AffineTransform transform)
        {
            Point3f newPoint = new Point3f();

            //matrix multiplication
            newPoint.X = (transform.R[0, 0] * point.X) + (transform.R[0, 1] * point.Y) + (transform.R[0, 2] * point.Z) + transform.t[0];
            newPoint.Y = (transform.R[1, 0] * point.X) + (transform.R[1, 1] * point.Y) + (transform.R[1, 2] * point.Z) + transform.t[1];
            newPoint.Z = (transform.R[2, 0] * point.X) + (transform.R[2, 1] * point.Y) + (transform.R[2, 2] * point.Z) + transform.t[2];
            return newPoint;
        }

        // directly apply transformation to vertices
        public static List<Single> Apply3DTransform(List<Single> vertices, AffineTransform transform)
        {
            if (vertices.Count % 3 != 0)
            {
                throw new System.ArgumentOutOfRangeException("list of vertices must divide by 3");
            }

            var pointCount = vertices.Count / 3;
            var newVertices = new List<Single>();

            for (int i = 0; i < pointCount; i++)
            {
                //matrix multiplication
                newVertices.Add((transform.R[0, 0] * vertices[i * 3]) + (transform.R[0, 1] * vertices[i * 3 + 1]) + (transform.R[0, 2] * vertices[i * 3 + 2]) + transform.t[0]);
                newVertices.Add((transform.R[1, 0] * vertices[i * 3]) + (transform.R[1, 1] * vertices[i * 3 + 1]) + (transform.R[1, 2] * vertices[i * 3 + 2]) + transform.t[1]);
                newVertices.Add((transform.R[2, 0] * vertices[i * 3]) + (transform.R[2, 1] * vertices[i * 3 + 1]) + (transform.R[2, 2] * vertices[i * 3 + 2]) + transform.t[2]);
            }

            return newVertices;
        }

        //parse list of vertices received by client into list of point3f to allow transformations
        public static List<Point3f> VerticesToPoint3f(List<Single> vertices)
        {
            if (vertices.Count % 3 != 0)
            {
                throw new System.ArgumentOutOfRangeException("list of vertices must divide by 3");
            }

            List<Point3f> points = new List<Point3f>();

            for (int i = 0; i < vertices.Count / 3; i++)
            {
                var point = new Point3f();

                point.X = vertices[i * 3];
                point.Y = vertices[i * 3 + 1];
                point.Z = vertices[i * 3 + 2];

                points.Add(point);
            }

            return points;
        }

        //parse list of points back into vertices for display
        public static List<Single> Point3fToVertices(List<Point3f> points)
        {
            List<Single> verts = new List<Single>();

            foreach (Point3f point in points)
            {
                verts.Add(point.X);
                verts.Add(point.Y);
                verts.Add(point.Z);
            }

            return verts;
        }

        public static AffineTransform GetXRotationTransform(float degrees)
        {
            float rad = degrees * (float)Math.PI / 180.0f;
            var transform = new AffineTransform();
            transform.R = new float[,]{
                {1, 0, 0},
                {0, (float)Math.Cos(rad), -((float)Math.Sin(rad))},
                {0, (float)Math.Sin(rad), (float)Math.Cos(rad)}};
            return transform;
        }

        public static AffineTransform GetYRotationTransform(float degrees)
        {
            float rad = degrees * (float)Math.PI / 180.0f;
            var transform = new AffineTransform();
            transform.R = new float[,]{
                {(float)Math.Cos(rad), 0, (float)Math.Sin(rad)},
                {0, 1, 0},
                {-((float)Math.Sin(rad)), 0, (float)Math.Cos(rad)}};
            return transform;
        }

        public static AffineTransform GetZRotationTransform(float degrees)
        {
            float rad = degrees * (float)Math.PI / 180.0f;
            var transform = new AffineTransform();
            transform.R = new float[,]{
                {(float)Math.Cos(rad), -((float)Math.Sin(rad)), 0},
                {(float)Math.Sin(rad), (float)Math.Cos(rad), 0},
                {0, 0, 1}};
            return transform;
        }

        // EXPERIMENTAL find mean of source for normalizing coordinates about centroid
        // allow more intuitive placement in 3D
        public static Point3f FindMean(List<Single> verts)
        {
            if (verts.Count % 3 != 0)
            {
                throw new System.ArgumentOutOfRangeException("list of vertices must divide by 3");
            }

            Point3f mean = new Point3f
            {
                X = 0,
                Y = 0,
                Z = 0
            };

            for (int i = 0; i < verts.Count / 3; i++)
            {
                mean.X += verts[i * 3];
                mean.Y += verts[i * 3 + 1];
                mean.Z += verts[i * 3 + 2];
            }

            mean.X /= verts.Count;
            mean.Y /= verts.Count;
            mean.Z /= verts.Count;

            return mean;
        }

        // 3x3 matrix multiplication
        public static AffineTransform CompoundTransform(AffineTransform tran1, AffineTransform tran2)
        {
            AffineTransform compound = new AffineTransform
            {
                R = new float[,] { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } }
            };

            //ROWS
            for (int i = 0; i < 3; i++)
            {
                //COLUMNS
                for (int j = 0; j < 3; j++)
                {
                    //3 terms
                    for (int k = 0; k < 3; k++)
                    {
                        compound.R[i, j] += tran1.R[i, k] * tran2.R[k, j];
                    }
                }
            }

            compound.t = new float[] { tran1.t[0] + tran2.t[0], tran1.t[1] + tran2.t[1], tran1.t[2] + tran2.t[2] };

            return compound;
        }

        // EXPERIMENTAL normalise around mean to allow more intuitive placement of sources in space
        public static List<Single> NormaliseAroundMean(List<Single> verts)
        {
            if (verts.Count % 3 != 0)
            {
                throw new System.ArgumentOutOfRangeException("list of vertices must divide by 3");
            }

            Point3f mean = FindMean(verts);

            for (int i = 0; i < verts.Count / 3; i++)
            {
                verts[i * 3] -= mean.X;
                verts[i * 3 + 1] -= mean.Y;
                verts[i * 3 + 2] -= mean.Z;
            }

            return verts;
        }
    }


    [Serializable]
    public class MarkerPose
    {
        public AffineTransform pose = new AffineTransform();
        public int id = -1;

        public MarkerPose()
        {
            UpdateRotationMatrix();
        }

        public void SetOrientation(float X, float Y, float Z)
        {
            r[0] = X;
            r[1] = Y;
            r[2] = Z;

            UpdateRotationMatrix();
        }

        public void GetOrientation(out float X, out float Y, out float Z)
        {
            X = r[0];
            Y = r[1];
            Z = r[2];
        }

        private void UpdateRotationMatrix()
        {
            float radX = r[0] * (float)Math.PI / 180.0f;
            float radY = r[1] * (float)Math.PI / 180.0f;
            float radZ = r[2] * (float)Math.PI / 180.0f;

            float c1 = (float)Math.Cos(radZ);
            float c2 = (float)Math.Cos(radY);
            float c3 = (float)Math.Cos(radX);
            float s1 = (float)Math.Sin(radZ);
            float s2 = (float)Math.Sin(radY);
            float s3 = (float)Math.Sin(radX);

            //Z Y X rotation
            pose.R[0, 0] = c1 * c2;
            pose.R[0, 1] = c1 * s2 * s3 - c3 * s1;
            pose.R[0, 2] = s1 * s3 + c1 * c3 * s2;
            pose.R[1, 0] = c2 * s1;
            pose.R[1, 1] = c1 * c3 + s1 * s2 * s3;
            pose.R[1, 2] = c3 * s1 * s2 - c1 * s3;
            pose.R[2, 0] = -s2;
            pose.R[2, 1] = c2 * s3;
            pose.R[2, 2] = c2 * c3;
        }

        private float[] r = new float[3];
    }

    public enum TrackingState
    {
        TrackingState_NotTracked = 0,
        TrackingState_Inferred = 1,
        TrackingState_Tracked = 2
    }

    public enum JointType
    {
        JointType_SpineBase = 0,
        JointType_SpineMid = 1,
        JointType_Neck = 2,
        JointType_Head = 3,
        JointType_ShoulderLeft = 4,
        JointType_ElbowLeft = 5,
        JointType_WristLeft = 6,
        JointType_HandLeft = 7,
        JointType_ShoulderRight = 8,
        JointType_ElbowRight = 9,
        JointType_WristRight = 10,
        JointType_HandRight = 11,
        JointType_HipLeft = 12,
        JointType_KneeLeft = 13,
        JointType_AnkleLeft = 14,
        JointType_FootLeft = 15,
        JointType_HipRight = 16,
        JointType_KneeRight = 17,
        JointType_AnkleRight = 18,
        JointType_FootRight = 19,
        JointType_SpineShoulder = 20,
        JointType_HandTipLeft = 21,
        JointType_ThumbLeft = 22,
        JointType_HandTipRight = 23,
        JointType_ThumbRight = 24,
        JointType_Count = (JointType_ThumbRight + 1)
    }

    public struct Joint
    {
        public Point3f position;
        public JointType jointType;
        public TrackingState trackingState;
    }

    public struct Body
    {
        public bool bTracked;
        public List<Joint> lJoints;
        public List<Point2f> lJointsInColorSpace;
    }

    public class Utils
    {
        public static void saveToPly(string filename, List<Single> vertices, List<byte> colors, bool binary)
        {
            int nVertices = vertices.Count / 3;

            FileStream fileStream = File.Open(filename, FileMode.Create);

            System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(fileStream);
            System.IO.BinaryWriter binaryWriter = new System.IO.BinaryWriter(fileStream);

            //PLY file header is written here.
            if (binary)
                streamWriter.WriteLine("ply\nformat binary_little_endian 1.0");
            else
                streamWriter.WriteLine("ply\nformat ascii 1.0\n");
            streamWriter.Write("element vertex " + nVertices.ToString() + "\n");
            streamWriter.Write("property float x\nproperty float y\nproperty float z\nproperty uchar red\nproperty uchar green\nproperty uchar blue\nend_header\n");
            streamWriter.Flush();

            //Vertex and color data are written here.
            if (binary)
            {
                for (int j = 0; j < vertices.Count / 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                        binaryWriter.Write(vertices[j * 3 + k]);
                    for (int k = 0; k < 3; k++)
                    {
                        byte temp = colors[j * 3 + k];
                        binaryWriter.Write(temp);
                    }
                }
            }
            else
            {
                for (int j = 0; j < vertices.Count / 3; j++)
                {
                    string s = "";
                    for (int k = 0; k < 3; k++)
                        s += vertices[j * 3 + k].ToString(CultureInfo.InvariantCulture) + " ";
                    for (int k = 0; k < 3; k++)
                        s += colors[j * 3 + k].ToString(CultureInfo.InvariantCulture) + " ";
                    streamWriter.WriteLine(s);
                }
            }
            streamWriter.Flush();
            binaryWriter.Flush();
            fileStream.Close();
        }
    }
}