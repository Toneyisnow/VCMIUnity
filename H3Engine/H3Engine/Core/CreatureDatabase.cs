using H3Engine.Common;
using H3Engine.Core.Constants;
using System;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// Provides creature-level lookup and random creature selection for map initialization.
    /// Based on Heroes III creature data (CRTRAITS).
    /// </summary>
    public static class CreatureDatabase
    {
        // Mapping from level (1-7) to list of creature IDs at that level
        private static readonly Dictionary<int, List<ECreatureId>> CreaturesByLevel;

        static CreatureDatabase()
        {
            CreaturesByLevel = new Dictionary<int, List<ECreatureId>>();
            for (int i = 1; i <= 7; i++)
            {
                CreaturesByLevel[i] = new List<ECreatureId>();
            }

            // Castle
            Register(ECreatureId.PIKEMAN, 1);
            Register(ECreatureId.HALBERDIER, 1);
            Register(ECreatureId.ARCHER, 2);
            Register(ECreatureId.MARKSMAN, 2);
            Register(ECreatureId.GRIFFIN, 3);
            Register(ECreatureId.ROYAL_GRIFFIN, 3);
            Register(ECreatureId.SWORDSMAN, 4);
            Register(ECreatureId.CRUSADER, 4);
            Register(ECreatureId.MONK, 5);
            Register(ECreatureId.ZEALOT, 5);
            Register(ECreatureId.CAVALIER, 6);
            Register(ECreatureId.CHAMPION, 6);
            Register(ECreatureId.ANGEL, 7);
            Register(ECreatureId.ARCHANGEL, 7);

            // Rampart
            Register(ECreatureId.CENTAUR, 1);
            Register(ECreatureId.CENTAUR_CAPTAIN, 1);
            Register(ECreatureId.DWARF, 2);
            Register(ECreatureId.BATTLE_DWARF, 2);
            Register(ECreatureId.WOOD_ELF, 3);
            Register(ECreatureId.GRAND_ELF, 3);
            Register(ECreatureId.PEGASUS, 4);
            Register(ECreatureId.SILVER_PEGASUS, 4);
            Register(ECreatureId.DENDROID_GUARD, 5);
            Register(ECreatureId.DENDROID_SOLDIER, 5);
            Register(ECreatureId.UNICORN, 6);
            Register(ECreatureId.WAR_UNICORN, 6);
            Register(ECreatureId.GREEN_DRAGON, 7);
            Register(ECreatureId.GOLD_DRAGON, 7);

            // Tower
            Register(ECreatureId.GREMLIN, 1);
            Register(ECreatureId.MASTER_GREMLIN, 1);
            Register(ECreatureId.STONE_GARGOYLE, 2);
            Register(ECreatureId.OBSIDIAN_GARGOYLE, 2);
            Register(ECreatureId.STONE_GOLEM, 3);
            Register(ECreatureId.IRON_GOLEM, 3);
            Register(ECreatureId.MAGE, 4);
            Register(ECreatureId.ARCH_MAGE, 4);
            Register(ECreatureId.GENIE, 5);
            Register(ECreatureId.MASTER_GENIE, 5);
            Register(ECreatureId.NAGA, 6);
            Register(ECreatureId.NAGA_QUEEN, 6);
            Register(ECreatureId.GIANT, 7);
            Register(ECreatureId.TITAN, 7);

            // Inferno
            Register(ECreatureId.IMP, 1);
            Register(ECreatureId.FAMILIAR, 1);
            Register(ECreatureId.GOG, 2);
            Register(ECreatureId.MAGOG, 2);
            Register(ECreatureId.HELL_HOUND, 3);
            Register(ECreatureId.CERBERUS, 3);
            Register(ECreatureId.DEMON, 4);
            Register(ECreatureId.HORNED_DEMON, 4);
            Register(ECreatureId.PIT_FIEND, 5);
            Register(ECreatureId.PIT_LORD, 5);
            Register(ECreatureId.EFREET, 6);
            Register(ECreatureId.EFREET_SULTAN, 6);
            Register(ECreatureId.DEVIL, 7);
            Register(ECreatureId.ARCH_DEVIL, 7);

            // Necropolis
            Register(ECreatureId.SKELETON, 1);
            Register(ECreatureId.SKELETON_WARRIOR, 1);
            Register(ECreatureId.WALKING_DEAD, 2);
            Register(ECreatureId.ZOMBIE_LORD, 2);
            Register(ECreatureId.WIGHT, 3);
            Register(ECreatureId.WRAITH, 3);
            Register(ECreatureId.VAMPIRE, 4);
            Register(ECreatureId.VAMPIRE_LORD, 4);
            Register(ECreatureId.LICH, 5);
            Register(ECreatureId.POWER_LICH, 5);
            Register(ECreatureId.BLACK_KNIGHT, 6);
            Register(ECreatureId.DREAD_KNIGHT, 6);
            Register(ECreatureId.BONE_DRAGON, 7);
            Register(ECreatureId.GHOST_DRAGON, 7);

            // Dungeon
            Register(ECreatureId.TROGLODYTE, 1);
            Register(ECreatureId.INFERNAL_TROGLODYTE, 1);
            Register(ECreatureId.HARPY, 2);
            Register(ECreatureId.HARPY_HAG, 2);
            Register(ECreatureId.BEHOLDER, 3);
            Register(ECreatureId.EVIL_EYE, 3);
            Register(ECreatureId.MEDUSA, 4);
            Register(ECreatureId.MEDUSA_QUEEN, 4);
            Register(ECreatureId.MINOTAUR, 5);
            Register(ECreatureId.MINOTAUR_KING, 5);
            Register(ECreatureId.MANTICORE, 6);
            Register(ECreatureId.SCORPICORE, 6);
            Register(ECreatureId.RED_DRAGON, 7);
            Register(ECreatureId.BLACK_DRAGON, 7);

            // Stronghold
            Register(ECreatureId.GOBLIN, 1);
            Register(ECreatureId.HOBGOBLIN, 1);
            Register(ECreatureId.WOLF_RIDER, 2);
            Register(ECreatureId.WOLF_RAIDER, 2);
            Register(ECreatureId.ORC, 3);
            Register(ECreatureId.ORC_CHIEFTAIN, 3);
            Register(ECreatureId.OGRE, 4);
            Register(ECreatureId.OGRE_MAGE, 4);
            Register(ECreatureId.ROC, 5);
            Register(ECreatureId.THUNDERBIRD, 5);
            Register(ECreatureId.CYCLOP, 6);
            Register(ECreatureId.CYCLOP_KING, 6);
            Register(ECreatureId.BEHEMOTH, 7);
            Register(ECreatureId.ANCIENT_BEHEMOTH, 7);

            // Fortress
            Register(ECreatureId.GNOLL, 1);
            Register(ECreatureId.GNOLL_MARAUDER, 1);
            Register(ECreatureId.LIZARDMAN, 2);
            Register(ECreatureId.LIZARD_WARRIOR, 2);
            Register(ECreatureId.SERPENT_FLY, 3);
            Register(ECreatureId.DRAGON_FLY, 3);
            Register(ECreatureId.BASILISK, 4);
            Register(ECreatureId.GREATER_BASILISK, 4);
            Register(ECreatureId.GORGON, 5);
            Register(ECreatureId.MIGHTY_GORGON, 5);
            Register(ECreatureId.WYVERN, 6);
            Register(ECreatureId.WYVERN_MONARCH, 6);
            Register(ECreatureId.HYDRA, 7);
            Register(ECreatureId.CHAOS_HYDRA, 7);

            // Conflux
            Register(ECreatureId.PIXIE, 1);
            Register(ECreatureId.SPRITE, 1);
            Register(ECreatureId.AIR_ELEMENTAL, 2);
            Register(ECreatureId.STORM_ELEMENTAL, 2);
            Register(ECreatureId.WATER_ELEMENTAL, 3);
            Register(ECreatureId.ICE_ELEMENTAL, 3);
            Register(ECreatureId.FIRE_ELEMENTAL, 4);
            Register(ECreatureId.ENERGY_ELEMENTAL, 4);
            Register(ECreatureId.EARTH_ELEMENTAL, 5);
            Register(ECreatureId.MAGMA_ELEMENTAL, 5);
            Register(ECreatureId.PSYCHIC_ELEMENTAL, 6);
            Register(ECreatureId.MAGIC_ELEMENTAL, 6);
            Register(ECreatureId.FIREBIRD, 7);
            Register(ECreatureId.PHOENIX, 7);

            // Neutral creatures (levels 1-7 only, excluding level 8/10 dragons)
            Register(ECreatureId.HALFLING, 1);
            Register(ECreatureId.PEASANT, 1);
            Register(ECreatureId.BOAR, 2);
            Register(ECreatureId.ROGUE, 2);
            Register(ECreatureId.MUMMY, 3);
            Register(ECreatureId.NOMAD, 3);
            Register(ECreatureId.SHARPSHOOTER, 4);
            Register(ECreatureId.TROLL, 5);
            Register(ECreatureId.GOLD_GOLEM, 5);
            Register(ECreatureId.DIAMOND_GOLEM, 6);
            Register(ECreatureId.ENCHANTER, 6);
        }

        private static void Register(ECreatureId creatureId, int level)
        {
            CreaturesByLevel[level].Add(creatureId);
        }

        /// <summary>
        /// Returns a random creature of the specified level (1-7).
        /// </summary>
        public static ECreatureId GetRandomCreatureOfLevel(int level, Random random)
        {
            if (level < 1 || level > 7 || !CreaturesByLevel.ContainsKey(level))
            {
                throw new ArgumentException($"Invalid creature level: {level}. Must be 1-7.");
            }

            var candidates = CreaturesByLevel[level];
            return candidates[random.Next(candidates.Count)];
        }

        /// <summary>
        /// Returns a random creature of any level (1-7).
        /// </summary>
        public static ECreatureId GetRandomCreature(Random random)
        {
            // First pick a random level, then a random creature of that level
            int level = random.Next(1, 8); // 1-7
            return GetRandomCreatureOfLevel(level, random);
        }

        /// <summary>
        /// Returns a random creature of the specified level, filtered to only include
        /// creatures whose ID exists in the availableIds set (i.e. have a template in the map).
        /// Returns NONE if no matching creature is found.
        /// </summary>
        public static ECreatureId GetRandomCreatureOfLevelWithFilter(int level, Random random, HashSet<int> availableIds)
        {
            if (level < 1 || level > 7 || !CreaturesByLevel.ContainsKey(level))
                return ECreatureId.NONE;

            var candidates = new List<ECreatureId>();
            foreach (var id in CreaturesByLevel[level])
            {
                if (availableIds.Contains((int)id))
                    candidates.Add(id);
            }

            if (candidates.Count == 0)
                return ECreatureId.NONE;

            return candidates[random.Next(candidates.Count)];
        }

        /// <summary>
        /// Returns a random creature of any level (1-7), filtered to only include
        /// creatures whose ID exists in the availableIds set.
        /// Returns NONE if no matching creature is found.
        /// </summary>
        public static ECreatureId GetRandomCreatureWithFilter(Random random, HashSet<int> availableIds)
        {
            // Collect all available creatures across all levels
            var candidates = new List<ECreatureId>();
            for (int lvl = 1; lvl <= 7; lvl++)
            {
                foreach (var id in CreaturesByLevel[lvl])
                {
                    if (availableIds.Contains((int)id))
                        candidates.Add(id);
                }
            }

            if (candidates.Count == 0)
                return ECreatureId.NONE;

            return candidates[random.Next(candidates.Count)];
        }

        /// <summary>
        /// Gets the level for a random monster object type.
        /// Returns 0 for RANDOM_MONSTER (any level), 1-7 for level-specific, -1 if not a random monster type.
        /// </summary>
        public static int GetRandomMonsterLevel(EObjectType objectType)
        {
            switch (objectType)
            {
                case EObjectType.RANDOM_MONSTER: return 0;
                case EObjectType.RANDOM_MONSTER_L1: return 1;
                case EObjectType.RANDOM_MONSTER_L2: return 2;
                case EObjectType.RANDOM_MONSTER_L3: return 3;
                case EObjectType.RANDOM_MONSTER_L4: return 4;
                case EObjectType.RANDOM_MONSTER_L5: return 5;
                case EObjectType.RANDOM_MONSTER_L6: return 6;
                case EObjectType.RANDOM_MONSTER_L7: return 7;
                default: return -1;
            }
        }
    }
}


