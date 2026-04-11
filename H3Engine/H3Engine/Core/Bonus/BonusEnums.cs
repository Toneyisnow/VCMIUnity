// Migrated from VCMI lib/bonuses/BonusEnum.h
// All bonus-system enumerations in one file so consumers can import a single namespace.

using System;

namespace H3Engine.Core.Bonus
{
    // ======================================================================
    //  BonusType
    // ======================================================================

    /// <summary>
    /// What property a bonus affects.
    /// Corresponds to VCMI BonusType.
    ///
    /// Subtypes (the Bonus.Subtype field) are context-sensitive:
    ///   PRIMARY_SKILL     → (int)EPrimarySkill
    ///   SECONDARY_SKILL_PREMY → (int)ESecondarySkill
    ///   GENERATE_RESOURCE → (int)EResourceType
    ///   SPELL_IMMUNITY    → (int)ESpellId
    ///   HATE              → (int)ECreatureId  (creature hated for extra damage)
    ///   CREATURE_DAMAGE   → 0 = min, 1 = max, 2 = both
    ///   All others        → -1 (unused / N/A)
    /// </summary>
    public enum BonusType
    {
        NONE = 0,

        // ── Movement ─────────────────────────────────────────────────────
        /// <summary>Hero land movement points (flat or percent).</summary>
        MOVEMENT = 1,
        /// <summary>Hero sea / flying movement points.</summary>
        FLYING_MOVEMENT = 2,
        /// <summary>Hero can walk on water tiles.</summary>
        WATER_WALKING = 3,
        /// <summary>Creature: has flying movement in battle.</summary>
        FLYING = 4,

        // ── Primary & secondary skills ────────────────────────────────────
        /// <summary>
        /// Modifier to a primary skill.
        /// Subtype = (int)EPrimarySkill  (ATTACK / DEFENSE / SPELL_POWER / KNOWLEDGE).
        /// </summary>
        PRIMARY_SKILL = 5,
        /// <summary>
        /// Increases effective secondary-skill level for one skill.
        /// Subtype = (int)ESecondarySkill.
        /// </summary>
        SECONDARY_SKILL_PREMY = 6,

        // ── Creature combat stats ─────────────────────────────────────────
        /// <summary>Creature speed (initiative / movement in battle).</summary>
        STACKS_SPEED = 7,
        /// <summary>Creature base damage. Subtype: 0 = min, 1 = max, 2 = both.</summary>
        CREATURE_DAMAGE = 8,
        /// <summary>Creature max hit-points.</summary>
        CREATURE_HP = 9,
        /// <summary>Number of ranged shots available.</summary>
        SHOTS = 10,
        /// <summary>Weekly creature growth (flat).</summary>
        CREATURE_GROWTH = 11,
        /// <summary>Weekly creature growth (percent).</summary>
        CREATURE_GROWTH_PERCENT = 12,

        // ── Attack & defence modifiers ────────────────────────────────────
        /// <summary>Reduces all incoming attack stat (percent).</summary>
        GENERAL_ATTACK_REDUCTION = 13,
        /// <summary>Reduces all incoming damage (percent).</summary>
        GENERAL_DAMAGE_REDUCTION = 14,
        /// <summary>Boosts damage dealt (percent).</summary>
        PERCENTAGE_DAMAGE_BOOST = 15,

        // ── Morale / Luck ─────────────────────────────────────────────────
        MORALE = 16,
        LUCK   = 17,

        // ── Spell-related ─────────────────────────────────────────────────
        /// <summary>% boost to spell damage (all schools or one; subtype = school or -1).</summary>
        SPELL_DAMAGE = 18,
        /// <summary>Extra turns added to spell duration.</summary>
        SPELL_DURATION = 19,
        /// <summary>Creature/hero is immune to a specific spell. Subtype = spell ID.</summary>
        SPELL_IMMUNITY = 20,
        /// <summary>Immune to all spells of level ≤ subtype.</summary>
        LEVEL_SPELL_IMMUNITY = 21,
        /// <summary>Blocks casting of spells whose level > subtype.</summary>
        BLOCK_MAGIC_ABOVE = 22,
        /// <summary>Percent chance to fully resist an incoming spell.</summary>
        MAGIC_RESISTANCE = 23,
        /// <summary>Percent chance to reflect a spell back at the caster.</summary>
        MAGIC_MIRROR = 24,
        /// <summary>Complete immunity to mind-affecting spells.</summary>
        MIND_IMMUNITY = 25,
        FIRE_IMMUNITY  = 26,
        WATER_IMMUNITY = 27,
        EARTH_IMMUNITY = 28,
        AIR_IMMUNITY   = 29,
        /// <summary>Negates all natural immunities of the target.</summary>
        NEGATE_ALL_NATURAL_IMMUNITIES = 30,

