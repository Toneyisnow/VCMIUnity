// Migrated from VCMI lib/constants/EntityIdentifiers.h
// Entity identifier types for game objects - artifacts, creatures, heroes, spells, etc.

using System;
using System.Collections.Generic;

namespace H3Engine.Core.Constants
{
    // Static identifier types for runtime instances

    /// <summary>Unique identifier for artifact instances in play.</summary>
    public class ArtifactInstanceID : StaticIdentifier<ArtifactInstanceID>
    {
        public ArtifactInstanceID() { }
        public ArtifactInstanceID(int value) : base(value) { }

        public static string Encode(int index) => index >= 0 ? index.ToString() : "";
    }

    /// <summary>Query ID for server requests.</summary>
    public class QueryID : StaticIdentifier<QueryID>
    {
        public QueryID() { }
        public QueryID(int value) : base(value) { }

        public static readonly QueryID NONE = new QueryID(-1);
        public static readonly QueryID CLIENT = new QueryID(-2);
    }

    /// <summary>Battle instance identifier.</summary>
    public class BattleID : StaticIdentifier<BattleID>
    {
        public BattleID() { }
        public BattleID(int value) : base(value) { }

        public static readonly BattleID NONE = new BattleID(-1);
    }

    /// <summary>Map object instance identifier.</summary>
    public class ObjectInstanceID : StaticIdentifier<ObjectInstanceID>
    {
        public ObjectInstanceID() { }
        public ObjectInstanceID(int value) : base(value) { }

        public static readonly ObjectInstanceID NONE = new ObjectInstanceID(-1);

        public static int Decode(string identifier) => int.TryParse(identifier, out var result) ? result : -1;

        public static string Encode(int index) => index >= 0 ? index.ToString() : "";
    }

    /// <summary>Quest instance identifier.</summary>
    public class QuestInstanceID : StaticIdentifier<QuestInstanceID>
    {
        public QuestInstanceID() { }
        public QuestInstanceID(int value) : base(value) { }

        public static readonly QuestInstanceID NONE = new QuestInstanceID(-1);

        public static int Decode(string identifier) => int.TryParse(identifier, out var result) ? result : -1;

        public static string Encode(int index) => index >= 0 ? index.ToString() : "";
    }

    /// <summary>Campaign scenario identifier.</summary>
    public class CampaignScenarioID : StaticIdentifier<CampaignScenarioID>
    {
        public CampaignScenarioID() { }
        public CampaignScenarioID(int value) : base(value) { }

        public static readonly CampaignScenarioID NONE = new CampaignScenarioID(-1);

        public static int Decode(string identifier) => int.TryParse(identifier, out var result) ? result : -1;

        public static string Encode(int index) => index >= 0 ? index.ToString() : "";
    }

    /// <summary>Army slot (creature stack position).</summary>
    public class SlotID : StaticIdentifier<SlotID>
    {
        public SlotID() { }
        public SlotID(int value) : base(value) { }

        public static readonly SlotID COMMANDER_SLOT_PLACEHOLDER = new SlotID(-2);
        public static readonly SlotID SUMMONED_SLOT_PLACEHOLDER = new SlotID(-3); // for summoned creatures during battle
        public static readonly SlotID WAR_MACHINES_SLOT = new SlotID(-4); // for war machines during battle
        public static readonly SlotID ARROW_TOWERS_SLOT = new SlotID(-5); // for arrow towers during battle

        public bool ValidSlot() => GetNum() >= 0 && GetNum() < NumericConstants.ARMY_SIZE;
    }

    /// <summary>Player color identifier.</summary>
    public class PlayerColor : StaticIdentifier<PlayerColor>
    {
        public PlayerColor() { }
        public PlayerColor(int value) : base(value) { }

        public enum EPlayerColor
        {
            PLAYER_LIMIT_I = 8,
        }

        public static readonly PlayerColor SPECTATOR = new PlayerColor(-4); // 252
        public static readonly PlayerColor CANNOT_DETERMINE = new PlayerColor(-3); // 253
        public static readonly PlayerColor UNFLAGGABLE = new PlayerColor(-2); // 254 - neutral objects
        public static readonly PlayerColor NEUTRAL = new PlayerColor(-1); // 255
        public static readonly PlayerColor PLAYER_LIMIT = new PlayerColor(8); // player limit per map

