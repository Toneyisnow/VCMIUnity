// Migrated from VCMI lib/entities/artifact/CArtifact.h
// Static artifact type definition 鈥?loaded once from config, never mutated during gameplay.
// Runtime state (which hero equips which copy) lives in ArtifactInstance / ArtifactSet.

using H3Engine.Common;
using H3Engine.Core.Bonus;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// Static definition of an artifact type.
    /// Supersedes the former H3Artifact placeholder class.
    /// Corresponds to VCMI's CArtifact (and its mixin base classes).
    ///
    /// Instance state (which copy a hero has equipped, charges remaining, etc.) is in
    /// <see cref="ArtifactInstance"/>; equipped slots are tracked in <see cref="H3Engine.Core.ArtifactSet"/>.
    /// </summary>
    public class ArtifactType
    {
        // 鈹€鈹€ Identity 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        public EArtifactId Id
        {
            get; set;
        } = EArtifactId.NONE;

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

        /// <summary>
        /// Flavour text shown when the artifact appears as a map event reward.
        /// Corresponds to CArtifact::getEventTranslated().
        /// </summary>
        public string EventText
        {
            get; set;
        }

        // 鈹€鈹€ Visuals 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>Index into the artifact icon sprite sheet.</summary>
        public int IconIndex
        {
            get; set;
        }

        /// <summary>Large icon image path (used in hero screen).</summary>
        public string Image
        {
            get; set;
        }

        /// <summary>
        /// DEF/animation used for the adventure-map pickup object.
        /// Corresponds to CArtifact::advMapDef.
        /// </summary>
        public string AdvMapDef
        {
            get; set;
        }

        // 鈹€鈹€ Economy 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>Gold cost to buy at a marketplace.</summary>
        public int Price
        {
            get; set;
        }

        // 鈹€鈹€ Classification 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Rarity class (SPECIAL / TREASURE / MINOR / MAJOR / RELIC).
        /// Used for random artifact generation and display colour.
        /// </summary>
        public EArtifactClass ArtClass
        {
            get; set;
        } = EArtifactClass.ART_SPECIAL;

        /// <summary>
        /// When true this artifact only appears on maps that contain water.
        /// </summary>
        public bool OnlyOnWaterMap
        {
            get; set;
        }

        // 鈹€鈹€ Slot placement 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Allowed equipment slots per bearer type.
        /// Key: bearer (HERO / CREATURE / COMMANDER / ALTAR)
        /// Value: list of valid EArtifactPosition slots for that bearer.
        /// Corresponds to CArtifact::possibleSlots.
        /// </summary>
        public Dictionary<EArtBearer, List<EArtifactPosition>> PossibleSlots
        {
            get; set;
        }

        // 鈹€鈹€ War machine 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Creature that acts as this artifact in battle (Ballista, First Aid Tent, etc.).
        /// ECreatureId.NONE if this artifact is not a war machine.
        /// </summary>
        public ECreatureId WarMachine
        {
            get; set;
        } = ECreatureId.NONE;

        // 鈹€鈹€ Combined artifact 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Component artifact IDs that must all be equipped to assemble this combined artifact.
        /// Empty if this artifact is not a combined one.
        /// Corresponds to CCombinedArtifact::constituents.
        /// </summary>
        public List<EArtifactId> Constituents
        {
            get; set;
        }

        /// <summary>
        /// When true the combined artifact parts are merged ("fused") into a single slot
        /// instead of keeping individual part slots occupied.
        /// Corresponds to CCombinedArtifact::fused.
        /// </summary>
        public bool IsFused
        {
            get; set;
        }

        // 鈹€鈹€ Spell scroll 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Spell stored in this scroll.  Only meaningful when <see cref="IsScroll"/> is true.
        /// Corresponds to CScrollArtifactInstance::getScrollSpellID().
        /// </summary>
        public ESpellId ScrollSpell
        {
            get; set;
        } = ESpellId.NONE;

        // 鈹€鈹€ Charged artifact 鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€鈹€

        /// <summary>
        /// Starting charges for charged artifacts (e.g. Tomes of X Magic).
        /// 0 means this artifact is not charged.
        /// Corresponds to CChargedArtifact::defaultStartCharges.
        /// </summary>
        public int DefaultStartCharges
        {
            get; set;
        }

        /// <summary>
        /// If true the artifact is removed from the hero when all charges are depleted.
        /// </summary>
        public bool RemoveOnDepletion
        {
            get; set;
        }

        // ── Bonus definitions ─────────────────────────────────────────────────

        /// <summary>
        /// The bonuses this artifact type confers on its bearer when equipped.
        /// Loaded from game data (h3m / JSON config); never mutated at runtime.
        ///
        /// Each element describes one modifier (e.g. "+2 Attack", "+600 movement",
        /// "+10% magic resistance").  At runtime these are copied onto the
        /// <see cref="ArtifactInstance"/> node so each worn copy has its own live
        /// bonuses in the bonus tree.
        ///
        /// Corresponds to VCMI CArtifact::instanceBonuses.
        /// </summary>
        public List<Bonus> Bonuses
        {
            get; set;
        }

        // ── Computed helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns true for MAJOR and RELIC class artifacts.
        /// Corresponds to CArtifact::isBig().
        /// </summary>
        public bool IsBig()
        {
            return (ArtClass & (EArtifactClass.ART_MAJOR | EArtifactClass.ART_RELIC)) != 0;
        }

        /// <summary>
        /// Returns true if the artifact can be sold or traded (not a war machine / grail).
        /// Corresponds to CArtifact::isTradable().
        /// </summary>
        public bool IsTradable()
        {
            // War machines and the grail are non-tradable
            if (WarMachine != ECreatureId.NONE)
                return false;
            if (Id == EArtifactId.GRAIL)
                return false;
            return true;
        }

        /// <summary>Returns true if this is a spell scroll artifact.</summary>
        public bool IsScroll()
        {
            return Id == EArtifactId.SPELL_SCROLL || ScrollSpell != ESpellId.NONE;
        }

        /// <summary>Returns true if this artifact is assembled from multiple parts.</summary>
        public bool IsCombined()
        {
            return Constituents != null && Constituents.Count > 0;
        }

        /// <summary>Returns true if this artifact has a limited number of uses.</summary>
        public bool IsCharged()
        {
            return DefaultStartCharges > 0;
        }
    }
}


