using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPath
{
    float Length { get; }
    Vector3 First { get; }
    Vector3 Last { get; }
    int PointCount { get; }

    Vector3 PositionAt(int i);
    Vector3 DirectionAt(int i);
    Vector3 GetNext(int i);
    Vector3 GetPrevious(int i);
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
        for (int i = 1; i < points.Length; i++)
        {
            Vector3 thisp = points[i];
            Debug.DrawLine(lastp, thisp);
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
            total += (points[i] - points[i - 1]).sqrMagnitude;
        }

        length = Mathf.Sqrt(total);
    }

    public Vector3 GetNext(int i)
    {
        throw new System.NotImplementedException();
    }

    public Vector3 GetPrevious(int i)
    {
        throw new System.NotImplementedException();
    }
}
