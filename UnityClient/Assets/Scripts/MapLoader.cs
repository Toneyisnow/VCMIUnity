using H3Engine;
using H3Engine.Common;
using H3Engine.Core;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    private H3Map h3Map = null;

    private Engine h3Engine = null;

    private MapTextureManager mapTextureManager = null;

    public void Initialize(H3Map h3Map)
    {
        this.h3Engine = Engine.GetInstance();

        this.h3Map = h3Map;
        this.mapTextureManager = new MapTextureManager(h3Map);
    }

    public void RenderMap()
    {
        this.mapTextureManager.PreloadTextures();

        RenderTerrain();
        RenderRoad();
        RenderRiver();

        RenderMapObjects();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RenderTerrain()
    {
        for (int xx = 0; xx < h3Map.Header.Width; xx++)
        {
            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];

                Sprite sprite = mapTextureManager.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                var position = GetMapPositionInPixel(xx, yy, 10);
                CreateSubChildObject("Terrain", GetMapPositionInPixel(xx, yy, 10), sprite);
            }
        }
    }

    private void RenderRoad()
    {
        for (int xx = 0; xx < h3Map.Header.Width; xx++)
        {
            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                Sprite sprite = mapTextureManager.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
                var position = GetMapPositionInPixel(xx, yy, 9);
                CreateSubChildObject("Road", GetMapPositionInPixel(xx, yy, 9), sprite);
            }
        }
    }

    private void RenderRiver()
    {
        for (int xx = 0; xx < h3Map.Header.Width; xx++)
        {
            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                Sprite sprite = mapTextureManager.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);

                CreateSubChildObject("River", GetMapPositionInPixel(xx, yy, 8), sprite);
            }
        }
    }

    private void RenderMapObjects()
    {
        int objectIndex = 0;
        foreach (CGObject obj in h3Map.Objects)
        {
            ObjectTemplate template = obj.Template;
            if (template == null)
            {
                continue;
            }

            MapPosition position = obj.Position;

            BundleImageDefinition bundleImage = h3Engine.RetrieveBundleImage(template.AnimationFile);

            if (template.Type == H3Engine.Common.EObjectType.CAMPFIRE
                || template.Type == H3Engine.Common.EObjectType.LEARNING_STONE
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR1
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR2
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR3
                || template.Type == H3Engine.Common.EObjectType.CREATURE_GENERATOR4
                )
            {
                // Load Animation
                //LoadAnimatedMapObject(position.PosX, position.PosY, bundleImage);
            }
            else if (template.Type == EObjectType.MONSTER)
            {
                Sprite[] sprites = mapTextureManager.LoadMonsterSprites(template.AnimationFile);
                CreateSubChildAnimatedObject("Monsters", GetMapPositionInPixel(position.PosX, position.PosY, -9), sprites);

            }
            else if (template.Type == EObjectType.ARTIFACT)
            {
                Sprite[] sprites = mapTextureManager.LoadArtifactSprites(template.AnimationFile);
                CreateSubChildAnimatedObject("Artifacts", GetMapPositionInPixel(position.PosX, position.PosY, -9), sprites);

            }
            else if (MapTextureManager.IsDecoration(template.Type.GetHashCode()))     // Map Decorations
            {
                Sprite sprite = mapTextureManager.LoadDecorationSprite(template.AnimationFile, template.Type.GetHashCode());
                CreateSubChildObject("Decorations", GetMapPositionInPixel(position.PosX, position.PosY, -1), sprite);

                //GameObject newObject = Instantiate(mapArtifactPrefab, transform);
                //newObject.transform.position = GetMapPositionInPixel(position.PosX, position.PosY, 2);

                //Sprite sprite = textureStorage.LoadMapDecorationSprite(template.AnimationFile, template.Type.GetHashCode());

                //AdventureMapCoordinate mapCoordinate = newObject.GetComponent<AdventureMapCoordinate>();
                //mapCoordinate.Initialize(bundleImage.Width, bundleImage.Height);

                //AnimatedMapObject animated = newObject.GetComponent<AnimatedMapObject>();
                // animated.Initialize(sprite);

            }
            else
            {
                // Load Single Image
                ////ImageData image = bundleImage.GetImageData(0, 0);
                //// image.ExportDataToPNG();

                //// Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromPNGData(template.AnimationFile, image.GetPNGData());
                ////Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromImage(template.AnimationFile, image);
                ////LoadImageSprite(objTexture, "Obj" + objectIndex, position.PosX, position.PosY, 0);
            }

            objectIndex++;
        }
    }

    private Vector3 GetMapPositionInPixel(int x, int y, int z)
    {
        ////return new Vector3((float)(x * 0.32 - 4), 4 - (float)(y * 0.32), z - (float)(y * 0.01));
        return new Vector3((float)(x * 0.32 - 4), 4 - (float)(y * 0.32), z);
    }

    private GameObject CreateSubChildObject(string subName, Vector3 position, Sprite sprite)
    {
        GameObject newObject = new GameObject();
        newObject.transform.position = position;
        
        Transform child = transform.Find(subName);
        if (child == null)
        {
            GameObject artifacts = new GameObject();
            artifacts.name = subName;
            artifacts.transform.parent = transform;
            child = artifacts.transform;
        }

        newObject.transform.parent = child;

        SpriteRenderer renderer = newObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        return newObject;
    }

    private GameObject CreateSubChildAnimatedObject(string subName, Vector3 position, Sprite[] sprites)
    {
        GameObject newObject = new GameObject();
        newObject.transform.position = position;

        Transform child = transform.Find(subName);
        if (child == null)
        {
            GameObject artifacts = new GameObject();
            artifacts.name = subName;
            artifacts.transform.parent = transform;
            child = artifacts.transform;
        }

        newObject.transform.parent = child;

        newObject.AddComponent<SpriteRenderer>();
        AnimatedMapObject animated = newObject.AddComponent<AnimatedMapObject>();
        animated.Initialize(sprites);

        return newObject;
    }
}
