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

    public interface IPathNetwork<P> where P : IPath
    {
        void RebuildNetwork();

        PathNode<P> GetClosestNode(Vector3 position);
        Vector3 GetClosestPoint(Vector3 position, out PathNode<P> node, out float alongPath);

        List<PathEnd<P>> GetClosebyEnds(P inPath, int pointIndex, float searchRadius = 0);

    }

    public struct PathNode<P> where P : IPath
    {
        public P Path { get; set; }
        public int Index { get; set; }

        public bool IsNull { get { return Path == null; } }

        public Vector3 GetPosition()
        {
            return Path[Index];
        }

        public PathNode(P _path, int _index)
        {
            this.Path = _path;
            this.Index = _index;
        }

        public bool IsLast() => Index == Path.PointCount - 1;

        public PathNode<P> Next()
        {
            Debug.Assert(Path != null, "Path is null");
            Debug.Assert(Path.PointCount > 0, "No points");
            Debug.Assert(Index >= 0, "GetNext index is less than zero");

            if (Index >= Path.PointCount - 1) return new PathNode<P>(default, -1);

            return new PathNode<P>(Path, Index + 1);
        }

        public PathNode<P> Previous()
        {
            if (Index == 0) return new PathNode<P>(default, -1);

            return new PathNode<P>(Path, Index - 1);
        }
    }

    public struct PathEnd<P> where P : IPath
    {
        public P Path { get; set; }
        public bool IsLast { get; set; }

        public int Index { get { return IsLast ? Path.PointCount - 1 : 0; } }

        public Vector3 GetPosition() => Path[Index];

        public PathNode<P> GetNode() => new PathNode<P>() { Path = Path, Index = Index };

        public Vector3 GetOutDirection()
        {
            Vector3 p1, p2;

            if (IsLast)
            {
                p1 = Path[Path.PointCount - 1];
                p2 = Path[Path.PointCount - 2];
            }
            else
            {
                p1 = Path[0];
                p2 = Path[1];
            }

            return (p2 - p1).normalized;
        }
    }

    /*
    public interface IEnd
    {
        IPath Path { get; set; }
        bool IsLast { get; set; }
    }

    public interface INode
    {
        IPath Path { get; set; }
        int Index { get; set; }
    }*/
}