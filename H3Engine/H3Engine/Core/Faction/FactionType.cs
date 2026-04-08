// Migrated from VCMI lib/entities/faction/CFaction.h
// Static definition of a faction (alignment, terrain, boat type, puzzle map).

using H3Engine.Common;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// One piece of the obelisk puzzle map for this faction.
    /// Corresponds to VCMI's SPuzzleInfo struct.
    /// </summary>
    public class PuzzleInfo
    {
        /// <summary>Pixel position on the world map where this puzzle piece sits.</summary>
        public int X
        {
            get; set;
        }

        public int Y
        {
            get; set;
        }

        /// <summary>Index of the puzzle piece image (0-based).</summary>
        public int Number
        {
            get; set;
        }

        /// <summary>
        /// Order in which this piece is uncovered as obelisks are visited.
        /// Lower value = uncovered earlier.
        /// </summary>
        public int WhenUncovered
        {
            get; set;
        }

        /// <summary>Path to the puzzle piece image file.</summary>
        public string Filename
        {
            get; set;
        }
    }

    /// <summary>
    /// Boat type used by this faction's shipyard and water prisons.
    /// Corresponds to VCMI's BoatId enum (CASTLE / RAMPART / TOWER).
    /// </summary>
    public enum EBoatType
    {
        CASTLE  = 0,
        RAMPART = 1,
        TOWER   = 2,
    }

    /// <summary>
    /// Static definition of a faction.
    /// Contains gameplay data (alignment, native terrain, boat) and presentation data
    /// (creature backgrounds, puzzle map).
    /// Corresponds to VCMI's CFaction class.
    ///
    /// Town-specific data (buildings, creatures per tier, graphics) lives in
    /// <see cref="TownType"/>.  A faction without a town (e.g. Neutral) has
    /// <see cref="Town"/> == null.
    /// </summary>
    public class FactionType
    {
        // --- Identity ---

        public int Id
        {
            get; set;
        }

        public string Identifier
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        // --- Gameplay properties ---

        /// <summary>
        /// Terrain type native to this faction's units (no movement penalty).
        /// Corresponds to CFaction::nativeTerrain.
        /// </summary>
        public ETerrainType NativeTerrain
        {
            get; set;
        } = ETerrainType.DIRT;

        /// <summary>
        /// Moral alignment: GOOD, EVIL, or NEUTRAL.
        /// </summary>
        public EAlignment Alignment
        {
            get; set;
        } = EAlignment.NEUTRAL;

        /// <summary>
        /// When true the map generator prefers placing this faction's towns underground.
        /// </summary>
        public bool PreferUndergroundPlacement
        {
            get; set;
        }

        /// <summary>
        /// Special factions (e.g. Neutral) are not selectable by human players.
        /// </summary>
        public bool IsSpecial
        {
            get; set;
        }

        /// <summary>
        /// Boat variant used by this faction's shipyard.
        /// Corresponds to CFaction::boatType.
        /// </summary>
        public EBoatType BoatType
        {
            get; set;
        } = EBoatType.CASTLE;

        // --- Town template (null for factions without a town) ---

        /// <summary>
        /// Town template: buildings, creatures, UI data.
        /// Null for factions that have no town (e.g. Neutral monster factions).
        /// </summary>
        public TownType Town
        {
            get; set;
        }

        // --- Graphics ---

        /// <summary>
        /// Background image (120-px version) shown behind creatures in the army panel.
        /// </summary>
        public string CreatureBg120
        {
            get; set;
        }

        /// <summary>
        /// Background image (130-px version) shown in exchange / garrison windows.
        /// </summary>
        public string CreatureBg130
        {
            get; set;
        }

        // --- Puzzle map ---

        /// <summary>
        /// All puzzle pieces for this faction's world-map obelisk puzzle.
        /// Corresponds to CFaction::puzzleMap.
        /// </summary>
        public List<PuzzleInfo> PuzzleMap
        {
            get; set;
        }
    }
}