        public bool IsValidPlayer() => GetNum() >= 0 && GetNum() < 8;

        public bool IsSpectator() => GetNum() == SPECTATOR.GetNum();

        public override string ToString() => Encode(GetNum());

        public static int Decode(string identifier)
        {
            for (int i = 0; i < StringConstants.PLAYER_COLOR_NAMES.Length; i++)
            {
                if (StringConstants.PLAYER_COLOR_NAMES[i] == identifier)
                    return i;
            }
            return -1;
        }

        public static string Encode(int index)
        {
            if (index == -1)
                return "neutral";
            if (index < 0 || index >= StringConstants.PLAYER_COLOR_NAMES.Length)
                return "invalid";
            return StringConstants.PLAYER_COLOR_NAMES[index];
        }

        public static readonly PlayerColor[] ALL_PLAYERS = {
            new PlayerColor(0), new PlayerColor(1), new PlayerColor(2), new PlayerColor(3),
            new PlayerColor(4), new PlayerColor(5), new PlayerColor(6), new PlayerColor(7)
        };
    }

    /// <summary>Team identifier.</summary>
    public class TeamID : StaticIdentifier<TeamID>
    {
        public TeamID() { }
        public TeamID(int value) : base(value) { }

        public static readonly TeamID NO_TEAM = new TeamID(-1);
    }

    /// <summary>Teleport channel identifier.</summary>
    public class TeleportChannelID : StaticIdentifier<TeleportChannelID>
    {
        public TeleportChannelID() { }
        public TeleportChannelID(int value) : base(value) { }
    }

    // Entity identifier types - these need game service lookups

    /// <summary>Hero class identifier.</summary>
    public class HeroClassID : EntityIdentifier<HeroClassID>
    {
        public HeroClassID() { }
        public HeroClassID(int value) : base(value) { }

        public override int Decode(string identifier) => ResolveIdentifier("heroClass", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            // Would lookup in HeroClass service
            return index.ToString();
        }

        public override string EntityType() => "heroClass";
    }

    /// <summary>Hero type identifier.</summary>
    public class HeroTypeID : EntityIdentifier<HeroTypeID>
    {
        public HeroTypeID() { }
        public HeroTypeID(int value) : base(value) { }

        public static readonly HeroTypeID NONE = new HeroTypeID(-1);
        public static readonly HeroTypeID RANDOM = new HeroTypeID(-2);
        public static readonly HeroTypeID CAMP_STRONGEST = new HeroTypeID(-3);
        public static readonly HeroTypeID CAMP_GENERATED = new HeroTypeID(-2);
        public static readonly HeroTypeID CAMP_RANDOM = new HeroTypeID(-1);

        public bool IsValid() => GetNum() >= 0;

        public override int Decode(string identifier)
        {
            if (identifier == "random") return -2;
            if (identifier == "strongest") return -3;
            return ResolveIdentifier("hero", identifier);
        }

        public override string Encode(int index)
        {
            if (index == -1) return "";
            if (index == -2) return "random";
            if (index == -3) return "strongest";
            return index.ToString();
        }

        public override string EntityType() => "hero";
    }

    /// <summary>Secondary skill identifier.</summary>
    public class SecondarySkillBase : IdentifierBase
    {
        public enum Type : int
        {
            NONE = -1,
            PATHFINDING = 0,
            ARCHERY,
            LOGISTICS,
            SCOUTING,
            DIPLOMACY,
            NAVIGATION,
            LEADERSHIP,
            WISDOM,
            MYSTICISM,
            LUCK,
            BALLISTICS,
            EAGLE_EYE,
            NECROMANCY,
            ESTATES,
            FIRE_MAGIC,
            AIR_MAGIC,
            WATER_MAGIC,
            EARTH_MAGIC,
            SCHOLAR,
            TACTICS,
            ARTILLERY,
            LEARNING,
            OFFENCE,
            ARMORER,
            INTELLIGENCE,
            SORCERY,
            RESISTANCE,
            FIRST_AID,
            SKILL_SIZE
        }
    }

