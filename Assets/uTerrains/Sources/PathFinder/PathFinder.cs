using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltimateTerrains.Pathfinder
{
    /// <summary>
    /// Class providing 3D pathfinding capabilities using A*.</summary>
    /// <remarks>
    /// A* algorithm taken from http://roy-t.nl/index.php/2011/09/24/another-faster-version-of-a-2d3d-in-c/ and adapted to TerraVol.
    /// Heaviliy optimized for speed AND low memory usage.
    /// Path-smoothing algorithm added for TerraVol.
    /// On rare cases finds the 'almost optimal' path instead of the perfect path
    /// this is because we immediately return when we find the exit instead of finishing
    /// 'neighbour' loop.</remarks>
    public static class PathFinder
    {
        public const int MAX_STEPS = 10000;

        /// <summary>
        /// Method that finds the best path from start to end.
        /// </summary>
        /// <param name="terrain"> The TerraVol map.</param>
        /// <param name="start"> Position in Block coordinates where the path should start.</param>
        /// <param name="end"> Position in Block coordinates where the path should end.</param>
        /// <param name="aboveGroundOnly"> If true, the path will be searched above the ground only (not in the air).</param>
        /// <param name="smoothPath"> If true, the path will be smoothed (ie. unnecessary intermediate nodes will be removed).</param>
        /// <returns>The starting node traversable via .next to the end, or null if there is no path</returns>        
        public static SearchNode FindPath(UltimateTerrain terrain, Vector3i start, Vector3i end, bool aboveGroundOnly, bool smoothPath = true)
        {
            //note we just flip start and end here so you don't have to.            
            SearchNode path = FindPathReversed(terrain, end, start, aboveGroundOnly);
            if (smoothPath && path != null)
                SmoothPath(terrain, path, aboveGroundOnly);
            return path;
        }

        /// <summary>
        /// Method that finds the best path from start to end. Doesn't reverse outcome.
        /// </summary>
        /// <param name="map"> The TerraVol map.</param>
        /// <param name="start"> Position in Block coordinates where the path should start.</param>
        /// <param name="end"> Position in Block coordinates where the path should end.</param>
        /// <param name="aboveGroundOnly"> If true, the path will be searched above the ground only (not in the air).</param>
        /// <returns>The end node where each .next is a step back</returns>
        private static SearchNode FindPathReversed(UltimateTerrain terrain, Vector3i start, Vector3i end, bool aboveGroundOnly)
        {
            SearchNode startNode = new SearchNode(start, 0, 0, null);

            MinHeap openList = new MinHeap();
            openList.Add(startNode);

            var brWorld = new Dictionary<Vector3i, bool>(new Vector3iComparer());
            brWorld.Add(start, true);

            int count = 0;

            while (openList.HasNext() && count < MAX_STEPS) {
                count++;
                SearchNode current = openList.ExtractFirst();

                if (current.position.DistanceSquared(end) <= 3) {
                    return new SearchNode(end, current.pathCost + 1, current.cost + 1, current);
                }

                Surr[] surro;
                if (aboveGroundOnly)
                    surro = surroundingGround;
                else
                    surro = surrounding;

                for (int i = 0; i < surro.Length; i++) {
                    Surr surr = surro[i];
                    Vector3i tmp = current.position + surr.Point;

                    if (!brWorld.ContainsKey(tmp)) {
                        var voxel = terrain.GetVoxelAt(tmp);
                        if (!voxel.IsInside) {
                            brWorld.Add(tmp, true);
                            int pathCost = current.pathCost + surr.Cost;
                            int cost = pathCost + tmp.DistanceSquared(end);
                            SearchNode node = new SearchNode(tmp, cost, pathCost, current);
                            openList.Add(node);
                        }
                    }
                }
            }

            return null; //no path found
        }

        /// <summary>
        /// Smooth path.
        /// </summary>
        /// <param name="map"> The map.</param>
        /// <param name="path"> The path to smooth.</param>
        /// <param name="aboveGroundOnly"> If true, the path will be searched above the ground only (not in the air).</param>
        public static void SmoothPath(UltimateTerrain terrain, SearchNode path, bool aboveGroundOnly)
        {
            SearchNode checkPoint = path;
            SearchNode currentPoint = path.next;
            if (currentPoint == null)
                return;

            while (currentPoint.next != null) {
                if (Walkable(terrain, checkPoint.position, currentPoint.next.position, aboveGroundOnly)) {
                    // Make a straight path between those points
                    currentPoint = currentPoint.next;
                    checkPoint.next = currentPoint;
                } else {
                    checkPoint = currentPoint;
                    currentPoint = currentPoint.next;
                }
            }
        }

        private static bool Walkable(UltimateTerrain terrain, Vector3i start, Vector3i end, bool aboveGroundOnly)
        {
            Vector3 dir = ((Vector3) (end - start));
            if (aboveGroundOnly)
                dir.y = 0;
            dir = dir.normalized;
            Vector3 step = 0.1f * dir;

            Vector3i[] current = new Vector3i[4];
            for (int i = 0; i < 4; i++)
                current[i] = start;

            Vector3i[] temp = new Vector3i[4];

            Vector3[] currentTemp = new Vector3[4];
            currentTemp[0] = new Vector3(0.2f, 0.5f, 0.2f);
            currentTemp[1] = new Vector3(0.8f, 0.5f, 0.2f);
            currentTemp[2] = new Vector3(0.8f, 0.5f, 0.8f);
            currentTemp[3] = new Vector3(0.2f, 0.5f, 0.2f);


            while (current[0] != end && current[1] != end && current[2] != end && current[3] != end) {
                for (int i = 0; i < 4; i++) {
                    currentTemp[i] += step;
                    var currentPos = start + new Vector3i(currentTemp[i]);
                    temp[i] = currentPos;

                    // Check only if the cell changed
                    if (currentPos != current[i]) {
                        var voxel = terrain.GetVoxelAt(currentPos);

                        // Check if path is blocked
                        if (voxel.IsInside) {
                            // Path is blocked, let's try upside and downside cells
                            if (aboveGroundOnly) {
                                // upside
                                voxel = terrain.GetVoxelAt(currentPos + Vector3i.up);
                                if (voxel.IsInside) {
                                    // downside
                                    voxel = terrain.GetVoxelAt(currentPos + Vector3i.down);
                                    if (voxel.IsInside) {
                                        return false;
                                    }

                                    current[i] = temp[i] + Vector3i.down;
                                    currentTemp[i] += Vector3.down;
                                } else {
                                    current[i] = temp[i] + Vector3i.up;
                                    currentTemp[i] += Vector3.up;
                                }
                            }
                        } else {
                            current[i] = temp[i];
                        }
                    }
                }
            }

            return true;
        }

        class Surr
        {
            public Surr(int x, int y, int z)
            {
                Point = new Vector3i(x, y, z);
                Cost = x * x + y * y + z * z;
            }

            public Vector3i Point;
            public int Cost;
        }

        //Neighbour options
        private static Surr[] surrounding = new Surr[]
        {
            //Top slice (Y=1)
            new Surr(-1, 1, 1), new Surr(0, 1, 1), new Surr(1, 1, 1),
            new Surr(-1, 1, 0), new Surr(0, 1, 0), new Surr(1, 1, 0),
            new Surr(-1, 1, -1), new Surr(0, 1, -1), new Surr(1, 1, -1),
            //Middle slice (Y=0)
            new Surr(-1, 0, 1), new Surr(0, 0, 1), new Surr(1, 0, 1),
            new Surr(-1, 0, 0), new Surr(1, 0, 0), //(0,0,0) is self
            new Surr(-1, 0, -1), new Surr(0, 0, -1), new Surr(1, 0, -1),
            //Bottom slice (Y=-1)
            new Surr(-1, -1, 1), new Surr(0, -1, 1), new Surr(1, -1, 1),
            new Surr(-1, -1, 0), new Surr(0, -1, 0), new Surr(1, -1, 0),
            new Surr(-1, -1, -1), new Surr(0, -1, -1), new Surr(1, -1, -1)
        };

        //Neighbour options
        private static Surr[] surroundingGround = new Surr[]
        {
            //Middle slice (Y=0)
            new Surr(-1, 0, 1), new Surr(0, 0, 1), new Surr(1, 0, 1),
            new Surr(-1, 0, 0), new Surr(1, 0, 0), //(0,0,0) is self
            new Surr(-1, 0, -1), new Surr(0, 0, -1), new Surr(1, 0, -1),
            //Top slice (Y=1)
            new Surr(-1, 1, 1), new Surr(0, 1, 1), new Surr(1, 1, 1),
            new Surr(-1, 1, 0) /*, new Surr(0,1,0)*/, new Surr(1, 1, 0),
            new Surr(-1, 1, -1), new Surr(0, 1, -1), new Surr(1, 1, -1),
            //Bottom slice (Y=-1)
            new Surr(-1, -1, 1), new Surr(0, -1, 1), new Surr(1, -1, 1),
            new Surr(-1, -1, 0), new Surr(0, -1, 0), new Surr(1, -1, 0),
            new Surr(-1, -1, -1), new Surr(0, -1, -1), new Surr(1, -1, -1)
        };
    }
}