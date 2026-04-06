// Corresponds to VCMI lib/mapObjects/army/CArmedInstance.h
// Merged with the existing H3Engine ArmedInstance: Message and AddStack() kept,
// GuardArmy renamed to Army (GuardArmy kept as compatibility alias).

using H3Engine.Core;
using System;
using System.Collections.Generic;

namespace H3Engine.MapObjects
{
    /// <summary>
    /// Base class for any map object that carries an army and can participate in battle.
    /// Corresponds to VCMI's CArmedInstance (CGObjectInstance + CCreatureSet).
    ///
    /// Subclasses: <see cref="HeroInstance"/>, <see cref="TownInstance"/>,
    /// <see cref="CGCreature"/>, <see cref="CGPandoraBox"/>, …
    /// </summary>
    public class ArmedInstance : CGObject
    {
        public ArmedInstance()
        {
            Army = new CreatureSet();
        }

        // ── Army ─────────────────────────────────────────────────────────────

        /// <summary>
        /// The army this object owns (up to 7 creature stacks).
        /// Corresponds to the CCreatureSet part of CArmedInstance.
        /// </summary>
        public CreatureSet Army
        {
            get; set;
        }

        /// <summary>
        /// Backwards-compatibility alias for <see cref="Army"/>.
        /// Prefer using <see cref="Army"/> in new code.
        /// </summary>
        [Obsolete("Use Army instead")]
        public CreatureSet GuardArmy
        {
            get => Army;
            set => Army = value;
        }

        /// <summary>
        /// Optional message shown to the visiting hero before the battle begins
        /// (used by guarded objects such as Pandora's Box and roaming creatures).
        /// No direct equivalent in CArmedInstance – stored here for convenience.
        /// </summary>
        public string Message
        {
            get; set;
        }

        // ── Battle state ─────────────────────────────────────────────────────

        /// <summary>
        /// ID of the current battle this object is engaged in.
        /// -1 means the object is not in battle.
        /// Simplified equivalent of CArmedInstance::battle (BattleInfo*).
        /// </summary>
        public int BattleId
        {
            get; set;
        } = -1;

        /// <summary>Returns true if this object is currently in a battle.</summary>
        public bool IsInBattle => BattleId >= 0;

        // ── Army helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Adds a creature stack to the army at the specified slot index.
        /// Corresponds to CCreatureSet::addToSlot in VCMI.
        /// </summary>
        public void AddStack(int slotIndex, StackDescriptor stack)
        {
            Army.Stacks.Add(stack);
        }

        /// <summary>
        /// Returns the number of stacks currently in the army.
        /// Corresponds to CCreatureSet::stacksCount() in VCMI.
        /// </summary>
        public int StacksCount => Army.Stacks?.Count ?? 0;
    }
}
