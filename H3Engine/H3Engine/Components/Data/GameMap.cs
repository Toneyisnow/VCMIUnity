using H3Engine.Core;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System.Collections;
using System.Collections.Generic;

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

    }
}