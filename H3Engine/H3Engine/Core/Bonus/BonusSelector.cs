// Migrated from VCMI lib/bonuses/BonusSelector.h/.cpp
// Composable predicate for filtering Bonus objects inside a BonusList query.

using System;
using H3Engine.Core.Constants;

namespace H3Engine.Core.Bonus
{
    // ======================================================================
    //  BonusSelector
    // ======================================================================

    /// <summary>
    /// A composable predicate that tests whether a <see cref="Bonus"/> should be
    /// included in a query result.
    ///
    /// BonusSelectors can be composed with <see cref="And"/>, <see cref="Or"/>,
    /// and <see cref="Not"/> to build arbitrary filters.
    ///
    /// Corresponds to VCMI CSelector (a std::function&lt;bool(const Bonus*)&gt; wrapper
    /// with And/Or/Not composition methods).
    /// </summary>
    public class BonusSelector
    {
        private readonly Func<Bonus, bool> predicate;

        // ── Constructor ───────────────────────────────────────────────────────

        public BonusSelector(Func<Bonus, bool> predicate)
        {
            this.predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        // ── Test ──────────────────────────────────────────────────────────────

        /// <summary>Returns true if <paramref name="b"/> matches this selector.</summary>
        public bool Matches(Bonus b) => b != null && predicate(b);

        // ── Composition ───────────────────────────────────────────────────────

        /// <summary>Returns a selector that matches only when BOTH selectors match.</summary>
        public BonusSelector And(BonusSelector other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new BonusSelector(b => predicate(b) && other.predicate(b));
        }

        /// <summary>Returns a selector that matches when EITHER selector matches.</summary>
        public BonusSelector Or(BonusSelector other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new BonusSelector(b => predicate(b) || other.predicate(b));
        }

        /// <summary>Returns a selector that matches when this selector does NOT match.</summary>
        public BonusSelector Not()
            => new BonusSelector(b => !predicate(b));
    }


    // ======================================================================
    //  Selector  (static factory class)
    // ======================================================================

    /// <summary>
    /// Static factory methods that produce the most common <see cref="BonusSelector"/>
    /// instances, mirroring the <c>Selector</c> namespace in VCMI.
    ///
    /// Usage examples
    /// ──────────────
    /// <code>
    /// // Get total attack bonus:
    /// int atk = hero.ValOfBonuses(
    ///     Selector.ByType(BonusType.PRIMARY_SKILL)
    ///             .And(Selector.BySubtype((int)EPrimarySkill.ATTACK)));
    ///
    /// // All bonuses from artifacts:
    /// var arts = hero.GetAllBonuses(Selector.BySource(BonusSource.ARTIFACT_INSTANCE));
    ///
    /// // Permanent bonuses only:
    /// var perms = hero.GetAllBonuses(Selector.ByDuration(BonusDuration.PERMANENT));
    /// </code>
    ///
    /// Corresponds to the <c>Selector</c> namespace / helper functions in VCMI.
    /// </summary>
    public static class Selector
    {
        // ── Singleton selectors ───────────────────────────────────────────────

        /// <summary>Matches every bonus (pass-through).</summary>
        public static readonly BonusSelector All  = new BonusSelector(_ => true);

        /// <summary>Matches no bonus.</summary>
        public static readonly BonusSelector None = new BonusSelector(_ => false);

        // ── Type / subtype ────────────────────────────────────────────────────

        /// <summary>Matches bonuses of the given <paramref name="type"/>.</summary>
        public static BonusSelector ByType(BonusType type)
            => new BonusSelector(b => b.Type == type);

        /// <summary>Matches bonuses with the given subtype (ignores BonusType).</summary>
        public static BonusSelector BySubtype(int subtype)
            => new BonusSelector(b => b.Subtype == subtype);

        /// <summary>
        /// Matches bonuses whose <see cref="Bonus.Type"/> AND <see cref="Bonus.Subtype"/>
        /// both match. This is the most common compound selector for primary skills, etc.
        /// </summary>
        public static BonusSelector ByTypeAndSubtype(BonusType type, int subtype)
            => new BonusSelector(b => b.Type == type && b.Subtype == subtype);

        // ── Source ────────────────────────────────────────────────────────────

        /// <summary>Matches bonuses from a given source category.</summary>
        public static BonusSelector BySource(BonusSource source)
            => new BonusSelector(b => b.Source == source);

        /// <summary>
        /// Matches bonuses from a specific source object
        /// (source category + numeric ID must both match).
        /// </summary>
        public static BonusSelector BySourceAndId(BonusSource source, int sourceId)
            => new BonusSelector(b => b.Source == source && b.SourceId == sourceId);

        // ── ValType ───────────────────────────────────────────────────────────

        /// <summary>Matches bonuses with the given value type.</summary>
        public static BonusSelector ByValType(BonusValueType valType)
            => new BonusSelector(b => b.ValType == valType);

        // ── Duration ─────────────────────────────────────────────────────────

        /// <summary>Matches bonuses that have at least one of the given duration flag(s).</summary>
        public static BonusSelector ByDuration(BonusDuration duration)
            => new BonusSelector(b => (b.Duration & duration) != 0);

        // ── Effect range ──────────────────────────────────────────────────────

        /// <summary>Matches bonuses that apply in the given combat context.</summary>
        public static BonusSelector ByEffectRange(BonusLimitEffect range)
            => new BonusSelector(b => b.EffectRange == range);

        // ── Compound convenience selectors ────────────────────────────────────

        /// <summary>
        /// Matches all PERMANENT bonuses from artifact instances.
        /// Useful to quickly ask "what do my equipped artifacts give me?".
        /// </summary>
        public static readonly BonusSelector ArtifactInstancePermanent =
            new BonusSelector(b =>
                b.Source   == BonusSource.ARTIFACT_INSTANCE &&
                (b.Duration & BonusDuration.PERMANENT) != 0);

        /// <summary>
        /// Matches all spell-effect bonuses (duration = ONE_BATTLE or shorter).
        /// </summary>
        public static readonly BonusSelector SpellEffects =
            new BonusSelector(b => b.Source == BonusSource.SPELL_EFFECT);

        // ── Primary skill convenience ─────────────────────────────────────────

        /// <summary>
        /// Returns a selector that matches PRIMARY_SKILL bonuses for
        /// <paramref name="skill"/> (using EPrimarySkill cast to int as subtype).
        /// </summary>
        public static BonusSelector PrimarySkill(Constants.EPrimarySkill skill)
            => ByTypeAndSubtype(BonusType.PRIMARY_SKILL, (int)skill);

        /// <summary>
        /// Returns a selector that matches SECONDARY_SKILL_PREMY bonuses for
        /// <paramref name="skill"/> (using ESecondarySkill cast to int as subtype).
        /// </summary>
        public static BonusSelector SecondarySkill(Constants.ESecondarySkill skill)
            => ByTypeAndSubtype(BonusType.SECONDARY_SKILL_PREMY, (int)skill);
    }
}
