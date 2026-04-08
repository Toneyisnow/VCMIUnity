using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.Engine;
using H3Engine.MapObjects;

namespace H3Engine.Engine.PathFinder
{
    /// <summary>
    /// Carries all per-hero information needed during one pathfinding run.
    /// Create a new context whenever the hero state or game state changes.
    /// </summary>
    public class PathfinderContext
    {
        /// <summary>The hero for whom paths are being calculated.</summary>
        public HeroInstance Hero { get; }

        /// <summary>Owner of the hero 鈥?used to distinguish allies from enemies.</summary>
        public EPlayerColor PlayerColor { get; }

        /// <summary>
        /// Maximum movement points available per turn for this hero.
        /// Typically 1500鈥?000 depending on hero stats and army speed.
        /// </summary>
        public int MaxMovePoints { get; }

        /// <summary>
        /// Movement points the hero has remaining at the START of this pathfinding
        /// (i.e. they may already have spent some movement this turn).
        /// </summary>
        public int CurrentMovePoints { get; }

        /// <summary>The game map with terrain tiles and current object positions.</summary>
        public GameMap GameMap { get; }

        /// <summary>
        /// Monotonically increasing version number. The cache uses this to detect
        /// stale entries: whenever any hero moves or a game action occurs that can
        /// change tile accessibility, increment this value and call
        /// PathfinderCache.InvalidateAll() (or selectively Invalidate the affected hero).
        /// </summary>
        public int GameStateVersion { get; }

        public PathfinderContext(
            HeroInstance hero,
            int maxMovePoints,
            int currentMovePoints,
            GameMap gameMap,
            int gameStateVersion)
        {
            Hero = hero;
            PlayerColor = hero.CurrentOwner;
            MaxMovePoints = maxMovePoints;
            CurrentMovePoints = currentMovePoints;
            GameMap = gameMap;
            GameStateVersion = gameStateVersion;
        }
    }
}


