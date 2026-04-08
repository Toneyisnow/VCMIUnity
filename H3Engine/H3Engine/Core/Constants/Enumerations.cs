// Migrated from VCMI lib/constants and H3Engine.Common
// All game enumeration types consolidated in one location

using System;

namespace H3Engine.Core.Constants
{
    /// <summary>
    /// Alignment types for creatures and heroes.
    /// </summary>
    public enum EAlignment : sbyte
    {
        ANY = -1,
        GOOD = 0,
        EVIL = 1,
        NEUTRAL = 2
    }

    /// <summary>Market modes for trading.</summary>
    public enum EMarketMode : sbyte
    {
        RESOURCE_RESOURCE = 0,
        RESOURCE_PLAYER = 1,
        CREATURE_RESOURCE = 2,
        RESOURCE_ARTIFACT = 3,
        ARTIFACT_RESOURCE = 4,
        ARTIFACT_EXP = 5,
        CREATURE_EXP = 6,
        CREATURE_UNDEAD = 7,
        RESOURCE_SKILL = 8,
        MARKET_AFTER_LAST_PLACEHOLDER = 9
    }

    /// <summary>AI tactics.</summary>
    public enum EAiTactic : sbyte
    {
        NONE = -1,
        RANDOM = 0,
        WARRIOR = 1,
        BUILDER = 2,
        EXPLORER = 3
    }

    /// <summary>Building construction state.</summary>
    public enum EBuildingState : sbyte
    {
        HAVE_CAPITAL,
        NO_WATER,
        FORBIDDEN,
        ADD_MAGES_GUILD,
        ALREADY_PRESENT,
        CANT_BUILD_TODAY,
        NO_RESOURCES,
        ALLOWED,
        PREREQUIRES,
        MISSING_BASE,
        BUILDING_ERROR,
        TOWN_NOT_OWNED
    }

    /// <summary>Spell casting problems.</summary>
    public enum ESpellCastProblem : sbyte
    {
        OK = 0,
        NO_HERO_TO_CAST_SPELL = 1,
        CASTS_PER_TURN_LIMIT = 2,
        NO_SPELLBOOK = 3,
        HERO_DOESNT_KNOW_SPELL = 4,
        NOT_ENOUGH_MANA = 5,
        ADVMAP_SPELL_INSTEAD_OF_BATTLE_SPELL = 6,
        SPELL_LEVEL_LIMIT_EXCEEDED = 7,
        NO_SPELLS_TO_DISPEL = 8,
        NO_APPROPRIATE_TARGET = 9,
        STACK_IMMUNE_TO_SPELL = 10,
        WRONG_SPELL_TARGET = 11,
        ONGOING_TACTIC_PHASE = 12,
        MAGIC_IS_BLOCKED = 13, // Orb of Inhibition and similar
        INVALID = 14
    }

    /// <summary>Commander secondary skills.</summary>
    public static class ECommander
    {
        public enum SecondarySkills
        {
            ATTACK,
            DEFENSE,
            HEALTH,
            DAMAGE,
            SPEED,
            SPELL_POWER,
            CASTS,
            RESISTANCE
        }

        public const int MAX_SKILL_LEVEL = 5;
    }

    /// <summary>Town wall parts.</summary>
    public enum EWallPart : sbyte
    {
        INDESTRUCTIBLE_PART_OF_GATE = -3,
        INDESTRUCTIBLE_PART = -2,
        INVALID = -1,
        KEEP = 0,
        BOTTOM_TOWER = 1,
        BOTTOM_WALL = 2,
        BELOW_GATE = 3,
        OVER_GATE = 4,
        UPPER_WALL = 5,
        UPPER_TOWER = 6,
        GATE = 7,
        PARTS_COUNT = 8
    }

    /// <summary>Town wall state.</summary>
    public enum EWallState : sbyte
    {
        NONE = -1, // no wall
        DESTROYED = 0,
        DAMAGED = 1,
        INTACT = 2,
        REINFORCED = 3 // walls in towns with castle
    }

    /// <summary>Gate state.</summary>
    public enum EGateState : sbyte
    {
        NONE = 0,
        CLOSED = 1,
        BLOCKED = 2, // gate is blocked in closed state, e.g. by creature
        OPENED = 3,
        DESTROYED = 4
    }

    /// <summary>Tile placement state during pathfinding.</summary>
    public enum ETileType : sbyte
    {
        FREE = 0,
        POSSIBLE = 1,
        BLOCKED = 2,
        USED = 3
    }