    /// <summary>Secondary skill with entity lookup.</summary>
    public class SecondarySkill : EntityIdentifierWithEnum<SecondarySkill, SecondarySkillBase>
    {
        public SecondarySkill() { }
        public SecondarySkill(int value) : base(value) { }

        public override int Decode(string identifier) => ResolveIdentifier("secondarySkill", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            if (index >= 0 && index < NSecondarySkill.names.Length)
                return NSecondarySkill.names[index];
            return "";
        }

        public override string EntityType() => "secondarySkill";
    }

    /// <summary>Primary skill identifier.</summary>
    public class PrimarySkill : StaticIdentifier<PrimarySkill>
    {
        public PrimarySkill() { }
        public PrimarySkill(int value) : base(value) { }

        public static readonly PrimarySkill NONE = new PrimarySkill(-1);
        public static readonly PrimarySkill ATTACK = new PrimarySkill(0);
        public static readonly PrimarySkill DEFENSE = new PrimarySkill(1);
        public static readonly PrimarySkill SPELL_POWER = new PrimarySkill(2);
        public static readonly PrimarySkill KNOWLEDGE = new PrimarySkill(3);

        public static readonly PrimarySkill[] ALL_SKILLS = {
            ATTACK, DEFENSE, SPELL_POWER, KNOWLEDGE
        };

        public static int Decode(string identifier) => ResolveIdentifier("primarySkill", identifier);

        public static string Encode(int index)
        {
            if (index >= 0 && index < NPrimarySkill.names.Length)
                return NPrimarySkill.names[index];
            return "";
        }
    }

    /// <summary>Faction identifier.</summary>
    public class FactionID : EntityIdentifier<FactionID>
    {
        public FactionID() { }
        public FactionID(int value) : base(value) { }

        public static readonly FactionID NONE = new FactionID(-2);
        public static readonly FactionID DEFAULT = new FactionID(-1);
        public static readonly FactionID RANDOM = new FactionID(-1);
        public static readonly FactionID ANY = new FactionID(-1);
        public static readonly FactionID CASTLE = new FactionID(0);
        public static readonly FactionID RAMPART = new FactionID(1);
        public static readonly FactionID TOWER = new FactionID(2);
        public static readonly FactionID INFERNO = new FactionID(3);
        public static readonly FactionID NECROPOLIS = new FactionID(4);
        public static readonly FactionID DUNGEON = new FactionID(5);
        public static readonly FactionID STRONGHOLD = new FactionID(6);
        public static readonly FactionID FORTRESS = new FactionID(7);
        public static readonly FactionID CONFLUX = new FactionID(8);
        public static readonly FactionID NEUTRAL = new FactionID(9);

        public bool IsValid() => GetNum() >= 0;

        public override int Decode(string identifier) => ResolveIdentifier("faction", identifier);

        public override string Encode(int index)
        {
            if (index >= 0 && index < NFaction.names.Length)
                return NFaction.names[index];
            return "";
        }

        public override string EntityType() => "faction";
    }

    /// <summary>Building identifier.</summary>
    public class BuildingID : StaticIdentifier<BuildingID>
    {
        public BuildingID() { }
        public BuildingID(int value) : base(value) { }

        public static string Encode(int index) => index.ToString();

        public static int Decode(string identifier) => int.TryParse(identifier, out var result) ? result : -1;
    }

    /// <summary>Artifact identifier.</summary>
    public class ArtifactIDBase : EntityIdentifier<ArtifactIDBase>
    {
        public ArtifactIDBase() { }
        public ArtifactIDBase(int value) : base(value) { }
    }

    /// <summary>Concrete artifact identifier.</summary>
    public class ArtifactID : EntityIdentifier<ArtifactID>
    {
        public ArtifactID() { }
        public ArtifactID(int value) : base(value) { }

        public override int Decode(string identifier) => ResolveIdentifier("artifact", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            return index.ToString();
        }

        public override string EntityType() => "artifact";
    }

    /// <summary>Creature identifier.</summary>
    public class CreatureIDBase : EntityIdentifier<CreatureIDBase>
    {
        public CreatureIDBase() { }
        public CreatureIDBase(int value) : base(value) { }
    }

