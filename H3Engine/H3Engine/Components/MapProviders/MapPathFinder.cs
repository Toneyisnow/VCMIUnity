using H3Engine.Common;
using H3Engine.Components.Data;
using H3Engine.Core;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections.Generic;

namespace H3Engine.Components.MapProviders
{
    /// <summary>
    /// Adventure-map pathfinder using Dijkstra's algorithm with a binary min-heap.
    ///
    /// Design mirrors VCMI's CPathfinder:
    ///   - A PathsInfo (node storage) is allocated per hero per game-state version.
    ///   - Accessibility is evaluated dynamically via PathAccessibilityEvaluator so
    ///     per-player and per-turn differences are handled correctly.
    ///   - Movement cost accounts for terrain type and road type (land layer only).
    ///   - Multi-turn paths are supported: when move points run out the hero "rests"
    ///     and the counter increments by one turn (cost += 1.0).
    ///   - Results are cached in PathfinderCache; call cache.Invalidate(heroId) or
    ///     cache.InvalidateAll() whenever the map state changes.
    ///
    /// Usage:
    ///   var ctx = new PathfinderContext(hero, maxMP, currentMP, gameMap, cache.CurrentGameStateVersion);
    ///   PathsInfo info = pathFinder.GetPathsInfo(ctx);
    ///   List&lt;MapPathNode&gt; path = info.GetPath(targetX, targetY);
    ///   HashSet&lt;MapPathNode&gt; reachable = info.GetReachableThisTurn();
    /// </summary>
    public class MapPathFinder
    {
        // ------------------------------------------------------------------ //
        //  8-directional neighbour offsets                                     //
        // ------------------------------------------------------------------ //

        private static readonly int[] DX = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] DY = { -1, -1, -1, 0, 0, 1, 1, 1 };
        private static readonly bool[] IsDiag = { true, false, true, false, false, true, false, true };

        // ------------------------------------------------------------------ //
        //  Terrain movement costs (base, per tile, in movement points)        //
        //  Source: Heroes III game data                                        //
        // ------------------------------------------------------------------ //

        /// <summary>Movement cost for each terrain type without any road.</summary>
        private static int TerrainMoveCost(ETerrainType t)
        {
            switch (t)
            {
                case ETerrainType.DIRT:
                case ETerrainType.GRASS:
                case ETerrainType.SNOW:
                case ETerrainType.SUBTERRANEAN: return 100;

                case ETerrainType.SAND:
                case ETerrainType.ROUGH:
                case ETerrainType.LAVA:         return 125;

                case ETerrainType.SWAMP:        return 175;

                default:                        return 100; // fallback
            }
        }

        /// <summary>Movement cost when a road covers the destination tile.</summary>
        private static int RoadMoveCost(ERoadType r)
        {
            switch (r)
            {
                case ERoadType.COBBLESTONE_ROAD: return 50;
                case ERoadType.GRAVEL_ROAD:      return 65;
                case ERoadType.DIRT_ROAD:        return 75;
                default:                         return 100;
            }
        }

        /// <summary>
        /// Returns the movement-point cost for a hero stepping onto <paramref name="destTile"/>.
        /// Roads on the destination tile override the terrain cost.
        /// Diagonal movement costs √2 ≈ 1.414× more (rounded to nearest integer).
        /// </summary>
        private static int CalculateMoveCost(TerrainTile destTile, bool isDiagonal)
        {
            int cost = destTile.RoadType != ERoadType.NO_ROAD
                ? RoadMoveCost(destTile.RoadType)
                : TerrainMoveCost(destTile.TerrainType);

            // Diagonal: multiply by 141/100  (≈ √2)
            return isDiagonal ? cost * 141 / 100 : cost;
        }

        // ------------------------------------------------------------------ //
        //  Priority queue (binary min-heap, lazy-deletion style)              //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// A min-heap entry that snapshots the cost at the time the node was enqueued.
        /// When we pop an entry we check node.Locked to discard outdated entries.
        /// </summary>
        private struct HeapEntry
        {
            public float EnqueuedCost;
            public MapPathNode Node;

            public HeapEntry(float cost, MapPathNode node)
            {
                EnqueuedCost = cost;
                Node = node;
            }
        }

        private sealed class MinHeap
        {
            private readonly List<HeapEntry> data = new List<HeapEntry>();

            public int Count => data.Count;

            public void Push(float cost, MapPathNode node)
            {
                data.Add(new HeapEntry(cost, node));
                SiftUp(data.Count - 1);
            }

