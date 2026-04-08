// Migrated from VCMI lib/entities/building/CBuilding.h
// Static definition of a single town building (costs, requirements, production, bonuses).
// Runtime "which buildings are constructed" lives in TownInstance.Buildings.

using H3Engine.Common;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// Static definition of a town building type.
    /// Corresponds to VCMI's CBuilding class.
    ///
    /// Instance data (which buildings a specific town has built) lives in
    /// <see cref="H3Engine.MapObjects.TownInstance"/>.
    /// </summary>
    public class BuildingType
    {
        /// <summary>
        /// Controls how the building can be constructed.
        /// Corresponds to CBuilding::EBuildMode.
        /// </summary>
        public enum EBuildMode
        {
            /// <summary>Normal building 鈥?player must pay and click Build.</summary>
            NORMAL  = 0,
            /// <summary>Appears automatically when all requirements are present.</summary>
            AUTO    = 1,
            /// <summary>Cannot be built by normal means (e.g. pre-placed on map).</summary>
            SPECIAL = 2,
            /// <summary>Requires the Grail artifact to be placed in this town.</summary>
            GRAIL   = 3,
        }

        // --- Identity ---

        public EBuildingId Id
        {
            get; set;
        } = EBuildingId.NONE;

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

        // --- Upgrade chain ---

        /// <summary>
        /// The building that this one upgrades (-1 / NONE = not an upgrade).
        /// Corresponds to CBuilding::upgrade.
        /// </summary>
        public EBuildingId Upgrade
        {
            get; set;
        } = EBuildingId.NONE;

        /// <summary>
        /// Sub-type ID for special buildings (mage guild, marketplace, etc.).
        /// -1 means this is not a special building.
        /// Corresponds to CBuilding::subId / BuildingSubID::EBuildingSubID.
        /// </summary>
        public int SubId
        {
            get; set;
        } = -1;

        /// <summary>
        /// If true, this upgrade replaces its parent's bonuses entirely instead of stacking.
        /// </summary>
        public bool UpgradeReplacesBonuses
        {
            get; set;
        }

        /// <summary>
        /// If true, hero must manually visit the building on the town screen.
        /// </summary>
        public bool ManualHeroVisit
        {
            get; set;
        }

        // --- Economy ---

        /// <summary>
        /// Construction cost, keyed by resource type.
        /// Corresponds to CBuilding::resources (TResources).
        /// </summary>
        public Dictionary<EResourceType, int> Resources
        {
            get; set;
        }

        /// <summary>
        /// Resources produced per turn when built, keyed by resource type.
        /// Corresponds to CBuilding::produce.
        /// </summary>
        public Dictionary<EResourceType, int> Produce
        {
            get; set;
        }

        // --- Requirements ---

        /// <summary>
        /// Flat list of buildings that must already be built before this one can be constructed.
        /// Simplified from VCMI's LogicalExpression (AND-of-buildings).
        /// </summary>
        public List<EBuildingId> Requirements
        {
            get; set;
        }

        // --- Combat / Fortification ---

        /// <summary>
        /// War machine artifact provided by this building (e.g. Ballista Yard 鈫?Ballista).
        /// NONE if not applicable.
        /// </summary>
        public EArtifactId WarMachine
        {
            get; set;
        } = EArtifactId.NONE;

        /// <summary>
        /// Fortification stats added to the town when this building is present.
        /// </summary>
        public TownFortifications Fortifications
        {
            get; set;
        }

        // --- Market ---

        /// <summary>
        /// Market trading modes enabled by this building (empty = not a marketplace).
        /// Corresponds to CBuilding::marketModes.
        /// </summary>
        public List<EMarketMode> MarketModes
        {
            get; set;
        }

        // --- Build mode ---

        public EBuildMode Mode
        {
            get; set;
        } = EBuildMode.NORMAL;
    }
}


