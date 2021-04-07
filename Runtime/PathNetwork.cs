using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Nothke.Paths
{
    public interface IPathNetwork
    {
        void RebuildNetwork();
        INode GetClosestNode(Vector3 position);

        List<IEnd> GetClosebyEnds(IPath inPath, int pointIndex, float searchRadius = 0);
    }

    public class PathNetwork : MonoBehaviour
    {
        public static PathNetwork e;

        public bool buildAtStart = true;

        void Awake()
        {
            e = this;

            if (buildAtStart)
                RebuildNetwork(true);
        }

        public Path[] allPaths;
        public float autoSearchRadius = 0.2f;

        List<Node> closeNodesBuffer = new List<Node>();
        List<End> endsBuffer = new List<End>();

        [ContextMenu("Rebuild Network")]
        public void RebuildNetwork()
        {
            allPaths = FindObjectsOfType<Path>();
        }

        public void RebuildNetwork(bool rebuildKnots)
        {
            allPaths = FindObjectsOfType<Path>();

            if (rebuildKnots)
            {
                for (int i = 0; i < allPaths.Length; i++)
                {
                    allPaths[i].BuildKnots();
                }
            }
        }

        [System.Obsolete("This only gets a single next path and doesn't support branching, use GetClosebyEnds() instead")]
        public Path GetNextPath(Path inPath, bool first, out bool outPathFirst)
        {
            Vector3 queryPoint = first ? inPath.First : inPath.Last;

            // find closest node of path
            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;
                if (path == inPath) continue;

                if ((path.First - queryPoint).magnitude < autoSearchRadius)
                {
                    outPathFirst = true;
                    return path;
                }

                if ((path.Last - queryPoint).magnitude < autoSearchRadius)
                {
                    outPathFirst = false;
                    return path;
                }
            }

            outPathFirst = false;
            return null;
        }

        void RebuildNetworkIfNecessary()
        {
            //if (allPaths.Length == 0)
            //RebuildNetwork();
        }

        public Node GetClosestNode(Vector3 position)
        {
            RebuildNetworkIfNecessary();

            Node node = new Node();

            float minDistance = Mathf.Infinity;

            foreach (var path in allPaths)
            {
                for (int i = 0; i < path.knots.Length; i++)
                {
                    float distance = Vector3.SqrMagnitude(path.knots[i] - position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        node.path = path;
                        node.index = i;
                    }
                }
            }

            return node;
        }

        public Node GetNextNode(Node inNode, Vector3 queryPoint, VehicleType vehicleType)
        {
            RebuildNetworkIfNecessary();

            //GetClosebyEndNodes(closeNodesBuffer, inNode);
            GetClosebyEndNodes(allPaths, closeNodesBuffer, inNode.path, inNode.index, autoSearchRadius, false);

            if (closeNodesBuffer.Count == 0)
            {
                //Debug.Log("Reached end of path");
                return new Node(null, -1);
            }

            // Remove paths that don't match the type
            for (int i = closeNodesBuffer.Count - 1; i >= 0; i--)
            {
                if (!TypeBelongsToMask(vehicleType, closeNodesBuffer[i].path.vehicleMask))
                    closeNodesBuffer.RemoveAt(i);
            }

            if (closeNodesBuffer.Count == 0)
            {
                Debug.LogError($"Found paths but no matching type for {vehicleType}");
                return new Node(null, -1);
            }

            int iterations = 10;

            for (int i = 0; i < iterations; i++)
            {
                Node node = closeNodesBuffer[Random.Range(0, closeNodesBuffer.Count)];

                if (node.index == 0)
                {
                    //Debug.Log($"Found path {node.GetPosition()}");
                    //Debug.DrawRay(node.GetPosition(), Vector3.right * 500 + Vector3.up * 500, Color.green, 10);
                    return node;
                }
            }

            //Debug.Log($"{name} did not find acceptable path in {iterations} iterations", gameObject);
            return new Node(null, -1);
        }

        public static bool TypeBelongsToMask(VehicleType type, VehicleMask mask)
        {
            int vt = (int)type;
            int bm = (int)mask;
            bool result = ((1 << vt) & bm) != 0;

            //Debug.Log($"{System.Convert.ToString(bm, 2)} {vt} == {result}");

            return result;
        }

        /*
        [System.Obsolete]
        public void GetClosebyEndNodes(List<Node> closeNodes, Node inNode, bool includeSelf = false)
        {
            //List<Node> closeNodes = new List<Node>();
            closeNodes.Clear();

            Vector3 inPos = inNode.GetPosition();
            //Debug.DrawRay(inPos, -Vector3.up * 1000, Color.cyan, 10);

            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;
                if (!includeSelf && path == inNode.path) continue; // prevents returning

                Node node = new Node();

                //Debug.DrawRay(path.First.position, Vector3.up * 1000, Color.red, 10);

                if ((path.First.position - inPos).magnitude < autoSearchRadius)
                {
                    node.path = path;
                    node.index = path.FirstIndex;
                    closeNodes.Add(node);
                }

                if ((path.Last.position - inPos).magnitude < autoSearchRadius)
                {
                    node.path = path;
                    node.index = path.LastIndex;
                    closeNodes.Add(node);
                }
            }

            //Debug.Log($"Found nodes: {closeNodes.Count}");
        }*/

        public List<End> GetClosebyEnds(Path inPath, int pointIndex, float searchRadius = 0)
        {
            GetClosebyEnds(allPaths, endsBuffer, inPath, pointIndex, searchRadius != 0 ? searchRadius : autoSearchRadius);
            return endsBuffer;
        }

        public static void GetClosebyEnds(Path[] allPaths, List<End> closeEnds, Path inPath, int pointIndex, float searchRadius, bool includeSelf = false)
        {
            closeEnds.Clear();

            Vector3 inPos = inPath.points[pointIndex].position;

            float searchRadiusSqr = searchRadius * searchRadius;

            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;
                if (!includeSelf && path == inPath) continue; // prevents returning

                if ((path.First - inPos).sqrMagnitude < searchRadiusSqr)
                    closeEnds.Add(new End() { path = path, isLast = false });

                if ((path.Last - inPos).sqrMagnitude < searchRadiusSqr)
                    closeEnds.Add(new End() { path = path, isLast = true });
            }
        }

        public static void GetClosebyEndNodes(Path[] allPaths, List<Node> closeNodes, Path inPath, int pointIndex, float searchRadius, bool includeSelf = false)
        {
            closeNodes.Clear();

            Vector3 inPos = inPath.points[pointIndex].position;

            float searchRadiusSqr = searchRadius * searchRadius;

            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;
                if (!includeSelf && path == inPath) continue; // prevents returning

                if ((path.First - inPos).sqrMagnitude < searchRadiusSqr)
                    closeNodes.Add(new Node(path, path.FirstIndex));

                if ((path.Last - inPos).sqrMagnitude < searchRadiusSqr)
                    closeNodes.Add(new Node(path, path.LastIndex - 1));
            }
        }

        [System.Obsolete]
        public Path GetNextPath(Path inPath, Vector3 queryPoint, out bool outPathFirst)
        {
            RebuildNetworkIfNecessary();

            //Vector3 queryPoint = first ? inPath.first.position : inPath.last.position;

            // find closest node of path
            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;

                if ((path.First - queryPoint).magnitude < autoSearchRadius)
                {
                    outPathFirst = true;
                    return path;
                }


                if ((path.Last - queryPoint).magnitude < autoSearchRadius)
                {
                    outPathFirst = false;
                    return path;
                }
            }

            outPathFirst = false;
            return null;
        }
    }
}