    /// <summary>Teleport channel type.</summary>
    public enum ETeleportChannelType : sbyte
    {
        IMPASSABLE = 0,
        BIDIRECTIONAL = 1,
        UNIDIRECTIONAL = 2,
        MIXED = 3
    }

    /// <summary>Spell mastery levels.</summary>
    public static class MasteryLevel
    {
        public enum Type
        {
            NONE = 0,
            BASIC = 1,
            ADVANCED = 2,
            EXPERT = 3,
            LEVELS_SIZE = 4
        }
    }

    /// <summary>Date component types.</summary>
    public enum Date : sbyte
    {
        DAY = 0,
        DAY_OF_WEEK = 1,
        WEEK = 2,
        MONTH = 3,
        DAY_OF_MONTH = 4
    }

    /// <summary>Battle action types.</summary>
    public enum EActionType : sbyte
    {
        NO_ACTION = 0,
        END_TACTIC_PHASE = 1,
        RETREAT = 2,
        SURRENDER = 3,
        HERO_SPELL = 4,
        WALK = 5,
        WAIT = 6,
        DEFEND = 7,
        WALK_AND_ATTACK = 8,
        SHOOT = 9,
        CATAPULT = 10,
        MONSTER_SPELL = 11,
        BAD_MORALE = 12,
        STACK_HEAL = 13,
        WALK_AND_CAST = 14
    }

    /// <summary>Digging ability status.</summary>
    public enum EDiggingStatus : sbyte
    {
        UNKNOWN = -1,
        CAN_DIG = 0,
        LACK_OF_MOVEMENT = 1,
        WRONG_TERRAIN = 2,
        TILE_OCCUPIED = 3,
        BACKPACK_IS_FULL = 4
    }

    /// <summary>Player game status.</summary>
    public enum EPlayerStatus : sbyte
    {
        WRONG = -1,
        INGAME = 0,
        LOSER = 1,
        WINNER = 2
    }

    /// <summary>Player relations.</summary>
    public enum PlayerRelations : sbyte
    {
        ENEMIES = 0,
        ALLIES = 1,
        SAME_PLAYER = 2
    }

    /// <summary>Entity metaclass types.</summary>
    public enum EMetaclass : sbyte
    {
        INVALID = 0,
        ARTIFACT = 1,
        CREATURE = 2,
        FACTION = 3,
        EXPERIENCE = 4,
        HERO = 5,
        HEROCLASS = 6,
        LUCK = 7,
        MANA = 8,
        MORALE = 9,
        MOVEMENT = 10,
        OBJECT = 11,
        PRIMARY_SKILL = 12,
        SECONDARY_SKILL = 13,
        SPELL = 14,
        RESOURCE = 15
    }

    /// <summary>Healing level.</summary>
    public enum EHealLevel : sbyte
    {
        HEAL = 0,
        RESURRECT = 1,
        OVERHEAL = 2
    }

    /// <summary>Healing power.</summary>
    public enum EHealPower : sbyte
    {
        ONE_BATTLE = 0,
        PERMANENT = 1
    }

    /// <summary>Battle result type.</summary>
    public enum EBattleResult : sbyte
    {
        NORMAL = 0,
        ESCAPE = 1,
        SURRENDER = 2
    }

    /// <summary>Fog of war visibility.</summary>
    public enum ETileVisibility : sbyte
    {
        HIDDEN = 0,
        REVEALED = 1
    }

    /// <summary>Army formation.</summary>
    public enum EArmyFormation : sbyte
    {
        LOOSE = 0,
        TIGHT = 1
    }

    /// <summary>Movement mode.</summary>
    public enum EMovementMode : sbyte
    {
        STANDARD = 0,
        DIMENSION_DOOR = 1,
        MONOLITH = 2,
        CASTLE_GATE = 3,
        TOWN_PORTAL = 4
    }

    /// <summary>Map level.</summary>
    public enum EMapLevel : sbyte
    {
        ANY = -1,
        SURFACE = 0,
        UNDERGROUND = 1
    }

    /// <summary>Week type for creature growth.</summary>
    public enum EWeekType : sbyte
    {
        FIRST_WEEK = 0,
        NORMAL = 1,
        DOUBLE_GROWTH = 2,
        BONUS_GROWTH = 3,
        DEITYOFFIRE = 4,
        PLAGUE = 5
    }

