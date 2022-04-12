using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Paths
{
    public class GameObjectlessPath : MonoBehaviour, IPath
    {
        public VehicleMask vehicleMask = VehicleMask.All;

        public bool noSpawn;

        //public Transform[] points;
        public Vector3[] points;
        public Vector3 this[int i] => PositionAt(i);

        public Vector3[] knots;

#if UNITY_EDITOR
        [Header("Presentation")]
        [SerializeField] Color color = Color.blue;

        public string Name => name;
#endif

        public Vector3 First { get { return PositionAt(0); } }
        public Vector3 Last { get { return PositionAt(points.Length - 1); } }

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
                var nodesOfPath = VectorUtils.SplitEvenly(PositionAt(i), PositionAt(i + 1), 1);
                knotsList.AddRange(nodesOfPath);
#else
                knotsList.Add(PositionAt(i));
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
            points = new Vector3[transform.childCount];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = transform.GetChild(i).localPosition;
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

        public Vector3 GetNext(int current)
        {
            if (current >= LastIndex)
                return PositionAt(current);
            else
                return PositionAt(current + 1);
        }

        public Vector3 GetPrevious(int current)
        {
            if (current <= 0)
                return PositionAt(current);
            else
                return PositionAt(current - 1);
        }

        public float GetLength()
        {
            float total = 0;

            if (points == null || points.Length == 0)
                return total;

            for (int i = 1; i < points.Length; i++)
            {
                total += Vector3.Distance(
                    PositionAt(i - 1),
                    PositionAt(i));
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
                    {
                        var state = Random.state;

                        int id = GetInstanceID();
                        Random.InitState(id);
                        pathColor = Utils.Oklab.OklchToColor(1, 0.3f, Random.value);

                        Random.state = state;
                    }
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
                Vector3 p0 = PositionAt(i);
                Vector3 p1 = PositionAt(i + 1);

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

        private void OnDrawGizmosSelected()
        {
            if (!IsValid()) return;

            var options = PathVisibilityOptions.E;

            if (options.highlightSelectedPaths)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    //Vector3 up = Vector3.up * 0.1f;
                    Vector3 p0 = PositionAt(i);
                    Vector3 p1 = PositionAt(i + 1);

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
            return transform.TransformPoint(points[i]);
        }

        public Vector3 DirectionAt(int i)
        {
            if (PointCount <= 1)
                return Vector3.forward;

            if (i == 0)
                return (PositionAt(1) - PositionAt(0)).normalized;
            else if (i == LastIndex)
                return (PositionAt(LastIndex - 1) - Last).normalized;
            else
                return (PositionAt(i + 1) - PositionAt(i - 1)).normalized;
        }

        /*
        Vector3 IPath.GetNext(int i)
        {
            return GetNext(i);
        }

        Vector3 IPath.GetPrevious(int i)
        {
            return GetPrevious(i);
        }*/

        public float GetLengthOfSegmentAfter(int i)
        {
            if (i > 0 && i < points.Length - 1)
                return (PositionAt(i + 1) - PositionAt(i)).magnitude;
            else return 0;
        }

        public float GetDistanceTo(int end)
        {
            if (end <= 0)
                return 0;
            else if (end >= PointCount - 1)
                return Length;

            float total = 0;
            Vector3 p = PositionAt(0);
            for (int i = 1; i < end + 1; i++)
            {
                Vector3 p1 = PositionAt(i);
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