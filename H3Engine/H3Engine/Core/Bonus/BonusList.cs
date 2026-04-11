// Migrated from VCMI lib/bonuses/BonusList.h/.cpp
// Stores a flat list of Bonus references and provides totalValue() with the
// full VCMI evaluation formula.

using System;
using System.Collections;
using System.Collections.Generic;

namespace H3Engine.Core.Bonus
{
    /// <summary>
    /// A flat, mutable list of <see cref="Bonus"/> references with value-calculation
    /// helpers.
    ///
    /// Key responsibilities
    /// ───────────────────
    /// • Container for bonuses collected by a <see cref="CBonusSystemNode"/> query.
    /// • <see cref="StackBonuses"/>  – removes non-stacking duplicates (same stacking
    ///   group → keep only highest absolute value).
    /// • <see cref="TotalValue"/>    – computes the final integer stat value using
    ///   the VCMI multi-bucket formula.
    ///
    /// Corresponds to VCMI CArtifactSet / BonusList class.
    /// </summary>
    public class BonusList : IEnumerable<Bonus>
    {
        // ── Storage ──────────────────────────────────────────────────────────
        private readonly List<Bonus> bonuses = new List<Bonus>();

        // ── IEnumerable ───────────────────────────────────────────────────────
        public IEnumerator<Bonus> GetEnumerator() => bonuses.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()   => bonuses.GetEnumerator();

        // ── Collection helpers ────────────────────────────────────────────────
        public int  Count => bonuses.Count;
        public bool IsEmpty => bonuses.Count == 0;

        public void Add(Bonus b)
        {
            if (b != null) bonuses.Add(b);
        }

        public void AddRange(IEnumerable<Bonus> range)
        {
            foreach (var b in range) Add(b);
        }

        public void Remove(Bonus b) => bonuses.Remove(b);

        public void Clear() => bonuses.Clear();

        public Bonus this[int index] => bonuses[index];

        // ── Filtering ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a new BonusList containing only the bonuses that satisfy
        /// <paramref name="selector"/>.
        /// </summary>
        public BonusList Filter(BonusSelector selector)
        {
            var result = new BonusList();
            foreach (var b in bonuses)
                if (selector.Matches(b))
                    result.Add(b);
            return result;
        }

        /// <summary>
        /// Returns the first bonus matching <paramref name="selector"/>, or null.
        /// </summary>
        public Bonus GetFirst(BonusSelector selector)
        {
            foreach (var b in bonuses)
                if (selector.Matches(b))
                    return b;
            return null;
        }

        // ── Stacking ──────────────────────────────────────────────────────────

        /// <summary>
        /// Removes non-stacking duplicates in-place.
        ///
        /// Rules (matching VCMI stackBonuses):
        /// • Bonuses with an empty <see cref="Bonus.Stacking"/> string always stack.
        /// • Bonuses with <see cref="Bonus.Stacking"/> == "ALWAYS" always stack.
        /// • Within the same (Stacking, Type, Subtype, ValType) group, only the bonus
        ///   with the highest absolute value is kept.
        ///
        /// Corresponds to VCMI BonusList::stackBonuses().
        /// </summary>
        public void StackBonuses()
        {
            // Sort so that identical stacking keys end up adjacent,
            // highest absolute value first within each group.
            bonuses.Sort((a, b) =>
            {
                int c = string.Compare(a.Stacking, b.Stacking, StringComparison.Ordinal);
                if (c != 0) return c;
                c = a.Type.CompareTo(b.Type);
                if (c != 0) return c;
                c = a.Subtype.CompareTo(b.Subtype);
                if (c != 0) return c;
                c = a.ValType.CompareTo(b.ValType);
                if (c != 0) return c;
                // Descending by absolute value so we keep the best one first.
                return Math.Abs(b.Val).CompareTo(Math.Abs(a.Val));
            });

            for (int i = bonuses.Count - 1; i > 0; i--)
            {
                var cur  = bonuses[i];
                var prev = bonuses[i - 1];

                // Empty stacking or "ALWAYS" → always stack, never remove.
                if (string.IsNullOrEmpty(cur.Stacking) || cur.Stacking == "ALWAYS")
                    continue;

                // Same group key → keep only the first (highest absolute value).
                if (cur.Stacking == prev.Stacking
                    && cur.Type    == prev.Type
                    && cur.Subtype == prev.Subtype
                    && cur.ValType == prev.ValType)
                {
                    bonuses.RemoveAt(i);
                }
            }
        }

        // ── Value calculation ─────────────────────────────────────────────────

