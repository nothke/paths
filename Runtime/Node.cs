using UnityEngine;
using System.Collections;

namespace Nothke.Paths
{
    public struct End : IEnd
    {
        public Path path;
        public bool isLast;

        public int Index { get { return isLast ? path.points.Length - 1 : 0; } }

        public IPath Path { get => path; set => path = (Path)value; }
        public bool IsLast { get => isLast; set => isLast = value; }

        public Vector3 GetPosition()
        {
            int i = Index;
            return path.points[Index].position;
        }

        public Node GetNode()
        {
            return new Node(path, Index);
        }

        public Vector3 GetOutDirection()
        {
            Vector3 p1, p2;

            if (isLast)
            {
                p1 = path.points[path.points.Length - 1].position;
                p2 = path.points[path.points.Length - 2].position;
            }
            else
            {
                p1 = path.points[0].position;
                p2 = path.points[1].position;
            }

            return (p2 - p1).normalized;
        }
    }

    [System.Serializable]
    public struct Node : INode
    {
        public Path path;
        public int index;

        public bool IsNull { get { return path == null; } }

        public IPath Path { get => path; set => path = (Path)value; }
        public int Index { get => index; set => index = value; }

        public Vector3 GetPosition()
        {
            return path.knots[index];
        }

        public Node(Path _path, int _index)
        {
            path = _path;
            index = _index;
        }

        public bool IsLast()
        {
            return index == path.knots.Length - 1;
        }

        public Node GetNext()
        {
            Debug.Assert(path != null, "Path is null");
            Debug.Assert(path.knots != null, "Knots are null");
            Debug.Assert(path.knots.Length > 0, "No knots");
            Debug.Assert(index >= 0, "GetNext index is less than zero");

            if (index >= path.knots.Length - 1) return new Node(null, -1);

            return new Node(path, index + 1);
        }

        public Node GetPrevious()
        {
            if (index == 0) return new Node(null, -1);

            return new Node(path, index - 1);
        }
    }
}