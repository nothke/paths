#define SPLIT_EVENLY

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if SPLIT_EVENLY
using Nothke.Utilities;
#endif

public interface IPathCustomType
{
    Color lineColor { get; }
}

namespace Nothke.Paths
{
    public class Path : MonoBehaviour, IPath
    {
        public VehicleMask vehicleMask = VehicleMask.All;

        public bool noSpawn;

        public Transform[] points;
        public Vector3 this[int i] { get => points[i].position; }

        public Vector3[] knots;

#if UNITY_EDITOR
        [Header("Presentation")]
        [SerializeField] Color color = Color.blue;
#endif

        public Vector3 First { get { return points[0].position; } }
        public Vector3 Last { get { return points[points.Length - 1].position; } }

        public int LastIndex { get { return points.Length - 1; } }
        public int FirstIndex { get { return 0; } }

        public float Length => GetLength();
        public int PointCount => points.Length;

        public void BuildKnots()
        {
            List<Vector3> knotsList = new List<Vector3>();
            for (int i = 0; i < points.Length - 1; i++)
            {
#if SPLIT_EVENLY
                var nodesOfPath = VectorUtils.SplitEvenly(points[i].position, points[i + 1].position, 1);
                knotsList.AddRange(nodesOfPath);
#else
            knotsList.Add(points[i].position);
#endif
            }

            // Remove duplicates
            for (int i = knotsList.Count - 1; i > 0; i--)
            {
                if (knotsList[i] == knotsList[i - 1])
                    knotsList.RemoveAt(i);
            }

            knots = knotsList.ToArray();
        }

        [ContextMenu("Get From Children")]
        public void GetFromChildren()
        {
            points = new Transform[transform.childCount];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.GetChild(i);
            }

        }

        [ContextMenu("Reverse")]
        public void Reverse()
        {
            int childCount = transform.childCount;

            List<Transform> children = new List<Transform>();

            foreach (Transform child in transform)
            {
                children.Add(child);
            }

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(childCount - 1 - i);
            }

