using H3Engine.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityClient.Components.Data;

namespace UnityClient.Components.Mapping
{
    /// <summary>
    /// 
    /// </summary>
    public class GameMapProvider
    {
        private GameMap gameMap;

        public GameMapProvider(GameMap gameMap)
        {
            this.gameMap = gameMap;
        }

        public static GameMapProvider LoadFromH3Map(H3Map h3Map, int level)
        {
            return null;
        }

        public GameMap Map
        {
            get
            {
                return gameMap;
            }
        }

    }
}
