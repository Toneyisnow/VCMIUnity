using H3Engine.Mapping;
using System;
using System.Collections;

namespace H3Engine.Components.Data
{

    public class GameData
    {
        // Map information
        public int MapLevelCount
        {
            get; private set;
        }

        private GameMap[] LevelMaps = null;

        // Player information


        // Event information


        public static GameData LoadFromH3Map(H3Map h3Map)
        {

            return null;
        }
        
        public GameMap MapAtLevel(int level)
        {
            if (LevelMaps == null || level < 0 || level > LevelMaps.Length)
            {
                throw new ArgumentException("MapAtLevel: level out of range.");
            }

            return LevelMaps[level];
        }

    }
}