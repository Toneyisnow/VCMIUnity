using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.Engine;
using H3Engine.MapObjects;
using System;
using System.Collections.Generic;

namespace H3Engine.Engine.PathFinder
{
    /// <summary>
    /// Evaluates per-tile accessibility for a hero on the adventure map.
    ///
    /// Design (mirrors VCMI PathfinderUtil::evaluateAccessibility):
    ///   - Static info  : built once from non-moving objects (towns, mines, dwellings,
    ///                    monsters, artifacts, etc.) using their block/visit masks.
    ///   - Dynamic info : hero positions are checked live from GameMap.Heroes so that
    ///                    moving heroes are always reflected correctly.
    ///
    /// When the game state changes (hero moves, monster is killed, object is picked up)
    /// the cache holding this evaluator is invalidated and a new evaluator is built on
    /// the next pathfinding request.
    /// </summary>
    public class PathAccessibilityEvaluator
    {
        // ------------------------------------------------------------------ //
        //  Static tile info built at construction time                        //
        // ------------------------------------------------------------------ //

        private struct TileObjectInfo
        {
            /// <summary>True if a non-visit mask covers this tile (pure block).</summary>
            public bool IsBlocked;

            /// <summary>True if a visit mask covers this tile (hero steps here to interact).</summary>
            public bool IsVisitable;

            /// <summary>True if the covering object is a roaming monster (CGCreature).</summary>
            public bool IsMonster;

            /// <summary>Object type of the interactable covering this tile (if IsVisitable).</summary>
            public EObjectType ObjectType;

            /// <summary>Owner of the covering object (used for alliance checks).</summary>
            public EPlayerColor ObjectOwner;
        }

        /// <summary>Key: y * mapWidth + x</summary>
        private readonly Dictionary<int, TileObjectInfo> staticTileInfo;

        private readonly int mapWidth;
        private readonly int mapHeight;
        private readonly GameMap gameMap;

        // ------------------------------------------------------------------ //
        //  Construction                                                        //
        // ------------------------------------------------------------------ //

        public PathAccessibilityEvaluator(GameMap gameMap)
        {
            this.gameMap = gameMap;
            mapWidth  = gameMap.Width;
            mapHeight = gameMap.Height;
            staticTileInfo = new Dictionary<int, TileObjectInfo>();
            BuildStaticTileInfo();
        }

        /// <summary>
        /// Iterates every map object (except heroes, which are dynamic) and uses its
        /// BlockMask / VisitMask to populate staticTileInfo.
        ///
        /// Mask layout (same as VCMI / MapBlockManager):
        ///   index i 鈫?column = i % 8 left of anchor, row = i / 8 above anchor
        ///   so tile = (obj.PosX - i%8,  obj.PosY - i/8)
        /// </summary>
        private void BuildStaticTileInfo()
        {
            foreach (var obj in gameMap.Objects)
            {
                // Heroes are dynamic 鈥?handled separately in EvaluateAccessibility
                if (obj.ObjectType == EObjectType.HERO ||
                    obj.ObjectType == EObjectType.HERO_PLACEHOLDER ||
                    obj.ObjectType == EObjectType.RANDOM_HERO)
                    continue;

                var template = obj.Template;
                if (template == null) continue;

                int blockLen = template.BlockMask != null ? template.BlockMask.Length : 0;
                int visitLen = template.VisitMask != null ? template.VisitMask.Length : 0;
                int maskLen  = Math.Max(blockLen, visitLen);
                if (maskLen == 0) continue;

                bool isMonster = obj is CGCreature;

                for (int i = 0; i < maskLen; i++)
                {
                    bool isBlock = (i < blockLen) && template.BlockMask[i];
                    bool isVisit = (i < visitLen) && template.VisitMask[i];

                    if (!isBlock && !isVisit) continue;

                    int xPos = obj.Position.PosX - (i % 8);
                    int yPos = obj.Position.PosY - (i / 8);

                    if (xPos < 0 || xPos >= mapWidth || yPos < 0 || yPos >= mapHeight)
                        continue;

                    int key = yPos * mapWidth + xPos;

                    TileObjectInfo info;
                    staticTileInfo.TryGetValue(key, out info);

                    if (isVisit)
                    {
                        // Visit point wins over a pure block on the same tile
                        info.IsVisitable  = true;
                        info.IsBlocked    = false;
                        info.ObjectType   = obj.ObjectType;
                        info.ObjectOwner  = obj.CurrentOwner;
                        info.IsMonster    = isMonster;
                    }
                    else if (!info.IsVisitable)
                    {
                        // Only mark blocked if no visit point already claimed this tile
                        info.IsBlocked = true;
                    }

                    staticTileInfo[key] = info;
                }
            }
        }