    /// <summary>Concrete creature identifier.</summary>
    public class CreatureID : EntityIdentifier<CreatureID>
    {
        public CreatureID() { }
        public CreatureID(int value) : base(value) { }

        public override int Decode(string identifier) => ResolveIdentifier("creature", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            return index.ToString();
        }

        public override string EntityType() => "creature";
    }

    /// <summary>Spell identifier.</summary>
    public class SpellIDBase : EntityIdentifier<SpellIDBase>
    {
        public SpellIDBase() { }
        public SpellIDBase(int value) : base(value) { }
    }

    /// <summary>Concrete spell identifier.</summary>
    public class SpellID : EntityIdentifier<SpellID>
    {
        public SpellID() { }
        public SpellID(int value) : base(value) { }

        public const int PRESET = -1;
        public const int SPELLBOOK_PRESET = -2;

        public override int Decode(string identifier)
        {
            if (identifier == "preset") return PRESET;
            if (identifier == "spellbook_preset") return SPELLBOOK_PRESET;
            return ResolveIdentifier("spell", identifier);
        }

        public override string Encode(int index)
        {
            if (index == -1) return "";
            if (index == PRESET) return "preset";
            if (index == SPELLBOOK_PRESET) return "spellbook_preset";
            return index.ToString();
        }

        public override string EntityType() => "spell";
    }

    /// <summary>Spell school identifier.</summary>
    public class SpellSchool : EntityIdentifier<SpellSchool>
    {
        public SpellSchool() { }
        public SpellSchool(int value) : base(value) { }

        public static readonly SpellSchool ANY = new SpellSchool(-1);
        public static readonly SpellSchool AIR = new SpellSchool(0);
        public static readonly SpellSchool FIRE = new SpellSchool(1);
        public static readonly SpellSchool EARTH = new SpellSchool(2);
        public static readonly SpellSchool WATER = new SpellSchool(3);

        public override int Decode(string identifier) => ResolveIdentifier("spellSchool", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "any";
            return index.ToString();
        }

        public override string EntityType() => "spellSchool";
    }

    /// <summary>Game resource identifier.</summary>
    public class GameResID : EntityIdentifier<GameResID>
    {
        public GameResID() { }
        public GameResID(int value) : base(value) { }

        public override int Decode(string identifier) => ResolveIdentifier("resource", identifier);

        public override string Encode(int index)
        {
            if (index >= 0 && index < StringConstants.RESOURCE_NAMES.Length)
                return StringConstants.RESOURCE_NAMES[index];
            return "";
        }

        public override string EntityType() => "resource";
    }

    // Terrain and map-related identifiers

    /// <summary>Terrain type identifier.</summary>
    public class TerrainId : EntityIdentifier<TerrainId>
    {
        public TerrainId() { }
        public TerrainId(int value) : base(value) { }

        public const int NONE = -1;
        public const int NATIVE_TERRAIN = -2;

        public override int Decode(string identifier)
        {
            if (identifier == "native") return NATIVE_TERRAIN;
            return ResolveIdentifier("terrain", identifier);
        }

        public override string Encode(int index)
        {
            if (index == NONE) return "";
            if (index == NATIVE_TERRAIN) return "native";
            return index.ToString();
        }

        public override string EntityType() => "terrain";
    }

    /// <summary>Road type identifier.</summary>
    public class RoadId : EntityIdentifier<RoadId>
    {
        public RoadId() { }
        public RoadId(int value) : base(value) { }

        public static readonly RoadId NO_ROAD = new RoadId(0);
        public static readonly RoadId DIRT_ROAD = new RoadId(1);
        public static readonly RoadId GRAVEL_ROAD = new RoadId(2);
        public static readonly RoadId COBBLESTONE_ROAD = new RoadId(3);

        public override int Decode(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return 0;
            return ResolveIdentifier("road", identifier);
        }

        public override string Encode(int index)
        {
            if (index == 0) return "";
            return index.ToString();
        }

        public override string EntityType() => "road";
    }

    /// <summary>River type identifier.</summary>
    public class RiverId : EntityIdentifier<RiverId>
    {
        public RiverId() { }
        public RiverId(int value) : base(value) { }