        /// <summary>
        /// Computes the final integer value for this set of bonuses,
        /// using the VCMI multi-bucket formula.
        ///
        /// Formula (simplified for H3Engine; mirrors BonusList::totalValue):
        /// <code>
        ///   // 1. Separate by valType
        ///   // 2. Adjust each ADDITIVE bonus by any PERCENT_TO_SOURCE that shares
        ///      its BonusSource
        ///   // 3. base = baseValue
        ///   //         + Σ BASE_NUMBER
        ///   //         + Σ adjusted ADDITIVE_VALUE
        ///   // 4. base = base × (100 + Σ PERCENT_TO_BASE) / 100
        ///   // 5. result = base × (100 + Σ PERCENT_TO_ALL) / 100
        ///   // 6. result = clamp(result, indepMax, indepMin)
        /// </code>
        ///
        /// Corresponds to VCMI BonusList::totalValue(int baseValue).
        /// </summary>
        public int TotalValue(int baseValue = 0)
        {
            // ── Bucket accumulators ───────────────────────────────────────────
            int sumBase        = 0;   // BASE_NUMBER bonuses
            int sumPercentBase = 0;   // PERCENT_TO_BASE
            int sumPercentAll  = 0;   // PERCENT_TO_ALL

            // PERCENT_TO_SOURCE: percentages keyed by source category.
            // We accumulate per-source percent modifiers for ADDITIVE bonuses.
            var percentToSource = new Dictionary<BonusSource, int>();

            // PERCENT_TO_TARGET_TYPE: percentages keyed by targetSourceType.
            var percentToTarget = new Dictionary<BonusSource, int>();

            // ADDITIVE_VALUE bonuses (collected before adjustment).
            var additives = new List<Bonus>();

            // INDEPENDENT bounds.
            int? indepMax = null;   // result must be at least this
            int? indepMin = null;   // result must be at most this

            // ── Bucket pass ───────────────────────────────────────────────────
            foreach (var b in bonuses)
            {
                switch (b.ValType)
                {
                    case BonusValueType.BASE_NUMBER:
                        sumBase += b.Val;
                        break;

                    case BonusValueType.ADDITIVE_VALUE:
                        additives.Add(b);
                        break;

                    case BonusValueType.PERCENT_TO_BASE:
                        sumPercentBase += b.Val;
                        break;

                    case BonusValueType.PERCENT_TO_ALL:
                        sumPercentAll += b.Val;
                        break;

                    case BonusValueType.PERCENT_TO_SOURCE:
                        if (!percentToSource.ContainsKey(b.Source))
                            percentToSource[b.Source] = 0;
                        percentToSource[b.Source] += b.Val;
                        break;

                    case BonusValueType.PERCENT_TO_TARGET_TYPE:
                        if (!percentToTarget.ContainsKey(b.TargetSourceType))
                            percentToTarget[b.TargetSourceType] = 0;
                        percentToTarget[b.TargetSourceType] += b.Val;
                        break;

                    case BonusValueType.INDEPENDENT_MAX:
                        // indepMax = soft floor: result must reach at least this value.
                        indepMax = indepMax.HasValue ? Math.Max(indepMax.Value, b.Val) : b.Val;
                        break;

                    case BonusValueType.INDEPENDENT_MIN:
                        // indepMin = soft cap: result must not exceed this value.
                        indepMin = indepMin.HasValue ? Math.Min(indepMin.Value, b.Val) : b.Val;
                        break;
                }
            }

            // ── Adjust each ADDITIVE bonus by matching PERCENT_TO_SOURCE ──────
            int sumAdditive = 0;
            foreach (var b in additives)
            {
                int val = b.Val;

                // Apply PERCENT_TO_SOURCE for this bonus's source category.
                if (percentToSource.TryGetValue(b.Source, out int pSrc))
                    val = val + val * pSrc / 100;

                // Apply PERCENT_TO_TARGET_TYPE where this bonus's source is the target.
                if (percentToTarget.TryGetValue(b.Source, out int pTgt))
                    val = val + val * pTgt / 100;

                sumAdditive += val;
            }

            // ── Build the base ────────────────────────────────────────────────
            int total = baseValue + sumBase + sumAdditive;

            // ── Apply PERCENT_TO_BASE ─────────────────────────────────────────
            if (sumPercentBase != 0)
                total = total * (100 + sumPercentBase) / 100;

            // ── Apply PERCENT_TO_ALL ──────────────────────────────────────────
            if (sumPercentAll != 0)
                total = total * (100 + sumPercentAll) / 100;

            // ── Independent bounds ────────────────────────────────────────────
            if (indepMax.HasValue || indepMin.HasValue)
            {
                if (indepMax.HasValue && indepMin.HasValue)
                {
                    // If min > max the standard VCMI rule: cap wins (indepMin).
                    if (indepMin.Value < indepMax.Value)
                        indepMax = indepMin;
                    total = Math.Max(total, indepMax.Value);
                    total = Math.Min(total, indepMin.Value);
                }
                else if (indepMax.HasValue)
                {
                    // Only a floor is defined.
                    total = Math.Max(total, indepMax.Value);
                }
                else
                {
                    // Only a cap is defined.
                    total = Math.Min(total, indepMin.Value);
                }
            }

            return total;
        }

        /// <summary>
        /// Convenience: filter by selector then compute total value.
        /// </summary>
        public int ValOfBonuses(BonusSelector selector, int baseValue = 0)
            => Filter(selector).TotalValue(baseValue);
    }
}
