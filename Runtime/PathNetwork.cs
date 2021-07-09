using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Nothke.Paths
{
    public class PathNetwork : MonoBehaviour, IPathNetwork<Path>
    {
        public static PathNetwork e;

        public bool buildAtStart = true;

        void Awake()
        {
            e = this;

            closeNodesBuffer = new List<PathNode<Path>>(8);
            endsBuffer = new List<PathEnd<Path>>(8);

            if (buildAtStart)
                RebuildNetwork(true);
        }

        public Path[] allPaths;
        public float autoSearchRadius = 0.2f;

        List<PathNode<Path>> closeNodesBuffer = new List<PathNode<Path>>();
        List<PathEnd<Path>> endsBuffer = new List<PathEnd<Path>>();

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

        public PathNode<Path> GetClosestNode(Vector3 position)
        {
            RebuildNetworkIfNecessary();

            var node = new PathNode<Path>();

            float minDistance = Mathf.Infinity;

            foreach (var path in allPaths)
            {
                for (int i = 0; i < path.knots.Length; i++)
                {
                    float distance = Vector3.SqrMagnitude(path.knots[i] - position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        node.Path = path;
                        node.Index = i;
                    }
                }
            }

            return node;
        }

        public Path GetNextRandomPathForVehicle(PathNode<Path> inNode, VehicleType vehicleType)
        {
            RebuildNetworkIfNecessary();

            GetClosebyEndNodes(allPaths, closeNodesBuffer, inNode.Path, inNode.Index, autoSearchRadius, false, true);

            if (closeNodesBuffer.Count == 0)
            {
                Debug.LogWarning("Terminating path");
                return null;
            }

            for (int i = closeNodesBuffer.Count - 1; i >= 0; i--)
            {
                // Remove paths that don't match the type
                if (!TypeBelongsToMask(vehicleType, closeNodesBuffer[i].Path.vehicleMask))
                    closeNodesBuffer.RemoveAt(i);
            }

            if (closeNodesBuffer.Count == 0)
            {
                Debug.LogError($"Found paths but no matching type for {vehicleType}");
                return null;
            }

            PathNode<Path> node = closeNodesBuffer[Random.Range(0, closeNodesBuffer.Count)];
            return node.Path;
        }

        public static bool TypeBelongsToMask(VehicleType type, VehicleMask mask)
        {
            int vt = (int)type;
            int bm = (int)mask;
            bool result = ((1 << vt) & bm) != 0;

            //Debug.Log($"{System.Convert.ToString(bm, 2)} {vt} == {result}");

            return result;
        }

        public List<PathEnd<Path>> GetClosebyEnds(Path inPath, int pointIndex, float searchRadius = 0)
        {
            GetClosebyEnds(allPaths, endsBuffer, inPath, pointIndex, searchRadius != 0 ? searchRadius : autoSearchRadius);
            return endsBuffer;
        }

        public static void GetClosebyEnds(Path[] allPaths, List<PathEnd<Path>> closeEnds, Path inPath, int pointIndex, float searchRadius, bool includeSelf = false, bool onlyForwardFacing = false)
        {
            closeEnds.Clear();

            Vector3 inPos = inPath.points[pointIndex].position;

            float searchRadiusSqr = searchRadius * searchRadius;

            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;
                if (!includeSelf && path == inPath) continue; // prevents returning

                if ((path.First - inPos).sqrMagnitude < searchRadiusSqr)
                    closeEnds.Add(new PathEnd<Path>() { Path = path, IsLast = false });

                if (!onlyForwardFacing && (path.Last - inPos).sqrMagnitude < searchRadiusSqr)
                    closeEnds.Add(new PathEnd<Path>() { Path = path, IsLast = true });
            }
        }

        public static void GetClosebyEndNodes(Path[] allPaths, List<PathNode<Path>> closeNodes, Path inPath, int pointIndex, float searchRadius, bool includeSelf = false, bool onlyForwardFacing = false)
        {
            closeNodes.Clear();

            Vector3 inPos = inPath.points[pointIndex].position;

            float searchRadiusSqr = searchRadius * searchRadius;

            foreach (var path in allPaths)
            {
                if (!path.IsValid()) continue;
                if (!includeSelf && path == inPath) continue; // prevents returning

                if ((path.First - inPos).sqrMagnitude < searchRadiusSqr)
                    closeNodes.Add(new PathNode<Path>(path, path.FirstIndex));

                if (!onlyForwardFacing && (path.Last - inPos).sqrMagnitude < searchRadiusSqr)
                    closeNodes.Add(new PathNode<Path>(path, path.LastIndex - 1));
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

        [ContextMenu("Find Disconnected")]
        public void FindDisconnectedEnds()
        {
            Path[] paths = FindObjectsOfType<Path>();

            Debug.Log("Found " + paths.Length + " paths. Looking for terminations:");

            var ends = new List<PathEnd<Path>>();
            int ct = 0;

            foreach (var path in paths)
            {
                GetClosebyEnds(paths, ends, path, path.LastIndex, autoSearchRadius, false, true);
                //Debug.Log($"Path: {path.name}, ends ct: {ends.Count}");
                if (ends.Count == 0)
                {
                    Debug.LogWarning($"Path: {path.name} has no ends", path);
                    Debug.DrawRay(path.Last, Vector3.up * 100, Color.red, 10);
                    ct++;
                }

                ends.Clear();
            }

            if (ct == 0)
                Debug.Log("Found no disconnected paths");
            else
                Debug.LogWarning($"Found {ct} disconnected ends.");
        }

        public Vector3 GetClosestPoint(Vector3 position, out PathNode<Path> node, out float alongPath)
        {
            float minDistance = Mathf.Infinity;
            Vector3 minPoint = default;
            var _node = new PathNode<Path>();

            foreach (var path in allPaths)
            {
                for (int i = 0; i < path.knots.Length; i++)
                {
                    float distance = Vector3.SqrMagnitude(path.knots[i] - position);

                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        minPoint = path.knots[i];

                        _node.Path = path;
                        _node.Index = i;
                    }
                }
            }

            alongPath = 0;
            node = _node;
            return minPoint;
        }
    }
}