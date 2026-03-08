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
        private const int SortOrder_PathArrow = 95;
        private const int SortOrder_Edge = 100;

        private const float TILE_SIZE = 0.32f;
        private const float MAP_OFFSET_X = -4f;
        private const float MAP_OFFSET_Y = 4f;

        private GameMap gameMap = null;

        private H3DataAccess h3dataAccess = null;

        private MapTextureManager mapTextureManager = null;

        // Cache parent transforms to avoid repeated Find() calls
        private Dictionary<string, Transform> parentCache = new Dictionary<string, Transform>();

        // Hero tracking: maps HeroInstance identifier to the hero's GameObject
        private Dictionary<uint, GameObject> heroGameObjects = new Dictionary<uint, GameObject>();

        // Path arrow GameObjects currently displayed on map
        private List<GameObject> pathArrowObjects = new List<GameObject>();

        public GameMap GameMap { get { return gameMap; } }

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

        #region Public API for Hero Movement

        /// <summary>
        /// Find the HeroInstance at a given tile coordinate, or null if none.
        /// Hero sprites are larger than one tile and the Position is the bottom-right
        /// action tile, so we check a small area around each hero's position.
        /// </summary>
        public HeroInstance GetHeroAtTile(int tileX, int tileY)
        {
            if (gameMap.Heroes == null) return null;
            foreach (HeroInstance hero in gameMap.Heroes)
            {
                int hx = hero.Position.PosX;
                int hy = hero.Position.PosY;
                // Hero sprite covers roughly a 3x3 area with position at bottom-right
                if (tileX >= hx - 2 && tileX <= hx && tileY >= hy - 2 && tileY <= hy)
                {
                    return hero;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the GameObject for a given hero.
        /// </summary>
        public GameObject GetHeroGameObject(HeroInstance hero)
        {
            if (hero == null) return null;
            heroGameObjects.TryGetValue(hero.Identifier, out GameObject go);
            return go;
        }

        /// <summary>
        /// Display path arrows on the map for a list of tile positions.
        /// path[0] = start (hero position), path[last] = destination.
        /// </summary>
        public void ShowPath(List<Vector2Int> path)
        {
            ClearPath();

            if (path == null || path.Count < 2) return;

            // For each tile in the path (skip the start, which is the hero's current tile)
            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int current = path[i];
                bool isDestination = (i == path.Count - 1);

                Sprite arrowSprite;
                if (isDestination)
                {
                    arrowSprite = mapTextureManager.LoadCursorSprite("X");
                }
                else
                {
                    // Calculate direction from current to next
                    Vector2Int next = path[i + 1];
                    string key = GetDirectionSpriteKey(current, next);
                    arrowSprite = mapTextureManager.LoadCursorSprite(key);
                    if (arrowSprite == null)
                    {
                        arrowSprite = mapTextureManager.LoadCursorSprite("X");
                    }
                }

                if (arrowSprite != null)
                {
                    GameObject arrow = CreateSubChildObject("PathArrows", GetMapPosition(current.x, current.y), arrowSprite, SortOrder_PathArrow, "PathArrow");
                    pathArrowObjects.Add(arrow);
                }
            }
        }

        /// <summary>
        /// Remove all path arrow objects from the map.
        /// </summary>
        public void ClearPath()
        {
            foreach (GameObject arrow in pathArrowObjects)
            {
                if (arrow != null) Destroy(arrow);
            }
            pathArrowObjects.Clear();
        }

        /// <summary>
        /// Convert tile coordinates to world position.
        /// </summary>
        public Vector3 TileToWorldPosition(int tileX, int tileY)
        {
            return GetMapPosition(tileX, tileY);
        }

        /// <summary>
        /// Update the hero's data position after movement completes.
        /// </summary>
        public void UpdateHeroPosition(HeroInstance hero, int newTileX, int newTileY)
        {
            hero.Position = new MapPosition(newTileX, newTileY, hero.Position.Level);
        }

        #endregion

        #region Direction Sprite Key Mapping

        /// <summary>
        /// Map movement direction (from current to next tile) to an adag.def sprite key.
        /// Uses the double-arrow keys for path continuation tiles.
        /// </summary>
        private string GetDirectionSpriteKey(Vector2Int from, Vector2Int to)
        {
            int dx = to.x - from.x;
            int dy = to.y - from.y;

            // dy is inverted: positive dy = moving down on screen
            // Map (dx, dy) to H3 path arrow keys
            if (dx == 0 && dy == -1) return "AA";     // North
            if (dx == 1 && dy == -1) return @"//A";   // NE
            if (dx == 1 && dy == 0) return ">>";       // East
            if (dx == 1 && dy == 1) return @"\\V";     // SE
            if (dx == 0 && dy == 1) return "VV";       // South
            if (dx == -1 && dy == 1) return @"//V";    // SW
            if (dx == -1 && dy == 0) return "<<";      // West
            if (dx == -1 && dy == -1) return @"\\A";   // NW

            return "X"; // fallback
        }

        #endregion

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
                    mapTileComponent.Initialize(xx, yy);
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
                    GameObject heroGO = CreateSubChildAnimatedObject("Heroes", GetMapPosition(position.PosX, position.PosY), sprites, SortOrder_Hero, "Hero_" + obj.Identifier);

                    // Track hero GameObjects for movement
                    heroGameObjects[obj.Identifier] = heroGO;
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
            return new Vector3(x * TILE_SIZE + MAP_OFFSET_X, MAP_OFFSET_Y - y * TILE_SIZE, 0);
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
