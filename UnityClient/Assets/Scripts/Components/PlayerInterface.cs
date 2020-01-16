using H3Engine.Components.Data;
using H3Engine.Components.MapProviders;
using H3Engine.Components.Protocols;
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
    public class PlayerInterface : IGameInterface
    {
        public GameData GameData
        {
            get; private set;
        }

        public PlayerInterface()
        {

        }

        public static PlayerInterface StartGame(H3Map h3Map)
        {
            PlayerInterface playerIn = new PlayerInterface();
            playerIn.GameData = GameData.LoadFromH3Map(h3Map);

            return playerIn;
        }

        public static PlayerInterface LoadGame(byte[] h3Save)
        {
            PlayerInterface playerIn = new PlayerInterface();

            return playerIn;
        }

        public void SaveGame(string saveFileName)
        {
            
        }

        //////////////////// Implement the IGameInterface

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="pathNodes"></param>
        public void showHeroMoving(int heroId, List<MapPathNode> pathNodes)
        {

        }




    }
}
