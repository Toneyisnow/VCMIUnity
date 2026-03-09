using H3Engine.Core;
using System.Collections.Generic;

namespace H3Engine.Components.MapProviders
{
    /// <summary>
    /// Stores the result of one pathfinding run for a specific hero.
    /// Contains a 2-D node array covering the entire map at a single level.
    /// Analogous to VCMI's CPathsInfo.
    /// </summary>
    public class PathsInfo
    {
        private readonly MapPathNode[,] nodes; // [x, y]

        public int Width { get; }
        public int Height { get; }
        public int MapLevel { get; }

        /// <summary>
        /// The game-state version at which this PathsInfo was computed.
        /// Used by PathfinderCache to detect stale entries.
        /// </summary>
        public int GameStateVersion { get; }

        public PathsInfo(int width, int height, int mapLevel, int gameStateVersion)
        {
            Width = width;
            Height = height;
            MapLevel = mapLevel;
            GameStateVersion = gameStateVersion;

            nodes = new MapPathNode[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    nodes[x, y] = new MapPathNode();
                    nodes[x, y].Position = new MapPosition(x, y, mapLevel);
                }
        }

        /// <summary>Returns the node at map coordinates (x, y).</summary>
        public MapPathNode GetNode(int x, int y) => nodes[x, y];

        /// <summary>
        /// Reconstructs the path from the hero's start position to (targetX, targetY).
        /// Returns null if the target is not reachable.
        /// The list is ordered from start to destination.
        /// </summary>
        public List<MapPathNode> GetPath(int targetX, int targetY)
        {
            if (targetX < 0 || targetX >= Width || targetY < 0 || targetY >= Height)
                return null;

            var dest = nodes[targetX, targetY];
            if (!dest.IsReachable)
                return null;

            var path = new List<MapPathNode>();
            var current = dest;
            while (current != null)
            {
                path.Add(current);
                current = current.PreviousNode;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Returns all nodes reachable within the current turn (Turns == 0)
        /// without any remaining movement spent on the final step.
        /// Useful for highlighting tiles the hero can move to this turn.
        /// </summary>
        public HashSet<MapPathNode> GetReachableThisTurn()
        {
            var result = new HashSet<MapPathNode>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var n = nodes[x, y];
                    if (n.IsReachable && n.Turns == 0)
                        result.Add(n);
                }
            return result;
        }

        /// <summary>
        /// Returns ALL reachable nodes across any number of turns.
        /// </summary>
        public HashSet<MapPathNode> GetAllReachableNodes()
        {
            var result = new HashSet<MapPathNode>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var n = nodes[x, y];
                    if (n.IsReachable)
                        result.Add(n);
                }
            return result;
        }
    }
}
