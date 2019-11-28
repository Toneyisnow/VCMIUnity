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
        HEROES3_DATA_FOLDER = Path.Combine(Application.dataPath, @"Resources\GameData");

        mainSprites = new List<Sprite>();

        // LoadImage();

        // LoadAnimation();

        TestTextureRenderer();

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

    void LoadAnimation()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_sprlod"));


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
        obj.transform.position = new Vector3(0, 0, 0);

        AdventureMapCoordinate mapCoordinate = obj.GetComponent<AdventureMapCoordinate>();
        mapCoordinate.Initialize(animation.Width, animation.Height);

        AnimatedObject animated = obj.GetComponent<AnimatedObject>();
        animated.Initialize(animation);
    }

    void TestTextureRenderer()
    {
        Engine engine = Engine.GetInstance();

        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3bitmap.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3sprite.lod"));

        H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");

        H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 0);

        Dictionary<Texture2D, List<Vector2>> textureDict = new Dictionary<Texture2D, List<Vector2>>();
        Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();
        int levelCount = (map.Header.IsTwoLevel ? 2 : 1);
        for (int level = 0; level < 1; level++)
        {
            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    TerrainTile tile = map.TerrainTiles[level, xx, yy];

                    ImageData terrainImage = engine.RetrieveTerrainImage(tile.TerrainType, tile.TerrainView);
                    Texture2D terrainTexture = TextureStorage.GetInstance().LoadTerrainTexture(tile.TerrainType, tile.TerrainView, tile.TerrainRotation, terrainImage.GetPNGData(tile.TerrainRotation));

                    List<Vector2> positions = null;
                    if (textureDict.ContainsKey(terrainTexture))
                    {
                        positions = textureDict[terrainTexture];
                    }
                    else
                    {
                        positions = new List<Vector2>();
                        textureDict[terrainTexture] = positions;
                    }

                    positions.Add(new Vector2(xx, yy));
                }
            }

            Shader shader = Shader.Find("Sprites/Default");
            foreach (Texture2D texture in textureDict.Keys)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetTexture("tl", texture);

                Material material = new Material(shader);
                //// material.enableInstancing = true;
                material.mainTexture = texture;
                
                List<Vector2> positions = textureDict[texture];
                foreach (Vector2 pos in positions)
                {
                    string spriteKey = string.Format(@"{0}-{1}-{2}", "tl", pos.x, pos.y);
                    //// Sprite sprite = null;

                    /*
                    if (spriteDictionary.ContainsKey(spriteKey))
                    {
                        sprite = spriteDictionary[spriteKey];
                    }
                    else
                    {
                        sprite = CreateSpriteFromTexture(texture);
                        spriteDictionary[spriteKey] = sprite;
                    }
                    */

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                    GameObject g1 = new GameObject();
                    g1.transform.position = GetMapPositionInPixel((int)pos.x, (int)pos.y, -1);
                    g1.transform.parent = transform;
                    
                    SpriteRenderer renderer = g1.AddComponent<SpriteRenderer>();
                    renderer.drawMode = SpriteDrawMode.Simple;
                    
                    renderer.sharedMaterial = material;
                    renderer.sprite = sprite;

                    //// renderer.sprite = sprite;

                    //// renderer.sharedMaterial.SetTexture(spriteKey, terrainTexture);
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
