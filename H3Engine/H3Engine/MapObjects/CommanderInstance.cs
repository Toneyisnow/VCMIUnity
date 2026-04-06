// Migrated from VCMI lib/mapObjects/army/CCommanderInstance.h
// A commander is a special named creature attached to a hero that can
// gain experience and secondary skills independently.
//
// NOTE: CCommanderInstance in VCMI inherits from CStackInstance, which in turn
// inherits from CBonusSystemNode + CStackBasicDescriptor + CArtifactSet.
// Those base classes are not yet fully ported.  This class captures the
// data fields; the bonus / artifact systems can be wired in later.

using H3Engine.Common;
using System.Collections.Generic;

namespace H3Engine.MapObjects
{
    /// <summary>
    /// A commander creature attached to a hero.
    /// Each hero has at most one commander; most heroes have none.
    ///
    /// Corresponds to VCMI's CCommanderInstance (extends CStackInstance).
    /// Full base-class behaviour (bonus system, artifact set) is deferred
    /// until CStackInstance is ported.
    /// </summary>
    public class CommanderInstance
    {
        // ── Identity ──────────────────────────────────────────────────────────

        /// <summary>
        /// The creature type that determines this commander's base stats.
        /// Corresponds to CStackBasicDescriptor::type (CreatureID).
        /// </summary>
        public ECreatureId CreatureType
        {
            get; set;
        } = ECreatureId.NONE;

        /// <summary>
        /// Unique name of this commander (commanders are named individually).
        /// Corresponds to CCommanderInstance::name.
        /// </summary>
        public string Name
        {
            get; set;
        }

        // ── State ─────────────────────────────────────────────────────────────

        /// <summary>
        /// True while the commander is alive; false after being killed in battle.
        /// Corresponds to CCommanderInstance::alive.
        /// </summary>
        public bool Alive
        {
            get; set;
        } = true;

        /// <summary>
        /// Current level of the commander.
        /// Corresponds to CCommanderInstance::level.
        /// </summary>
        public int Level
        {
            get; set;
        } = 1;

        /// <summary>
        /// Total experience points accumulated by this commander.
        /// Corresponds to CStackInstance::totalExperience.
        /// </summary>
        public long TotalExperience
        {
            get; set;
        }

        // ── Skills ────────────────────────────────────────────────────────────

        /// <summary>
        /// Secondary skill levels for this commander, indexed by skill slot.
        /// Each entry is a skill level (0 = not learned).
        /// Corresponds to CCommanderInstance::secondarySkills (vector&lt;ui8&gt;).
        /// </summary>
        public List<byte> SecondarySkills
        {
            get; set;
        }

        /// <summary>
        /// Special ability IDs unlocked by this commander.
        /// Corresponds to CCommanderInstance::specialSkills (set&lt;ui8&gt;).
        /// </summary>
        public List<byte> SpecialSkills
        {
            get; set;
        }

        // ── Stack quantity ────────────────────────────────────────────────────

        /// <summary>
        /// Number of creatures in this stack (commanders always have 1).
        /// Corresponds to CStackBasicDescriptor::count.
        /// </summary>
        public int Count
        {
            get; set;
        } = 1;
    }
}
