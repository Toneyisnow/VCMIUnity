// Migrated from VCMI lib/entities/artifact/CArtifactInstance.h
// Represents one runtime copy of an artifact (as opposed to its static type definition).

using H3Engine.Common;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// One part of a combined artifact instance, recording which constituent artifact
    /// occupies which slot on the bearer.
    /// Corresponds to CCombinedArtifactInstance::PartInfo in VCMI.
    /// </summary>
    public class CombinedArtPartInfo
    {
        /// <summary>Instance ID of the constituent artifact.</summary>
        public int ArtifactInstanceId
        {
            get; set;
        } = -1;

        /// <summary>Type ID of the constituent artifact.</summary>
        public EArtifactId ArtifactId
        {
            get; set;
        } = EArtifactId.NONE;

        /// <summary>Equipment slot this constituent occupies on the bearer.</summary>
        public EArtifactPosition Slot
        {
            get; set;
        } = EArtifactPosition.PRE_FIRST;
    }

    /// <summary>
    /// Runtime instance of a single artifact.
    /// Each copy on the map or in a hero's equipment is its own ArtifactInstance;
    /// multiple heroes can each carry a separate instance of the same artifact type.
    ///
    /// Corresponds to VCMI's CArtifactInstance class (and its mixin base classes:
    /// CCombinedArtifactInstance, CScrollArtifactInstance,
    /// CGrowingArtifactInstance, CChargedArtifactInstance).
    /// </summary>
    public class ArtifactInstance
    {
        // 鈹€鈹€ Identity 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Unique instance ID within the game state.
        /// Corresponds to CArtifactInstance::id (ArtifactInstanceID).
        /// </summary>
        public int InstanceId
        {
            get; set;
        } = -1;

        /// <summary>
        /// The artifact type this instance belongs to.
        /// Corresponds to CArtifactInstance::artTypeID.
        /// </summary>
        public EArtifactId TypeId
        {
            get; set;
        } = EArtifactId.NONE;

        // 鈹€鈹€ Charged artifact (CCombinedArtifactInstance) 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Remaining charges for charged artifacts (Tomes, etc.).
        /// 0 for non-charged artifacts.
        /// Corresponds to CChargedArtifactInstance::getCharges().
        /// </summary>
        public int Charges
        {
            get; set;
        }

        // 鈹€鈹€ Scroll artifact 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// The spell stored in a spell scroll instance.
        /// ESpellId.NONE for non-scroll artifacts.
        /// Corresponds to CScrollArtifactInstance::getScrollSpellID().
        /// </summary>
        public ESpellId ScrollSpell
        {
            get; set;
        } = ESpellId.NONE;

        // 鈹€鈹€ Combined artifact (CCombinedArtifactInstance) 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Which constituent parts make up this combined artifact instance,
        /// and which slot each part occupies.
        /// Empty for non-combined artifacts.
        /// Corresponds to CCombinedArtifactInstance::partsInfo.
        /// </summary>
        public List<CombinedArtPartInfo> PartsInfo
        {
            get; set;
        }

        // 鈹€鈹€ Computed helpers 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>Returns true if this instance is a combined artifact (has assembled parts).</summary>
        public bool IsCombined()
        {
            return PartsInfo != null && PartsInfo.Count > 0;
        }

        /// <summary>Returns true if this instance is a spell scroll.</summary>
        public bool IsScroll()
        {
            return TypeId == EArtifactId.SPELL_SCROLL || ScrollSpell != ESpellId.NONE;
        }

        /// <summary>Returns true if this instance has remaining charges.</summary>
        public bool IsCharged()
        {
            return Charges > 0;
        }
    }
}


