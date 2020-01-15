using H3Engine.Components.Data;
using H3Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.MapProviders
{
    public class MapPathFinder
    {
        private GameMap gameMap = null;

        private MapPathNodeInfo[,] nodeInfoMetrix = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameMap"></param>
        public MapPathFinder(GameMap gameMap)
        {

        }


        public HashSet<MapPathNode> SearchAllHeroMoveCandidates(int heroId)
        {


            return null;
        }


        public List<MapPathNode> GetPathTo(int heroId, MapPosition targetPosition)
        {

            return null;
        }

    }
}
