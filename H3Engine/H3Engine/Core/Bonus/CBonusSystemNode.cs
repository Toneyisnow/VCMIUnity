// Migrated from VCMI lib/bonuses/CBonusSystemNode.h/.cpp
//                  + lib/bonuses/IBonusBearer.h/.cpp
//
// This single file contains both the IBonusBearer interface (for duck-typing)
// and CBonusSystemNode (the concrete tree node all bonus-bearing game objects
// inherit from).

using System;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core.Bonus
{
    // ======================================================================
    //  IBonusBearer
    // ======================================================================

    /// <summary>
    /// Minimal interface for any object that can be queried for bonus values.
    /// The full helper API is provided by <see cref="CBonusSystemNode"/>.
    ///
    /// Corresponds to VCMI IBonusBearer.
    /// </summary>
    public interface IBonusBearer
    {
        /// <summary>Returns all bonuses that match <paramref name="selector"/>.</summary>
        BonusList GetAllBonuses(BonusSelector selector);

        /// <summary>
        /// Returns a monotonically increasing version counter that increments
        /// whenever any bonus in the reachable tree is added or removed.
        /// Used to invalidate cached query results.
        /// </summary>
        int GetTreeVersion();
    }


    // ======================================================================
    //  CBonusSystemNode
    // ======================================================================

    /// <summary>
    /// Base class for every game entity that participates in the bonus system
    /// (heroes, creatures, artifact instances, spell effects, …).
    ///
    /// Tree structure
    /// ──────────────
    /// Each node stores:
    ///   • <b>localBonuses</b>  – bonuses attached directly to this node.
    ///   • <b>parents</b>       – nodes whose bonuses are inherited by this node.
    ///   • <b>children</b>      – nodes that inherit from this node (back-references).
    ///
    /// When <see cref="GetAllBonuses"/> is called on a node it recursively walks
    /// all parent chains, collects matching bonuses, optionally stacks them, and
    /// returns the result.
    ///
    /// Caching
    /// ───────
    /// Each node maintains a cache keyed by (selectorHash, treeVersion).
    /// The treeVersion is propagated up to every ancestor whenever a bonus is
    /// added or removed anywhere in the subtree.
    ///
    /// Corresponds to VCMI CBonusSystemNode.
    /// </summary>
    public class CBonusSystemNode : IBonusBearer
    {
        // ── Tree fields ───────────────────────────────────────────────────────
        private readonly List<Bonus>             localBonuses = new List<Bonus>();
        private readonly List<CBonusSystemNode>  parents      = new List<CBonusSystemNode>();
        private readonly List<CBonusSystemNode>  children     = new List<CBonusSystemNode>();

        // ── Change tracking ───────────────────────────────────────────────────
        /// <summary>
        /// Monotonically increasing counter; bumped whenever the reachable bonus
        /// tree changes.  Child nodes propagate version changes to all their parents.
        /// </summary>
        private int treeVersion;

        // ── Cache ─────────────────────────────────────────────────────────────
        // Key: (selector hash, tree version at query time)  →  BonusList snapshot.
        // Using a simple Dictionary is safe here because we are single-threaded.
        private readonly Dictionary<long, BonusList> queryCache
            = new Dictionary<long, BonusList>();

        // ── Identity ──────────────────────────────────────────────────────────

        /// <summary>Semantic type of this node in the tree hierarchy.</summary>
        public BonusNodeType NodeType { get; set; } = BonusNodeType.NONE;

        /// <summary>Human-readable name for diagnostics / logging.</summary>
        public virtual string NodeName => NodeType.ToString();

        // ══════════════════════════════════════════════════════════════════════
        //  Tree management
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Attaches this node as a child of <paramref name="parent"/>.
        /// After attachment this node will inherit all bonuses reachable from
        /// <paramref name="parent"/>.
        ///
        /// Corresponds to VCMI CBonusSystemNode::attachTo(parent).
        /// </summary>
        public void AttachTo(CBonusSystemNode parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (parents.Contains(parent)) return;   // already attached

            parents.Add(parent);
            parent.children.Add(this);

            // Invalidate caches up the tree.
            NodeHasChanged();
        }

        /// <summary>
        /// Detaches this node from <paramref name="parent"/>.
        ///
        /// Corresponds to VCMI CBonusSystemNode::detachFrom(parent).
        /// </summary>
        public void DetachFrom(CBonusSystemNode parent)
        {
            if (parent == null) return;
            if (!parents.Remove(parent)) return;

            parent.children.Remove(this);
            NodeHasChanged();
        }

        /// <summary>Detaches from all parent nodes.</summary>
        public void DetachFromAll()
        {
            // Iterate over a copy because DetachFrom modifies the list.
            foreach (var p in parents.ToArray())
                DetachFrom(p);
        }

        /// <summary>Read-only view of this node's parent nodes.</summary>
        public IReadOnlyList<CBonusSystemNode> Parents => parents;

        /// <summary>Read-only view of this node's child nodes.</summary>
        public IReadOnlyList<CBonusSystemNode> Children => children;

        // ══════════════════════════════════════════════════════════════════════
        //  Local bonus management
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Adds a bonus directly to this node.
        ///
        /// Corresponds to VCMI CBonusSystemNode::addNewBonus(b).
        /// </summary>
        public void AddNewBonus(Bonus b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            localBonuses.Add(b);
            NodeHasChanged();
        }

        /// <summary>
        /// Removes a specific bonus from this node's local list.
        ///
        /// Corresponds to VCMI CBonusSystemNode::removeBonus(b).
        /// </summary>
        public void RemoveBonus(Bonus b)
        {
            if (localBonuses.Remove(b))
                NodeHasChanged();
        }

        /// <summary>
        /// Removes all local bonuses matching <paramref name="selector"/>.
        ///
        /// Corresponds to VCMI CBonusSystemNode::removeBonuses(selector).
        /// </summary>
        public void RemoveBonuses(BonusSelector selector)
        {
            int removed = localBonuses.RemoveAll(b => selector.Matches(b));
            if (removed > 0) NodeHasChanged();
        }

        /// <summary>Read-only view of the bonuses stored directly on this node.</summary>
        public IReadOnlyList<Bonus> LocalBonuses => localBonuses;

        // ══════════════════════════════════════════════════════════════════════
        //  IBonusBearer implementation
        // ══════════════════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public int GetTreeVersion() => treeVersion;

        /// <summary>
        /// Returns a <see cref="BonusList"/> of all bonuses (from this node and all
        /// reachable ancestors) that match <paramref name="selector"/>.
        ///
        /// Results are cached per selector hash + tree version so repeated queries
        /// for the same stat are cheap.
        ///
        /// Corresponds to VCMI CBonusSystemNode::getAllBonuses.
        /// </summary>
        public BonusList GetAllBonuses(BonusSelector selector)
        {
            if (selector == null) selector = Selector.All;

            long cacheKey = BuildCacheKey(selector);
            if (queryCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var result = new BonusList();
            CollectBonusesRecursive(result, selector, new HashSet<CBonusSystemNode>());
            result.StackBonuses();

            queryCache[cacheKey] = result;
            return result;
        }

        // ── Private recursive collector ───────────────────────────────────────

        private void CollectBonusesRecursive(
            BonusList result,
            BonusSelector selector,
            HashSet<CBonusSystemNode> visited)
        {
            if (!visited.Add(this)) return;   // prevent infinite loops in diamond graphs

            // Collect local bonuses first.
            foreach (var b in localBonuses)
            {
                if (!selector.Matches(b)) continue;

                // If the bonus has a limiter, ask it whether to accept.
                if (b.Limiter != null)
                {
                    var ctx = new BonusLimitationContext { Bonus = b, Source = this };
                    var decision = b.Limiter.Limit(ctx);
                    if (decision != LimiterDecision.ACCEPT) continue;
                }

                // If the bonus has an updater, let it recompute the value.
                if (b.Updater != null)
                {
                    var updated = b.Updater.CreateUpdatedBonus(b, this);
                    if (updated != null) result.Add(updated);
                }
                else
                {
                    result.Add(b);
                }
            }

            // Walk parent chain.
            foreach (var parent in parents)
                parent.CollectBonusesRecursive(result, selector, visited);
        }

        // ── Cache key ─────────────────────────────────────────────────────────

        /// <summary>
        /// Packs (treeVersion, selector.GetHashCode) into a long cache key.
        /// The treeVersion component ensures stale results are never returned.
        /// </summary>
        private long BuildCacheKey(BonusSelector selector)
        {
            // High 32 bits = tree version, low 32 bits = selector identity hash.
            return ((long)(uint)treeVersion << 32) | (uint)selector.GetHashCode();
        }

        // ── Version propagation ───────────────────────────────────────────────

        /// <summary>
        /// Bumps this node's tree version and propagates the change upward to all
        /// ancestor nodes, invalidating their caches too.
        ///
        /// Corresponds to VCMI CBonusSystemNode::nodeHasChanged().
        /// </summary>
        protected void NodeHasChanged()
        {
            // Traverse upward with BFS to avoid redundant revisits.
            var queue   = new Queue<CBonusSystemNode>();
            var visited = new HashSet<CBonusSystemNode>();

            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (!visited.Add(node)) continue;

                node.treeVersion++;
                node.queryCache.Clear();    // invalidate cached queries on this node

                foreach (var p in node.parents)
                    queue.Enqueue(p);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  IBonusBearer helper query API
        //  (mirrors VCMI IBonusBearer non-virtual helpers)
        // ══════════════════════════════════════════════════════════════════════

        // ── Raw value queries ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the total value of all bonuses matching <paramref name="selector"/>,
        /// starting from <paramref name="baseValue"/>.
        /// </summary>
        public int ValOfBonuses(BonusSelector selector, int baseValue = 0)
            => GetAllBonuses(selector).TotalValue(baseValue);

        /// <summary>
        /// Returns the total value of all bonuses of <paramref name="type"/> with
        /// any subtype.
        /// </summary>
        public int ValOfBonuses(BonusType type, int baseValue = 0)
            => ValOfBonuses(Selector.ByType(type), baseValue);

        /// <summary>
        /// Returns the total value of all bonuses of <paramref name="type"/> with
        /// the specified <paramref name="subtype"/>.
        /// </summary>
        public int ValOfBonuses(BonusType type, int subtype, int baseValue = 0)
            => ValOfBonuses(Selector.ByTypeAndSubtype(type, subtype), baseValue);

        // ── Existence checks ──────────────────────────────────────────────────

        /// <summary>Returns true if any bonus matches <paramref name="selector"/>.</summary>
        public bool HasBonus(BonusSelector selector)
            => GetAllBonuses(selector).Count > 0;

        /// <summary>Returns true if any bonus of the given type exists.</summary>
        public bool HasBonusOfType(BonusType type)
            => HasBonus(Selector.ByType(type));

        /// <summary>Returns true if any bonus of the given type + subtype exists.</summary>
        public bool HasBonusOfType(BonusType type, int subtype)
            => HasBonus(Selector.ByTypeAndSubtype(type, subtype));

        /// <summary>Returns true if any bonus from <paramref name="source"/> exists.</summary>
        public bool HasBonusFrom(BonusSource source)
            => HasBonus(Selector.BySource(source));

        /// <summary>Returns true if any bonus from a specific source object exists.</summary>
        public bool HasBonusFrom(BonusSource source, int sourceId)
            => HasBonus(Selector.BySourceAndId(source, sourceId));

        // ── Filtered list retrieval ───────────────────────────────────────────

        /// <summary>Returns all bonuses originating from <paramref name="source"/>.</summary>
        public BonusList GetBonusesFrom(BonusSource source)
            => GetAllBonuses(Selector.BySource(source));

        /// <summary>
        /// Returns all bonuses of <paramref name="type"/> (any subtype).
        /// </summary>
        public BonusList GetBonusesOfType(BonusType type)
            => GetAllBonuses(Selector.ByType(type));

        /// <summary>
        /// Returns all bonuses of <paramref name="type"/> with <paramref name="subtype"/>.
        /// </summary>
        public BonusList GetBonusesOfType(BonusType type, int subtype)
            => GetAllBonuses(Selector.ByTypeAndSubtype(type, subtype));

        /// <summary>
        /// Returns the first bonus matching <paramref name="selector"/>, or null.
        /// </summary>
        public Bonus GetFirstBonus(BonusSelector selector)
            => GetAllBonuses(selector).GetFirst(selector);

        // ── Primary skill convenience ─────────────────────────────────────────

        /// <summary>
        /// Returns the total value of PRIMARY_SKILL bonuses for <paramref name="skill"/>
        /// starting from <paramref name="baseValue"/>.
        ///
        /// This is the main method consumers should use for attack/defense/power/knowledge.
        /// </summary>
        public int GetPrimarySkillBonus(EPrimarySkill skill, int baseValue = 0)
            => ValOfBonuses(BonusType.PRIMARY_SKILL, (int)skill, baseValue);

        /// <summary>
        /// Returns the total value of MOVEMENT bonuses (flat), with no base.
        /// </summary>
        public int GetMovementBonus()
            => ValOfBonuses(BonusType.MOVEMENT);

        /// <summary>
        /// Returns the summed PERCENT_TO_BASE MOVEMENT bonus (e.g. from Logistics).
        /// </summary>
        public int GetMovementPercentBonus()
            => GetAllBonuses(
                    Selector.ByType(BonusType.MOVEMENT)
                            .And(Selector.ByValType(BonusValueType.PERCENT_TO_BASE)))
               .TotalValue();

        // ── Morale / Luck ─────────────────────────────────────────────────────

        /// <summary>Returns cumulative morale modifier.</summary>
        public int GetMoraleBonus() => ValOfBonuses(BonusType.MORALE);

        /// <summary>Returns cumulative luck modifier.</summary>
        public int GetLuckBonus() => ValOfBonuses(BonusType.LUCK);
    }
}