        // ── Creature special abilities ────────────────────────────────────
        /// <summary>Creature can perform ranged attacks.</summary>
        SHOOTER = 31,
        /// <summary>Creature can attack walls (catapult).</summary>
        CATAPULT = 32,
        /// <summary>Creature retaliates against every attack (no limit).</summary>
        UNLIMITED_RETALIATIONS = 33,
        /// <summary>Creature gets an additional attack this round.</summary>
        ADDITIONAL_ATTACK = 34,
        /// <summary>+5% damage per hex moved before attacking (Cavalier / Champion).</summary>
        JOUSTING = 35,
        /// <summary>Attack hits 3 hexes simultaneously (Hydra).</summary>
        THREE_HEADED_ATTACK = 36,
        /// <summary>Attack hits all adjacent enemies.</summary>
        ATTACKS_ALL_ADJACENT = 37,
        /// <summary>Immune to charge/jousting bonus damage.</summary>
        CHARGE_IMMUNITY = 38,
        /// <summary>Percent chance to deal double damage on an attack.</summary>
        DOUBLE_DAMAGE_CHANCE = 39,

        // ── HP & Regeneration ─────────────────────────────────────────────
        /// <summary>Regenerates HP at the start of each round.</summary>
        HP_REGENERATION = 40,
        /// <summary>Fully restores HP at the start of each round (Vampire Lords, etc.).</summary>
        FULL_HP_REGENERATION = 41,

        // ── Creature traits ───────────────────────────────────────────────
        UNDEAD      = 42,
        NON_LIVING  = 43,
        /// <summary>War machine / siege weapon flag.</summary>
        SIEGE_WEAPON = 44,
        /// <summary>King flag tier 1: creatures with this deal 50% extra damage to it.</summary>
        KING1 = 45,
        KING2 = 46,
        KING3 = 47,
        /// <summary>Vampire: drain HP equal to damage dealt.</summary>
        LIFE_DRAIN = 48,
        /// <summary>Kills creatures outright based on caster level (Power Lich / Commander).</summary>
        DEATH_STARE = 49,
        POISON = 50,
        ACID_BREATH_DEFENSE = 51,
        ACID_BREATH_DAMAGE  = 52,

        // ── Battle-field / range modifiers ────────────────────────────────
        /// <summary>No penalty for long-range shooting.</summary>
        NO_DISTANCE_PENALTY = 53,
        /// <summary>Ranged unit takes no melee penalty when engaged.</summary>
        NO_MELEE_PENALTY = 54,
        /// <summary>No penalty for shooting through obstacles.</summary>
        NO_OBSTACLES_PENALTY = 55,
        /// <summary>Attack does not trigger retaliation.</summary>
        BLOCKS_RETALIATION = 56,
        /// <summary>Dragon-breath: attack hits two hexes in a line.</summary>
        TWO_HEX_ATTACK_BREATH = 57,
        /// <summary>Binding attack: immobilises the target.</summary>
        BIND_EFFECT = 58,
        FEAR    = 59,
        FEARLESS = 60,

        // ── Hero-specific ─────────────────────────────────────────────────
        /// <summary>Radius (in tiles) the hero can see through fog of war.</summary>
        SIGHT_RANGE = 61,
        /// <summary>Reduces surrender cost (percent).</summary>
        SURRENDER_DISCOUNT = 62,
        /// <summary>Hero regains this percent of max mana per day.</summary>
        MANA_CHANNELING = 63,
        /// <summary>Hero regains this many mana points per day (flat).</summary>
        MANA_REGENERATION = 64,
        /// <summary>Level of skeletons raised by Necromancy (0 = Walking Dead, 1 = Skeleton).</summary>
        NECROMANCER_RESURRECTION_LEVEL = 65,
        /// <summary>Eagle Eye: percent chance to learn a battle spell.</summary>
        LEARN_BATTLE_SPELL_CHANCE = 66,
        /// <summary>Eagle Eye: maximum spell level that can be learned.</summary>
        LEARN_BATTLE_SPELL_LEVEL_LIMIT = 67,
        /// <summary>Prevents the defeated enemy from retreating.</summary>
        ENEMY_CANT_ESCAPE = 68,
        /// <summary>Percent bonus to experience from battles.</summary>
        BONUS_EXPERIENCE_POINTS = 69,
        /// <summary>No movement-point penalty when boarding / disembarking a ship.</summary>
        FREE_SHIP_BOARDING = 70,

