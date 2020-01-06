using H3Engine.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityClient.Components.Data;
using UnityClient.Components.Mapping;

namespace UnityClient.Components
{
    public class GameManager
    {
        private H3Map h3Map;

        private GameData gameData = null;
        private GameMapProvider groundMapProvider = null;
        private GameMapProvider underGroundMapProvider = null;

        public GameManager()
        {

        }

        public GameMap ActiveMap
        {
            get
            {
                return groundMapProvider.Map;
            }
        }

        public static GameManager StartGame(H3Map h3Map)
        {
            GameManager manager = new GameManager();
            manager.h3Map = h3Map;

            return manager;
        }

        public static GameManager LoadGame(byte[] h3Save)
        {
            GameManager manager = new GameManager();

            return manager;
        }

        public void SaveGame(string saveFileName)
        {
            
        }

        public bool ValidatePlayerExecution()
        {
            return true;
        }

        public void DoPlayerExecution()
        {

        }




    }
}