    /// <summary>Color scheme.</summary>
    public enum ColorScheme : sbyte
    {
        NONE = 0,
        KEEP = 1,
        GRAYSCALE = 2,
        H2_SCHEME = 3
    }

    /// <summary>Value change mode.</summary>
    public enum ChangeValueMode : sbyte
    {
        RELATIVE = 0,
        ABSOLUTE = 1
    }

    /// <summary>Combat event type.</summary>
    public enum CombatEventType : sbyte
    {
        INVALID = 0,
        BEFORE_ATTACK = 1,
        AFTER_ATTACK = 2,
        BEFORE_ATTACKED = 3,
        AFTER_ATTACKED = 4,
        WAIT = 5,
        DEFEND = 6,
        BEFORE_MOVE = 7,
        AFTER_MOVE = 8,
        UNIT_SPELLCAST = 9
    }

    /// <summary>Building sub-ID types.</summary>
    public static class BuildingSubID
    {
        public enum EBuildingSubID
        {
            DEFAULT = -50,
            NONE = -1,
            CASTLE_GATE = 0,
            MYSTIC_POND = 1,
            LIBRARY = 2,
            PORTAL_OF_SUMMONING = 3,
            ESCAPE_TUNNEL = 4,
            TREASURY = 5,
            BANK = 6,
            AURORA_BOREALIS = 7
        }
    }

    /// <summary>Spell IDs from VCMI.</summary>
    public enum ESpellId : short
    {
        PRESET = -2,
        NONE = -1,
        SUMMON_BOAT, SCUTTLE_BOAT, VISIONS, VIEW_EARTH, DISGUISE, VIEW_AIR,
        FLY, WATER_WALK, DIMENSION_DOOR, TOWN_PORTAL,
        QUICKSAND, LAND_MINE, FORCE_FIELD, FIRE_WALL, EARTHQUAKE,
        MAGIC_ARROW, ICE_BOLT, LIGHTNING_BOLT, IMPLOSION,
        CHAIN_LIGHTNING, FROST_RING, FIREBALL, INFERNO,
        METEOR_SHOWER, DEATH_RIPPLE, DESTROY_UNDEAD, ARMAGEDDON,
        SHIELD, AIR_SHIELD, FIRE_SHIELD, PROTECTION_FROM_AIR,
        PROTECTION_FROM_FIRE, PROTECTION_FROM_WATER,
        PROTECTION_FROM_EARTH, ANTI_MAGIC, DISPEL, MAGIC_MIRROR,
        CURE, RESURRECTION, ANIMATE_DEAD, SACRIFICE, BLESS,
        CURSE, BLOODLUST, PRECISION, WEAKNESS, STONE_SKIN,
        DISRUPTING_RAY, PRAYER, MIRTH, SORROW, FORTUNE,
        MISFORTUNE, HASTE, SLOW, SLAYER, FRENZY,
        TITANS_LIGHTNING_BOLT, COUNTERSTRIKE, BERSERK, HYPNOTIZE,
        FORGETFULNESS, BLIND, TELEPORT, REMOVE_OBSTACLE, CLONE,
        SUMMON_FIRE_ELEMENTAL, SUMMON_EARTH_ELEMENTAL, SUMMON_WATER_ELEMENTAL, SUMMON_AIR_ELEMENTAL,
        STONE_GAZE, POISON, BIND, DISEASE, PARALYZE, AGE, DEATH_CLOUD, THUNDERBOLT,
        DISPEL_HELPFUL_SPELLS, DEATH_STARE, ACID_BREATH_DEFENSE, ACID_BREATH_DAMAGE
    }

    /// <summary>Spell schools.</summary>
    public enum ESpellSchool : sbyte
    {
        AIR = 0,
        FIRE = 1,
        WATER = 2,
        EARTH = 3
    }

    /// <summary>Resource types.</summary>
    public enum EResourceType : sbyte
    {
        GOLD = 0,
        WOOD = 1,
        ORE = 2,
        GEMS = 3,
        SULFUR = 4,
        CRYSTALS = 5,
        PEARLS = 6
    }

