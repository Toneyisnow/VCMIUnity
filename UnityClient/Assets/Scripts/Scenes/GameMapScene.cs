using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine;
using System.IO;
using H3Engine.Campaign;
using H3Engine.Mapping;

public class GameMapScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Engine engine = Engine.GetInstance();

        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3bitmap.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3sprite.lod"));

        H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
        H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 3);

        Transform gameMap = transform.Find("GameMap");
        MapLoader mapLoader = gameMap.gameObject.GetComponent<MapLoader>();
        mapLoader.Initialize(map, 0);
        mapLoader.RenderMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private static string GetGameDataFilePath(string filename)
    {
        return Path.Combine(Application.streamingAssetsPath, filename);
    }
}
