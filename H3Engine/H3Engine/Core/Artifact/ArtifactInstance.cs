// Migrated from VCMI lib/entities/artifact/CArtifactInstance.h
// Represents one runtime copy of an artifact (as opposed to its static type definition).
//
// Key change vs. previous version: ArtifactInstance now extends CBonusSystemNode so
// that its bonuses propagate automatically to any parent node (the wearing hero) via
// AttachTo / DetachFrom.

using H3Engine.Core.Bonus;
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
    ///
    /// Extends <see cref="CBonusSystemNode"/> so that, when the instance is
    /// equipped on a hero (<c>AttachTo(hero)</c>), its bonuses automatically
    /// propagate into the hero's bonus tree.  Unequipping calls
    /// <c>DetachFrom(hero)</c>.
    ///
    /// Each copy on the map or in a hero's equipment is its own ArtifactInstance;
    /// multiple heroes can each carry a separate instance of the same artifact type.
    ///
    /// Corresponds to VCMI's CArtifactInstance class (and its mixin base classes:
    /// CCombinedArtifactInstance, CScrollArtifactInstance,
    /// CGrowingArtifactInstance, CChargedArtifactInstance).
    /// </summary>
    public class ArtifactInstance : CBonusSystemNode
    {
        // ── Identity ──────────────────────────────────────────────────────────

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

        // ── Charged artifact ──────────────────────────────────────────────────

        /// <summary>
        /// Remaining charges for charged artifacts (Tomes, etc.).
        /// 0 for non-charged artifacts.
        /// Corresponds to CChargedArtifactInstance::getCharges().
        /// </summary>
        public int Charges
        {
            get; set;
        }

        // ── Scroll artifact ───────────────────────────────────────────────────

        /// <summary>
        /// The spell stored in a spell scroll instance.
        /// ESpellId.NONE for non-scroll artifacts.
        /// Corresponds to CScrollArtifactInstance::getScrollSpellID().
        /// </summary>
        public ESpellId ScrollSpell
        {
            get; set;
        } = ESpellId.NONE;

        // ── Combined artifact ─────────────────────────────────────────────────

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

        // ── Bonus node identity ───────────────────────────────────────────────

        public override string NodeName
            => $"ArtifactInstance#{InstanceId}({TypeId})";

        // ── Initialisation from type ──────────────────────────────────────────

        /// <summary>
        /// Copies the static bonuses from <paramref name="type"/> onto this instance's
        /// local bonus list and sets up the node type.
        ///
        /// Call this once after constructing a new instance (e.g. when creating an
        /// instance from a pickup on the map or from hero starting equipment).
        ///
        /// Corresponds to VCMI CArtifactInstance::attachToBonusSystem / setType.
        /// </summary>
        public void InitFromType(ArtifactType type)
        {
            if (type == null) return;

            TypeId   = type.Id;
            NodeType = BonusNodeType.ARTIFACT_INSTANCE;

            // Copy every bonus defined on the artifact type as a local bonus of
            // this instance.  We copy (not share) so that instance-specific
            // mutations (e.g. growing artifacts changing val) don't affect the
            // type definition.
            if (type.Bonuses == null) return;

            foreach (var srcBonus in type.Bonuses)
            {
                // Clone the bonus and stamp it with ARTIFACT_INSTANCE source + this
                // artifact type's ID so callers can filter by source later.
                var instBonus = new Bonus
                {
                    Type             = srcBonus.Type,
                    Subtype          = srcBonus.Subtype,
                    Val              = srcBonus.Val,
                    ValType          = srcBonus.ValType,
                    Source           = BonusSource.ARTIFACT_INSTANCE,
                    SourceId         = (int)TypeId,
                    Duration         = srcBonus.Duration,
                    Stacking         = srcBonus.Stacking,
                    EffectRange      = srcBonus.EffectRange,
                    TargetSourceType = srcBonus.TargetSourceType,
                    Hidden           = srcBonus.Hidden,
                    Limiter          = srcBonus.Limiter,
                    Propagator       = srcBonus.Propagator,
                    Updater          = srcBonus.Updater,
                };
                AddNewBonus(instBonus);
            }

            // Charged artifacts: set initial charge count.
            if (type.IsCharged() && Charges == 0)
                Charges = type.DefaultStartCharges;

            // Scroll artifacts: copy the scroll spell.
            if (type.IsScroll() && ScrollSpell == ESpellId.NONE)
                ScrollSpell = type.ScrollSpell;
        }

        // ── Computed helpers ──────────────────────────────────────────────────

        /// <summary>Returns true if this instance is a combined artifact (has assembled parts).</summary>
        public bool IsCombined()
            => PartsInfo != null && PartsInfo.Count > 0;

        /// <summary>Returns true if this instance is a spell scroll.</summary>
        public bool IsScroll()
            => TypeId == EArtifactId.SPELL_SCROLL || ScrollSpell != ESpellId.NONE;

        /// <summary>Returns true if this instance has remaining charges.</summary>
        public bool IsCharged()
            => Charges > 0;
    }
}
