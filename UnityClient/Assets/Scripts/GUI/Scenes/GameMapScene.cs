using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        private GameManager gameManager = null;

        private GamerMapControl gamerMapControl = null;

        private MenuControl menuControl = null;


        // Start is called before the first frame update
        void Start()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_bmp.lod"));
            engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_spr.lod"));
            engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3bitmap.lod"));
            engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3sprite.lod"));

            H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
            H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 3);

            gameManager = GameManager.StartGame(map);

            gamerMapControl = new GamerMapControl();


            GameObject gameMap = GameObject.Find("GameMap");
            MapLoader mapLoader = gameMap.GetComponent<MapLoader>();
            mapLoader.Initialize(map, 0);
            mapLoader.RenderMap();
        }

        // Update is called once per frame
        void Update()
        {

        }
        
    }
}