    /// <summary>Creature IDs.</summary>
    public enum ECreatureId : short
    {
        NONE = -1,
        PIKEMAN = 0, HALBERDIER = 1, ARCHER = 2, MARKSMAN = 3, GRIFFIN = 4, ROYAL_GRIFFIN = 5, 
        SWORDSMAN = 6, CRUSADER = 7, MONK = 8, ZEALOT = 9, CAVALIER = 10, CHAMPION = 11, 
        ANGEL = 12, ARCHANGEL = 13,
        CENTAUR = 14, CENTAUR_CAPTAIN = 15, DWARF = 16, BATTLE_DWARF = 17, WOOD_ELF = 18, 
        GRAND_ELF = 19, PEGASUS = 20, SILVER_PEGASUS = 21, DENDROID_GUARD = 22, DENDROID_SOLDIER = 23, 
        UNICORN = 24, WAR_UNICORN = 25, GREEN_DRAGON = 26, GOLD_DRAGON = 27,
        GREMLIN = 28, MASTER_GREMLIN = 29, STONE_GARGOYLE = 30, OBSIDIAN_GARGOYLE = 31, 
        STONE_GOLEM = 32, IRON_GOLEM = 33, MAGE = 34, ARCH_MAGE = 35, GENIE = 36, MASTER_GENIE = 37, 
        NAGA = 38, NAGA_QUEEN = 39, GIANT = 40, TITAN = 41,
        IMP = 42, FAMILIAR = 43, GOG = 44, MAGOG = 45, HELL_HOUND = 46, CERBERUS = 47, 
        DEMON = 48, HORNED_DEMON = 49, PIT_FIEND = 50, PIT_LORD = 51, EFREET = 52, EFREET_SULTAN = 53, 
        DEVIL = 54, ARCH_DEVIL = 55,
        SKELETON = 56, SKELETON_WARRIOR = 57, WALKING_DEAD = 58, ZOMBIE_LORD = 59, WIGHT = 60, 
        WRAITH = 61, VAMPIRE = 62, VAMPIRE_LORD = 63, LICH = 64, POWER_LICH = 65, 
        BLACK_KNIGHT = 66, DREAD_KNIGHT = 67, BONE_DRAGON = 68, GHOST_DRAGON = 69,
        TROGLODYTE = 70, INFERNAL_TROGLODYTE = 71, HARPY = 72, HARPY_HAG = 73, BEHOLDER = 74, 
        EVIL_EYE = 75, MEDUSA = 76, MEDUSA_QUEEN = 77, MINOTAUR = 78, MINOTAUR_KING = 79, 
        MANTICORE = 80, SCORPICORE = 81, RED_DRAGON = 82, BLACK_DRAGON = 83,
        GOBLIN = 84, HOBGOBLIN = 85, WOLF_RIDER = 86, WOLF_RAIDER = 87, ORC = 88, 
        ORC_CHIEFTAIN = 89, OGRE = 90, OGRE_MAGE = 91, ROC = 92, THUNDERBIRD = 93, 
        CYCLOP = 94, CYCLOP_KING = 95, BEHEMOTH = 96, ANCIENT_BEHEMOTH = 97,
        GNOLL = 98, GNOLL_MARAUDER = 99, LIZARDMAN = 100, LIZARD_WARRIOR = 101, GORGON = 102, 
        MIGHTY_GORGON = 103, SERPENT_FLY = 104, DRAGON_FLY = 105, BASILISK = 106, GREATER_BASILISK = 107, 
        WYVERN = 108, WYVERN_MONARCH = 109, HYDRA = 110, CHAOS_HYDRA = 111,
        AIR_ELEMENTAL = 112, EARTH_ELEMENTAL = 113, FIRE_ELEMENTAL = 114, WATER_ELEMENTAL = 115, 
        GOLD_GOLEM = 116, DIAMOND_GOLEM = 117, PIXIE = 118, SPRITE = 119, PSYCHIC_ELEMENTAL = 120, 
        MAGIC_ELEMENTAL = 121, ICE_ELEMENTAL = 123, MAGMA_ELEMENTAL = 125, STORM_ELEMENTAL = 127, 
        ENERGY_ELEMENTAL = 129, FIREBIRD = 130, PHOENIX = 131,
        AZURE_DRAGON = 132, CRYSTAL_DRAGON = 133, FAERIE_DRAGON = 134, RUST_DRAGON = 135, 
        ENCHANTER = 136, SHARPSHOOTER = 137, HALFLING = 138, PEASANT = 139, BOAR = 140, 
        MUMMY = 141, NOMAD = 142, ROGUE = 143, TROLL = 144,
        CATAPULT = 145, BALLISTA = 146, FIRST_AID_TENT = 147, AMMO_CART = 148, ARROW_TOWERS = 149
    }

