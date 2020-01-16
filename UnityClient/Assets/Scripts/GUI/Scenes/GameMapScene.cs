using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3Engine.DataAccess;
using H3Engine.Components.Data;

using H3Engine;
using System.IO;
using H3Engine.Campaign;
using H3Engine.Mapping;
using Assets.Scripts.Utils;
using UnityClient.GUI.Mapping;
using UnityClient.Components;
using UnityClient.Components.Mapping;

namespace UnityClient.GUI.Scenes
{
    public class GameMapScene : MonoBehaviour
    {
        private PlayerInterface playerInterface = null;

        private GamerMapControl gamerMapControl = null;

        private MenuControl menuControl = null;


        // Start is called before the first frame update
        void Start()
        {
            H3DataAccess dataAccess = H3DataAccess.GetInstance();

            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_bmp.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_spr.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3bitmap.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3sprite.lod"));

            H3Campaign campaign = dataAccess.RetrieveCampaign("slayer.h3c");
            H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 3);


            playerInterface = PlayerInterface.StartGame(map);

            gamerMapControl = new GamerMapControl();

            GameMap gameMap = playerInterface.GameData.MapAtLevel(0);

            GameObject gameMapUI = GameObject.Find("GameMap");
            MapLoader mapLoader = gameMapUI.GetComponent<MapLoader>();
            mapLoader.Initialize(gameMap);
            mapLoader.RenderMap();
        }

        // Update is called once per frame
        void Update()
        {

        }
        
    }
}