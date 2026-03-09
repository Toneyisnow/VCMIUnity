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

        // Hero DEF file names: maps HeroInstance identifier to animation file name
        private Dictionary<uint, string> heroDefFileNames = new Dictionary<uint, string>();

        // Hero original sprites (all groups flat) for restoring after movement
        private Dictionary<uint, Sprite[]> heroOriginalSprites = new Dictionary<uint, Sprite[]>();

        // Path arrow GameObjects currently displayed on map
        private List<GameObject> pathArrowObjects = new List<GameObject>();

        /// <summary>
        /// Hero DEF group mapping by movement direction (dx, dy).
        /// Hero DEF structure: groups 0-4 = standing (N, NE, E, SE, S),
        /// groups 5-9 = walking (N, NE, E, SE, S).
        /// NW/W/SW use flipped NE/E/SE groups.
        /// Based on VCMI moveGroups: {0, 10, 5, 6, 7, 8, 9, 12, 11}
        /// Returns (groupIndex, flipX).
        /// </summary>
        private static (int group, bool flipX) GetHeroMoveGroup(int dx, int dy)
        {
            // Walking groups in DEF: 5=N, 6=NE, 7=E, 8=SE, 9=S
            // Groups 6/7/8 face right (NE/E/SE) natively.
            // For NW/W/SW, flip the corresponding right-facing group.
            // N
            if (dx == 0 && dy == -1) return (5, false);
            // NE
            if (dx == 1 && dy == -1) return (6, false);
            // E
            if (dx == 1 && dy == 0) return (7, false);
            // SE
            if (dx == 1 && dy == 1) return (8, false);
            // S
            if (dx == 0 && dy == 1) return (9, false);
            // SW = flipped SE (group 8)
            if (dx == -1 && dy == 1) return (8, true);
            // W = flipped E (group 7)
            if (dx == -1 && dy == 0) return (7, true);
            // NW = flipped NE (group 6)
            if (dx == -1 && dy == -1) return (6, true);

            // Fallback: standing N
            return (0, false);
        }

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
        /// Uses VCMI's 9x9 direction lookup table considering both enter and leave directions.
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
                    // Destination marker: index 0 = "X"
                    arrowSprite = mapTextureManager.LoadCursorSpriteByIndex(0);
                }
                else
                {
                    // Calculate enter direction (from previous tile to current)
                    Vector2Int prev = path[i - 1];
                    int enterDir = GetDirectionIndex(prev, current);

                    // Calculate leave direction (from current tile to next)
                    Vector2Int next = path[i + 1];
                    int leaveDir = GetDirectionIndex(current, next);

                    int arrowIndex = DirectionToArrowIndex[enterDir, leaveDir];
                    arrowSprite = mapTextureManager.LoadCursorSpriteByIndex(arrowIndex);
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
        /// <summary>
        /// Convert tile coordinates to hero world position.
        /// Hero sprites are wider than 1 tile, so X is shifted +1 to align the
        /// visible character with the correct terrain tile.
        /// </summary>
        public Vector3 HeroTileToWorldPosition(int tileX, int tileY)
        {
            return GetMapPosition(tileX + 1, tileY);
        }

        /// <summary>
        /// Update the hero's data position after movement completes.
        /// </summary>
        public void UpdateHeroPosition(HeroInstance hero, int newTileX, int newTileY)
        {
            hero.Position = new MapPosition(newTileX, newTileY, hero.Position.Level);
        }

        /// <summary>
        /// Set the hero's animation to the walking sprites for the given movement direction.
        /// dx, dy: tile movement direction (-1, 0, or 1).
        /// </summary>
        public void SetHeroMovingAnimation(HeroInstance hero, int dx, int dy)
        {
            if (hero == null) return;
            if (!heroGameObjects.TryGetValue(hero.Identifier, out GameObject heroGO)) return;
            if (!heroDefFileNames.TryGetValue(hero.Identifier, out string defFileName)) return;

            var (group, flipX) = GetHeroMoveGroup(dx, dy);
            // Pass flipX to create pre-flipped sprites with correct pivot (0,0) for flipped, (1,0) for normal
            Sprite[] groupSprites = mapTextureManager.LoadHeroGroupSprites(defFileName, group, flipX);

            AnimatedMapObject animated = heroGO.GetComponent<AnimatedMapObject>();
            if (animated != null && groupSprites != null && groupSprites.Length > 0)
            {
                animated.SetSprites(groupSprites, 8);
            }
        }

        /// <summary>
        /// Set hero to idle/standing animation facing the given movement direction.
        /// Standing groups 0-4 correspond to walking groups 5-9 (N, NE, E, SE, S).
        /// </summary>
        public void SetHeroIdleAnimation(HeroInstance hero, int dx, int dy)
        {
            if (hero == null) return;
            if (!heroGameObjects.TryGetValue(hero.Identifier, out GameObject heroGO)) return;
            if (!heroDefFileNames.TryGetValue(hero.Identifier, out string defFileName)) return;

            // Standing group = walking group - 5 (groups 0-4 = standing N, NE, E, SE, S)
            var (walkGroup, flipX) = GetHeroMoveGroup(dx, dy);
            int standGroup = walkGroup - 5;

            Sprite[] groupSprites = mapTextureManager.LoadHeroGroupSprites(defFileName, standGroup, flipX);

            AnimatedMapObject animated = heroGO.GetComponent<AnimatedMapObject>();
            if (animated != null && groupSprites != null && groupSprites.Length > 0)
            {
                animated.SetSprites(groupSprites, 18);
            }
        }

        #endregion

        #region VCMI Direction Lookup Table

        /// <summary>
        /// VCMI's 9x9 direction-to-arrow-index lookup table from MapRenderer.cpp.
        /// Rows = enter direction (from previous tile), Columns = leave direction (to next tile).
        /// Direction index: (dx+1) + 3*(dy+1) where dx/dy are -1, 0, or 1.
        /// 0=NW, 1=N, 2=NE, 3=W, 4=center, 5=E, 6=SW, 7=S, 8=SE
        /// </summary>
        private static readonly int[,] DirectionToArrowIndex = new int[,]
        {
            {16, 17, 18, 7,  0, 19, 6,  5,  12},
            {8,  9,  18, 7,  0, 19, 6,  13, 20},
            {8,  1,  10, 7,  0, 19, 14, 21, 20},
            {24, 17, 18, 15, 0, 11, 6,  5,  4 },
            {0,  0,  0,  0,  0, 0,  0,  0,  0 },
            {8,  1,  2,  15, 0, 11, 22, 21, 20},
            {24, 17, 10, 23, 0, 3,  14, 5,  4 },
            {24, 9,  2,  23, 0, 3,  22, 13, 4 },
            {16, 1,  2,  23, 0, 3,  22, 21, 12},
        };

        /// <summary>
        /// Calculate the direction index (0-8) for movement from one tile to an adjacent tile.
        /// Formula: (dx+1) + 3*(dy+1)
        /// </summary>
        private int GetDirectionIndex(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Clamp(to.x - from.x, -1, 1);
            int dy = Mathf.Clamp(to.y - from.y, -1, 1);
            return (dx + 1) + 3 * (dy + 1);
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
                    // Hero sprites are wider than 1 tile; with pivot (1,0) the character
                    // appears 1 tile to the left of the position. Shift X+1 to compensate.
                    GameObject heroGO = CreateSubChildAnimatedObject("Heroes", GetMapPosition(position.PosX + 1, position.PosY), sprites, SortOrder_Hero, "Hero_" + obj.Identifier);

                    // Track hero GameObjects and original sprites for movement
                    heroGameObjects[obj.Identifier] = heroGO;
                    heroOriginalSprites[obj.Identifier] = sprites;

                    // Derive the walking animation DEF file name:
                    // Template uses editor sprite (e.g. "ah02_e.def"), walking animation is "ah02_.def"
                    string walkDefFile = template.AnimationFile;
                    if (walkDefFile.Contains("_e.def"))
                    {
                        walkDefFile = walkDefFile.Replace("_e.def", "_.def");
                    }
                    heroDefFileNames[obj.Identifier] = walkDefFile;
                    Debug.Log(string.Format("[MapLoader] Hero {0}: templateDef={1}, walkDef={2}", obj.Identifier, template.AnimationFile, walkDefFile));
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
