// Migrated from H3Engine/Core/H3Hero.cs
// H3HeroId and H3Hero moved here from HeroClass.cs.
//
// Key changes in this version:
//   • H3Hero now extends CBonusSystemNode (NodeType = HERO).
//   • EquipArtifact / UnequipArtifact wire artifact-instance bonus nodes
//     into the hero's bonus tree so their bonuses are automatically included
//     in all GetAllBonuses / ValOfBonuses queries.
//   • GetEffectivePrimarySkill replaces the TODO placeholders with a real
//     bonus-system query.
//   • GetEffectiveMovePoint is fully implemented (army speed + Logistics +
//     artifact flat bonuses + artifact percent bonuses).

using H3Engine.Common;
using H3Engine.Core.Bonus;
using System;
using System.Collections.Generic;
using H3Engine.Core.Constants;
using BonusItem = H3Engine.Core.Bonus.Bonus;

namespace H3Engine.Core
{
    public class H3HeroId
    {
        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }
    }


    /// <summary>
    /// Runtime state of a single hero instance loaded from an h3m map or game save.
    ///
    /// Static type data (name, biography, initial army, specialty template, …) lives in
    /// <see cref="HeroType"/>; this class holds the mutable per-instance values.
    ///
    /// Extends <see cref="CBonusSystemNode"/> (NodeType = HERO) so that:
    ///   • Artifact instances can attach their bonus subtrees via AttachTo(hero).
    ///   • All stat queries (primary skills, movement, morale, luck…) automatically
    ///     aggregate bonuses from the full tree.
    ///
    /// Corresponds to VCMI's CGHeroInstance data fields.
    /// </summary>
    public class H3Hero : CBonusSystemNode
    {
        public static H3Hero InitializeFromTemplate(int templateId)
        {
            return null;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Identity
        // ══════════════════════════════════════════════════════════════════════

        public string Identifier
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public int PortraitIndex
        {
            get; set;
        }

        public string Biography
        {
            get; set;
        }

        /// <summary>
        /// Instance gender.  EHeroGender.DEFAULT means "use hero type's gender".
        /// Corresponds to CGHeroInstance::gender in VCMI.
        /// </summary>
        public EHeroGender Gender
        {
            get; set;
        } = EHeroGender.DEFAULT;

        // ══════════════════════════════════════════════════════════════════════
        //  Progression
        // ══════════════════════════════════════════════════════════════════════

        public long Experience
        {
            get; set;
        }

        public int Level
        {
            get; set;
        }

        public int MaxExperience
        {
            get; set;
        }

        public int MaxLevel
        {
            get; set;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Skills
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Base primary skill values.  Always 4 entries in order:
        /// [0] Attack, [1] Defence, [2] Power, [3] Knowledge.
        /// Use <see cref="GetEffectivePrimarySkill"/> to get the final value
        /// including artifact and specialty bonuses.
        /// </summary>
        public List<int> PrimarySkills
        {
            get; set;
        }

        /// <summary>
        /// Key: Skill Id  Value: Level of Skill (1 = basic, 2 = adv., 3 = expert).
        /// </summary>
        public List<AbilitySkill> SecondarySkills
        {
            get; set;
        }

        public List<ESpellId> Spells
        {
            get; set;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Equipment / army
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Slot layout (which artifact ID sits in which position).</summary>
        public ArtifactSet Artifacts
        {
            get; set;
        }

        /// <summary>
        /// Maps each equipped position to the live ArtifactInstance whose bonus
        /// node has been attached to this hero's bonus tree.
        /// Populated by <see cref="EquipArtifact"/>, cleared by
        /// <see cref="UnequipArtifact"/>.
        /// </summary>
        private readonly Dictionary<EArtifactPosition, ArtifactInstance> equippedInstances
            = new Dictionary<EArtifactPosition, ArtifactInstance>();

        public HeroSpecialty Specialty
        {
            get; set;
        }

        public CreatureSet Army
        {
            get; set;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Constructor
        // ══════════════════════════════════════════════════════════════════════

        public H3Hero()
        {
            NodeType = BonusNodeType.HERO;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Artifact equip / unequip
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Equips <paramref name="instance"/> at <paramref name="pos"/> on this hero:
        ///   1. Writes the slot in <see cref="Artifacts"/>.
        ///   2. Attaches the instance's bonus node to this hero node so its bonuses
        ///      become part of all subsequent hero stat queries.
        ///
        /// Any previously equipped instance at <paramref name="pos"/> is unequipped
        /// first (moved to backpack).
        ///
        /// Corresponds to VCMI CGHeroInstance::putArtifact / CArtifactSet::putArtifact.
        /// </summary>
        public void EquipArtifact(ArtifactInstance instance, EArtifactPosition pos)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (Artifacts == null) Artifacts = new ArtifactSet();

            // Unequip whatever is currently in that slot.
            if (equippedInstances.ContainsKey(pos))
                UnequipArtifact(pos);

            // Write into the slot layout.
            Artifacts.PutAt(instance.TypeId, pos, instance.InstanceId);

            // Remember the live instance and wire its bonus node to this hero.
            equippedInstances[pos] = instance;
            instance.AttachTo(this);
        }

        /// <summary>
        /// Unequips the artifact at <paramref name="pos"/>:
        ///   1. Detaches the instance's bonus node from this hero.
        ///   2. Removes the slot entry from <see cref="Artifacts"/>.
        ///
        /// Corresponds to VCMI CGHeroInstance::removeArtifact.
        /// </summary>
        public void UnequipArtifact(EArtifactPosition pos)
        {
            if (equippedInstances.TryGetValue(pos, out var inst))
            {
                inst.DetachFrom(this);
                equippedInstances.Remove(pos);
            }
            Artifacts?.RemoveFrom(pos);
        }

        /// <summary>
        /// Returns the live <see cref="ArtifactInstance"/> equipped at
        /// <paramref name="pos"/>, or null if the slot is empty.
        /// </summary>
        public ArtifactInstance GetEquippedArtifact(EArtifactPosition pos)
        {
            equippedInstances.TryGetValue(pos, out var inst);
            return inst;
        }

        /// <summary>
        /// Returns all currently equipped artifact instances.
        /// </summary>
        public IEnumerable<ArtifactInstance> GetAllEquippedArtifacts()
            => equippedInstances.Values;

        // ══════════════════════════════════════════════════════════════════════
        //  Primary skill queries
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the effective value of <paramref name="skill"/> after applying
        /// all active bonuses (artifacts, specialties, spell effects …).
        ///
        /// Uses the bonus tree: base value is taken from <see cref="PrimarySkills"/>
        /// and all PRIMARY_SKILL bonuses matching the subtype are aggregated via
        /// <see cref="CBonusSystemNode.GetPrimarySkillBonus"/>.
        ///
        /// Corresponds to VCMI CGHeroInstance::getPrimSkillLevel.
        /// </summary>
        public int GetEffectivePrimarySkill(EPrimarySkill skill)
        {
            int baseValue = 0;
            if (PrimarySkills != null && PrimarySkills.Count > (int)skill)
                baseValue = PrimarySkills[(int)skill];

            return GetPrimarySkillBonus(skill, baseValue);
        }

        /// <summary>Effective Attack skill (base + all bonuses).</summary>
        public int GetEffectiveAttack()    => GetEffectivePrimarySkill(EPrimarySkill.ATTACK);
        /// <summary>Effective Defense skill (base + all bonuses).</summary>
        public int GetEffectiveDefense()   => GetEffectivePrimarySkill(EPrimarySkill.DEFENSE);
        /// <summary>Effective Spell Power (base + all bonuses).</summary>
        public int GetEffectiveSpellPower()=> GetEffectivePrimarySkill(EPrimarySkill.SPELL_POWER);
        /// <summary>Effective Knowledge (base + all bonuses).</summary>
        public int GetEffectiveKnowledge() => GetEffectivePrimarySkill(EPrimarySkill.KNOWLEDGE);

        // ══════════════════════════════════════════════════════════════════════
        //  Movement
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Base movement points per turn.  Default 1500 (foot heroes).
        /// In H3 this depends on the slowest creature speed in army:
        ///   Speed 0-4: 1500, Speed 5: 1560, Speed 6: 1630, Speed 7: 1700,
        ///   Speed 8: 1760, Speed 9: 1830, Speed 10: 1900, Speed 11+: 2000.
        /// </summary>
        public int MovePoint { get; set; } = 1500;

        /// <summary>
        /// Base movement-point values indexed by lowest creature speed in hero's army.
        /// Source: VCMI config/gameConfig.json heroes.movementPoints.land
        /// </summary>
        private static readonly int[] MovePointsBySpeed = {
            1500, 1500, 1500, 1500, 1500,  // speed 0-4
            1560, 1630, 1700, 1760, 1830,  // speed 5-9
            1900, 2000                      // speed 10-11+
        };

        /// <summary>
        /// Calculates effective movement points after applying army speed,
        /// Logistics skill, and all MOVEMENT bonuses from the bonus tree
        /// (artifact bonuses, specialties, spell effects, …).
        ///
        /// Formula (mirrors VCMI TurnInfo.cpp):
        /// <code>
        ///   step 1 : baseMovePoints   = army-speed lookup
        ///   step 2 : logistics        = PERCENT_TO_BASE MOVEMENT bonuses (Logistics skill)
        ///   step 3 : baseMovePoints   = baseMovePoints * (100 + logistics%) / 100
        ///   step 4 : flatBonus        = ADDITIVE_VALUE  MOVEMENT bonuses (Boots of Speed etc.)
        ///   step 5 : effectivePoints  = baseMovePoints + flatBonus
        /// </code>
        ///
        /// Note: Logistics is modelled as a PERCENT_TO_BASE MOVEMENT bonus added by
        /// the secondary-skill system (see H3Hero constructor or skill-level-up code).
        /// Here we query the bonus tree for that percentage directly so the Logistics
        /// TODO is also resolved.
        ///
        /// Corresponds to VCMI TurnInfo::getMaxMovePoints / CGHeroInstance::movementPointsLimit.
        /// </summary>
        public int GetEffectiveMovePoint()
        {
            // ── Step 1: Base points from army speed ───────────────────────────
            int baseMovePoints = MovePoint;

            if (Army != null && Army.Stacks != null && Army.Stacks.Count > 0)
            {
                int lowestSpeed = int.MaxValue;
                foreach (var stack in Army.Stacks)
                {
                    if (stack.Creature != null && stack.Creature.Speed > 0
                        && stack.Creature.Speed < lowestSpeed)
                    {
                        lowestSpeed = stack.Creature.Speed;
                    }
                }

                if (lowestSpeed < int.MaxValue)
                {
                    int speedIdx = Math.Min(lowestSpeed, MovePointsBySpeed.Length - 1);
                    baseMovePoints = MovePointsBySpeed[speedIdx];
                }
            }

            // ── Step 2: Percent-to-base MOVEMENT bonuses (Logistics, etc.) ────
            // Query the bonus tree for all PERCENT_TO_BASE MOVEMENT bonuses.
            // Logistics secondary skill should register its bonus via AddNewBonus
            // when the skill level changes.
            int percentBonus = GetAllBonuses(
                    Selector.ByType(BonusType.MOVEMENT)
                            .And(Selector.ByValType(BonusValueType.PERCENT_TO_BASE)))
                .TotalValue();

            // ── Step 3: Apply percentage ──────────────────────────────────────
            int afterPercent = baseMovePoints * (100 + percentBonus) / 100;

            // ── Step 4: Flat MOVEMENT bonuses (Boots of Speed: +600, etc.) ────
            int flatBonus = GetAllBonuses(
                    Selector.ByType(BonusType.MOVEMENT)
                            .And(Selector.ByValType(BonusValueType.ADDITIVE_VALUE)))
                .TotalValue();

            // ── Step 5: Final result ──────────────────────────────────────────
            return afterPercent + flatBonus;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Secondary-skill bonus helpers
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Registers the movement bonus for the current Logistics skill level as a
        /// PERCENT_TO_BASE MOVEMENT bonus on this node.
        ///
        /// Call this whenever the hero's Logistics level changes (level-up, skill loss).
        /// Old Logistics bonuses are removed before adding the new one.
        ///
        /// Logistics: Basic +10%, Advanced +20%, Expert +30%.
        /// </summary>
        public void RefreshLogisticsBonus()
        {
            // Remove any existing Logistics movement bonus.
            RemoveBonuses(
                Selector.ByType(BonusType.MOVEMENT)
                        .And(Selector.BySource(BonusSource.SECONDARY_SKILL))
                        .And(Selector.BySourceAndId(
                                BonusSource.SECONDARY_SKILL,
                                (int)ESecondarySkill.LOGISTICS)));

            // Find current Logistics level.
            int percent = 0;
            if (SecondarySkills != null)
            {
                foreach (var skill in SecondarySkills)
                {
                    if (skill.Id != ESecondarySkill.LOGISTICS) continue;
                    switch (skill.Level)
                    {
                        case ESecondarySkillLevel.BASIC:    percent = 10; break;
                        case ESecondarySkillLevel.ADVANCED: percent = 20; break;
                        case ESecondarySkillLevel.EXPERT:   percent = 30; break;
                    }
                    break;
                }
            }

            if (percent == 0) return;

            AddNewBonus(new BonusItem
            {
                Type     = BonusType.MOVEMENT,
                Subtype  = -1,
                Val      = percent,
                ValType  = BonusValueType.PERCENT_TO_BASE,
                Source   = BonusSource.SECONDARY_SKILL,
                SourceId = (int)ESecondarySkill.LOGISTICS,
                Duration = BonusDuration.PERMANENT,
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  Bonus node identity
        // ══════════════════════════════════════════════════════════════════════

        public override string NodeName
            => $"H3Hero({Name ?? Identifier ?? "?"})";
    }
}