        // ── Diplomacy ─────────────────────────────────────────────────────
        /// <summary>Wandering creatures will join the hero's army for free.</summary>
        CREATURE_JOINS_FOR_FREE = 71,

        // ── Resources / economy ───────────────────────────────────────────
        /// <summary>Hero generates N of this resource per day. Subtype = (int)EResourceType.</summary>
        GENERATE_RESOURCE = 72,

        // ── Special attack variants ───────────────────────────────────────
        ADDITIONAL_RETALIATION = 73,
        /// <summary>Unit is always under a specific spell effect. Subtype = spell ID.</summary>
        ENCHANTED = 74,
        /// <summary>Creature can cast a spell. Subtype = spell ID.</summary>
        SPELLCASTER = 75,
        /// <summary>Unit is reborn with some HP after being killed (Phoenix).</summary>
        REBIRTH = 76,

        // ── Morale / luck immunity ────────────────────────────────────────
        BLOCK_MORALE = 77,
        BLOCK_LUCK   = 78,

        // ── Damage vs specific targets ────────────────────────────────────
        /// <summary>+50% damage against a specific creature. Subtype = (int)ECreatureId.</summary>
        HATE   = 79,
        /// <summary>Slayer spell: bonus damage against King-flagged units.</summary>
        SLAYER = 80,

        // ── Misc ──────────────────────────────────────────────────────────
        DISGUISED   = 81,
        VISIONS     = 82,
        HYPNOTIZE   = 83,
        TRANSMUTATION = 84,
        SOUL_STEAL  = 85,

        // ── Necromancy ─────────────────────────────────────────────────────────
        /// <summary>Percent of killed non-undead units raised as undead (Necromancy skill).</summary>
        UNDEAD_RAISE_PERCENTAGE = 86,

        // ── Ranged combat ──────────────────────────────────────────────────────
        /// <summary>Ranged unit ignores wall obstacles when shooting.</summary>
        NO_WALL_PENALTY = 87,
        /// <summary>Ranged unit can shoot even when engaged in melee without penalty.</summary>
        FREE_SHOOTING = 88,

        // ── Battle-start spells ────────────────────────────────────────────────
        /// <summary>Casts a spell at battle start. Subtype = spell ID, Val = spell level.</summary>
        OPENING_BATTLE_SPELL = 89,

        // ── Spell-book bonuses ─────────────────────────────────────────────────
        /// <summary>Hero knows all spells of a school. Subtype = (int)ESpellSchool.</summary>
        SPELLS_OF_SCHOOL = 90,
        /// <summary>Hero knows all spells up to a level. Subtype = max spell level.</summary>
        SPELLS_OF_LEVEL = 91,

        // ── Mana ───────────────────────────────────────────────────────────────
        /// <summary>Regenerates Val% of hero's max mana per day (Wizard's Well).</summary>
        MANA_PERCENTAGE_REGENERATION = 92,

        // ── Adventure-map specials ─────────────────────────────────────────────
        /// <summary>Hero and army are immune to whirlpool damage.</summary>
        WHIRLPOOL_PROTECTION = 93,
        /// <summary>Enemy hero cannot retreat from this battle.</summary>
        BATTLE_NO_FLEEING = 94,
        /// <summary>Blocks all magic for both sides in battle (Orb of Inhibition).</summary>
        BLOCK_ALL_MAGIC = 95,

        // ── Necromancy upgrades ────────────────────────────────────────────────
        /// <summary>Necromancy raises this creature type instead of skeletons. Subtype = ECreatureId.</summary>
        IMPROVED_NECROMANCY = 96,

