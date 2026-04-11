// Migrated from VCMI lib/bonuses/Bonus.h + Limiters.h + Propagators.h + Updaters.h
// One file for the Bonus data class and the minimal stub interfaces it depends on.

using H3Engine.Core.Constants;

namespace H3Engine.Core.Bonus
{
    // ======================================================================
    //  Stub interfaces (minimal; expand when specific limiters/updaters
    //  are needed in later milestones)
    // ======================================================================

    /// <summary>
    /// Context passed to a limiter so it can inspect the current evaluation environment.
    /// Corresponds to VCMI BonusLimitationContext.
    /// </summary>
    public class BonusLimitationContext
    {
        /// <summary>The bonus currently being evaluated.</summary>
        public Bonus Bonus { get; set; }

        /// <summary>The node that owns the bonus (artifact instance, spell, etc.).</summary>
        public CBonusSystemNode Source { get; set; }

        /// <summary>The node that is querying for bonuses (usually the hero).</summary>
        public CBonusSystemNode Target { get; set; }
    }

    /// <summary>
    /// Decision returned by a limiter.
    /// Corresponds to VCMI ILimiter::EDecision.
    /// </summary>
    public enum LimiterDecision
    {
        /// <summary>The bonus is accepted and should be included.</summary>
        ACCEPT        = 0,
        /// <summary>The bonus is rejected and should be excluded.</summary>
        DISCARD       = 1,
        /// <summary>This limiter abstains; other limiters decide.</summary>
        NOT_SURE      = 2,
        /// <summary>The limiter cannot evaluate in the current context.</summary>
        NOT_APPLICABLE = 3,
    }

    /// <summary>
    /// Conditional gate on a bonus. A bonus is included in a query only when
    /// its limiter returns ACCEPT (or the limiter is null).
    /// Corresponds to VCMI ILimiter.
    /// </summary>
    public interface ILimiter
    {
        LimiterDecision Limit(BonusLimitationContext context);
    }

    /// <summary>
    /// Controls which bonus-system nodes receive a propagated copy of a bonus.
    /// Corresponds to VCMI IPropagator.
    /// </summary>
    public interface IPropagator
    {
        /// <summary>Returns true if the bonus should be attached to <paramref name="dest"/>.</summary>
        bool ShouldBeAttached(CBonusSystemNode dest);

        /// <summary>The node type this propagator targets.</summary>
        BonusNodeType PropagatorType { get; }
    }

    /// <summary>
    /// Dynamically recalculates or transforms a bonus's value when it is retrieved.
    /// Corresponds to VCMI IUpdater.
    /// </summary>
    public interface IUpdater
    {
        /// <summary>
        /// Returns a (possibly new) Bonus whose value has been recalculated for the
        /// given <paramref name="context"/> node (e.g. scales with hero level).
        /// </summary>
        Bonus CreateUpdatedBonus(Bonus original, CBonusSystemNode context);
    }


    // ======================================================================
    //  Bonus
    // ======================================================================

    /// <summary>
    /// Represents a single modifier in the bonus system.
    /// Instances are owned by <see cref="CBonusSystemNode"/>s and referenced by
    /// <see cref="BonusList"/> queries.
    ///
    /// Corresponds to VCMI struct Bonus.
    /// </summary>
    public class Bonus
    {
        // ── Type / subtype / value ──────────────────────────────────────────

        /// <summary>What property is being modified.</summary>
        public BonusType Type { get; set; } = BonusType.NONE;

        /// <summary>
        /// Secondary discriminator; meaning is Type-dependent (see BonusType docs).
        /// -1 means "no subtype".
        /// </summary>
        public int Subtype { get; set; } = -1;

        /// <summary>The numeric value of this modifier.</summary>
        public int Val { get; set; }

        /// <summary>How the value is combined with the running total.</summary>
        public BonusValueType ValType { get; set; } = BonusValueType.ADDITIVE_VALUE;

        // ── Source ───────────────────────────────────────────────────────────

        /// <summary>Category of game entity that created this bonus.</summary>
        public BonusSource Source { get; set; } = BonusSource.OTHER;

        /// <summary>
        /// Numeric ID of the specific source object
        /// (e.g. EArtifactId cast to int, ESpellId cast to int, etc.).
        /// -1 means no specific object.
        /// Corresponds to VCMI BonusSourceID sid.
        /// </summary>
        public int SourceId { get; set; } = -1;

