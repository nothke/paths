using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Paths
{
    public static class PathUtils
    {
        public static void DrawlineWithArrow(Vector3 p0, Vector3 p1, float scale)
        {
            Gizmos.DrawLine(p0, p1);

            Vector3 dir = (p1 - p0).normalized;
            Vector3 right = Vector3.Cross(dir, Vector3.up);

            Vector3 mid = (p0 + p1) * 0.5f;
            Gizmos.DrawLine(mid, mid - (dir + right) * scale);
            Gizmos.DrawLine(mid, mid - (dir - right) * scale);
        }

        public static void DrawPath(IPath path, float directionArrowScale = 1, float pointCrossScale = 0, Vector3 offset = default)
        {
            Vector3 up = Vector3.up;

            for (int i = 0; i < path.PointCount - 1; i++)
            {
                Vector3 p0 = path[i] + offset;
                Vector3 p1 = path[i + 1] + offset;

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

        public static void DrawPath(IPath path, float fromAlong, float toAlong, float directionArrowScale = 1, float pointCrossScale = 0, Vector3 offset = default)
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

                p0 += offset;
                p1 += offset;

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
    }
}