    /// <summary>Building IDs.</summary>
    public enum EBuildingId : short
    {
        DEFAULT = -50, NONE = -1,
        MAGES_GUILD_1 = 0, MAGES_GUILD_2 = 1, MAGES_GUILD_3 = 2, MAGES_GUILD_4 = 3, MAGES_GUILD_5 = 4,
        TAVERN = 5, SHIPYARD = 6, FORT = 7, CITADEL = 8, CASTLE = 9,
        VILLAGE_HALL = 10, TOWN_HALL = 11, CITY_HALL = 12, CAPITOL = 13, MARKETPLACE = 14,
        RESOURCE_SILO = 15, BLACKSMITH = 16, SPECIAL_1 = 17, HORDE_1 = 18, HORDE_1_UPGR = 19,
        SHIP = 20, SPECIAL_2 = 21, SPECIAL_3 = 22, SPECIAL_4 = 23, HORDE_2 = 24,
        HORDE_2_UPGR = 25, GRAIL = 26, EXTRA_TOWN_HALL = 27, EXTRA_CITY_HALL = 28, EXTRA_CAPITOL = 29,
        DWELL_FIRST = 30, DWELL_LVL_2 = 31, DWELL_LVL_3 = 32, DWELL_LVL_4 = 33, DWELL_LVL_5 = 34, 
        DWELL_LVL_6 = 35, DWELL_LAST = 36,
        DWELL_UP_FIRST = 37, DWELL_LVL_2_UP = 38, DWELL_LVL_3_UP = 39, DWELL_LVL_4_UP = 40, 
        DWELL_LVL_5_UP = 41, DWELL_LVL_6_UP = 42, DWELL_UP_LAST = 43
    }

    /// <summary>Artifact IDs.</summary>
    public enum EArtifactId : short
    {
        NONE = -1,
        SPELLBOOK = 0, SPELL_SCROLL = 1, GRAIL = 2, CATAPULT = 3, BALLISTA = 4, 
        AMMO_CART = 5, FIRST_AID_TENT = 6, CENTAUR_AXE = 7, BLACKSHARD_OF_THE_DEAD_KNIGHT = 8, 
        GREATER_GNOLLS_FAIL = 9,
        ARMAGEDDONS_BLADE = 128, TITANS_THUNDER = 135,
        ART_SELECTION = 144, ART_LOCK = 145, AXE_OF_SMASHING = 146, MITHRIL_MAIL = 147, 
        SWORD_OF_SHARPNESS = 148, HELM_OF_IMMORTALITY = 149, PENDANT_OF_SORCERY = 150, 
        BOOTS_OF_HASTE = 151, BOW_OF_SEEKING = 152, DRAGON_EYE_RING = 153
    }

    /// <summary>Artifact position slots.</summary>
    public enum EArtifactPosition : sbyte
    {
        FIRST_AVAILABLE = -2, PRE_FIRST = -1,
        HEAD = 0, SHOULDERS = 1, NECK = 2, RIGHT_HAND = 3, LEFT_HAND = 4, TORSO = 5,
        RIGHT_RING = 6, LEFT_RING = 7, FEET = 8,
        MISC1 = 9, MISC2 = 10, MISC3 = 11, MISC4 = 12,
        MACH1 = 13, MACH2 = 14, MACH3 = 15, MACH4 = 16,
        SPELLBOOK = 17, MISC5 = 18,
        AFTER_LAST = 19
    }

    /// <summary>Secondary skills.</summary>
    public enum ESecondarySkill : sbyte
    {
        WRONG = -2, DEFAULT = -1,
        PATHFINDING = 0, ARCHERY = 1, LOGISTICS = 2, SCOUTING = 3, DIPLOMACY = 4, NAVIGATION = 5, 
        LEADERSHIP = 6, WISDOM = 7, MYSTICISM = 8, LUCK = 9, BALLISTICS = 10, EAGLE_EYE = 11, 
        NECROMANCY = 12, ESTATES = 13, FIRE_MAGIC = 14, AIR_MAGIC = 15, WATER_MAGIC = 16, 
        EARTH_MAGIC = 17, SCHOLAR = 18, TACTICS = 19, ARTILLERY = 20, LEARNING = 21, OFFENCE = 22, 
        ARMORER = 23, INTELLIGENCE = 24, SORCERY = 25, RESISTANCE = 26, FIRST_AID = 27, SKILL_SIZE = 28
    }