            GetFromChildren();
        }

        public bool IsValid()
        {
            if (points == null) return false;
            if (points.Length < 2) return false;

            return true;
        }

        public Transform GetNext(int current)
        {
            if (current == LastIndex)
                return null;
            else
                return points[current + 1];
        }

        public Transform GetPrevious(int current)
        {
            if (current == 0)
                return null;
            else
                return points[current - 1];
        }

        public float GetLength()
        {
            float total = 0;

            if (points == null || points.Length == 0)
                return total;

            for (int i = 1; i < points.Length; i++)
            {
                total += Vector3.Distance(
                    points[i - 1].position,
                    points[i].position);
            }

            return total;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var options = PathVisibilityOptions.E;

            if (options.liveEdit)
                GetFromChildren();

            if (!IsValid()) return;

            Vector3 up = Vector3.up * options.pointCrossesSize;

            var origRandState = Random.state;

            Color pathColor;
            switch (options.colorMode)
            {
                case PathVisibilityOptions.ColorMode.PathColor: pathColor = color; break;

                case PathVisibilityOptions.ColorMode.Random:
                    int id = GetInstanceID();
                    Random.InitState(id);
                    pathColor = Random.ColorHSV(0, 1, 0.7f, 0.8f, 1, 1);
                    break;

                case PathVisibilityOptions.ColorMode.SpawnAllowed:
                    pathColor = noSpawn ? Color.red : Color.yellow;
                    break;

                case PathVisibilityOptions.ColorMode.ByType:
                    var type = GetComponent<IPathCustomType>();
                    pathColor = type != null ? type.lineColor : Color.yellow;
                    break;

                case PathVisibilityOptions.ColorMode.TypeMask:
                    int m = (int)vehicleMask;
                    Random.InitState(m);
                    pathColor = Random.ColorHSV(0, 1, 0.7f, 0.8f, 1, 1);

                    break;

                default: pathColor = color; break;
            }

            Random.state = origRandState;

            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 p0 = points[i].position;
                Vector3 p1 = points[i + 1].position;

                Gizmos.color = pathColor;
                Gizmos.DrawLine(p0, p1);

                Vector3 dir = (p1 - p0).normalized;
                Vector3 right = Vector3.Cross(dir, Vector3.up);

                if (options.drawPointCrosses)
                {
                    Gizmos.DrawLine(p0 - right * options.pointCrossesSize, p0 + right * options.pointCrossesSize);
                    Gizmos.DrawLine(p0 - up, p0 + up);
                }

                // Direction Arrows
                if (options.drawDirectionArrows)
                {
                    Vector3 mid = (p0 + p1) * 0.5f;
                    Gizmos.DrawLine(mid, mid - (dir + right) * options.directionArrowsScale);
                    Gizmos.DrawLine(mid, mid - (dir - right) * options.directionArrowsScale);

                    if (noSpawn)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(mid, Vector3.up);
                    }
                }
            }

            if (Application.isPlaying && options.showKnots)
            {
                for (int i = 0; i < knots.Length - 1; i++)
                {
                    Gizmos.DrawWireSphere(knots[i], 0.2f);
                }
            }

            Gizmos.DrawWireSphere(First, options.endNodesSize);
            Gizmos.DrawWireSphere(Last, options.endNodesSize);
        }

        public static void DrawlineWithArrow(Vector3 p0, Vector3 p1, float scale)
        {
            Gizmos.DrawLine(p0, p1);

            Vector3 dir = (p1 - p0).normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up);

            Vector3 mid = (p0 + p1) * 0.5f;
            Gizmos.DrawLine(mid, mid - (dir + right) * scale);
            Gizmos.DrawLine(mid, mid - (dir - right) * scale);
        }

        public static void DrawPath(IPath path, float directionArrowScale = 1, float pointCrossScale = 0, float heightOffset = 0)
        {
            Vector3 up = Vector3.up;

            for (int i = 0; i < path.PointCount - 1; i++)
            {
                Vector3 p0 = path[i] + up * heightOffset;
                Vector3 p1 = path[i + 1] + up * heightOffset;

                Gizmos.DrawLine(p0, p1);

                Vector3 dir = (p1 - p0).normalized;
                Vector3 right = Vector3.Cross(dir, Vector3.up);

                if (pointCrossScale > 0)
                {
                    Gizmos.DrawLine(p0 - right * pointCrossScale, p0 + right * pointCrossScale);
                    Gizmos.DrawLine(p0 - up, p0 + up);
                }

                // Direction Arrows
                if (directionArrowScale > 0)
                {
                    Vector3 mid = (p0 + p1) * 0.5f;
                    Gizmos.DrawLine(mid, mid - (dir + right) * directionArrowScale);
                    Gizmos.DrawLine(mid, mid - (dir - right) * directionArrowScale);
                }
            }
        }

        public static void DrawPath(IPath path, float fromAlong, float toAlong, float directionArrowScale = 1, float pointCrossScale = 0, float heightOffset = 0)
        {
            if (fromAlong > toAlong)
                return;

            Vector3 up = Vector3.up;

            bool started = false;
            bool ended = false;

            for (int i = 1; i < path.PointCount; i++)
            {
                float pointAlong = path.GetDistanceTo(i);

                if (pointAlong < fromAlong && !started)
                    continue;

                if (pointAlong > toAlong && !ended)
                    ended = true;

                Vector3 p0 = path[i - 1];
                Vector3 p1 = path[i];

                if (!started)
                {
                    p0 = path.PositionAlong(fromAlong);
                    started = true;
                }

                if (ended)
                {
                    p1 = path.PositionAlong(toAlong);
                }

                p0 += up * heightOffset;
                p1 += up * heightOffset;

                Gizmos.DrawLine(p0, p1);

                Vector3 dir = (p1 - p0).normalized;
                Vector3 right = Vector3.Cross(dir, Vector3.up);

                if (pointCrossScale > 0)
                {
                    Gizmos.DrawLine(p0 - right * pointCrossScale, p0 + right * pointCrossScale);
                    Gizmos.DrawLine(p0 - up, p0 + up);
                }

                // Direction Arrows
                if (directionArrowScale > 0)
                {
                    Vector3 mid = (p0 + p1) * 0.5f;
                    Gizmos.DrawLine(mid, mid - (dir + right) * directionArrowScale);
                    Gizmos.DrawLine(mid, mid - (dir - right) * directionArrowScale);
                }

                if (ended)
                    return;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!IsValid()) return;

            var options = PathVisibilityOptions.E;

            if (options.highlightSelectedPaths)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    //Vector3 up = Vector3.up * 0.1f;
                    Vector3 p0 = points[i].position;
                    Vector3 p1 = points[i + 1].position;

                    Gizmos.color = options.highlightSelectedColor;
                    Gizmos.DrawLine(p0, p1);

                    Vector3 mid = (p0 + p1) * 0.5f;
                    Vector3 dir = (p1 - p0).normalized;
                    Vector3 right = Vector3.Cross(dir, Vector3.up);
                    Gizmos.DrawLine(mid, mid - (dir + right) * options.directionArrowsScale);
                    Gizmos.DrawLine(mid, mid - (dir - right) * options.directionArrowsScale);
                }
            }
        }
#endif

        public Vector3 PositionAt(int i)
        {
            return points[i].position;
        }

        public Vector3 DirectionAt(int i)
        {
            return points[i].transform.forward;
        }

        Vector3 IPath.GetNext(int i)
        {
            return GetNext(i).position;
        }

        Vector3 IPath.GetPrevious(int i)
        {
            return GetPrevious(i).position;
        }

        public float GetLengthOfSegmentAfter(int i)
        {
            if (i > 0 && i < points.Length - 1)
                return (points[i + 1].position - points[i].position).magnitude;
            else return 0;
        }

        public float GetDistanceTo(int end)
        {
            if (end <= 0)
                return 0;
            else if (end >= PointCount - 1)
                return Length;

            float total = 0;
            Vector3 p = points[0].position;
            for (int i = 1; i < end + 1; i++)
            {
                Vector3 p1 = points[i].position;
                total += (p1 - p).magnitude;
                p = p1;
            }

            return total;
        }

        public Vector3 GetClosestPointOnPath(in Vector3 source, out int segmentIndex, out float alongPath)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 PositionAlong(float along)
        {
            if (along <= 0)
                return First;
            else if (along > Length)
                return Last;

            for (int i = 1; i < points.Length; i++)
            {
                float pointAlong = GetDistanceTo(i);

                // is between last 2 points
                if (pointAlong > along)
                {
                    Vector3 p0 = this[i - 1];
                    Vector3 p1 = this[i];

                    float diff = pointAlong - along;
                    return p1 + (p0 - p1).normalized * diff;
                }
            }

            return Vector3.zero;
        }
    }
}