        // ── Alignment ─────────────────────────────────────────────────────────
        /// <summary>Army ignores mixed-alignment morale penalty (Angelic Alliance).</summary>
        NONEVIL_ALIGNMENT_MIX = 97,

        // ── Resistance aura ────────────────────────────────────────────────────
        /// <summary>Reduces all units' magic resistance in the battle.</summary>
        SPELL_RESISTANCE_AURA = 98,

        // Sentinel
        COUNT = 99,
    }


    // ======================================================================
    //  BonusSource
    // ======================================================================

    /// <summary>
    /// The category of game entity that is the origin of a bonus.
    /// Corresponds to VCMI BonusSource.
    /// </summary>
    public enum BonusSource
    {
        /// <summary>The bonus comes from an artifact type definition.</summary>
        ARTIFACT          = 0,
        /// <summary>The bonus comes from a specific artifact instance worn by a hero.</summary>
        ARTIFACT_INSTANCE = 1,
        /// <summary>Bonus from a generic adventure-map object type.</summary>
        OBJECT_TYPE       = 2,
        /// <summary>Bonus from a specific adventure-map object instance.</summary>
        OBJECT_INSTANCE   = 3,
        /// <summary>Innate creature ability.</summary>
        CREATURE_ABILITY  = 4,
        /// <summary>Native terrain bonus for this creature / hero type.</summary>
        TERRAIN_NATIVE    = 5,
        /// <summary>Terrain overlay effect (e.g. Magic Plains, Holy Ground).</summary>
        TERRAIN_OVERLAY   = 6,
        /// <summary>An active spell effect on the unit or hero.</summary>
        SPELL_EFFECT      = 7,
        /// <summary>A town structure benefit.</summary>
        TOWN_STRUCTURE    = 8,
        /// <summary>Hero base primary-skill value (not a bonus per se, but modelled as one).</summary>
        HERO_BASE_SKILL   = 9,
        /// <summary>Secondary skill benefit.</summary>
        SECONDARY_SKILL   = 10,
        /// <summary>Hero specialty bonus.</summary>
        HERO_SPECIAL      = 11,
        /// <summary>Army-wide bonus.</summary>
        ARMY              = 12,
        /// <summary>Campaign starting bonus.</summary>
        CAMPAIGN_BONUS    = 13,
        /// <summary>WoG / HotA stack experience.</summary>
        STACK_EXPERIENCE  = 14,
        /// <summary>Commander unit bonus.</summary>
        COMMANDER         = 15,
        /// <summary>Global game-settings bonus.</summary>
        GLOBAL            = 16,
        /// <summary>Miscellaneous / fallback.</summary>
        OTHER             = 17,
    }


    // ======================================================================
    //  BonusValueType
    // ======================================================================

    /// <summary>
    /// How a bonus value is combined with the running total.
    /// Corresponds to VCMI BonusValueType.
    ///
    /// The full evaluation order (BonusList.TotalValue):
    ///   base  = baseValue
    ///         + Σ BASE_NUMBER
    ///         + Σ ADDITIVE_VALUE  (each adjusted by PERCENT_TO_SOURCE / PERCENT_TO_TARGET_TYPE)
    ///   base  = base * (100 + Σ PERCENT_TO_BASE) / 100
    ///   total = base * (100 + Σ PERCENT_TO_ALL) / 100
    ///   total = clamp(total, INDEPENDENT_MAX, INDEPENDENT_MIN)
    /// </summary>
    public enum BonusValueType
    {
        /// <summary>Added directly to the running total after base-percentage adjustments.</summary>
        ADDITIVE_VALUE         = 0,
        /// <summary>Added to the base before any percentage modifiers are applied.</summary>
        BASE_NUMBER            = 1,
        /// <summary>Percentage applied to the fully computed total (after PERCENT_TO_BASE).</summary>
        PERCENT_TO_ALL         = 2,
        /// <summary>Percentage applied only to the base (before PERCENT_TO_ALL).</summary>
        PERCENT_TO_BASE        = 3,
        /// <summary>
        /// Percentage applied only to ADDITIVE bonuses that share the same BonusSource
        /// as this modifier.
        /// </summary>
        PERCENT_TO_SOURCE      = 4,
        /// <summary>
        /// Like PERCENT_TO_SOURCE but targets bonuses whose source matches
        /// <see cref="Bonus.TargetSourceType"/> instead.
        /// </summary>
        PERCENT_TO_TARGET_TYPE = 5,
        /// <summary>Final result must be at least this value (soft minimum / floor).</summary>
        INDEPENDENT_MAX        = 6,
        /// <summary>Final result must be at most this value (soft maximum / cap).</summary>
        INDEPENDENT_MIN        = 7,
    }


