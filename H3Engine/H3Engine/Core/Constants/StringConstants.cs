// Migrated from VCMI lib/constants/StringConstants.h
// String ID constants that are hardcoded and not suitable for config files

using System;
using System.Collections.Generic;
using System.Linq;

namespace H3Engine.Core.Constants
{
    /// <summary>
    /// String constants for game values.
    /// Corresponds to VCMI's string constant namespaces.
    /// </summary>
    public static class StringConstants
    {
        /// <summary>Resource names in order: wood, mercury, ore, sulfur, crystal, gems, gold</summary>
        public static readonly string[] RESOURCE_NAMES = {
            "wood", "mercury", "ore", "sulfur", "crystal", "gems", "gold"
        };

        /// <summary>Player color names (red, blue, tan, green, orange, purple, teal, pink)</summary>
        public static readonly string[] PLAYER_COLOR_NAMES = {
            "red", "blue", "tan", "green", "orange", "purple", "teal", "pink"
        };

        /// <summary>Alignment names</summary>
        public static readonly string[] ALIGNMENT_NAMES = { "good", "evil", "neutral" };

        /// <summary>Difficulty names</summary>
        public static readonly string[] DIFFICULTY_NAMES = { "pawn", "knight", "rook", "queen", "king" };
    }

    /// <summary>Primary skill names and strings.</summary>
    public static class NPrimarySkill
    {
        public static readonly string[] names = { "attack", "defence", "spellpower", "knowledge" };
    }

    /// <summary>Secondary skill names and strings.</summary>
    public static class NSecondarySkill
    {
        public static readonly string[] names = {
            "pathfinding",  "archery",      "logistics",    "scouting",     "diplomacy",    //  5
            "navigation",   "leadership",   "wisdom",       "mysticism",    "luck",         // 10
            "ballistics",   "eagleEye",     "necromancy",   "estates",      "fireMagic",    // 15
            "airMagic",     "waterMagic",   "earthMagic",   "scholar",      "tactics",      // 20
            "artillery",    "learning",     "offence",      "armorer",      "intelligence", // 25
            "sorcery",      "resistance",   "firstAid"
        };

        public static readonly string[] levels = { "none", "basic", "advanced", "expert" };
    }

    /// <summary>Building type names.</summary>
    public static class EBuildingType
    {
        public static readonly string[] names = {
            "mageGuild1",       "mageGuild2",       "mageGuild3",       "mageGuild4",       "mageGuild5",       //  5
            "tavern",           "shipyard",         "fort",             "citadel",          "castle",           // 10
            "villageHall",      "townHall",         "cityHall",         "capitol",          "marketplace",      // 15
            "resourceSilo",     "blacksmith",       "special1",         "horde1",           "horde1Upgr",       // 20
            "ship",             "special2",         "special3",         "special4",         "horde2",           // 25
            "horde2Upgr",       "grail",            "extraTownHall",    "extraCityHall",    "extraCapitol",     // 30
            "dwellingLvl1",     "dwellingLvl2",     "dwellingLvl3",     "dwellingLvl4",     "dwellingLvl5",     // 35
            "dwellingLvl6",     "dwellingLvl7",     "dwellingUpLvl1",   "dwellingUpLvl2",   "dwellingUpLvl3",   // 40
            "dwellingUpLvl4",   "dwellingUpLvl5",   "dwellingUpLvl6",   "dwellingUpLvl7",   "dwellingLvl8",
            "dwellingUpLvl8"
        };
    }

    /// <summary>Faction names.</summary>
    public static class NFaction
    {
        public static readonly string[] names = {
            "castle",       "rampart",      "tower",
            "inferno",      "necropolis",   "dungeon",
            "stronghold",   "fortress",     "conflux"
        };
    }

    /// <summary>Artifact position names for different bearer types.</summary>
    public static class NArtifactPosition
    {
        public static readonly string[] namesHero = {
            "head", "shoulders", "neck", "rightHand", "leftHand", "torso",  //6
            "rightRing", "leftRing", "feet",                                  //9
            "misc1", "misc2", "misc3", "misc4",                               //13
            "mach1", "mach2", "mach3", "mach4",                               //17
            "spellbook", "misc5"                                              //19
        };

        public static readonly string[] namesCreature = { "creature1" };

        public static readonly string[] namesCommander = {
            "commander1", "commander2", "commander3", "commander4", "commander5",
            "commander6", "commander7", "commander8", "commander9"
        };

        public static readonly string backpack = "backpack";
    }

    /// <summary>Metaclass type names.</summary>
    public static class NMetaclass
    {
        public static readonly string[] names = {
            "",
            "artifact", "creature", "faction", "experience", "hero",
            "heroClass", "luck", "mana", "morale", "movement",
            "object", "primarySkill", "secondarySkill", "spell", "resource"
        };
    }

    /// <summary>Pathfinding layer names.</summary>
    public static class NPathfindingLayer
    {
        public static readonly string[] names = { "land", "sail", "water", "air" };
    }

    /// <summary>Mapped string keys to enum values.</summary>
    public static class MappedKeys
    {
        public static readonly Dictionary<string, BuildingSubID.EBuildingSubID> SPECIAL_BUILDINGS =
            new Dictionary<string, BuildingSubID.EBuildingSubID>
        {
            { "mysticPond", BuildingSubID.EBuildingSubID.MYSTIC_POND },
            { "castleGate", BuildingSubID.EBuildingSubID.CASTLE_GATE },
            { "portalOfSummoning", BuildingSubID.EBuildingSubID.PORTAL_OF_SUMMONING },
            { "library", BuildingSubID.EBuildingSubID.LIBRARY },
            { "escapeTunnel", BuildingSubID.EBuildingSubID.ESCAPE_TUNNEL },
            { "treasury", BuildingSubID.EBuildingSubID.TREASURY },
            { "bank", BuildingSubID.EBuildingSubID.BANK },
            { "auroraBorealis", BuildingSubID.EBuildingSubID.AURORA_BOREALIS }
        };

        public static readonly Dictionary<string, EMarketMode> MARKET_NAMES_TO_TYPES =
            new Dictionary<string, EMarketMode>
        {
            { "resource-resource", EMarketMode.RESOURCE_RESOURCE },
            { "resource-player", EMarketMode.RESOURCE_PLAYER },
            { "creature-resource", EMarketMode.CREATURE_RESOURCE },
            { "resource-artifact", EMarketMode.RESOURCE_ARTIFACT },
            { "artifact-resource", EMarketMode.ARTIFACT_RESOURCE },
            { "artifact-experience", EMarketMode.ARTIFACT_EXP },
            { "creature-experience", EMarketMode.CREATURE_EXP },
            { "creature-undead", EMarketMode.CREATURE_UNDEAD },
            { "resource-skill", EMarketMode.RESOURCE_SKILL },
        };
    }
}
