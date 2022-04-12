using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nothke.Paths
{
    public class GenericPathNetwork : MonoBehaviour, IPathNetwork<IPath>
    {
        public bool collectAtStart = true;

        public IPath[] allPaths;
        public float autoSearchRadius = 0.2f;

        // Temp buffers
        List<PathNode<IPath>> closeNodesBuffer = new List<PathNode<IPath>>();
        List<PathEnd<IPath>> endsBuffer = new List<PathEnd<IPath>>();

        void Awake()
        {
            closeNodesBuffer = new List<PathNode<IPath>>(8);
            endsBuffer = new List<PathEnd<IPath>>(8);

            if (collectAtStart)
            {
                List<IPath> paths = new List<IPath>();

                var _paths = FindObjectsOfType<Path>();
                var _golessPaths = FindObjectsOfType<GameObjectlessPath>();
                paths.AddRange(_paths);
                paths.AddRange(_golessPaths);

                allPaths = paths.ToArray();

                Debug.Log("GenericPathNetwork found " + allPaths.Length + " paths");
            }
        }

        public List<PathEnd<IPath>> GetClosebyEnds(IPath inPath, int pointIndex, float searchRadius = 0)
        {
            GetClosebyEnds(allPaths, endsBuffer, inPath, pointIndex, searchRadius != 0 ? searchRadius : autoSearchRadius);
            return endsBuffer;
        }

        public PathNode<IPath> GetClosestNode(Vector3 position)
        {
            var node = new PathNode<IPath>();

            float minDistance = Mathf.Infinity;

            foreach (var path in allPaths)
            {
                if (path is IPathWithKnots knotPath)
                {
                    for (int i = 0; i < knotPath.KnotCount(); i++)
                    {
                        float distance = Vector3.SqrMagnitude(knotPath.GetKnot(i) - position);

                        if (distance < minDistance)
                        {
                            minDistance = distance;

                            node.Path = path;
                            node.Index = i;
                        }
                    }
                }
            }

            return node;
        }

        public Vector3 GetClosestPoint(Vector3 position, out PathNode<IPath> node, out float alongPath)
        {
            node = new PathNode<IPath>();

            float minDistance = Mathf.Infinity;
            float minAlong = 0;
            Vector3 minPoint = default;

            foreach (var path in allPaths)
            {
                if (path is IPathWithKnots knotPath)
                {
                    Vector3 p = path.GetClosestPointOnPath(position, out int outi, out float along);

                    float distance = Vector3.SqrMagnitude(p - position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        node.Path = path;
                        node.Index = outi;
                        minAlong = along;
                        minPoint = p;
                    }
                }
            }

            alongPath = minAlong;
            return minPoint;
        }

        public static void GetClosebyEnds(IPath[] allPaths, List<PathEnd<IPath>> closeEnds, IPath inPath, int pointIndex, float searchRadius, bool includeSelf = false, bool onlyForwardFacing = false)
        {
            closeEnds.Clear();

            Vector3 inPos = inPath[pointIndex];

            float searchRadiusSqr = searchRadius * searchRadius;

            foreach (var path in allPaths)
            {
                if (path.PointCount < 2) continue;

                if (!includeSelf && path == inPath) continue; // prevents returning

                if ((path.First - inPos).sqrMagnitude < searchRadiusSqr)
                    closeEnds.Add(new PathEnd<IPath>() { Path = path, IsLast = false });

                if (!onlyForwardFacing && (path.Last - inPos).sqrMagnitude < searchRadiusSqr)
                    closeEnds.Add(new PathEnd<IPath>() { Path = path, IsLast = true });
            }
        }

        public void RebuildNetwork()
        {
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}