    /// <summary>Object types in the game world.</summary>
    public enum EObjectType : short
    {
        NO_OBJ = -1,
        ALTAR_OF_SACRIFICE = 2, ANCHOR_POINT = 3, ARENA = 4, ARTIFACT = 5, PANDORAS_BOX = 6, 
        BLACK_MARKET = 7, BOAT = 8, BORDERGUARD = 9, KEYMASTER = 10, BUOY = 11, CAMPFIRE = 12, 
        CARTOGRAPHER = 13, SWAN_POND = 14, COVER_OF_DARKNESS = 15, CREATURE_BANK = 16, 
        CREATURE_GENERATOR1 = 17, CREATURE_GENERATOR2 = 18, CREATURE_GENERATOR3 = 19, CREATURE_GENERATOR4 = 20,
        CURSED_GROUND1 = 21, CORPSE = 22, MARLETTO_TOWER = 23, DERELICT_SHIP = 24, DRAGON_UTOPIA = 25,
        EVENT = 26, EYE_OF_MAGI = 27, FAERIE_RING = 28, FLOTSAM = 29, FOUNTAIN_OF_FORTUNE = 30,
        FOUNTAIN_OF_YOUTH = 31, GARDEN_OF_REVELATION = 32, GARRISON = 33, HERO = 34, HILL_FORT = 35,
        GRAIL = 36, HUT_OF_MAGI = 37, IDOL_OF_FORTUNE = 38, LEAN_TO = 39, 
        LIBRARY_OF_ENLIGHTENMENT = 41, LIGHTHOUSE = 42, MONOLITH_ONE_WAY_ENTRANCE = 43,
        MONOLITH_ONE_WAY_EXIT = 44, MONOLITH_TWO_WAY = 45, MAGIC_PLAINS1 = 46, SCHOOL_OF_MAGIC = 47,
        MAGIC_SPRING = 48, MAGIC_WELL = 49, MERCENARY_CAMP = 51, MERMAID = 52, MINE = 53, 
        MONSTER = 54, MYSTICAL_GARDEN = 55, OASIS = 56, OBELISK = 57, REDWOOD_OBSERVATORY = 58,
        OCEAN_BOTTLE = 59, PILLAR_OF_FIRE = 60, STAR_AXIS = 61, PRISON = 62, PYRAMID = 63,
        WOG_OBJECT = 63, RALLY_FLAG = 64, RANDOM_ART = 65, RANDOM_TREASURE_ART = 66, 
        RANDOM_MINOR_ART = 67, RANDOM_MAJOR_ART = 68, RANDOM_RELIC_ART = 69, RANDOM_HERO = 70, 
        RANDOM_MONSTER = 71, RANDOM_MONSTER_L1 = 72, RANDOM_MONSTER_L2 = 73, RANDOM_MONSTER_L3 = 74, 
        RANDOM_MONSTER_L4 = 75, RANDOM_RESOURCE = 76, RANDOM_TOWN = 77, REFUGEE_CAMP = 78, 
        RESOURCE = 79, SANCTUARY = 80, SCHOLAR = 81, SEA_CHEST = 82, SEER_HUT = 83, CRYPT = 84, 
        SHIPWRECK = 85, SHIPWRECK_SURVIVOR = 86, SHIPYARD = 87, SHRINE_OF_MAGIC_INCANTATION = 88, 
        SHRINE_OF_MAGIC_GESTURE = 89, SHRINE_OF_MAGIC_THOUGHT = 90, SIGN = 91, SIRENS = 92, 
        SPELL_SCROLL = 93, STABLES = 94, TAVERN = 95, TEMPLE = 96, DEN_OF_THIEVES = 97, 
        TOWN = 98, TRADING_POST = 99, LEARNING_STONE = 100, TREASURE_CHEST = 101, 
        TREE_OF_KNOWLEDGE = 102, SUBTERRANEAN_GATE = 103, UNIVERSITY = 104, WAGON = 105, 
        WAR_MACHINE_FACTORY = 106, SCHOOL_OF_WAR = 107, WARRIORS_TOMB = 108, WATER_WHEEL = 109, 
        WATERING_HOLE = 110, WHIRLPOOL = 111, WINDMILL = 112, WITCH_HUT = 113, HOLE = 124,
        RANDOM_MONSTER_L5 = 162, RANDOM_MONSTER_L6 = 163, RANDOM_MONSTER_L7 = 164,
        BORDER_GATE = 212, FREELANCERS_GUILD = 213, HERO_PLACEHOLDER = 214, QUEST_GUARD = 215, 
        RANDOM_DWELLING = 216, RANDOM_DWELLING_LVL = 217, RANDOM_DWELLING_FACTION = 218, GARRISON2 = 219, 
        ABANDONED_MINE = 220, TRADING_POST_SNOW = 221, CLOVER_FIELD = 222, CURSED_GROUND2 = 223, 
        EVIL_FOG = 224, FAVORABLE_WINDS = 225, FIERY_FIELDS = 226, HOLY_GROUNDS = 227, 
        LUCID_POOLS = 228, MAGIC_CLOUDS = 229, MAGIC_PLAINS2 = 230, ROCKLANDS = 231
    }