            public HeapEntry Pop()
            {
                HeapEntry top = data[0];
                int last = data.Count - 1;
                data[0] = data[last];
                data.RemoveAt(last);
                if (data.Count > 0)
                    SiftDown(0);
                return top;
            }

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (data[p].EnqueuedCost <= data[i].EnqueuedCost) break;
                    HeapEntry tmp = data[i]; data[i] = data[p]; data[p] = tmp;
                    i = p;
                }
            }

            private void SiftDown(int i)
            {
                int n = data.Count;
                while (true)
                {
                    int s = i, l = 2 * i + 1, r = 2 * i + 2;
                    if (l < n && data[l].EnqueuedCost < data[s].EnqueuedCost) s = l;
                    if (r < n && data[r].EnqueuedCost < data[s].EnqueuedCost) s = r;
                    if (s == i) break;
                    HeapEntry tmp = data[i]; data[i] = data[s]; data[s] = tmp;
                    i = s;
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        private readonly PathfinderCache cache;

        public MapPathFinder(PathfinderCache cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Returns the PathsInfo for the hero described by <paramref name="context"/>,
        /// computing it via Dijkstra if not already cached.
        /// </summary>
        public PathsInfo GetPathsInfo(PathfinderContext context)
        {
            return cache.GetOrCompute(context, CalculatePaths);
        }

        /// <summary>
        /// Shortcut: returns the ordered node list from the hero's position to
        /// (targetX, targetY), or null when unreachable.
        /// </summary>
        public List<MapPathNode> GetPath(PathfinderContext context, int targetX, int targetY)
        {
            return GetPathsInfo(context).GetPath(targetX, targetY);
        }

        /// <summary>
        /// Debug: dump accessibility around the hero's position.
        /// </summary>
        public string DebugDumpAccessibility(PathfinderContext context)
        {
            var evaluator = new PathAccessibilityEvaluator(context.GameMap);
            int hx = context.Hero.Position.PosX;
            int hy = context.Hero.Position.PosY;
            return evaluator.DumpAccessibilityAround(hx, hy, context);
        }

        /// <summary>
        /// Shortcut: returns all tiles reachable this turn for the given hero.
        /// </summary>
        public HashSet<MapPathNode> GetReachableThisTurn(PathfinderContext context)
        {
            return GetPathsInfo(context).GetReachableThisTurn();
        }

        // ------------------------------------------------------------------ //
        //  Core: Dijkstra                                                      //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Runs Dijkstra from the hero's current position over the entire map.
        /// Returns a fully populated PathsInfo.
        ///
        /// Algorithm (mirrors VCMI CPathfinder::calculatePaths):
        ///   1. Initialize all nodes; set source node cost = 0.
        ///   2. Priority queue keyed by (turns + remaining fraction).
        ///   3. For each popped node, evaluate each of 8 neighbours:
        ///        a. Evaluate accessibility (terrain + objects + dynamic heroes).
        ///        b. Compute movement cost, updating turn counter if points exhausted.
        ///        c. If the new cost improves the neighbour, update and re-enqueue.
        ///   4. VISITABLE nodes (interactive objects, enemies) are updated but NOT
        ///      expanded further — movement stops there.
        /// </summary>
        private PathsInfo CalculatePaths(PathfinderContext context)
        {
            int w = context.GameMap.Width;
            int h = context.GameMap.Height;
            int mapLevel = context.GameMap.MapLevel;

            var info = new PathsInfo(w, h, mapLevel, context.GameStateVersion);
            var evaluator = new PathAccessibilityEvaluator(context.GameMap);

            // Source: hero's current tile
            int sx = context.Hero.Position.PosX;
            int sy = context.Hero.Position.PosY;

            MapPathNode source = info.GetNode(sx, sy);
            source.Cost       = 0f;
            source.MoveRemains = context.CurrentMovePoints;
            source.Turns      = 0;
            source.Accessibility = MapPathNode.ENodeAccessibility.ACCESSIBLE;
            source.NodeAction    = MapPathNode.ENodeAction.NORMAL;

            var pq = new MinHeap();
            pq.Push(0f, source);

            while (pq.Count > 0)
            {
                HeapEntry entry = pq.Pop();
                MapPathNode current = entry.Node;

                // Lazy deletion: skip if already finalized or the entry is outdated
                if (current.Locked) continue;
                // Cost mismatch means a better path was found and this entry is stale
                if (Math.Abs(entry.EnqueuedCost - current.Cost) > 1e-6f) continue;

                current.Locked = true;

                // VISITABLE nodes are path endpoints — do not expand from them
                if (current.Accessibility == MapPathNode.ENodeAccessibility.VISITABLE ||
                    current.Accessibility == MapPathNode.ENodeAccessibility.BLOCKVISIT)
                    continue;

                // Explore 8 neighbours
                for (int d = 0; d < 8; d++)
                {
                    int nx = current.Position.PosX + DX[d];
                    int ny = current.Position.PosY + DY[d];

                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

                    MapPathNode neighbour = info.GetNode(nx, ny);
                    if (neighbour.Locked) continue;

                    // Evaluate accessibility from the hero's perspective
                    var accessibility = evaluator.EvaluateAccessibility(nx, ny, context);
                    if (accessibility == MapPathNode.ENodeAccessibility.BLOCKED) continue;

                    // Movement cost for stepping onto the neighbour tile
                    var destTile = context.GameMap.TerrainTiles[nx, ny];
                    int moveCost = CalculateMoveCost(destTile, IsDiag[d]);

                    // Advance movement-point budget, rolling over to the next turn
                    // if required (unlimited turns — the game can cap this externally).
                    int newMoveRemains = current.MoveRemains - moveCost;
                    int newTurns       = current.Turns;

                    if (newMoveRemains < 0)
                    {
                        // Hero needs to rest and continue next turn
                        newTurns++;
                        newMoveRemains = context.MaxMovePoints - moveCost;

                        if (newMoveRemains < 0)
                            continue; // single tile costs more than a full turn — unreachable
                    }

                    // Path cost = turns + (points spent this turn) / maxPoints
                    // Lower is better: same turn with more movement left is cheaper.
                    float newCost = newTurns +
                        (float)(context.MaxMovePoints - newMoveRemains) / context.MaxMovePoints;

                    if (newCost >= neighbour.Cost) continue; // no improvement

                    // Update neighbour
                    neighbour.Cost         = newCost;
                    neighbour.MoveRemains  = newMoveRemains;
                    neighbour.Turns        = newTurns;
                    neighbour.Accessibility = accessibility;
                    neighbour.PreviousNode  = current;
                    neighbour.NodeAction    = evaluator.GetNodeAction(nx, ny, context);

                    pq.Push(newCost, neighbour);
                }
            }

            return info;
        }
    }
}
