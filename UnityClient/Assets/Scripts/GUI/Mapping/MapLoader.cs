using H3Engine.DataAccess;
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
using H3Engine.Components.Data;

namespace UnityClient.GUI.Mapping
{

    public class MapLoader : MonoBehaviour
    {
        // Sorting order layers (higher = rendered on top)
        private const int SortOrder_Terrain = 0;
        private const int SortOrder_Road = 10;
        private const int SortOrder_River = 20;
        private const int SortOrder_Decoration = 30;
        private const int SortOrder_Resource = 40;
        private const int SortOrder_Artifact = 50;
        private const int SortOrder_Mine = 60;
        private const int SortOrder_Building = 70;
        private const int SortOrder_Town = 80;
        private const int SortOrder_Hero = 90;
        private const int SortOrder_Edge = 100;

        private GameMap gameMap = null;

        private H3DataAccess h3dataAccess = null;

        private MapTextureManager mapTextureManager = null;

        // Cache parent transforms to avoid repeated Find() calls
        private Dictionary<string, Transform> parentCache = new Dictionary<string, Transform>();

        public void Initialize(GameMap gameMap)
        {
            this.h3dataAccess = H3DataAccess.GetInstance();
            this.gameMap = gameMap;
            this.mapTextureManager = new MapTextureManager(gameMap.H3Map);
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

            RenderEdge();

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
            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                for (int yy = 0; yy < gameMap.Height; yy++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);

                    GameObject gametile = CreateSubChildObject("Terrain", GetMapPosition(xx, yy), sprite, SortOrder_Terrain);
                    MapTile mapTileComponent = gametile.AddComponent<MapTile>();
                    mapTileComponent.Initialize(new Vector2(xx, yy), null);
                }
            }
        }

        private void RenderRoad()
        {
            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                for (int yy = 0; yy < gameMap.Height; yy++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
                    CreateSubChildObject("Road", GetMapPosition(xx, yy), sprite, SortOrder_Road);
                }
            }
        }

        private void RenderRiver()
        {
            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                for (int yy = 0; yy < gameMap.Height; yy++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);
                    CreateSubChildObject("River", GetMapPosition(xx, yy), sprite, SortOrder_River);
                }
            }
        }

        private void RenderMapObjects()
        {
            int objectIndex = 0;
            foreach (CGObject obj in gameMap.Objects)
            {
                ObjectTemplate template = obj.Template;
                MapPosition position = obj.Position;

                if (template.Type == EObjectType.ARTIFACT)
                {
                    Sprite[] sprites = mapTextureManager.LoadArtifactSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Artifacts", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Artifact);
                }
                else if (MapObjectHelper.IsDecorationObject(template.Type))
                {
                    Sprite sprite = mapTextureManager.LoadDecorationSprite(template.AnimationFile, template.Type.GetHashCode());
                    CreateSubChildObject("Decorations", GetMapPosition(position.PosX, position.PosY), sprite, SortOrder_Decoration);
                }
                else if (template.Type == EObjectType.MINE)
                {
                    Sprite[] sprites = mapTextureManager.LoadMineSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Mines", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Mine, "Mine_" + template.SubId);
                }
                else if (template.Type == EObjectType.RESOURCE)
                {
                    Sprite[] sprites = mapTextureManager.LoadResourceSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Resources", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Resource, "Resource_" + template.SubId);
                }
                else if (template.Type == EObjectType.TOWN || template.Type == EObjectType.RANDOM_TOWN)
                {
                    Sprite[] sprites = mapTextureManager.LoadTownSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Town", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Town, "Town_" + template.SubId);
                }
                else if (template.Type == EObjectType.HERO || template.Type == EObjectType.HERO_PLACEHOLDER)
                {
                    Sprite[] sprites = mapTextureManager.LoadHeroSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("Heroes", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Hero);
                }
                else
                {
                    Sprite[] sprites = mapTextureManager.LoadSingleBundleImageSprites(template.AnimationFile);
                    CreateSubChildAnimatedObject("AnimatedObjects", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Building, string.Format(@"{0}_{1}", template.Type.ToString(), template.SubId));
                }

                objectIndex++;
            }
        }

        private void RenderEdge()
        {
            Sprite sprite = null;
            int mapHeight = gameMap.Height;
            int mapWidth = gameMap.Width;

            Sprite spaceSprite = mapTextureManager.LoadEdgeSprite("X");

            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                sprite = mapTextureManager.LoadEdgeSprite("U");
                CreateSubChildObject("Edge", GetMapPosition(xx, -1), sprite, SortOrder_Edge);
                CreateSubChildObject("Edge", GetMapPosition(xx, -2), spaceSprite, SortOrder_Edge);

                sprite = mapTextureManager.LoadEdgeSprite("D");
                CreateSubChildObject("Edge", GetMapPosition(xx, mapHeight), sprite, SortOrder_Edge);
                CreateSubChildObject("Edge", GetMapPosition(xx, mapHeight + 1), spaceSprite, SortOrder_Edge);
            }

            for (int yy = 0; yy < gameMap.Height; yy++)
            {
                sprite = mapTextureManager.LoadEdgeSprite("L");
                CreateSubChildObject("Edge", GetMapPosition(-1, yy), sprite, SortOrder_Edge);
                CreateSubChildObject("Edge", GetMapPosition(-2, yy), spaceSprite, SortOrder_Edge);

                sprite = mapTextureManager.LoadEdgeSprite("R");
                CreateSubChildObject("Edge", GetMapPosition(mapWidth, yy), sprite, SortOrder_Edge);
                CreateSubChildObject("Edge", GetMapPosition(mapWidth + 1, yy), spaceSprite, SortOrder_Edge);
            }

            sprite = mapTextureManager.LoadEdgeSprite("UL");
            CreateSubChildObject("Edge", GetMapPosition(-1, -1), sprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(-2, -1), spaceSprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(-1, -2), spaceSprite, SortOrder_Edge);

            sprite = mapTextureManager.LoadEdgeSprite("UR");
            CreateSubChildObject("Edge", GetMapPosition(mapWidth, -1), sprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(mapWidth, -2), spaceSprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(mapWidth + 1, -1), spaceSprite, SortOrder_Edge);

            sprite = mapTextureManager.LoadEdgeSprite("DL");
            CreateSubChildObject("Edge", GetMapPosition(-1, mapHeight), sprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(-2, mapHeight), spaceSprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(-1, mapHeight + 1), spaceSprite, SortOrder_Edge);

            sprite = mapTextureManager.LoadEdgeSprite("DR");
            CreateSubChildObject("Edge", GetMapPosition(mapWidth, mapHeight), sprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(mapWidth + 1, mapHeight), spaceSprite, SortOrder_Edge);
            CreateSubChildObject("Edge", GetMapPosition(mapWidth, mapHeight + 1), spaceSprite, SortOrder_Edge);
        }

        private Vector3 GetMapPosition(int x, int y)
        {
            return new Vector3((float)(x * 0.32 - 4), 4 - (float)(y * 0.32), 0);
        }

        private Transform GetOrCreateParent(string subName)
        {
            if (parentCache.TryGetValue(subName, out Transform cached))
            {
                return cached;
            }

            Transform child = transform.Find(subName);
            if (child == null)
            {
                GameObject parent = new GameObject();
                parent.name = subName;
                parent.transform.parent = transform;
                child = parent.transform;
            }

            parentCache[subName] = child;
            return child;
        }

        private GameObject CreateSubChildObject(string subName, Vector3 position, Sprite sprite, int sortingOrder, string goName = "DefaultGameObject")
        {
            GameObject newObject = new GameObject();
            newObject.transform.position = position;
            newObject.name = goName;
            newObject.transform.parent = GetOrCreateParent(subName);

            SpriteRenderer renderer = newObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;

            return newObject;
        }

        private GameObject CreateSubChildAnimatedObject(string subName, Vector3 position, Sprite[] sprites, int sortingOrder, string goName = "DefaultGameObject")
        {
            GameObject newObject = new GameObject();
            newObject.transform.position = position;
            newObject.name = goName;
            newObject.transform.parent = GetOrCreateParent(subName);

            SpriteRenderer renderer = newObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;

            AnimatedMapObject animated = newObject.AddComponent<AnimatedMapObject>();
            animated.Initialize(sprites);

            return newObject;
        }
    }
}