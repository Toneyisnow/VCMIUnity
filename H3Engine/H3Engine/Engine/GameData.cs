// Moved from H3Engine.Components.Data.GameData 鈫?H3Engine.Engine

using H3Engine.Mapping;
using System;
using H3Engine.Core.Constants;

namespace H3Engine.Engine
{
    /// <summary>
    /// Top-level game state container: holds one or two level maps and
    /// global per-game data (players, events, 鈥?.
    /// </summary>
    public class GameData
    {
        public int MapLevelCount
        {
            get; private set;
        }

        private GameMap[] LevelMaps = null;

        public static GameData LoadFromH3Map(H3Map h3Map)
        {
            GameData gameData = new GameData();

            int mapLevel = (h3Map.Header.IsTwoLevel ? 2 : 1);
            gameData.LevelMaps = new GameMap[mapLevel];

            for (int level = 0; level < mapLevel; level++)
            {
                gameData.LevelMaps[level] = GameMap.LoadFromH3Map(h3Map, level);
            }

            gameData.MapLevelCount = mapLevel;
            return gameData;
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


