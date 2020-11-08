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
    public class Path : MonoBehaviour
    {
        public VehicleMask vehicleMask = VehicleMask.All;

        public bool noSpawn;

        public Transform[] points;

        public Vector3[] knots;

#if UNITY_EDITOR
        [Header("Presentation")]
        [SerializeField] Color color = Color.blue;
#endif

        public Transform First { get { return points[0]; } }
        public Transform Last { get { return points[points.Length - 1]; } }

        public int LastIndex { get { return points.Length - 1; } }
        public int FirstIndex { get { return 0; } }

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

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var options = PathVisibilityOptions.E;

            if (options.liveEdit)
                GetFromChildren();

            if (!IsValid()) return;

            Vector3 up = Vector3.up * options.pointCrossesSize;

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

            Gizmos.DrawWireSphere(First.position, options.endNodesSize);
            Gizmos.DrawWireSphere(Last.position, options.endNodesSize);
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

        [ContextMenu("Debug Length")]
        public void DebugLength()
        {
            float total = 0;

            for (int i = 1; i < points.Length; i++)
            {
                total += Vector3.Distance(
                    points[i - 1].position,
                    points[i].position);
            }

            Debug.Log($"Path length: {total}");
        }
#endif
    }
}