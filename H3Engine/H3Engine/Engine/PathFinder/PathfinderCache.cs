using H3Engine.MapObjects;
using System;
using System.Collections.Generic;

namespace H3Engine.Engine.PathFinder
{
    /// <summary>
    /// Caches computed PathsInfo results per hero to avoid re-running Dijkstra
    /// every frame. The cache uses a game-state version number to detect stale entries.
    ///
    /// Cache invalidation strategy (mirrors VCMI's PathfinderCache):
    ///   - Call <see cref="Invalidate(uint)"/> when a single hero moves or acts.
    ///   - Call <see cref="InvalidateAll"/> after any action that can change tile
    ///     accessibility for multiple heroes (e.g. a monster is killed, an object
    ///     is picked up, a town gate changes state).
    ///   - Bump <see cref="NextGameStateVersion"/> and pass the new version in the
    ///     PathfinderContext to guarantee cache misses even if Invalidate was not called.
    ///
    /// Thread safety: not thread-safe. The adventure-map pathfinder runs on the main
    /// game thread; add a lock here if that changes.
    /// </summary>
    public class PathfinderCache
    {
        private readonly Dictionary<uint, PathsInfo> cache = new Dictionary<uint, PathsInfo>();
        private int currentGameStateVersion = 0;

        // ------------------------------------------------------------------ //
        //  Version management                                                  //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Current version number. Pass this to PathfinderContext so the cache
        /// can detect stale entries by comparing stored vs requested version.
        /// </summary>
        public int CurrentGameStateVersion => currentGameStateVersion;

        /// <summary>
        /// Increments and returns the game-state version.
        /// Call this every time the map state changes in a way that may affect
        /// tile accessibility (hero moves, monster killed, object collected, etc.).
        /// </summary>
        public int NextGameStateVersion() => ++currentGameStateVersion;

        // ------------------------------------------------------------------ //
        //  Cache access                                                        //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns cached paths for the hero described by <paramref name="context"/>.
        /// If no valid entry exists the <paramref name="computeFunc"/> is invoked,
        /// its result is cached and returned.
        /// </summary>
        public PathsInfo GetOrCompute(
            PathfinderContext context,
            Func<PathfinderContext, PathsInfo> computeFunc)
        {
            uint heroId = context.Hero.Identifier;

            if (cache.TryGetValue(heroId, out PathsInfo cached) &&
                cached.GameStateVersion == context.GameStateVersion)
            {
                return cached;
            }

            PathsInfo info = computeFunc(context);
            cache[heroId] = info;
            return info;
        }

        // ------------------------------------------------------------------ //
        //  Invalidation                                                        //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Removes the cached paths for a single hero (e.g. after it moved).
        /// Other heroes' caches remain valid.
        /// </summary>
        public void Invalidate(uint heroId) => cache.Remove(heroId);

        /// <summary>
        /// Removes ALL cached paths. Use when a global map change occurs.
        /// </summary>
        public void InvalidateAll() => cache.Clear();
    }
}
