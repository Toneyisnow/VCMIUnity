// Corresponds to VCMI lib/mapObjects/CGHeroInstance.h
// Merged with existing H3Engine HeroInstance: all prior fields kept,
// new fields from CGHeroInstance added at the instance layer.

using H3Engine.Core;
using System.Collections.Generic;

namespace H3Engine.MapObjects
{
    /// <summary>
    /// Hero patrol configuration: whether the hero patrols and the patrol area.
    /// Corresponds to CGHeroInstance::Patrol in VCMI.
    /// </summary>
    public class HeroPatrol
    {
        public bool IsPatrolling
        {
            get; set;
        }

        public MapPosition InitialPosition
        {
            get; set;
        }

        /// <summary>
        /// Radius (in tiles) of the patrol area. 0 = no patrolling.
        /// Corresponds to CGHeroInstance::Patrol::patrolRadius.
        /// </summary>
        public uint PatrolRadius
        {
            get; set;
        }
    }

    /// <summary>
    /// A hero placed on the adventure map.
    /// Extends <see cref="ArmedInstance"/> (armed map object) and adds all
    /// hero-specific state: movement, spells, mana, town garrison, boat, commander.
    ///
    /// Corresponds to VCMI's CGHeroInstance, which inherits from:
    ///   CArmedInstance, CArtifactSet, spells::Caster, AFactionMember,
    ///   IBoatGenerator, ICreatureUpgrader, IOwnableObject.
    ///
    /// Static type data (name, biography, specialty, initial army, …) lives in
    /// <see cref="H3Hero.Data"/> → <see cref="HeroType"/>.
    /// </summary>
    public class HeroInstance : ArmedInstance
    {
        public HeroInstance(H3Hero data = null)
        {
            Data = data ?? new H3Hero();
            RestMovePoint = -1; // uninitialized – treat as full movement
        }

        // ── Data container ────────────────────────────────────────────────────

        /// <summary>
        /// Hero instance data loaded from the map / game save.
        /// Holds experience, skills, spells, artifacts, army and other
        /// mutable per-instance values.
        /// </summary>
        public H3Hero Data
        {
            get; set;
        }

        // ── Movement ─────────────────────────────────────────────────────────

        /// <summary>
        /// Remaining movement points for the current turn.
        /// -1 = uninitialized (treat as full).
        /// Corresponds to CGHeroInstance::movement.
        /// </summary>
        public int RestMovePoint
        {
            get; set;
        }

        /// <summary>
        /// Direction the hero is facing (0-7, compass rose + center).
        /// Corresponds to CGHeroInstance::moveDir.
        /// </summary>
        public byte MoveDirection
        {
            get; set;
        }

        /// <summary>
        /// Delegates to H3Hero for the effective (max) movement points this turn.
        /// </summary>
        public int GetEffectiveMovePoint() => Data.GetEffectiveMovePoint();

        /// <summary>
        /// Returns remaining move points; returns full if uninitialized.
        /// </summary>
        public int GetCurrentMovePoint() => RestMovePoint < 0 ? GetEffectiveMovePoint() : RestMovePoint;

        /// <summary>Resets movement to full at the start of a new turn.</summary>
        public void ResetMovePointForNewTurn() => RestMovePoint = GetEffectiveMovePoint();

        // ── Mana ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Current spell points (mana).
        /// -1 = uninitialized (UNINITIALIZED_MANA in VCMI).
        /// Corresponds to CGHeroInstance::mana.
        /// </summary>
        public int Mana
        {
            get; set;
        } = -1;

        /// <summary>Returns true if mana has been initialised.</summary>
        public bool IsManaInitialized => Mana >= 0;

        // ── Town / garrison ───────────────────────────────────────────────────

        /// <summary>
        /// True when this hero is stationed in a town garrison rather than
        /// moving freely on the map.
        /// Corresponds to CGHeroInstance::inTownGarrison.
        /// </summary>
        public bool InTownGarrison
        {
            get; set;
        }

        /// <summary>
        /// The town this hero is currently visiting or garrisoned in.
        /// Null if the hero is not in a town.
        /// Corresponds to CGHeroInstance::visitedTown (ObjectInstanceID).
        /// </summary>
        public TownInstance VisitedTown
        {
            get; set;
        }

        /// <summary>
        /// All map objects this hero has already visited during the current game.
        /// Corresponds to CGHeroInstance::visitedObjects.
        /// </summary>
        public List<CGObject> VisitedObjects
        {
            get; set;
        }

        // ── Boat ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Object ID of the boat this hero is currently sailing on.
        /// -1 means the hero is on land.
        /// Corresponds to CGHeroInstance::boardedBoat (ObjectInstanceID).
        /// </summary>
        public int BoardedBoatId
        {
            get; set;
        } = -1;

        /// <summary>Returns true if the hero is currently aboard a boat.</summary>
        public bool InBoat => BoardedBoatId >= 0;

        // ── Battle formation ──────────────────────────────────────────────────

        /// <summary>
        /// When true the hero's army uses the tactic formation phase at battle start.
        /// Corresponds to CGHeroInstance::tacticFormationEnabled.
        /// </summary>
        public bool TacticFormationEnabled
        {
            get; set;
        }

        // ── Portrait override ─────────────────────────────────────────────────

        /// <summary>
        /// If >= 0, overrides the hero's portrait with the portrait from the
        /// referenced hero type ID (used by some campaign heroes).
        /// Corresponds to CGHeroInstance::customPortraitSource (HeroTypeID).
        /// </summary>
        public int CustomPortraitSource
        {
            get; set;
        } = -1;

        // ── Commander ────────────────────────────────────────────────────────

        /// <summary>
        /// Optional commander creature attached to this hero.
        /// Null if the hero has no commander (most heroes).
        /// Corresponds to CGHeroInstance::commander (CCommanderInstance*).
        /// See <see cref="CommanderInstance"/> for the full definition.
        /// </summary>
        public CommanderInstance Commander
        {
            get; set;
        }

        // ── Patrol ────────────────────────────────────────────────────────────

        /// <summary>
        /// Patrol settings for this hero (position, radius, active flag).
        /// Corresponds to CGHeroInstance::patrol.
        /// </summary>
        public HeroPatrol Patrol
        {
            get; set;
        }
    }
}
