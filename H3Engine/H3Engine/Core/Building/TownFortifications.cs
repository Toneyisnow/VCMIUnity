// Migrated from VCMI lib/entities/building/TownFortifications.h
// Describes the fortification stats contributed by a fort/citadel/castle building.
// Used both on CBuilding (per-building contribution) and CTown / TownType (aggregate state).

using H3Engine.Common;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// Fortification stats of a town's defensive structures.
    /// Each fort-tier building adds its values on top of the base town values (additive merge).
    /// Corresponds to VCMI's TownFortifications struct.
    /// </summary>
    public class TownFortifications
    {
        // --- Shooter creatures assigned to each tower ---

        public ECreatureId CitadelShooter
        {
            get; set;
        } = ECreatureId.NONE;

        public ECreatureId UpperTowerShooter
        {
            get; set;
        } = ECreatureId.NONE;

        public ECreatureId LowerTowerShooter
        {
            get; set;
        } = ECreatureId.NONE;

        // --- Moat spell (e.g. Force Field at Castle moat) ---

        public ESpellId MoatSpell
        {
            get; set;
        } = ESpellId.NONE;

        // --- Structure health points (0 = not present) ---

        public int WallsHealth
        {
            get; set;
        }

        public int CitadelHealth
        {
            get; set;
        }

        public int UpperTowerHealth
        {
            get; set;
        }

        public int LowerTowerHealth
        {
            get; set;
        }

        public bool HasMoat
        {
            get; set;
        }

        /// <summary>
        /// Merge another TownFortifications into this one, taking the greater health
        /// value for each structure and keeping any non-default shooters/spells.
        /// Mirrors TownFortifications::operator+= in VCMI.
        /// </summary>
        public void MergeWith(TownFortifications other)
        {
            if (other.CitadelShooter != ECreatureId.NONE)
                CitadelShooter = other.CitadelShooter;
            if (other.UpperTowerShooter != ECreatureId.NONE)
                UpperTowerShooter = other.UpperTowerShooter;
            if (other.LowerTowerShooter != ECreatureId.NONE)
                LowerTowerShooter = other.LowerTowerShooter;
            if (other.MoatSpell != ESpellId.NONE)
                MoatSpell = other.MoatSpell;

            if (other.WallsHealth > WallsHealth)
                WallsHealth = other.WallsHealth;
            if (other.CitadelHealth > CitadelHealth)
                CitadelHealth = other.CitadelHealth;
            if (other.UpperTowerHealth > UpperTowerHealth)
                UpperTowerHealth = other.UpperTowerHealth;
            if (other.LowerTowerHealth > LowerTowerHealth)
                LowerTowerHealth = other.LowerTowerHealth;

            HasMoat = HasMoat || other.HasMoat;
        }
    }
}



