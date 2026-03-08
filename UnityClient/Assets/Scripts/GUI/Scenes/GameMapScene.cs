using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3Engine.DataAccess;
using H3Engine.Components.Data;

using H3Engine;
using System.IO;
using H3Engine.Campaign;
using H3Engine.Mapping;
using Assets.Scripts.Utils;
using UnityClient.GUI.Mapping;
using UnityClient.Components;
using UnityClient.Components.Mapping;
using H3Engine.MapObjects;
using H3Engine.Core;

namespace UnityClient.GUI.Scenes
{
    public enum GameMapState
    {
        Idle,
        HeroSelected,
        DestinationSelected,
        HeroMoving,
    }

    /// <summary>
    /// All campaign file names (SOD + AB, 28 entries across both LODs, unique names):
    ///   ab.h3c, blood.h3c, crag.h3c, evil1.h3c, evil2.h3c,
    ///   festival.h3c, final.h3c, fire.h3c, fool.h3c,
    ///   gelu.h3c, gem.h3c,
    ///   good1.h3c, good2.h3c, good3.h3c,
    ///   neutral.h3c, neutral1.h3c,
    ///   sandro.h3c, slayer.h3c, yog.h3c,
    ///   secret.h3c, secret1.h3c
    /// </summary>
    public class GameMapScene : MonoBehaviour
    {
        private PlayerInterface playerInterface = null;

        private GamerMapControl gamerMapControl = null;

        private MenuControl menuControl = null;

        private MapLoader mapLoader = null;

        private MapCamera mapCamera = null;

        private SimplePathFinder pathFinder = null;

        // Tile size constants (must match MapLoader)
        private const float TILE_SIZE = 0.32f;
        private const float MAP_OFFSET_X = -4f;
        private const float MAP_OFFSET_Y = 4f;

        // State machine
        private GameMapState currentState = GameMapState.Idle;

        // Selected hero
        private HeroInstance selectedHero = null;

        // Destination and path
        private Vector2Int selectedDestination;
        private List<Vector2Int> currentPath = null;

        // Movement animation
        private const float MOVE_SPEED = 3.0f; // tiles per second

        void Start()
        {
            H3DataAccess dataAccess = H3DataAccess.GetInstance();

            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_bmp.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_spr.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3bitmap.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3sprite.lod"));

            H3Campaign campaign = dataAccess.RetrieveCampaign("final.h3c");
            H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 3);

            playerInterface = PlayerInterface.StartGame(map);

            gamerMapControl = new GamerMapControl();

            GameMap gameMap = playerInterface.GameData.MapAtLevel(0);

            GameObject gameMapUI = GameObject.Find("GameMap");
            mapLoader = gameMapUI.GetComponent<MapLoader>();
            mapLoader.Initialize(gameMap);
            mapLoader.RenderMap();

            pathFinder = new SimplePathFinder(gameMap);

            // Get MapCamera for click detection
            GameObject cameraObj = GameObject.Find("GameMap");
            mapCamera = cameraObj.GetComponent<MapCamera>();
            if (mapCamera == null)
            {
                mapCamera = Camera.main.GetComponent<MapCamera>();
            }

            // Populate heroes list from Unity side (H3Engine DLL may be stale)
            gameMap.Heroes = new List<HeroInstance>();
            foreach (var obj in gameMap.Objects)
            {
                if (obj is HeroInstance hero)
                {
                    gameMap.Heroes.Add(hero);
                }
            }
            Debug.Log("[GameMapScene] Initialized. Heroes count: " + gameMap.Heroes.Count);
            foreach (var h in gameMap.Heroes)
            {
                Debug.Log("[GameMapScene]   Hero at (" + h.Position.PosX + ", " + h.Position.PosY + ") id=" + h.Identifier);
            }
        }

        void Update()
        {
            if (mapCamera == null || !mapCamera.WasClick)
                return;

            // Convert world position to tile coordinates
            // Sprite pivot is (1, 0) = bottom-right, so tiles extend left and up from anchor
            Vector3 worldPos = mapCamera.ClickWorldPosition;
            int tileX = Mathf.FloorToInt((worldPos.x - MAP_OFFSET_X) / TILE_SIZE) + 1;
            int tileY = Mathf.FloorToInt((MAP_OFFSET_Y - worldPos.y) / TILE_SIZE);

            Debug.Log(string.Format("[GameMapScene] Click detected! world=({0:F2}, {1:F2}) -> tile=({2}, {3}) state={4}",
                worldPos.x, worldPos.y, tileX, tileY, currentState));

            // Bounds check
            GameMap gameMap = mapLoader.GameMap;
            if (tileX < 0 || tileX >= gameMap.Width || tileY < 0 || tileY >= gameMap.Height)
            {
                Debug.Log("[GameMapScene] Click out of bounds, ignored.");
                return;
            }

            // Ignore clicks during movement
            if (currentState == GameMapState.HeroMoving)
                return;

            HeroInstance heroAtTile = mapLoader.GetHeroAtTile(tileX, tileY);
            Debug.Log("[GameMapScene] HeroAtTile: " + (heroAtTile != null ? "YES id=" + heroAtTile.Identifier : "none"));

            switch (currentState)
            {
                case GameMapState.Idle:
                    HandleIdleClick(tileX, tileY, heroAtTile);
                    break;

                case GameMapState.HeroSelected:
                    HandleHeroSelectedClick(tileX, tileY, heroAtTile);
                    break;

                case GameMapState.DestinationSelected:
                    HandleDestinationSelectedClick(tileX, tileY, heroAtTile);
                    break;
            }
        }

