using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Utilities
{
    public static class VectorUtils
    {
        public static Vector3[] Split(Vector3 start, Vector3 end, int number)
        {
            Vector3 diff = end - start;
            Vector3 e = diff / number;

            Vector3[] splits = new Vector3[number + 1];
            for (int i = 0; i < splits.Length; i++)
                splits[i] = start + e * i;

            return splits;
        }

        public static Vector3[] SplitEvenly(Vector3 start, Vector3 end, float segmentLength)
        {
            Vector3 diff = end - start;
            int number = Mathf.RoundToInt(diff.magnitude / segmentLength);
            if (number < 2) number = 1;
            Vector3 e = diff / number;

            Vector3[] splits = new Vector3[number + 1];
            for (int i = 0; i < splits.Length; i++)
                splits[i] = start + e * i;

            return splits;
        }

        public static bool AreClose(Vector3 v1, Vector3 v2)
        {
            return (v1 - v2).sqrMagnitude < 0.01f;
        }

        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
        {
            closestPointLine1 = Vector3.zero;
            //closestPointLine2 = Vector3.zero;

            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);

            float d = a * e - b * b;

            //lines are not parallel
            if (d != 0.0f)
            {

                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = (b * f - c * e) / d;
                //float t = (a * f - c * b) / d;

                closestPointLine1 = linePoint1 + lineVec1 * s;
                //closestPointLine2 = linePoint2 + lineVec2 * t;

                return true;
            }
            else
            {
                return false;
            }
        }

    }
}