    /// <summary>Terrain types.</summary>
    public enum ETerrainType : sbyte
    {
        WRONG = -2, BORDER = -1, DIRT = 0, SAND = 1, GRASS = 2, SNOW = 3, SWAMP = 4,
        ROUGH = 5, SUBTERRANEAN = 6, LAVA = 7, WATER = 8, ROCK = 9
    }

    /// <summary>Road types.</summary>
    public enum ERoadType : sbyte
    {
        NO_ROAD = 0, DIRT_ROAD = 1, GRAVEL_ROAD = 2, COBBLESTONE_ROAD = 3
    }

    /// <summary>River types.</summary>
    public enum ERiverType : sbyte
    {
        NO_RIVER = 0, CLEAR_RIVER = 1, ICY_RIVER = 2, MUDDY_RIVER = 3, LAVA_RIVER = 4
    }

    /// <summary>Player colors/factions.</summary>
    public enum EPlayerColor : short
    {
        RED = 0, BLUE = 1, TAN = 2, GREEN = 3, ORANGE = 4, PURPLE = 5, TEAL = 6, PINK = 7,
        NEUTRAL = 255
    }

    /// <summary>Secondary skill levels.</summary>
    public enum ESecondarySkillLevel : sbyte
    {
        NONE = 0, BASIC = 1, ADVANCED = 2, EXPERT = 3, LEVELS_SIZE = 4
    }

    /// <summary>Primary skills.</summary>
    public enum EPrimarySkill : sbyte
    {
        ATTACK = 0, DEFENSE = 1, SPELL_POWER = 2, KNOWLEDGE = 3, EXPERIENCE = 4
    }

    /// <summary>Secondary skill types.</summary>
    public enum ESecondarySkillType : sbyte
    {
        ADVENTURE = 1, BATTLE = 2
    }

    /// <summary>Specialty types.</summary>
    public enum ESpecialtyType : sbyte
    {
        MASTER_CREATURE = 1, MASTER_RESOURCE = 2
    }

    /// <summary>Ethnicity/faction types.</summary>
    public enum EEthnicity : sbyte
    {
        CASTLE = 0, RAMPART = 1, TOWER = 2, INFERNO = 3, NECROPOLIS = 4, 
        DUNGEON = 5, STRONGHOLD = 6, FORTRESS = 7, CONFLUX = 8
    }

    /// <summary>Campaign versions.</summary>
    public enum ECampaignVersion : sbyte
    {
        INVALID = 0,
        ROE = 4,
        AB = 5,
        SOD = 6,
        WOG = 6,
        HOTA = 32
    }

    /// <summary>Army formation types.</summary>
    public enum EArmyFormationType : sbyte
    {
        WIDE = 0,
        TIGHT = 1
    }

    /// <summary>Victory condition types.</summary>
    public enum EVictoryConditionType : short
    {
        ARTIFACT = 0, GATHERTROOP = 1, GATHERRESOURCE = 2, BUILDCITY = 3, BUILDGRAIL = 4, BEATHERO = 5,
        CAPTURECITY = 6, BEATMONSTER = 7, TAKEDWELLINGS = 8, TAKEMINES = 9, TRANSPORTITEM = 10, WINSTANDARD = 255
    }

    /// <summary>Loss condition types.</summary>
    public enum ELossConditionType : short
    {
        LOSSCASTLE = 0, LOSSHERO = 1, TIMEEXPIRES = 2, LOSSSTANDARD = 255
    }
}
