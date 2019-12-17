using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Utils;
using H3Engine.Mapping;
using H3Engine.Campaign;
using Assets.Scripts.Components;
using H3Engine.MapObjects;
using H3Engine.Core;
using UnityEngine.U2D;
using H3Engine.Common;

public class SampleTest : MonoBehaviour
{
    public GameObject mapObjectPrefab = null;


    private List<Sprite> mainSprites;

    private int spriteIndex = 0;
    private int frameCount = 0;

    private bool sceneLoaded = false;

    private GameObject gObject = null;

    private static string HEROES3_DATA_FOLDER = string.Empty;

    // Start is called before the first frame update

    private static string GetGameDataFilePath(string filename)
    {
        return Path.Combine(HEROES3_DATA_FOLDER, filename);
    }

    void Start()
    {
        //// HEROES3_DATA_FOLDER = Path.Combine(Application.dataPath, @"Resources\GameData");
        HEROES3_DATA_FOLDER = Application.streamingAssetsPath;

        mainSprites = new List<Sprite>();

        TestTileMap();

        // LoadSimpleImage();
        // LoadH3Image();

        // LoadImage();

        // LoadAnimation();

        // TestTextureRenderer();

        sceneLoaded = true;
        frameCount = 0;
    }

    void LoadResource()
    {
        Engine h3Engine = Engine.GetInstance();
        
        h3Engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
    }