        public static readonly RiverId NO_RIVER = new RiverId(0);
        public static readonly RiverId WATER_RIVER = new RiverId(1);
        public static readonly RiverId ICY_RIVER = new RiverId(2);
        public static readonly RiverId MUD_RIVER = new RiverId(3);
        public static readonly RiverId LAVA_RIVER = new RiverId(4);

        public override int Decode(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return 0;
            return ResolveIdentifier("river", identifier);
        }

        public override string Encode(int index)
        {
            if (index == 0) return "";
            return index.ToString();
        }

        public override string EntityType() => "river";
    }

    /// <summary>Map layer identifier.</summary>
    public class MapLayerId : EntityIdentifier<MapLayerId>
    {
        public MapLayerId() { }
        public MapLayerId(int value) : base(value) { }

        public static readonly MapLayerId NONE = new MapLayerId(-1);
        public static readonly MapLayerId SURFACE = new MapLayerId(0);
        public static readonly MapLayerId UNDERGROUND = new MapLayerId(1);
        public static readonly MapLayerId UNKNOWN = new MapLayerId(2);

        public override int Decode(string identifier)
        {
            if (string.IsNullOrEmpty(identifier)) return -1;
            return ResolveIdentifier("mapLayer", identifier);
        }

        public override string Encode(int index)
        {
            if (index == -1) return "";
            return index.ToString();
        }

        public override string EntityType() => "mapLayer";
    }

    /// <summary>Battlefield identifier.</summary>
    public class BattleField : EntityIdentifier<BattleField>
    {
        public BattleField() { }
        public BattleField(int value) : base(value) { }

        public static readonly BattleField NONE = new BattleField(-1);

        public override int Decode(string identifier) => ResolveIdentifier("battlefield", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            return index.ToString();
        }

        public override string EntityType() => "battlefield";
    }

    /// <summary>Obstacle identifier.</summary>
    public class Obstacle : StaticIdentifier<Obstacle>
    {
        public Obstacle() { }
        public Obstacle(int value) : base(value) { }
    }

    /// <summary>Boat type identifier.</summary>
    public class BoatId : EntityIdentifier<BoatId>
    {
        public BoatId() { }
        public BoatId(int value) : base(value) { }

        public static readonly BoatId NONE = new BoatId(-1);
        public static readonly BoatId NECROPOLIS = new BoatId(0);
        public static readonly BoatId CASTLE = new BoatId(1);
        public static readonly BoatId FORTRESS = new BoatId(2);

        public override int Decode(string identifier) => ResolveIdentifier("core:boat", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            return index.ToString();
        }

        public override string EntityType() => "boat";
    }

    /// <summary>Map object identifier (combines primary and sub types).</summary>
    public class MapObjectID : EntityIdentifier<MapObjectID>
    {
        public MapObjectID() { }
        public MapObjectID(int value) : base(value) { }

        public override int Decode(string identifier) => ResolveIdentifier("object", identifier);

        public override string Encode(int index)
        {
            if (index == -1) return "";
            return index.ToString();
        }

        public override string EntityType() => "object";
    }

    /// <summary>Map object sub-identifier.</summary>
    public class MapObjectSubID : EntityIdentifier<MapObjectSubID>
    {
        public MapObjectSubID() { }
        public MapObjectSubID(int value) : base(value) { }

        public override string EntityType() => "objectSubtype";
    }

    /// <summary>Building type unique identifier (combines faction and building).</summary>
    public class BuildingTypeUniqueID : StaticIdentifier<BuildingTypeUniqueID>
    {
        public BuildingTypeUniqueID() { }
        public BuildingTypeUniqueID(int value) : base(value) { }

        public BuildingTypeUniqueID(FactionID factionID, BuildingID buildingID)
            : base(factionID.GetNum() * 0x10000 + buildingID.GetNum())
        {
        }

        public BuildingID GetBuilding() => new BuildingID(GetNum() % 0x10000);

        public FactionID GetFaction() => new FactionID(GetNum() / 0x10000);

        public static int Decode(string identifier) => -1; // TODO

        public static string Encode(int index) => ""; // TODO
    }
}
