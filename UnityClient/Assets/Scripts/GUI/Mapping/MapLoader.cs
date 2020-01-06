using H3Engine;
using H3Engine.Common;
using H3Engine.Core;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityClient.GUI.Rendering;
using UnityEngine;


namespace UnityClient.GUI.Mapping
{

    public class MapLoader : MonoBehaviour
    {
        private static float ZOrder_Terrain = 10;
        private static float ZOrder_RoadRiver = 9;
        private static float ZOrder_Decoration = 6;
        private static float ZOrder_Resource = 5;
        private static float ZOrder_Artifact = 5;
        private static float ZOrder_Building = -5;
        private static float ZOrder_Creature = -8;
        private static float ZOrder_Hero = -9;
        private static float ZOrder_Fog = -10;


        private H3Map h3Map = null;

        private int mapLevel = 0;

        private Engine h3Engine = null;

        private MapTextureManager mapTextureManager = null;

        public void Initialize(H3Map h3Map, int mapLevel)
        {
            this.h3Engine = Engine.GetInstance();

            this.h3Map = h3Map;
            this.mapLevel = mapLevel;

            this.mapTextureManager = new MapTextureManager(h3Map);
        }

        public void RenderMap()
        {
            var lastTime = DateTime.Now;

            this.mapTextureManager.PreloadTextures();

            print("PreloadTextures:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderTerrain();
            print("RenderTerrain:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderRoad();
            print("RenderRoad:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderRiver();
            print("RenderRiver:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderMapObjects();
            print("RenderMapObjects:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

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
                    TerrainTile tile = h3Map.TerrainTiles[mapLevel, xx, yy];
                    Sprite sprite = mapTextureManager.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);

                    GameObject gametile = CreateSubChildObject("Terrain", GetMapPositionInPixel(xx, yy, ZOrder_Terrain), sprite);
                    MapTile mapTileComponent = gametile.AddComponent<MapTile>();
                    mapTileComponent.Initialize(new Vector2(xx, yy), null);
                }
            }
        }

        private void RenderRoad()
        {
            for (int xx = 0; xx < h3Map.Header.Width; xx++)
            {
                for (int yy = 0; yy < h3Map.Header.Height; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[mapLevel, xx, yy];
                    Sprite sprite = mapTextureManager.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
                    var position = GetMapPositionInPixel(xx, yy, 9);
                    CreateSubChildObject("Road", GetMapPositionInPixel(xx, yy, ZOrder_RoadRiver), sprite);
                }
            }
        }

        private void RenderRiver()
        {
            for (int xx = 0; xx < h3Map.Header.Width; xx++)
            {
                for (int yy = 0; yy < h3Map.Header.Height; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[mapLevel, xx, yy];
                    Sprite sprite = mapTextureManager.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);

                    CreateSubChildObject("River", GetMapPositionInPixel(xx, yy, ZOrder_RoadRiver), sprite);
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
                if (position.Level != mapLevel)
                {
                    continue;
                }

                BundleImageDefinition bundleImage = h3Engine.RetrieveBundleImage(template.AnimationFile);

                if (template.Type == EObjectType.ARTIFACT)
                {
                    Sprite[] sprites = mapTextureManager.LoadArtifactSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Artifacts", GetMapPositionInPixel(position.PosX, position.PosY, ZOrder_Artifact), sprites);
                }
                else if (MapObjectHelper.IsDecorationObject(template.Type))
                {
                    Sprite sprite = mapTextureManager.LoadDecorationSprite(template.AnimationFile, template.Type.GetHashCode());
                    CreateSubChildObject("Decorations", GetMapPositionInPixel(position.PosX, position.PosY, -1), sprite);
                }
                else if (template.Type == EObjectType.MINE)
                {
                    Sprite[] sprites = mapTextureManager.LoadMineSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Mines", GetMapPositionInPixel(position.PosX, position.PosY, -2), sprites);
                }
                else if (template.Type == EObjectType.RESOURCE)
                {
                    Sprite[] sprites = mapTextureManager.LoadResourceSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Resources", GetMapPositionInPixel(position.PosX, position.PosY, -4), sprites);
                }
                else if (template.Type == EObjectType.TOWN || template.Type == EObjectType.RANDOM_TOWN)
                {
                    Sprite[] sprites = mapTextureManager.LoadTownSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Town", GetMapPositionInPixel(position.PosX, position.PosY, -7), sprites);
                }
                else if (template.Type == EObjectType.HERO || template.Type == EObjectType.HERO_PLACEHOLDER)
                {
                    Sprite[] sprites = mapTextureManager.LoadHeroSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Heroes", GetMapPositionInPixel(position.PosX, position.PosY, -8), sprites);

                    // Load Single Image
                    ////ImageData image = bundleImage.GetImageData(0, 0);
                    //// image.ExportDataToPNG();

                    //// Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromPNGData(template.AnimationFile, image.GetPNGData());
                    ////Texture2D objTexture = TextureStorage.GetInstance().LoadTextureFromImage(template.AnimationFile, image);
                    ////LoadImageSprite(objTexture, "Obj" + objectIndex, position.PosX, position.PosY, 0);
                }
                else
                {
                    Sprite[] sprites = mapTextureManager.LoadSingleBundleImageSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("AnimatedObjects", GetMapPositionInPixel(position.PosX, position.PosY, -5), sprites);
                }

                objectIndex++;
            }
        }

        private Vector3 GetMapPositionInPixel(int x, int y, float z)
        {
            //// return new Vector3((float)(x * 0.32 - 4), 4 - (float)(y * 0.32), z - (float)(y * 0.01));
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
}