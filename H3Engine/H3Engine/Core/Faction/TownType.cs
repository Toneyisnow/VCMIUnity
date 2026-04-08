// Migrated from VCMI lib/entities/faction/CTown.h
// Static town template: buildings dictionary, creatures by tier, fortification base,
// graphics/audio client data.
// NOTE: CTown comments state that ClientInfo "should be moved from lib to client" 鈥?// mirrored here as TownClientInfo but marked accordingly.

using H3Engine.Common;
using H3Engine.Core;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// A visible structure on the town screen that corresponds to (or overlays) a building.
    /// Client-side only 鈥?carries no gameplay logic.
    /// Corresponds to VCMI's CStructure struct inside CTown.
    /// </summary>
    public class TownStructure
    {
        /// <summary>
        /// The building this structure represents. Null = always-visible decoration.
        /// </summary>
        public EBuildingId BuildingId
        {
            get; set;
        } = EBuildingId.NONE;

        /// <summary>
        /// Building used to determine built-state and cost tooltip.
        /// Usually the same as BuildingId.
        /// </summary>
        public EBuildingId BuildableId
        {
            get; set;
        } = EBuildingId.NONE;

        /// <summary>Pixel position on the town screen.</summary>
        public int PosX
        {
            get; set;
        }

        public int PosY
        {
            get; set;
        }

        public int PosZ
        {
            get; set;
        }

        /// <summary>Animation def file for the structure graphic.</summary>
        public string DefName
        {
            get; set;
        }

        /// <summary>Image shown as a border/overlay when the building is selected.</summary>
        public string BorderName
        {
            get; set;
        }

        /// <summary>Campaign-bonus image (shown when a bonus applies to this structure).</summary>
        public string CampaignBonus
        {
            get; set;
        }

        /// <summary>Clickable area image (used for hit-testing mouse clicks).</summary>
        public string AreaName
        {
            get; set;
        }

        public string Identifier
        {
            get; set;
        }

        /// <summary>
        /// If true and the building is an upgrade, the structure behaves like its parent
        /// (same hover text, same click behaviour).
        /// </summary>
        public bool HiddenUpgrade
        {
            get; set;
        }
    }

    /// <summary>
    /// Client-only presentation data for a town type.
    /// Mirrors CTown::ClientInfo in VCMI 鈥?should be consumed by the Unity UI layer only.
    /// </summary>
    public class TownClientInfo
    {
        // --- Icon indices ---
        // [fortPresent 0/1][buildLimitReached 0/1]
        public int[,] Icons
        {
            get; set;
        } = new int[2, 2];

        public string[,] IconSmall
        {
            get; set;
        } = new string[2, 2];

        public string[,] IconLarge
        {
            get; set;
        } = new string[2, 2];

        // --- Town screen media ---

        public string TavernVideo
        {
            get; set;
        }

        public List<string> MusicTheme
        {
            get; set;
        }

        public string TownBackground
        {
            get; set;
        }

        // --- Mage guild UI ---

        public List<string> GuildBackground
        {
            get; set;
        }

        public List<string> GuildWindow
        {
            get; set;
        }

        // --- Hall (building selection screen) ---

        public string HallBackground
        {
            get; set;
        }

        /// <summary>
        /// hallSlots[row][column] = list of building IDs in that hall cell.
        /// Corresponds to CTown::ClientInfo::hallSlots.
        /// </summary>
        public List<List<List<EBuildingId>>> HallSlots
        {
            get; set;
        }

        // --- Town screen structures ---

        /// <summary>
        /// All visible structures on the town screen.
        /// Corresponds to CTown::ClientInfo::structures.
        /// </summary>
        public List<TownStructure> Structures
        {
            get; set;
        }

        // --- Siege ---

        /// <summary>
        /// Prefix used to locate siege graphics per map layer ("surface" / "underground").
        /// </summary>
        public Dictionary<int, string> SiegePrefix
        {
            get; set;
        }

        public string TowerIconSmall
        {
            get; set;
        }

        public string TowerIconLarge
        {
            get; set;
        }

        // --- Buildings icon atlas ---

        /// <summary>Animation file that contains all building icons for this town.</summary>
        public string BuildingsIcons
        {
            get; set;
        }
    }

    /// <summary>
    /// Static town template: the set of possible buildings, creature tiers, fortifications
    /// and layout data for a specific faction's town.
    /// Corresponds to VCMI's CTown class.
    ///
    /// Runtime state ("which buildings are actually built", current garrison, etc.) lives in
    /// <see cref="H3Engine.MapObjects.TownInstance"/>.
    /// </summary>
    public class TownType
    {
        /// <summary>Back-reference to the owning faction.</summary>
        public FactionType Faction
        {
            get; set;
        }

        // --- Creatures ---

        /// <summary>
        /// Creature tiers available in this town.
        /// creatures[tier] = list of creature variants (base + upgrades) for that tier.
        /// Corresponds to CTown::creatures (vector of vectors of CreatureID).
        /// </summary>
        public List<List<ECreatureId>> Creatures
        {
            get; set;
        }

        // --- Buildings ---

        /// <summary>
        /// All building definitions for this town type, keyed by BuildingID.
        /// Corresponds to CTown::buildings.
        /// </summary>
        public Dictionary<EBuildingId, BuildingType> Buildings
        {
            get; set;
        }

        // --- Dwellings (adventure-map) ---

        /// <summary>
        /// Def names for adventure-map dwellings, index = tier (0 = tier-1 creatures).
        /// </summary>
        public List<string> Dwellings
        {
            get; set;
        }

        public List<string> DwellingNames
        {
            get; set;
        }

        // --- Horde buildings ---

        /// <summary>
        /// Creature level for each horde building slot (value -1 = slot not present).
        /// Key 0 = first horde building, key 1 = second horde building.
        /// </summary>
        public Dictionary<int, int> HordeLevels
        {
            get; set;
        }

        // --- Mage guild ---

        /// <summary>Maximum mage guild level available (1-5).</summary>
        public int MageLevel
        {
            get; set;
        }

        // --- Resources ---

        /// <summary>
        /// Primary resource produced by this town's resource silo.
        /// Corresponds to CTown::primaryRes.
        /// </summary>
        public EResourceType PrimaryResource
        {
            get; set;
        } = EResourceType.GOLD;

        // --- Fortifications (base / empty-town state) ---

        /// <summary>
        /// Base fortification state before any fort buildings are constructed.
        /// Defines default shooter units and moat spell.
        /// Corresponds to CTown::fortifications.
        /// </summary>
        public TownFortifications Fortifications
        {
            get; set;
        }

        // --- Tavern ---

        /// <summary>
        /// Default probability of heroes appearing in this town's tavern.
        /// Actual chance = sqrt(town.defaultTavernChance * heroClass.defaultTavernChance).
        /// </summary>
        public int DefaultTavernChance
        {
            get; set;
        }

        // --- Client (UI) data ---

        /// <summary>
        /// All client-side presentation data (icons, music, structures, siege graphics, etc.).
        /// Should be consumed only by the Unity UI layer, not by game logic.
        /// </summary>
        public TownClientInfo ClientInfo
        {
            get; set;
        }
    }
}



