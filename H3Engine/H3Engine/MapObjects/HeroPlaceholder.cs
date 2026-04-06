// Migrated from VCMI lib/mapObjects/CGHeroInstance.h (CGHeroPlaceholder)
// Represents a placeholder slot on the map that will be filled by a real hero
// at game start (either by power rank or by explicit hero type).

namespace H3Engine.MapObjects
{
    /// <summary>
    /// A placeholder object on the adventure map that resolves to a real hero
    /// when the map is started.  Two resolution modes:
    /// <list type="bullet">
    ///   <item>By power rank – the player's hero with the matching power rank fills the slot.</item>
    ///   <item>By type – a specific hero type is placed here.</item>
    /// </list>
    /// Corresponds to VCMI's CGHeroPlaceholder class.
    /// </summary>
    public class HeroPlaceholder : CGObject
    {
        // ── Resolution mode: by power rank ────────────────────────────────────

        /// <summary>
        /// Power rank of the hero that should fill this placeholder.
        /// Null if the placeholder is resolved by hero type instead.
        /// 0 = strongest hero, 1 = second-strongest, etc.
        /// Corresponds to CGHeroPlaceholder::powerRank (std::optional&lt;ui8&gt;).
        /// </summary>
        public byte? PowerRank
        {
            get; set;
        }

        // ── Resolution mode: by hero type ─────────────────────────────────────

        /// <summary>
        /// The specific hero type ID to place here.
        /// Null if the placeholder is resolved by power rank instead.
        /// Corresponds to CGHeroPlaceholder::heroType (std::optional&lt;HeroTypeID&gt;).
        /// </summary>
        public int? HeroTypeId
        {
            get; set;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>True if this placeholder is resolved by power rank.</summary>
        public bool IsByPowerRank => PowerRank.HasValue;

        /// <summary>True if this placeholder is resolved by a specific hero type.</summary>
        public bool IsByHeroType => HeroTypeId.HasValue;
    }
}
