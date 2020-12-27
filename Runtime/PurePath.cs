using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Paths
{
    public struct PureNode : INode
    {
        public IPath Path { get; set; }
        public int Index { get; set; }
    }

    public struct PureEnd : IEnd
    {
        public IPath Path { get; set; }
        public bool IsLast { get; set; }

        public Vector3 Position { get { return IsLast ? Path.Last : Path.First; } }
    }

    [System.Serializable]
    public class PurePath : IPath
    {
        public float length;
        public float Length => length;

        public Vector3 First => points[0];
        public Vector3 Last => points[points.Length - 1];

        public Vector3[] points;
        public int PointCount => points.Length;

        public Vector3 this[int i] { get => points[i]; }

        public float GetLengthOfSegmentAfter(int i)
        {
            if (i > 0 && i < points.Length - 1)
                return (points[i + 1] - points[i]).magnitude;
            else return 0;
        }

        public float GetDistanceTo(int end)
        {
            float total = 0;
            //for (int i = 1; i <= end; i++)
            //total += (points[i] - points[i - 1]).sqrMagnitude;

            for (int i = 0; i < end; i++)
            {
                total += Vector3.Distance(points[i + 1], points[i]);
            }

            return total;
            //return Mathf.Sqrt(total);
        }

        public Vector3 PositionAt(int i)
        {
            return points[i];
        }

        public Vector3 DirectionAt(int i)
        {
            if (i == 0)
                return (points[i + 1] - points[0]).normalized;
            else if (i == points.Length - 1)
                return (points[i] - points[i - 1]).normalized;
            else
                return (points[i + 1] - points[i - 1]).normalized;

        }

        public void DebugLines()
        {
            Vector3 lastp = points[0];
            Color color = Random.ColorHSV(0, 1, 1, 1, 1, 1);
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 thisp = points[i];
                Debug.DrawLine(lastp, thisp, color);
                lastp = thisp;
            }
        }

        public static PurePath FromTransforms(Transform[] transforms)
        {
            Vector3[] points = new Vector3[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                points[i] = transforms[i].position;
            }

            PurePath path = new PurePath()
            {
                points = points
            };

            path.RecalculateLength();

            return path;
        }


        public void RecalculateLength()
        {
            float total = 0;

            for (int i = 1; i < points.Length; i++)
            {
                total += Vector3.Distance(points[i - 1], points[i]);
            }

            length = total;
        }

        public Vector3 GetNext(int i)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetPrevious(int i)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetClosestPointOnPath(in Vector3 source, out int segmentIndex, out float alongPath)
        {
            float closestD = Mathf.Infinity;
            Vector3 closestP = default;
            segmentIndex = 0;
            float alongSegment = 0;

            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 p = ClosestPointOnLine(points[i], points[i + 1], source, out float _alongSegment);
                float d = (p - source).magnitude;

                if (d < closestD)
                {
                    closestP = p;
                    closestD = d;
                    segmentIndex = i;
                    alongSegment = _alongSegment;
                }
            }

            alongPath = GetDistanceTo(segmentIndex) + alongSegment;
            return closestP;
        }

        public static Vector3 ClosestPointOnLine(in Vector3 p1, in Vector3 p2, in Vector3 v, out float d)
        {
            Vector3 diff = p2 - p1;
            float length = diff.magnitude;
            d = Vector3.Dot(v - p1, diff) / length;
            d = Mathf.Clamp(d, 0, length);
            return p1 + diff / length * d;
        }
    }
}