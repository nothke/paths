using UnityEngine;
using System.Collections;

namespace Nothke.Paths
{
    /*
    [System.Serializable]
    public struct Node
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
    */
}