    void LoadImage()
    {
        Engine h3Engine = Engine.GetInstance();
        h3Engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        ImageData image = h3Engine.RetrieveImage("Bo53Muck.pcx");

        Texture2D texture = TextureStorage.GetInstance().LoadTextureFromPNGData("Bo53Muck", image.GetPNGData());
        Sprite sprite = CreateSpriteFromTexture(texture);

        GameObject go = new GameObject("SampleSprite");
        go.transform.parent = transform;
        // go.transform.position = new Vector3(10, 20, 0);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    void LoadImage2()
    {
        Engine h3Engine = Engine.GetInstance();
        h3Engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        ImageData image = h3Engine.RetrieveImage("BoArt021.pcx");
        image.ExportDataToPNG();

        Texture2D texture = TextureStorage.GetInstance().LoadTextureFromPNGData("BoArt021", image.GetPNGData());
        //// Sprite sprite = CreateSpriteFromTexture(texture);

        GameObject go = new GameObject("SampleSprite");
        go.transform.parent = transform;
        // go.transform.position = new Vector3(10, 20, 0);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        ////renderer.sharedMaterial.mainTexture = texture;

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        renderer.sprite = sprite;
    }

    void LoadSimpleImage()
    {
        string imagePath1 = Path.Combine(Application.dataPath, @"Images\HPL016Rn.png");
        string imagePath2 = Path.Combine(Application.dataPath, @"Images\HPL017Rn.png");
        string imagePath3 = Path.Combine(Application.dataPath, @"Images\HPL018Rn.png");
        string imagePath4 = Path.Combine(Application.dataPath, @"Images\HPL019Rn.png");

        int width = 58;
        int height = 64;
        Texture2D texture1 = new Texture2D(width, height);
        texture1.LoadImage(File.ReadAllBytes(imagePath1));

        Texture2D texture2 = new Texture2D(width, height);
        texture2.LoadImage(File.ReadAllBytes(imagePath2));

        Texture2D texture3 = new Texture2D(width, height);
        texture3.LoadImage(File.ReadAllBytes(imagePath3));

        Texture2D texture4 = new Texture2D(width, height);
        texture4.LoadImage(File.ReadAllBytes(imagePath4));

        Texture2D[] textures = new Texture2D[4] { texture1 , texture2 , texture3 , texture4 };

        Texture2D atlas = new Texture2D(512, 512);
        Rect[] rects = atlas.PackTextures(textures, 2);
        

        for(int i = 0; i < 4; i++)
        {
            Sprite sprite = Sprite.Create(atlas, new Rect(rects[i].xMin * atlas.width, rects[i].yMin * atlas.height, rects[i].width * atlas.width, rects[i].height * atlas.height), Vector2.zero);

            GameObject go = new GameObject("SampleSprite");
            go.transform.parent = transform;
            go.transform.position = new Vector3((float)0.8 * i - 1, 0, 0);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
        }
    }

    void LoadH3Image()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));

        string imagePath1 = Path.Combine(Application.dataPath, @"Images\HPL016Rn.png");
        string imagePath2 = Path.Combine(Application.dataPath, @"Images\HPL017Rn.png");
        string imagePath3 = Path.Combine(Application.dataPath, @"Images\HPL018Rn.png");
        string imagePath4 = Path.Combine(Application.dataPath, @"Images\HPL019Rn.png");

        int width = 58;
        int height = 64;

        ImageData image1 = engine.RetrieveImage("HPL016Rn.pcx");
        image1.ExportDataToPNG();
        Texture2D texture1 = TextureStorage.GetInstance().LoadTextureFromPNGData("HPL016Rn", image1.GetPNGData());

        ImageData image2 = engine.RetrieveImage("HPL017Rn.pcx");
        image2.ExportDataToPNG();
        Texture2D texture2 = TextureStorage.GetInstance().LoadTextureFromPNGData("HPL017Rn", image2.GetPNGData());

        ImageData image3 = engine.RetrieveImage("HPL018Rn.pcx");
        image3.ExportDataToPNG();
        Texture2D texture3 = TextureStorage.GetInstance().LoadTextureFromPNGData("HPL018Rn", image3.GetPNGData());

        ImageData image4 = engine.RetrieveImage("HPL019Rn.pcx");
        image4.ExportDataToPNG();
        Texture2D texture4 = TextureStorage.GetInstance().LoadTextureFromPNGData("HPL019Rn", image4.GetPNGData());
        

        Texture2D[] textures = new Texture2D[4] { texture1, texture2, texture3, texture4 };

        Texture2D atlas = new Texture2D(512, 512);
        Rect[] rects = atlas.PackTextures(textures, 2);


        for (int i = 0; i < 4; i++)
        {
            Sprite sprite = Sprite.Create(atlas, new Rect(rects[i].xMin * atlas.width, rects[i].yMin * atlas.height, rects[i].width * atlas.width, rects[i].height * atlas.height), Vector2.zero);

            GameObject go = new GameObject("SampleSprite");
            go.transform.parent = transform;
            go.transform.position = new Vector3((float)0.8 * i - 1, 0, 0);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
        }
    }

    void LoadAnimation()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));


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

    void LoadAnimationToGameObject()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));


        BundleImageDefinition animation = engine.RetrieveBundleImage("AVG2ele.def");

        GameObject obj = Instantiate(mapObjectPrefab, transform);
        obj.transform.position = new Vector3(0, 0, -1);

        AdventureMapCoordinate mapCoordinate = obj.GetComponent<AdventureMapCoordinate>();
        mapCoordinate.Initialize(animation.Width, animation.Height);

        AnimatedObject animated = obj.GetComponent<AnimatedObject>();
        animated.Initialize(animation);
    }

    void TestTileMap()
    {
        Engine engine = Engine.GetInstance();

        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3bitmap.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3sprite.lod"));

        H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
        H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 0);

        for (int i = 0; i <= 9; i++)
        {
            RenderTileMapByTerrainType(map, (ETerrainType)i);
        }

        RenderTileMapByRoadType(map, ERoadType.DIRT_ROAD);
        RenderTileMapByRoadType(map, ERoadType.GRAVEL_ROAD);
        RenderTileMapByRoadType(map, ERoadType.COBBLESTONE_ROAD);


        RenderTileMapByRiverType(map, ERiverType.CLEAR_RIVER);
        RenderTileMapByRiverType(map, ERiverType.LAVA_RIVER);

        int objectIndex = 0;
        foreach (CGObject obj in map.Objects)
        {
            ObjectTemplate template = obj.Template;
            if (template == null)
            {
                continue;
            }

            MapPosition position = obj.Position;

            BundleImageDefinition bundleImage = engine.RetrieveBundleImage(template.AnimationFile);

            if (template.Type == H3Engine.Common.EObjectType.MONSTER
                || template.Type == H3Engine.Common.EObjectType.CAMPFIRE
                || template.Type == H3Engine.Common.EObjectType.LEARNING_STONE
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR1
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR2
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR3
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR4
                )
            {
                // Load Animation
                LoadAnimatedMapObject(position.PosX, position.PosY, bundleImage);
            }
            else
            {
                // Load Single Image
                ImageData image = bundleImage.GetImageData(0, 0);
                //// image.ExportDataToPNG();

                //// Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromPNGData(template.AnimationFile, image.GetPNGData());
                Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromImage(template.AnimationFile, image);
                LoadImageSprite(objTexture, "Obj" + objectIndex, position.PosX, position.PosY, 0);
            }

            objectIndex++;
        }



    }

    private void RenderTileMapByTerrainType(H3Map map, ETerrainType terrainType)
    {
        TextureStorage textureStorage = TextureStorage.GetInstance();

        int levelCount = (map.Header.IsTwoLevel ? 2 : 1);
        for (int level = 0; level < 1; level++)
        {
            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    TerrainTile tile = map.TerrainTiles[level, xx, yy];
                    if (tile.TerrainType != terrainType)
                    {
                        continue;
                    }

                    Sprite sprite = textureStorage.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                    var position = GetMapPositionInPixel(xx, yy, 1);
                    position.z += (float)terrainType.GetHashCode() / 10;

                    Transform child = transform.Find("TL-" + terrainType.ToString());
                    if (child == null)
                    {
                        GameObject terrain = new GameObject();
                        terrain.name = "TL-" + terrainType.ToString();
                        terrain.transform.parent = transform;

                        //// terrain.transform.localPosition = new Vector3(0, 0, terrainType.GetHashCode());
                    }

                    GameObject g1 = new GameObject();
                    g1.transform.localPosition = position;
                    g1.transform.parent = child;

                    SpriteRenderer renderer = g1.AddComponent<SpriteRenderer>();
                    renderer.drawMode = SpriteDrawMode.Tiled;
                    renderer.sprite = sprite;
                }
            }
        }
    }

    private void RenderTileMapByRoadType(H3Map map, H3Engine.Common.ERoadType roadType)
    {
        TextureStorage textureStorage = TextureStorage.GetInstance();

        int levelCount = (map.Header.IsTwoLevel ? 2 : 1);
        for (int level = 0; level < 1; level++)
        {
            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    TerrainTile tile = map.TerrainTiles[level, xx, yy];
                    if (tile.RoadType != ERoadType.NO_ROAD)
                    {
                        int here = 1;
                    }

                    if (tile.RoadType != roadType)
                    {
                        continue;
                    }

                    Sprite sprite = textureStorage.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
                    var position = GetMapPositionInPixel(xx, yy, 1);

                    GameObject g1 = new GameObject();
                    g1.transform.position = position;
                    g1.transform.parent = transform;

                    SpriteRenderer renderer = g1.AddComponent<SpriteRenderer>();
                    renderer.drawMode = SpriteDrawMode.Tiled;
                    renderer.sprite = sprite;
                }
            }
        }
    }

    private void RenderTileMapByRiverType(H3Map map, H3Engine.Common.ERiverType riverType)
    {
        TextureStorage textureStorage = TextureStorage.GetInstance();

        int levelCount = (map.Header.IsTwoLevel ? 2 : 1);
        for (int level = 0; level < 1; level++)
        {
            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    TerrainTile tile = map.TerrainTiles[level, xx, yy];
                    if (tile.RiverType != riverType)
                    {
                        continue;
                    }

                    Sprite sprite = textureStorage.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);
                    var position = GetMapPositionInPixel(xx, yy, 1);

                    GameObject g1 = new GameObject();
                    g1.transform.position = position;
                    g1.transform.parent = transform;

                    SpriteRenderer renderer = g1.AddComponent<SpriteRenderer>();
                    renderer.drawMode = SpriteDrawMode.Tiled;
                    renderer.sprite = sprite;
                }
            }
        }
    }

    void LoadTerrain()
    {
        Engine engine = Engine.GetInstance();
        
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3bitmap.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3sprite.lod"));

        H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");

        H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 0);

        int levelCount = (map.Header.IsTwoLevel ? 2 : 1);
        for (int a = 0; a < 1; a++)
        {
            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    LoadTileAsGameObject(map, xx, yy, a);
                }
            }

            int objectIndex = 0;
            foreach (CGObject obj in map.Objects)
            {
                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }

                MapPosition position = obj.Position;

                BundleImageDefinition bundleImage = engine.RetrieveBundleImage(template.AnimationFile);

                if (template.Type == H3Engine.Common.EObjectType.MONSTER
                    || template.Type == H3Engine.Common.EObjectType.CAMPFIRE
                    || template.Type == H3Engine.Common.EObjectType.LEARNING_STONE
                    || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR1
                    || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR2
                    || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR3
                    || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR4
                    )
                {
                    // Load Animation
                    LoadAnimatedMapObject(position.PosX, position.PosY, bundleImage);
                }
                else
                {
                    // Load Single Image
                    ImageData image = bundleImage.GetImageData(0, 0);
                    image.ExportDataToPNG();

                    Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromPNGData(template.AnimationFile, image.GetPNGData());
                    LoadImageSprite(objTexture, "Obj" + objectIndex, position.PosX, position.PosY, -2);
                }

                objectIndex++;
            }
        }
    }

    void LoadTerrain2()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3bitmap.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3sprite.lod"));

        H3Map map = engine.LoadH3MapFile(@"D:\PlayGround\Heroes3\suiyi.h3m");

        for (int a = 0; a < (map.Header.IsTwoLevel ? 1 : 1); a++)
        {
            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    LoadTileAsGameObject(map, xx, yy, a);
                }
            }

            int objectIndex = 0;
            foreach (CGObject obj in map.Objects)
            {
                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }

                MapPosition position = obj.Position;

                BundleImageDefinition bundleImage = engine.RetrieveBundleImage(template.AnimationFile);
                ImageData image = bundleImage.GetImageData(0, 0);
                image.ExportDataToPNG();

                Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromPNGData(template.AnimationFile, image.GetPNGData());
                LoadImageSprite(objTexture, "Obj" + objectIndex, position.PosX, position.PosY, -2);
                
                objectIndex++;
            }
        }
    }

    private void LoadTileAsGameObject(H3Map map, int x, int y, int level)
    {
        Engine engine = Engine.GetInstance();

        TerrainTile tile = map.TerrainTiles[level, x, y];

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
        g1.transform.position = GetMapPositionInPixel(x, y, z);
        g1.transform.parent = transform;
        
        SpriteRenderer renderer = g1.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        
    }

    private void LoadAnimatedMapObject(int x, int y, BundleImageDefinition bundleImage)
    {
        GameObject newObject = Instantiate(mapObjectPrefab, transform);
        newObject.transform.position = GetMapPositionInPixel(x, y, -2);
        
        //AdventureMapCoordinate mapCoordinate = newObject.GetComponent<AdventureMapCoordinate>();
        //mapCoordinate.Initialize(bundleImage.Width, bundleImage.Height);

        AnimatedObject animated = newObject.GetComponent<AnimatedObject>();
        animated.Initialize(bundleImage);
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

    private Vector3 GetMapPositionInPixel(int x, int y, int z)
    {
        return new Vector3((float)(x * 0.32 - 4), 4 - (float)(y * 0.32), z - (float)(y * 0.01));
    }

    // Update is called once per frame
    void Update()
    {
        if (!sceneLoaded)
        {
            return;
        }
    }
}