        // ── Duration ─────────────────────────────────────────────────────────

        /// <summary>When this bonus expires.</summary>
        public BonusDuration Duration { get; set; } = BonusDuration.PERMANENT;

        /// <summary>
        /// Remaining turns or days for <see cref="BonusDuration.N_TURNS"/> /
        /// <see cref="BonusDuration.N_DAYS"/> bonuses.
        /// </summary>
        public int TurnsRemain { get; set; }

        // ── Stacking ─────────────────────────────────────────────────────────

        /// <summary>
        /// Non-stacking group identifier.
        /// Within the same group only the bonus with the highest absolute value is kept.
        /// The special value "ALWAYS" forces the bonus to always stack regardless.
        /// Empty string (default) means the bonus always stacks.
        /// </summary>
        public string Stacking { get; set; } = "";

        // ── Effect range ─────────────────────────────────────────────────────

        /// <summary>Restricts the bonus to melee-only or ranged-only combat.</summary>
        public BonusLimitEffect EffectRange { get; set; } = BonusLimitEffect.NO_LIMIT;

        // ── PERCENT_TO_TARGET_TYPE support ────────────────────────────────────

        /// <summary>
        /// When <see cref="ValType"/> is <see cref="BonusValueType.PERCENT_TO_TARGET_TYPE"/>
        /// this specifies which source category's bonuses are amplified.
        /// </summary>
        public BonusSource TargetSourceType { get; set; } = BonusSource.OTHER;

        // ── UI ───────────────────────────────────────────────────────────────

        /// <summary>When true the bonus is not displayed in the hero / unit information screen.</summary>
        public bool Hidden { get; set; }

        // ── Extensibility hooks ───────────────────────────────────────────────

        /// <summary>
        /// Optional conditional gate. When set, the bonus is only applied when
        /// the limiter returns <see cref="LimiterDecision.ACCEPT"/>.
        /// </summary>
        public ILimiter Limiter { get; set; }

        /// <summary>
        /// Optional propagation rule. When set, determines which other nodes
        /// should receive a copy of this bonus.
        /// </summary>
        public IPropagator Propagator { get; set; }

        /// <summary>
        /// Optional dynamic value calculator. When set, the bonus's effective value
        /// is computed at query time (e.g. scales with hero level).
        /// </summary>
        public IUpdater Updater { get; set; }

        // ── Factory helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a simple permanent, additive bonus with no source ID.
        /// Useful for hardcoded artifact / creature bonuses.
        /// </summary>
        public static Bonus Permanent(BonusType type, int subtype, int val,
            BonusSource source = BonusSource.OTHER)
        {
            return new Bonus
            {
                Type     = type,
                Subtype  = subtype,
                Val      = val,
                ValType  = BonusValueType.ADDITIVE_VALUE,
                Source   = source,
                Duration = BonusDuration.PERMANENT,
            };
        }

        /// <summary>
        /// Creates a permanent percentage bonus (PERCENT_TO_BASE).
        /// Useful for skill-level multipliers such as Logistics (+10/20/30%).
        /// </summary>
        public static Bonus PercentToBase(BonusType type, int subtype, int percent,
            BonusSource source = BonusSource.OTHER)
        {
            return new Bonus
            {
                Type     = type,
                Subtype  = subtype,
                Val      = percent,
                ValType  = BonusValueType.PERCENT_TO_BASE,
                Source   = source,
                Duration = BonusDuration.PERMANENT,
            };
        }

        /// <summary>
        /// Creates a permanent flat artifact bonus.
        /// Automatically sets Source = ARTIFACT_INSTANCE and SourceId to the artifact ID.
        /// </summary>
        public static Bonus FromArtifact(BonusType type, int subtype, int val, int artifactId)
        {
            return new Bonus
            {
                Type     = type,
                Subtype  = subtype,
                Val      = val,
                ValType  = BonusValueType.ADDITIVE_VALUE,
                Source   = BonusSource.ARTIFACT_INSTANCE,
                SourceId = artifactId,
                Duration = BonusDuration.PERMANENT,
            };
        }

        public override string ToString()
            => $"Bonus({Type}[{Subtype}] {Val} {ValType} from {Source}#{SourceId})";
    }
}