        // ------------------------------------------------------------------ //
        //  Public API                                                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns the accessibility of tile (x, y) for the hero described by
        /// <paramref name="context"/>.
        ///
        /// Evaluation order (mirrors VCMI PathfinderUtil::evaluateAccessibility):
        ///  1. Out of bounds                  鈫?BLOCKED
        ///  2. Water / Rock terrain           鈫?BLOCKED  (land hero)
        ///  3. Terrain IsBlocked flag         鈫?BLOCKED  (if not also visitable)
        ///  4. Ally hero on tile              鈫?BLOCKED
        ///  5. Enemy hero on tile             鈫?VISITABLE (BATTLE action)
        ///  6. Static object: blocked         鈫?BLOCKED
        ///  7. Static object: visitable       鈫?VISITABLE (VISIT / BATTLE action)
        ///  8. Otherwise                      鈫?ACCESSIBLE
        /// </summary>
        public MapPathNode.ENodeAccessibility EvaluateAccessibility(int x, int y, PathfinderContext context)
        {
            // 1. Bounds
            if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                return MapPathNode.ENodeAccessibility.BLOCKED;

            var tile = gameMap.TerrainTiles[x, y];

            // 2. Terrain type: water and solid rock are impassable for a land hero
            if (tile.IsWater || tile.TerrainType == ETerrainType.ROCK)
                return MapPathNode.ENodeAccessibility.BLOCKED;

            // 3. Terrain blocked flag (set by map loader based on object masks / terrain)
            //    A tile can be IsBlocked=true AND IsVisitable=true (the visit point of a
            //    large object). Only treat it as blocked when there is no visit point.
            if (tile.IsBlocked && !tile.IsVisitable)
                return MapPathNode.ENodeAccessibility.BLOCKED;

            int key = y * mapWidth + x;

            // 4 & 5. Dynamic: other heroes on this tile
            foreach (var hero in gameMap.Heroes)
            {
                if (hero == context.Hero) continue;          // ignore self
                if (hero.Position == null) continue;
                if (hero.Position.PosX != x || hero.Position.PosY != y) continue;

                // Allied hero blocks the tile; enemy hero can be attacked
                return hero.CurrentOwner == context.PlayerColor
                    ? MapPathNode.ENodeAccessibility.BLOCKED
                    : MapPathNode.ENodeAccessibility.VISITABLE;
            }

            // 6 & 7. Static object info
            if (staticTileInfo.TryGetValue(key, out var objInfo))
            {
                if (objInfo.IsBlocked)
                    return MapPathNode.ENodeAccessibility.BLOCKED;

                if (objInfo.IsVisitable)
                    return MapPathNode.ENodeAccessibility.VISITABLE;
            }

            return MapPathNode.ENodeAccessibility.ACCESSIBLE;
        }

        /// <summary>
        /// Returns the appropriate node action for tile (x, y).
        /// Call this only after EvaluateAccessibility has confirmed the tile is
        /// ACCESSIBLE or VISITABLE.
        /// </summary>
        public MapPathNode.ENodeAction GetNodeAction(int x, int y, PathfinderContext context)
        {
            int key = y * mapWidth + x;

            // Dynamic: enemy hero 鈫?BATTLE
            foreach (var hero in gameMap.Heroes)
            {
                if (hero == context.Hero) continue;
                if (hero.Position == null) continue;
                if (hero.Position.PosX == x && hero.Position.PosY == y)
                    return MapPathNode.ENodeAction.BATTLE;
            }

            // Static object
            if (staticTileInfo.TryGetValue(key, out var objInfo) && objInfo.IsVisitable)
                return objInfo.IsMonster ? MapPathNode.ENodeAction.BATTLE
                                        : MapPathNode.ENodeAction.VISIT;

            return MapPathNode.ENodeAction.NORMAL;
        }

        /// <summary>
        /// Debug: dump accessibility and block info for all tiles in a 5x5 area around (cx, cy).
        /// </summary>
        public string DumpAccessibilityAround(int cx, int cy, PathfinderContext context)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("[PathDebug] Accessibility around ({0},{1}):", cx, cy));

            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                {
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) continue;

                    var acc = EvaluateAccessibility(x, y, context);
                    int key = y * mapWidth + x;
                    string extra = "";

                    var tile = gameMap.TerrainTiles[x, y];
                    if (tile.IsBlocked) extra += " tile.IsBlocked";
                    if (tile.IsWater) extra += " WATER";

                    if (staticTileInfo.TryGetValue(key, out var objInfo))
                    {
                        if (objInfo.IsBlocked) extra += string.Format(" obj.BLOCKED({0})", objInfo.ObjectType);
                        if (objInfo.IsVisitable) extra += string.Format(" obj.VISIT({0})", objInfo.ObjectType);
                    }

                    string marker = (dx == 0 && dy == 0) ? " [HERO]" : "";
                    sb.AppendLine(string.Format("  ({0},{1}): {2}{3}{4}", x, y, acc, extra, marker));
                }
            }
            return sb.ToString();
        }
    }
}


