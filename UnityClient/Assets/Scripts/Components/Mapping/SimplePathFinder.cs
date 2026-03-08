using System;
using System.Collections.Generic;
using H3Engine.Common;
using H3Engine.Components.Data;
using H3Engine.Mapping;
using UnityEngine;

namespace UnityClient.Components.Mapping
{
    /// <summary>
    /// Simple A* pathfinder on the GameMap terrain grid.
    /// Supports 8-directional movement, avoids water and blocked tiles.
    /// </summary>
    public class SimplePathFinder
    {
        private GameMap gameMap;

        private static readonly Vector2Int[] Directions = new Vector2Int[]
        {
            new Vector2Int(0, -1),   // N
            new Vector2Int(1, -1),   // NE
            new Vector2Int(1, 0),    // E
            new Vector2Int(1, 1),    // SE
            new Vector2Int(0, 1),    // S
            new Vector2Int(-1, 1),   // SW
            new Vector2Int(-1, 0),   // W
            new Vector2Int(-1, -1),  // NW
        };

        public SimplePathFinder(GameMap gameMap)
        {
            this.gameMap = gameMap;
        }

        /// <summary>
        /// Find a path from (startX, startY) to (endX, endY) using A*.
        /// Returns a list of tile positions from start to end (inclusive), or null if no path.
        /// </summary>
        public List<Vector2Int> FindPath(int startX, int startY, int endX, int endY)
        {
            if (!IsInBounds(startX, startY) || !IsInBounds(endX, endY))
                return null;

            if (!IsWalkable(endX, endY))
                return null;

            Vector2Int start = new Vector2Int(startX, startY);
            Vector2Int end = new Vector2Int(endX, endY);

            // Open set as a sorted list (simple priority queue)
            var openSet = new SortedSet<PathNode>(new PathNodeComparer());
            var allNodes = new Dictionary<Vector2Int, PathNode>();

            PathNode startNode = new PathNode(start, 0f, Heuristic(start, end), null);
            openSet.Add(startNode);
            allNodes[start] = startNode;

            while (openSet.Count > 0)
            {
                // Get node with lowest fCost
                PathNode current;
                using (var enumerator = openSet.GetEnumerator())
                {
                    enumerator.MoveNext();
                    current = enumerator.Current;
                }

                if (current.Position == end)
                {
                    return ReconstructPath(current);
                }

                openSet.Remove(current);
                current.Closed = true;

                for (int i = 0; i < Directions.Length; i++)
                {
                    int nx = current.Position.x + Directions[i].x;
                    int ny = current.Position.y + Directions[i].y;

                    if (!IsInBounds(nx, ny)) continue;

                    Vector2Int neighborPos = new Vector2Int(nx, ny);

                    // Allow walking onto the destination even if it has an object
                    if (!IsWalkable(nx, ny) && neighborPos != end) continue;

                    if (allNodes.TryGetValue(neighborPos, out PathNode existing) && existing.Closed)
                        continue;

                    // Diagonal movement costs sqrt(2), cardinal costs 1
                    bool isDiagonal = Directions[i].x != 0 && Directions[i].y != 0;
                    float moveCost = isDiagonal ? 1.414f : 1f;
                    float newGCost = current.GCost + moveCost;

                    if (existing == null)
                    {
                        PathNode neighbor = new PathNode(neighborPos, newGCost, Heuristic(neighborPos, end), current);
                        openSet.Add(neighbor);
                        allNodes[neighborPos] = neighbor;
                    }
                    else if (newGCost < existing.GCost)
                    {
                        openSet.Remove(existing);
                        existing.GCost = newGCost;
                        existing.Parent = current;
                        openSet.Add(existing);
                    }
                }
            }

            return null; // No path found
        }

        /// <summary>
        /// Check if a tile is walkable (not water, not blocked).
        /// </summary>
        public bool IsWalkable(int x, int y)
        {
            if (!IsInBounds(x, y)) return false;

            TerrainTile tile = gameMap.TerrainTiles[x, y];
            if (tile.IsWater) return false;
            if (tile.IsBlocked) return false;

            return true;
        }

        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < gameMap.Width && y >= 0 && y < gameMap.Height;
        }

        private float Heuristic(Vector2Int a, Vector2Int b)
        {
            // Chebyshev distance (allows diagonal movement)
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return Mathf.Max(dx, dy) + 0.414f * Mathf.Min(dx, dy);
        }

        private List<Vector2Int> ReconstructPath(PathNode endNode)
        {
            var path = new List<Vector2Int>();
            PathNode current = endNode;
            while (current != null)
            {
                path.Add(current.Position);
                current = current.Parent;
            }
            path.Reverse();
            return path;
        }

        private class PathNode
        {
            public Vector2Int Position;
            public float GCost;      // Cost from start
            public float HCost;      // Heuristic cost to end
            public float FCost { get { return GCost + HCost; } }
            public PathNode Parent;
            public bool Closed;
            // Unique id for tie-breaking in the sorted set
            private static int nextId = 0;
            public int Id;

            public PathNode(Vector2Int pos, float gCost, float hCost, PathNode parent)
            {
                Position = pos;
                GCost = gCost;
                HCost = hCost;
                Parent = parent;
                Closed = false;
                Id = nextId++;
            }
        }

        private class PathNodeComparer : IComparer<PathNode>
        {
            public int Compare(PathNode a, PathNode b)
            {
                int result = a.FCost.CompareTo(b.FCost);
                if (result == 0) result = a.Id.CompareTo(b.Id);
                return result;
            }
        }
    }
}