        /// <summary>
        /// Idle: clicking a hero selects it.
        /// </summary>
        private void HandleIdleClick(int tileX, int tileY, HeroInstance heroAtTile)
        {
            if (heroAtTile != null)
            {
                SelectHero(heroAtTile);
            }
        }

        /// <summary>
        /// HeroSelected: clicking empty ground sets destination and shows path.
        /// Clicking another hero switches selection. Clicking the selected hero deselects.
        /// </summary>
        private void HandleHeroSelectedClick(int tileX, int tileY, HeroInstance heroAtTile)
        {
            if (heroAtTile != null)
            {
                if (heroAtTile == selectedHero)
                {
                    // Deselect
                    DeselectHero();
                }
                else
                {
                    // Switch to different hero
                    SelectHero(heroAtTile);
                }
                return;
            }

            // Clicked on empty ground - try to find path
            TrySelectDestination(tileX, tileY);
        }

        /// <summary>
        /// DestinationSelected: clicking the same destination again starts movement.
        /// Clicking a different empty tile changes destination. Clicking a hero switches selection.
        /// </summary>
        private void HandleDestinationSelectedClick(int tileX, int tileY, HeroInstance heroAtTile)
        {
            if (heroAtTile != null)
            {
                if (heroAtTile == selectedHero)
                {
                    DeselectHero();
                }
                else
                {
                    SelectHero(heroAtTile);
                }
                return;
            }

            // Check if clicking the same destination
            if (tileX == selectedDestination.x && tileY == selectedDestination.y && currentPath != null)
            {
                // Same destination clicked again - start moving
                StartHeroMovement();
            }
            else
            {
                // Different tile - update destination
                TrySelectDestination(tileX, tileY);
            }
        }

        #region State Transitions

        private void SelectHero(HeroInstance hero)
        {
            mapLoader.ClearPath();
            currentPath = null;

            selectedHero = hero;
            currentState = GameMapState.HeroSelected;

            print(string.Format("Hero selected at ({0}, {1})", hero.Position.PosX, hero.Position.PosY));
        }

        private void DeselectHero()
        {
            mapLoader.ClearPath();
            currentPath = null;
            selectedHero = null;
            currentState = GameMapState.Idle;

            print("Hero deselected");
        }

        private void TrySelectDestination(int tileX, int tileY)
        {
            if (selectedHero == null) return;

            int heroX = selectedHero.Position.PosX;
            int heroY = selectedHero.Position.PosY;

            List<Vector2Int> path = pathFinder.FindPath(heroX, heroY, tileX, tileY);
            if (path == null || path.Count < 2)
            {
                print(string.Format("No path to ({0}, {1})", tileX, tileY));
                return;
            }

            currentPath = path;
            selectedDestination = new Vector2Int(tileX, tileY);
            currentState = GameMapState.DestinationSelected;

            // Display path on map
            mapLoader.ShowPath(currentPath);

            print(string.Format("Destination selected at ({0}, {1}), path length: {2}", tileX, tileY, path.Count));
        }

        private void StartHeroMovement()
        {
            if (selectedHero == null || currentPath == null || currentPath.Count < 2)
                return;

            currentState = GameMapState.HeroMoving;
            mapLoader.ClearPath();

            print("Hero moving...");

            StartCoroutine(MoveHeroAlongPath());
        }

        #endregion

        #region Movement Animation

        private IEnumerator MoveHeroAlongPath()
        {
            GameObject heroGO = mapLoader.GetHeroGameObject(selectedHero);
            if (heroGO == null)
            {
                currentState = GameMapState.Idle;
                yield break;
            }

            // Move along each segment of the path (skip index 0 which is the start)
            for (int i = 1; i < currentPath.Count; i++)
            {
                Vector2Int targetTile = currentPath[i];
                Vector3 startPos = heroGO.transform.position;
                Vector3 endPos = mapLoader.TileToWorldPosition(targetTile.x, targetTile.y);

                float distance = Vector3.Distance(startPos, endPos);
                float duration = distance / (MOVE_SPEED * 0.32f);
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    heroGO.transform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }

                heroGO.transform.position = endPos;
            }

            // Movement complete - update hero data position
            Vector2Int finalTile = currentPath[currentPath.Count - 1];
            mapLoader.UpdateHeroPosition(selectedHero, finalTile.x, finalTile.y);

            print(string.Format("Hero arrived at ({0}, {1})", finalTile.x, finalTile.y));

            // Reset state
            currentPath = null;
            selectedHero = null;
            currentState = GameMapState.Idle;
        }

        #endregion
    }
}
