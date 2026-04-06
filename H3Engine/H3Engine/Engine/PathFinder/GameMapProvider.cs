using H3Engine.Engine;
using H3Engine.Core;
using H3Engine.MapObjects;
using System;
using System.Collections.Generic;

namespace H3Engine.Engine.PathFinder
{
    public class GameMapProvider
    {
        // Default movement points per turn when no hero-specific data is available.
        private const int DefaultMaxMovePoints = 1500;

        public GameMap Map
        {
            get; private set;
        }

        public MapPathFinder PathFinder
        {
            get; private set;
        }

        public PathfinderCache PathFinderCache
        {
            get; private set;
        }

        public GameMapProvider(GameMap gameMap)
        {
            this.Map = gameMap;
            this.PathFinderCache = new PathfinderCache();
            this.PathFinder = new MapPathFinder(this.PathFinderCache);
        }

        /// <summary>
        /// Calculates the path for a hero to a target position.
        /// Only returns nodes reachable within the current turn (Turns == 0).
        /// Returns an empty list when the hero is not found or the target is unreachable.
        /// </summary>
        public List<MapPathNode> GetHeroMovePath(int heroId, MapPosition targetPosition)
        {
            // Find the hero by identifier
            HeroInstance hero = FindHeroById(heroId);
            if (hero == null || hero.Position == null)
                return new List<MapPathNode>();

            var ctx = new PathfinderContext(
                hero,
                DefaultMaxMovePoints,
                DefaultMaxMovePoints,
                this.Map,
                PathFinderCache.CurrentGameStateVersion);

            List<MapPathNode> fullPath = PathFinder.GetPath(ctx, targetPosition.PosX, targetPosition.PosY);
            if (fullPath == null)
                return new List<MapPathNode>();

            // Trim to current-turn nodes only
            var result = new List<MapPathNode>();
            foreach (var node in fullPath)
            {
                result.Add(node);
                if (node.Turns > 0)
                    break; // stop after the first node that crosses into the next turn
            }

            return result;
        }

        /// <summary>
        /// Invalidates cached paths for a hero after it has moved or acted.
        /// Call this whenever a hero's position or the map state changes.
        /// </summary>
        public void InvalidateHeroPath(uint heroId)
        {
            PathFinderCache.Invalidate(heroId);
            PathFinderCache.NextGameStateVersion();
        }

        /// <summary>
        /// Invalidates all cached paths (e.g. after a monster is killed or an object is picked up).
        /// </summary>
        public void InvalidateAllPaths()
        {
            PathFinderCache.InvalidateAll();
            PathFinderCache.NextGameStateVersion();
        }

        private HeroInstance FindHeroById(int heroId)
        {
            if (Map.Heroes == null) return null;
            foreach (var hero in Map.Heroes)
            {
                if ((int)hero.Identifier == heroId)
                    return hero;
            }
            return null;
        }
    }
}
