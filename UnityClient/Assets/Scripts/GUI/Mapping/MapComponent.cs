using H3Engine.Common;
using H3Engine.Components.Data;
using H3Engine.Core;
using H3Engine.Components.MapProviders;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System.Collections;
using System.Collections.Generic;
using UnityClient.GUI.Rendering;
using UnityEngine;

namespace UnityClient.GUI.Mapping
{
    /// <summary>
    /// Main MonoBehaviour for the game map. Owns the Unity scene hierarchy,
    /// hero state, and coordinates the MapLoader and MovePathResolver tools.
    /// </summary>
    public class MapComponent : MonoBehaviour
    {
        // Sorting order layers (higher = rendered on top)
        internal const int SortOrder_Terrain    = 0;
        internal const int SortOrder_Road       = 10;
        internal const int SortOrder_River      = 20;
        internal const int SortOrder_Decoration = 30;
        internal const int SortOrder_Resource   = 40;
        internal const int SortOrder_Artifact   = 50;
        internal const int SortOrder_Mine       = 60;
        internal const int SortOrder_Building   = 70;
        internal const int SortOrder_Town       = 80;
        internal const int SortOrder_Hero       = 90;
        internal const int SortOrder_PathArrow  = 95;
        internal const int SortOrder_Edge       = 100;

        internal const float TILE_SIZE    = 0.32f;
        internal const float MAP_OFFSET_X = -4f;
        internal const float MAP_OFFSET_Y = 4f;

        private GameMap gameMap = null;
        private MapTextureManager mapTextureManager = null;

        // Cache parent transforms to avoid repeated Find() calls
        private Dictionary<string, Transform> parentCache = new Dictionary<string, Transform>();

        // Hero tracking: populated by MapLoader during RenderMap
        internal Dictionary<uint, GameObject> heroGameObjects    = new Dictionary<uint, GameObject>();
        internal Dictionary<uint, string>     heroDefFileNames   = new Dictionary<uint, string>();
        internal Dictionary<uint, Sprite[]>   heroOriginalSprites = new Dictionary<uint, Sprite[]>();

        private MapTileRenderer mapTileRenderer = null;
        private MovePathResolver movePathResolver = null;

        public GameMap GameMap { get { return gameMap; } }

        public MovePathResolver PathResolver { get { return movePathResolver; } }

        // -----------------------------------------------------------------------
        // Initialization
        // -----------------------------------------------------------------------

        public void Initialize(GameMap gameMap)
        {
            this.gameMap = gameMap;
            this.mapTextureManager = new MapTextureManager(gameMap.H3Map);

            this.mapTileRenderer = new MapTileRenderer(this, gameMap, mapTextureManager);
            this.movePathResolver = new MovePathResolver(this, mapTextureManager, heroGameObjects, heroDefFileNames);
        }

        // -----------------------------------------------------------------------
        // Map rendering (delegates to MapLoader)
        // -----------------------------------------------------------------------

        public void RenderMap()
        {
            mapTileRenderer.RenderMap();
        }

        public IEnumerator RenderMapCoroutine(LoadProgress progress)
        {
            return mapTileRenderer.RenderMapCoroutine(progress);
        }

        // -----------------------------------------------------------------------
        // Hero queries
        // -----------------------------------------------------------------------

        /// <summary>
        /// Find the HeroInstance at a given tile coordinate, or null if none.
        /// Hero sprites are larger than one tile; Position is the bottom-right action tile.
        /// </summary>
        public HeroInstance GetHeroAtTile(int tileX, int tileY)
        {
            if (gameMap.Heroes == null) return null;
            foreach (HeroInstance hero in gameMap.Heroes)
            {
                int hx = hero.Position.PosX;
                int hy = hero.Position.PosY;
                if (tileX >= hx - 2 && tileX <= hx && tileY >= hy - 2 && tileY <= hy)
                    return hero;
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
        /// Update the hero's data position after movement completes.
        /// </summary>
        public void UpdateHeroPosition(HeroInstance hero, int newTileX, int newTileY)
        {
            hero.Position = new MapPosition(newTileX, newTileY, hero.Position.Level);
        }

        // -----------------------------------------------------------------------
        // Internal helpers used by MapLoader and MovePathResolver
        // -----------------------------------------------------------------------

        internal static Vector3 GetMapPosition(int x, int y)
        {
            return new Vector3(x * TILE_SIZE + MAP_OFFSET_X, MAP_OFFSET_Y - y * TILE_SIZE, 0);
        }

        internal GameObject CreateSubChildObject(string subName, Vector3 position, Sprite sprite, int sortingOrder, string goName = "DefaultGameObject")
        {
            GameObject newObject = new GameObject();
            newObject.transform.position = position;
            newObject.name = goName;
            newObject.transform.parent = GetOrCreateParent(subName);

            SpriteRenderer sr = newObject.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            return newObject;
        }

        internal GameObject CreateSubChildAnimatedObject(string subName, Vector3 position, Sprite[] sprites, int sortingOrder, string goName = "DefaultGameObject")
        {
            GameObject newObject = new GameObject();
            newObject.transform.position = position;
            newObject.name = goName;
            newObject.transform.parent = GetOrCreateParent(subName);

            SpriteRenderer sr = newObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;

            AnimatedMapObject animated = newObject.AddComponent<AnimatedMapObject>();
            animated.Initialize(sprites);

            return newObject;
        }

        private Transform GetOrCreateParent(string subName)
        {
            if (parentCache.TryGetValue(subName, out Transform cached))
                return cached;

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
    }
}
