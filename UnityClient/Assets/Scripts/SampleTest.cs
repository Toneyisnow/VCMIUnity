using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Utils;
using H3Engine.Mapping;
using H3Engine.Campaign;
using Assets.Scripts;

public class SampleTest : MonoBehaviour
{
    private List<Sprite> mainSprites;

    private int spriteIndex = 0;
    private int frameCount = 0;

    private bool sceneLoaded = false;

    private GameObject gObject = null;

    private static readonly string HEROES3_DATA_FOLDER = @"D:\PlayGround\Heroes3\SOD_DATA\";

    // Start is called before the first frame update
    void Start()
    {
        mainSprites = new List<Sprite>();

        // LoadImage();

        // LoadAnimation();

        LoadTerrain();

        sceneLoaded = true;
        frameCount = 0;
    }

    void LoadImage()
    {
        Engine h3Engine = Engine.GetInstance();
        h3Engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
        ImageData image = h3Engine.RetrieveImage("Bo53Muck.pcx");

        Texture2D texture = TextureStorage.GetInstance().LoadTextureFromPNGData("Bo53Muck", image.GetPNGData());
        Sprite sprite = CreateSpriteFromTexture(texture);

        GameObject go = new GameObject("SampleSprite");
        go.transform.parent = transform;
        // go.transform.position = new Vector3(10, 20, 0);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    void LoadAnimation()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");

        
        BundleImageDefinition animation = engine.RetrieveBundleImage("AVG2ele.def");
        for (int g = 0; g < animation.Groups.Count; g++)
        {
            for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
            {
                ImageData image = animation.GetImageData(g, i);
                Texture2D texture = TextureStorage.GetInstance().LoadTextureFromPNGData("AVG2ele" + g + i, image.GetPNGData());
                Sprite sprite = CreateSpriteFromTexture(texture);
                mainSprites.Add(sprite);
            }
        }

        gObject = new GameObject("SampleSprite2");
        gObject.transform.parent = transform;
        gObject.transform.position = new Vector3(0, 0, 0);

        SpriteRenderer renderer = gObject.AddComponent<SpriteRenderer>();
        renderer.sprite = mainSprites[0];
    }

    void LoadTerrain()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

        H3Campaign campaign = engine.RetrieveCampaign("good1.h3c");

        H3Map map = campaign.Scenarios[0].MapData;

        for (int xx = 0; xx < map.Header.Width; xx++)
        {
            for (int yy = 0; yy < map.Header.Height; yy++)
            {
                LoadTileAsGameObject(map, xx, yy);
            }
        }
    }

    void LoadTerrain2()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
        engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

        H3Map map = engine.LoadH3MapFile(@"D:\Temp\h3\suiyi.h3m");

        for (int xx = 0; xx < map.Header.Width; xx++)
        {
            for (int yy = 0; yy < map.Header.Height; yy++)
            {
                LoadTileAsGameObject(map, xx, yy);
            }
        }
    }

    private void LoadTileAsGameObject(H3Map map, int x, int y)
    {
        Engine engine = Engine.GetInstance();

        TerrainTile tile = map.TerrainTiles[0, x, y];

        ImageData terrainImage = engine.RetrieveTerrainImage(tile.TerrainType, tile.TerrainView);
        Texture2D terrainTexture = TextureStorage.GetInstance().LoadTerrainTexture(tile.TerrainType, tile.TerrainView, tile.TerrainRotation, terrainImage.GetPNGData(tile.TerrainRotation));
        LoadImageSprite(terrainTexture, "Terrain", x, y, 0);

        ImageData roadImage = engine.RetrieveRoadImage(tile.RoadType, tile.RoadDir);
        if (roadImage != null)
        {
            Texture2D roadTexture = TextureStorage.GetInstance().LoadRoadTexture(tile.RoadType, tile.RoadDir, tile.RoadRotation, roadImage.GetPNGData(tile.RoadRotation));
            LoadImageSprite(roadTexture, "Road", x, y, -1);
        }

        ImageData riverImage = engine.RetrieveRiverImage(tile.RiverType, tile.RiverDir);
        if (riverImage != null)
        {
            Texture2D riverTexture = TextureStorage.GetInstance().LoadRiverTexture(tile.RiverType, tile.RiverDir, tile.RiverRotation, riverImage.GetPNGData(tile.RiverRotation));
            LoadImageSprite(riverTexture, "River", x, y, -1);
        }
    }

    private void LoadImageSprite(Texture2D texture, string spriteId, int x, int y, int z)
    {
        Sprite sprite = CreateSpriteFromTexture(texture);

        GameObject g1 = new GameObject(string.Format(@"{0}-{1}-{2}", spriteId, x, y));
        g1.transform.position = new Vector3((float)(x * 0.32 - 4), 4 - (float)(y * 0.32), z);
        g1.transform.parent = transform;
        
        SpriteRenderer renderer = g1.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    private Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        return sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!sceneLoaded || mainSprites.Count == 0)
        {
            return;
        }

        if (frameCount ++ > 6)
        {
            frameCount = 0;

            SpriteRenderer renderer = gObject.GetComponent<SpriteRenderer>();
            spriteIndex = (spriteIndex + 1) % mainSprites.Count;
            renderer.sprite = mainSprites[spriteIndex];
        }
    }
}
