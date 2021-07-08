using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Paths
{
    public interface IPath
    {
        float Length { get; }
        Vector3 First { get; }
        Vector3 Last { get; }
        int PointCount { get; }

        Vector3 PositionAt(int i);
        Vector3 PositionAlong(float along);
        Vector3 DirectionAt(int i);
        Vector3 GetNext(int i);
        Vector3 GetPrevious(int i);

        float GetLengthOfSegmentAfter(int i);
        float GetDistanceTo(int i);

        Vector3 this[int i] { get; }

        Vector3 GetClosestPointOnPath(in Vector3 source, out int segmentIndex, out float alongPath);
    }

    public interface IPathNetwork
    {
        void RebuildNetwork();

        INode GetClosestNode(Vector3 position);
        Vector3 GetClosestPoint(Vector3 position, out INode node, out float alongPath);

        List<IEnd> GetClosebyEnds(IPath inPath, int pointIndex, float searchRadius = 0);

    }

    public interface IEnd
    {
        IPath Path { get; set; }
        bool IsLast { get; set; }
    }

    public interface INode
    {
        IPath Path { get; set; }
        int Index { get; set; }
    }
}