using H3Engine.Common;
using H3Engine.Core;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace H3Engine.Components.Data
{

    public class GameMap
    {
        public H3Map H3Map
        {
            get; private set;
        }

        public int MapLevel
        {
            get; private set;
        }

        public GameMap()
        {

        }

        public static GameMap LoadFromH3Map(H3Map h3Map, int level)
        {
            GameMap gameMap = new GameMap();
            gameMap.H3Map = h3Map;
            gameMap.MapLevel = level;

            gameMap.Width = (int)h3Map.Header.Width;
            gameMap.Height = (int)h3Map.Header.Height;

            gameMap.TerrainTiles = new TerrainTile[gameMap.Width, gameMap.Height];
            for(int i = 0; i < gameMap.Width; i++)
            {
                for(int j = 0; j < gameMap.Height; j++)
                {
                    gameMap.TerrainTiles[i, j] = h3Map.TerrainTiles[level, i, j];
                }
            }

            gameMap.Objects = new List<CGObject>();
            gameMap.Heroes = new List<HeroInstance>();

            // Randomize random monster placeholders before processing objects
            Random random = new Random();
            RandomizeMonsters(h3Map, random);

            foreach (CGObject mapObject in h3Map.Objects)
            {
                MapPosition position = mapObject.Position;
                if (position.Level != level)
                {
                    continue;
                }

                ObjectTemplate template = mapObject.Template;
                if (template == null)
                {
                    continue;
                }

                if (MapObjectHelper.IsStaticObject(template.Type))
                {
                    // Add tile mask information to map

                }

                gameMap.Objects.Add(mapObject);

                if (template.Type == EObjectType.HERO || template.Type == EObjectType.HERO_PLACEHOLDER)
                {
                    if (mapObject is HeroInstance hero)
                    {
                        gameMap.Heroes.Add(hero);
                    }
                    else
                    {
                        // Debug: object has HERO type but is not HeroInstance - log the actual type
                        Debug.WriteLine("[GameMap] WARNING: Object at ({0},{1}) has type {2} but runtime type is {3}, not HeroInstance",
                            position.PosX, position.PosY, template.Type, mapObject.GetType().Name);
                    }
                }
            }

            return gameMap;
        }

        public int Width
        {
            get; private set;
        }

        public int Height
        {
            get; private set;
        }

        public TerrainTile[,] TerrainTiles
        {
            get; set;
        }

        /// <summary>
        /// Including: decoration, visitable objects, e.g. Town/CreatureGenerator/Mines
        /// </summary>
        public List<CGObject> Objects
        {
            get; set;
        }

        /// <summary>
        /// Including: resources, monsters, artifacts, etc.
        /// </summary>
        public List<CGObject> VisitableObjects
        {
            get; set;
        }

        public List<HeroInstance> Heroes
        {
            get; set;
        }

        /// <summary>
        /// Resolves RANDOM_MONSTER / RANDOM_MONSTER_L1~L7 placeholders into actual monsters.
        /// The resolved creature must have a matching MONSTER template already in the map's
        /// template list, so that the correct AnimationFile (DEF) is used for rendering.
        /// </summary>
        private static void RandomizeMonsters(H3Map h3Map, Random random)
        {
            // Build a lookup: creatureId (SubId) -> ObjectTemplate for MONSTER type
            // from the map's pre-loaded template list
            var monsterTemplateBySubId = new Dictionary<int, ObjectTemplate>();
            foreach (var tmpl in h3Map.ObjectTemplates)
            {
                if (tmpl.Type == EObjectType.MONSTER && !monsterTemplateBySubId.ContainsKey(tmpl.SubId))
                {
                    monsterTemplateBySubId[tmpl.SubId] = tmpl;
                }
            }

            // Collect the set of creature IDs that have templates available in this map
            var availableCreatureIds = new HashSet<int>(monsterTemplateBySubId.Keys);

            System.Console.WriteLine("[RandomizeMonsters] Found {0} MONSTER templates in map, available creature IDs: [{1}]",
                monsterTemplateBySubId.Count, string.Join(", ", availableCreatureIds));

            int randomizedCount = 0;
            foreach (var obj in h3Map.Objects)
            {
                if (!(obj is CGCreature creature))
                    continue;

                ObjectTemplate template = creature.Template;
                if (template == null)
                    continue;

                int level = CreatureDatabase.GetRandomMonsterLevel(template.Type);
                if (level < 0)
                    continue; // Not a random monster type

                // Resolve the random monster placeholder to a concrete creature
                ECreatureId resolvedCreatureId = ResolveRandomCreature(level, random, availableCreatureIds);

                if (resolvedCreatureId == ECreatureId.NONE)
                {
                    System.Console.WriteLine("[RandomizeMonsters] WARNING: Could not resolve {0} - no matching MONSTER template in map", template.Type);
                    continue;
                }

                // Update the stack with the resolved creature type
                if (creature.GuardArmy != null && creature.GuardArmy.Stacks.Count > 0)
                {
                    creature.GuardArmy.Stacks[0].CreatureId = resolvedCreatureId;
                }

                int creatureIndex = (int)resolvedCreatureId;

                // Replace template with the matching MONSTER template from the map
                creature.Template = monsterTemplateBySubId[creatureIndex];
                creature.SubId = creatureIndex;
                randomizedCount++;

                System.Console.WriteLine("[RandomizeMonsters] Randomized {0} -> MONSTER (CreatureId={1}, DEF={2})",
                    creature.OriginalObjectType, resolvedCreatureId, creature.Template.AnimationFile);
            }

            System.Console.WriteLine("[RandomizeMonsters] Total randomized: {0}", randomizedCount);
        }

        /// <summary>
        /// Resolves a random monster placeholder to a concrete creature ID.
        /// Override this method to customize random creature selection logic.
        /// </summary>
        /// <param name="level">0 for any level (RANDOM_MONSTER), 1-7 for specific level</param>
        /// <param name="random">Random number generator</param>
        /// <param name="availableCreatureIds">Set of creature IDs that have templates in the map</param>
        /// <returns>The resolved creature ID, or NONE if no match found</returns>
        private static ECreatureId ResolveRandomCreature(int level, Random random, HashSet<int> availableCreatureIds)
        {
            // DEBUG: Force all random monsters to Black Dragon for testing
            // TODO: Replace with proper random selection once rendering is confirmed working
            if (availableCreatureIds.Contains((int)ECreatureId.BLACK_DRAGON))
            {
                return ECreatureId.BLACK_DRAGON;
            }

            // Fallback to filtered random selection
            if (level == 0)
            {
                return CreatureDatabase.GetRandomCreatureWithFilter(random, availableCreatureIds);
            }
            else
            {
                return CreatureDatabase.GetRandomCreatureOfLevelWithFilter(level, random, availableCreatureIds);
            }
        }
    }
}
