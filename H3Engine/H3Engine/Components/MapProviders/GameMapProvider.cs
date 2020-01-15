using H3Engine.Components.Data;
using H3Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.MapProviders
{
    public class GameMapProvider
    {
        public GameMap Map
        {
            get; private set;
        }

        public MapPathFinder PathFinder
        {
            get; private set;
        }

        public GameMapProvider(GameMap gameMap)
        {
            this.Map = gameMap;
            this.PathFinder = new MapPathFinder(gameMap);
        }

        /// <summary>
        /// Calculate the move path for hero, only returns the path for today, might be stopped by events
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="mapPosition"></param>
        /// <returns></returns>
        public List<MapPathNode> GetHeroMovePath(int heroId, MapPosition mapPosition)
        {
            List<MapPathNode> mapPath = this.PathFinder.GetPathTo(heroId, mapPosition);

            // Trim the path to only today's path
            List<MapPathNode> result = new List<MapPathNode>();

            return result;
        }

    }
}