    // ======================================================================
    //  BonusDuration
    // ======================================================================

    /// <summary>
    /// How long a bonus lasts. Stored as bit-flags so multiple conditions can coexist.
    /// Corresponds to VCMI BonusDuration.
    /// </summary>
    [Flags]
    public enum BonusDuration
    {
        /// <summary>Lasts forever (most artifact / creature bonuses).</summary>
        PERMANENT            = 1 << 0,
        /// <summary>Removed at the end of the current battle.</summary>
        ONE_BATTLE           = 1 << 1,
        /// <summary>Removed after one adventure-map day.</summary>
        ONE_DAY              = 1 << 2,
        /// <summary>Removed after one adventure-map week.</summary>
        ONE_WEEK             = 1 << 3,
        /// <summary>Lasts exactly N battle turns (see <see cref="Bonus.TurnsRemain"/>).</summary>
        N_TURNS              = 1 << 4,
        /// <summary>Lasts exactly N adventure-map days.</summary>
        N_DAYS               = 1 << 5,
        /// <summary>Removed the next time the bearer is attacked.</summary>
        UNTIL_BEING_ATTACKED = 1 << 6,
        /// <summary>Removed the next time the bearer attacks.</summary>
        UNTIL_ATTACK         = 1 << 7,
        /// <summary>Removed when the stack gets its next turn.</summary>
        STACK_GETS_TURN      = 1 << 8,
        /// <summary>Removed when the commander is killed.</summary>
        COMMANDER_KILLED     = 1 << 9,
        /// <summary>Removed after the bearer performs its own attack (not retaliation).</summary>
        UNTIL_OWN_ATTACK     = 1 << 10,
    }


    // ======================================================================
    //  BonusLimitEffect
    // ======================================================================

    /// <summary>
    /// Restricts a bonus to one combat mode.
    /// Corresponds to VCMI BonusLimitEffect.
    /// </summary>
    public enum BonusLimitEffect
    {
        /// <summary>Bonus applies in all circumstances.</summary>
        NO_LIMIT            = 0,
        /// <summary>Bonus applies only in ranged (distance) combat.</summary>
        ONLY_DISTANCE_FIGHT = 1,
        /// <summary>Bonus applies only in melee combat.</summary>
        ONLY_MELEE_FIGHT    = 2,
    }


    // ======================================================================
    //  BonusNodeType
    // ======================================================================

    /// <summary>
    /// Semantic type of a node in the bonus-system tree.
    /// Used by propagators to route bonus inheritance correctly.
    /// Corresponds to VCMI BonusNodeType.
    /// </summary>
    public enum BonusNodeType
    {
        NONE              = 0,
        UNKNOWN           = 1,
        /// <summary>A creature stack instance.</summary>
        STACK_INSTANCE    = 2,
        /// <summary>A creature stack in its battle-context (ephemeral).</summary>
        STACK_BATTLE      = 3,
        /// <summary>The hero's complete army.</summary>
        ARMY              = 4,
        /// <summary>An artifact type-definition node.</summary>
        ARTIFACT          = 5,
        /// <summary>A creature type-definition node.</summary>
        CREATURE          = 6,
        /// <summary>A worn artifact instance node (attaches to HERO).</summary>
        ARTIFACT_INSTANCE = 7,
        /// <summary>A hero instance node.</summary>
        HERO              = 8,
        /// <summary>A player-level node.</summary>
        PLAYER            = 9,
        /// <summary>A team-level node.</summary>
        TEAM              = 10,
        /// <summary>Town + visiting hero composite node.</summary>
        TOWN_AND_VISITOR  = 11,
        /// <summary>Battle-wide effect node.</summary>
        BATTLE_WIDE       = 12,
        /// <summary>Commander node.</summary>
        COMMANDER         = 13,
        /// <summary>Global game-effects node.</summary>
        GLOBAL_EFFECTS    = 14,
    }
}
