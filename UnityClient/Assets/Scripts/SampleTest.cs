using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.DataAccess;
using H3Engine.Utils;
using H3Engine.Mapping;
using H3Engine.Campaign;
using H3Engine.MapObjects;
using H3Engine.Core;
using UnityEngine.U2D;
using H3Engine.Common;
using UnityClient.GUI.Rendering;
using UnityClient.GUI.Mapping;
using H3Engine.Components.Data;

public class SampleTest : MonoBehaviour
{
    public GameObject mapObjectPrefab = null;

    public GameObject mapArtifactPrefab = null;

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

        // LoadCampaignMap();

        // LoadSimpleImage();
        // LoadH3Image();

        // LoadImage();

        // LoadAnimation();

        // TestTextureRenderer();

        TestPlayVideo();

        // Handheld.PlayFullScreenMovie(Path.Combine(HEROES3_DATA_FOLDER, "H3X1intr.mp4"));

        sceneLoaded = true;
        frameCount = 0;
    }

    void LoadResource()
    {
        H3DataAccess h3Engine = H3DataAccess.GetInstance();
        
        h3Engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
    }

    void LoadImage()
    {
        H3DataAccess h3Engine = H3DataAccess.GetInstance();
        h3Engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        ImageData image = h3Engine.RetrieveImage("Bo53Muck.pcx");


        Texture2D texture = Texture2DExtension.LoadFromData(image);
        Sprite sprite = CreateSpriteFromTexture(texture);

        GameObject go = new GameObject("SampleSprite");
        go.transform.parent = transform;
        // go.transform.position = new Vector3(10, 20, 0);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    void LoadImage2()
    {
        H3DataAccess h3Engine = H3DataAccess.GetInstance();
        h3Engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        ImageData image = h3Engine.RetrieveImage("BoArt021.pcx");

        Texture2D texture = Texture2DExtension.LoadFromData(image);

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
        H3DataAccess engine = H3DataAccess.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));

        string imagePath1 = Path.Combine(Application.dataPath, @"Images\HPL016Rn.png");
        string imagePath2 = Path.Combine(Application.dataPath, @"Images\HPL017Rn.png");
        string imagePath3 = Path.Combine(Application.dataPath, @"Images\HPL018Rn.png");
        string imagePath4 = Path.Combine(Application.dataPath, @"Images\HPL019Rn.png");

        int width = 58;
        int height = 64;

        ImageData image1 = engine.RetrieveImage("HPL016Rn.pcx");
        Texture2D texture1 = Texture2DExtension.LoadFromData(image1);

        ImageData image2 = engine.RetrieveImage("HPL017Rn.pcx");
        Texture2D texture2 = Texture2DExtension.LoadFromData(image2);

        ImageData image3 = engine.RetrieveImage("HPL018Rn.pcx");
        Texture2D texture3 = Texture2DExtension.LoadFromData(image3);

        ImageData image4 = engine.RetrieveImage("HPL019Rn.pcx");
        Texture2D texture4 = Texture2DExtension.LoadFromData(image4);

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

    void TestPlayVideo()
    {
        GameObject camera = GameObject.Find("Main Camera");
        var videoPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.playOnAwake = false;

        // videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;
        // videoPlayer.targetCameraAlpha = 0.5F;

        videoPlayer.url = Path.Combine(HEROES3_DATA_FOLDER, "H3X1intr.mp4");
        videoPlayer.frame = 100;
        videoPlayer.isLooping = true;

        videoPlayer.Play();
    }

    void LoadAnimation()
    {
        H3DataAccess engine = H3DataAccess.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));


        BundleImageDefinition animation = engine.RetrieveBundleImage("AVG2ele.def");
        for (int g = 0; g < animation.Groups.Count; g++)
        {
            for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
            {
                ImageData image = animation.GetImageData(g, i);
                Texture2D texture = Texture2DExtension.LoadFromData(image);
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
        H3DataAccess engine = H3DataAccess.GetInstance();
        engine.LoadArchiveFile(GetGameDataFilePath("h3ab_spr.lod"));


        BundleImageDefinition animation = engine.RetrieveBundleImage("AVG2ele.def");

        GameObject obj = Instantiate(mapObjectPrefab, transform);
        obj.transform.position = new Vector3(0, 0, -1);

        AdventureMapCoordinate mapCoordinate = obj.GetComponent<AdventureMapCoordinate>();
        mapCoordinate.Initialize(animation.Width, animation.Height);

        AnimatedObject animated = obj.GetComponent<AnimatedObject>();
        animated.Initialize(animation);
    }

    void LoadCampaignMap()
    {
        H3DataAccess engine = H3DataAccess.GetInstance();

        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_bmp.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3ab_spr.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3bitmap.lod"));
        engine.LoadArchiveFile(GetGameDataFilePath("H3sprite.lod"));

        H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
        H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 0);
        GameMap gameMap = GameMap.LoadFromH3Map(map, 0);

        Transform gameMapGO = transform.Find("GameMap");
        MapLoader mapLoader = gameMapGO.gameObject.GetComponent<MapLoader>();
        mapLoader.Initialize(gameMap);
        mapLoader.RenderMap();
        

    }

    private void RenderTileMapByTerrainType(H3Map map, ETerrainType terrainType, MapTextureManager mapTextureManager)
    {
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
                    
                    Sprite sprite = mapTextureManager.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                    var position = GetMapPositionInPixel(xx, yy, 10);
                    //// position.z += (float)terrainType.GetHashCode() / 10;

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

                    Sprite sprite = null; // textureStorage.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
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

                    Sprite sprite = null; // textureStorage.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);
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
        H3DataAccess engine = H3DataAccess.GetInstance();
        
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
                    //// LoadTileAsGameObject(map, xx, yy, a);
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
                else if (template.Type == EObjectType.ARTIFACT)
                {
                    LoadAnimatedMapObject(position.PosX, position.PosY, bundleImage);
                }
                else
                {
                    // Load Single Image
                    ImageData image = bundleImage.GetImageData(0, 0);
                    Texture2D objTexture = Texture2DExtension.LoadFromData(image);
                    LoadImageSprite(objTexture, "Obj" + objectIndex, position.PosX, position.PosY, -2);
                }

                objectIndex++;
            }
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
        if (Input.touchCount > 0)
        {
            
        }


        if (!sceneLoaded)
        {
            return;
        }
    }
}
