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
using UnityClient.Components.Data;
using UnityClient.Components.Mapping;
using H3Engine.MapObjects;
using H3Engine.Core;
using H3Engine.Common;
using H3Engine.GUI;
using H3Engine.FileSystem;
using H3Engine.Components.MapProviders;
using UnityClient.GUI.Rendering;

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

        // --- Pathfinder (replaces SimplePathFinder) ---
        private PathfinderCache pathFinderCache = null;
        private MapPathFinder pathFinder = null;

        // Fallback movement points if hero data is unavailable.
        private const int DEFAULT_MAX_MOVEMENT_POINTS = 1500;

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
        private List<MapPathNode> currentPath = null;

        // Movement animation
        private const float MOVE_SPEED = 3.0f; // tiles per second

        // Pause movement flag: set when user clicks during HeroMoving
        private bool pauseMovementRequested = false;

        // Loading overlay state
        private bool isInitializing = true;
        private LoadProgress renderProgress = null;
        private GameObject loadingOverlay = null;
        private List<GameObject> progressBlocks = new List<GameObject>();
        private int lastVisibleBlocks = 0;
        private TextMesh loadingStatusText = null;

        private const float SCREEN_W = 800f;
        private const float SCREEN_H = 600f;
        private const float PPU = 100f;
        private const int PROGRESS_BLOCK_COUNT = 20;
        private const float PROGRESS_BLOCK_SIZE = 18f;
        private const float PROGRESS_BAR_X = 395f;
        private const float PROGRESS_BAR_Y = 548f;

        void Start()
        {
            // Set camera so that 1 asset pixel = 1 screen pixel at default zoom
            Camera.main.orthographicSize = Screen.height / (2f * PPU);

            StartCoroutine(InitializeCoroutine());
        }

        private IEnumerator InitializeCoroutine()
        {
            H3DataAccess dataAccess = H3DataAccess.GetInstance();

            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_bmp.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_spr.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3bitmap.lod"));
            dataAccess.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3sprite.lod"));

            // Use pre-loaded map from loading screen if available, otherwise load here
            H3Map map = CrossSceneData.LoadedMap;
            if (map != null)
            {
                CrossSceneData.LoadedMap = null; // Consume and clear
            }
            else
            {
                // Fallback: load campaign directly (for direct testing or if loading screen was skipped)
                H3Campaign campaign = CrossSceneData.CurrentCampaign;
                int scenarioIndex = CrossSceneData.SelectedScenarioIndex;
                if (campaign == null)
                {
                    campaign = dataAccess.RetrieveCampaign("final.h3c");
                    scenarioIndex = 3;
                }
                map = H3CampaignLoader.LoadScenarioMap(campaign, scenarioIndex);
            }

            playerInterface = PlayerInterface.StartGame(map);

            gamerMapControl = new GamerMapControl();

            GameMap gameMap = playerInterface.GameData.MapAtLevel(0);

            GameObject gameMapUI = GameObject.Find("GameMap");
            mapLoader = gameMapUI.GetComponent<MapLoader>();
            mapLoader.Initialize(gameMap);

            // Show loading overlay and render map with progress
            ShowLoadingOverlay(dataAccess);
            renderProgress = new LoadProgress();
            yield return null; // Let overlay render one frame

            yield return StartCoroutine(mapLoader.RenderMapCoroutine(renderProgress));

            DestroyLoadingOverlay();
            isInitializing = false;

            // Initialize pathfinder (cache + Dijkstra engine)
            pathFinderCache = new PathfinderCache();
            pathFinder = new MapPathFinder(pathFinderCache);

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
            // Update loading progress bar during initialization
            if (isInitializing)
            {
                if (renderProgress != null)
                {
                    UpdateProgressBar(renderProgress.Progress);
                    UpdateLoadingStatus(renderProgress.StatusMessage);
                }
                return;
            }

            if (mapCamera == null || !mapCamera.WasClick)
                return;

            // Convert world position to tile coordinates
            // Pivot (1,0) shifts visual tile area by 1 cell from the Floor grid.
            // Add +1 to both axes to compensate.
            Vector3 worldPos = mapCamera.ClickWorldPosition;
            int tileX = Mathf.FloorToInt((worldPos.x - MAP_OFFSET_X) / TILE_SIZE) + 1;
            int tileY = Mathf.FloorToInt((MAP_OFFSET_Y - worldPos.y) / TILE_SIZE) + 1;

            Debug.Log(string.Format("[GameMapScene] Click detected! world=({0:F2}, {1:F2}) -> tile=({2}, {3}) state={4}",
                worldPos.x, worldPos.y, tileX, tileY, currentState));

            // Bounds check
            GameMap gameMap = mapLoader.GameMap;
            if (tileX < 0 || tileX >= gameMap.Width || tileY < 0 || tileY >= gameMap.Height)
            {
                Debug.Log("[GameMapScene] Click out of bounds, ignored.");
                return;
            }

            // During movement, a click triggers pause (hero stops at next tile)
            if (currentState == GameMapState.HeroMoving)
            {
                pauseMovementRequested = true;
                Debug.Log("[GameMapScene] Pause movement requested by click.");
                return;
            }

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

            // Use hero's actual movement points
            int maxMP = selectedHero.GetEffectiveMovePoint();
            int currentMP = selectedHero.GetCurrentMovePoint();

            var ctx = new PathfinderContext(
                selectedHero,
                maxMP,
                currentMP,
                mapLoader.GameMap,
                pathFinderCache.CurrentGameStateVersion);

            // Debug: dump accessibility around hero's position
            print(pathFinder.DebugDumpAccessibility(ctx));

            List<MapPathNode> path = pathFinder.GetPath(ctx, tileX, tileY);
            if (path == null || path.Count < 2)
            {
                print(string.Format("No path to ({0}, {1})", tileX, tileY));
                return;
            }

            currentPath = path;
            selectedDestination = new Vector2Int(tileX, tileY);
            currentState = GameMapState.DestinationSelected;

            // Display path arrows on map (green for reachable, orange for unreachable this turn)
            mapLoader.ShowPath(currentPath);

            print(string.Format("Destination selected at ({0}, {1}), path length: {2}, maxMP: {3}, currentMP: {4}",
                tileX, tileY, path.Count, maxMP, currentMP));
        }

        private void StartHeroMovement()
        {
            if (selectedHero == null || currentPath == null || currentPath.Count < 2)
                return;

            currentState = GameMapState.HeroMoving;
            pauseMovementRequested = false;

            print("Hero moving...");

            StartCoroutine(MoveHeroAlongPath());
        }

        #endregion

        #region Movement Animation

        /// <summary>
        /// Find the index of the last node in the path that is reachable this turn (Turns == 0).
        /// Returns -1 if no node is reachable (shouldn't happen since path[0] is always Turns == 0).
        /// </summary>
        private int FindLastReachableIndex(List<MapPathNode> path)
        {
            int lastReachable = 0;
            for (int i = 1; i < path.Count; i++)
            {
                if (path[i].Turns == 0)
                {
                    lastReachable = i;
                }
                else
                {
                    break; // Nodes beyond are all future turns
                }
            }
            return lastReachable;
        }

        private IEnumerator MoveHeroAlongPath()
        {
            GameObject heroGO = mapLoader.GetHeroGameObject(selectedHero);
            if (heroGO == null)
            {
                print("[MoveHero] heroGO is null, aborting.");
                currentState = GameMapState.Idle;
                yield break;
            }

            // Determine how far the hero can move this turn
            int lastReachableIndex = FindLastReachableIndex(currentPath);
            print(string.Format("[MoveHero] path.Count={0}, lastReachableIndex={1}, node[1].Turns={2}",
                currentPath.Count, lastReachableIndex,
                currentPath.Count > 1 ? currentPath[1].Turns.ToString() : "N/A"));

            if (lastReachableIndex < 1)
            {
                // No reachable tile beyond start - hero can't move
                print("Hero has no movement points remaining this turn.");
                currentState = GameMapState.Idle;
                yield break;
            }

            // Track last movement direction for idle pose
            int lastDx = 0, lastDy = 1;

            // Move along each segment of the path up to the last reachable tile
            int stoppedAtIndex = lastReachableIndex; // will be updated if paused
            for (int i = 1; i <= lastReachableIndex; i++)
            {
                int prevX = currentPath[i - 1].Position.PosX;
                int prevY = currentPath[i - 1].Position.PosY;
                int targetX = currentPath[i].Position.PosX;
                int targetY = currentPath[i].Position.PosY;

                // Set hero walking animation to face the movement direction
                int dx = Mathf.Clamp(targetX - prevX, -1, 1);
                int dy = Mathf.Clamp(targetY - prevY, -1, 1);
                lastDx = dx;
                lastDy = dy;
                mapLoader.SetHeroMovingAnimation(selectedHero, dx, dy);

                // Remove the arrow on the tile we're moving towards (as hero starts overlapping it)
                // pathArrowObjects[0] corresponds to path[1], so index i-1
                mapLoader.RemovePathArrowAtIndex(i - 1);

                Vector3 startPos = heroGO.transform.position;
                Vector3 endPos = mapLoader.HeroTileToWorldPosition(targetX, targetY);

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

                // Check if pause was requested
                if (pauseMovementRequested && i < lastReachableIndex)
                {
                    print(string.Format("[MoveHero] Pausing at path index {0} ({1}, {2})", i, targetX, targetY));
                    stoppedAtIndex = i;
                    break;
                }
            }

            // Movement complete - idle in the last facing direction
            mapLoader.SetHeroIdleAnimation(selectedHero, lastDx, lastDy);

            MapPathNode stopNode = currentPath[stoppedAtIndex];
            int finalX = stopNode.Position.PosX;
            int finalY = stopNode.Position.PosY;
            mapLoader.UpdateHeroPosition(selectedHero, finalX, finalY);

            // Update remaining movement points from the pathfinder node's actual value
            selectedHero.RestMovePoint = stopNode.MoveRemains;

            // Invalidate cached paths for this hero
            pathFinderCache.Invalidate(selectedHero.Identifier);
            pathFinderCache.NextGameStateVersion();

            bool wasPaused = pauseMovementRequested && stoppedAtIndex < lastReachableIndex;
            bool hasRemainingPath = stoppedAtIndex < currentPath.Count - 1;
            pauseMovementRequested = false;

            if (wasPaused || hasRemainingPath)
            {
                // Either paused by click or ran out of movement points with orange tiles ahead.
                // Trim arrows and path to remaining portion, stay in DestinationSelected.
                mapLoader.TrimPathArrowsFromStart(stoppedAtIndex);
                currentPath = currentPath.GetRange(stoppedAtIndex, currentPath.Count - stoppedAtIndex);
                currentState = GameMapState.DestinationSelected;

                print(string.Format("Hero {0} at ({1}, {2}), restMP={3}, remaining path length={4}",
                    wasPaused ? "paused" : "out of move points",
                    finalX, finalY, selectedHero.RestMovePoint, currentPath.Count));
            }
            else
            {
                // Reached final destination
                print(string.Format("Hero reached destination at ({0}, {1}), restMP={2}",
                    finalX, finalY, selectedHero.RestMovePoint));

                // Clear any remaining arrows and reset state
                mapLoader.ClearPath();
                currentPath = null;
                selectedHero = null;
                currentState = GameMapState.Idle;
            }
        }

        #endregion

        #region Loading Overlay

        private Vector3 PixelToWorld(float px, float py, float z = 0f)
        {
            Camera cam = Camera.main;
            float viewHeight = cam.orthographicSize * 2f;
            float scale = viewHeight / (SCREEN_H / PPU);
            float halfW = SCREEN_W / PPU * scale / 2f;
            float halfH = SCREEN_H / PPU * scale / 2f;
            return new Vector3(px / PPU * scale - halfW, halfH - py / PPU * scale, z);
        }

        private void ShowLoadingOverlay(H3DataAccess dataAccess)
        {
            Camera cam = Camera.main;
            float viewHeight = cam.orthographicSize * 2f;
            float scale = viewHeight / (SCREEN_H / PPU);

            loadingOverlay = new GameObject("LoadingOverlay");
            loadingOverlay.transform.parent = transform;

            // Background: "loadbar" PCX from H3 game data
            ImageData bgImage = dataAccess.RetrieveImage("loadbar.pcx");
            if (bgImage != null)
            {
                GameObject bgObj = new GameObject("LoadingBG");
                bgObj.transform.parent = loadingOverlay.transform;
                bgObj.transform.position = PixelToWorld(0, 0, -5f);
                bgObj.transform.localScale = new Vector3(scale, scale, 1);

                SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
                bgRenderer.sprite = Texture2DExtension.CreateSpriteFromImageData(bgImage, new Vector2(0, 1));
                bgRenderer.sortingOrder = 50;
            }
            else
            {
                // Fallback: solid dark background
                GameObject bgObj = new GameObject("LoadingBG");
                bgObj.transform.parent = loadingOverlay.transform;
                bgObj.transform.position = new Vector3(0, 0, -5f);

                SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
                Texture2D bgTex = new Texture2D(1, 1);
                bgTex.SetPixel(0, 0, new UnityEngine.Color(0, 0, 0, 0.85f));
                bgTex.Apply();
                bgRenderer.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                bgRenderer.sortingOrder = 50;
                float viewWidth = viewHeight * cam.aspect;
                bgObj.transform.localScale = new Vector3(viewWidth, viewHeight, 1);
            }

            // Progress blocks: "loadprog" DEF
            progressBlocks.Clear();
            lastVisibleBlocks = 0;

            BundleImageDefinition loadProgDef = dataAccess.RetrieveBundleImage("loadprog.def");
            if (loadProgDef != null)
            {
                for (int i = 0; i < PROGRESS_BLOCK_COUNT; i++)
                {
                    ImageData blockImage = loadProgDef.GetImageData(0, i);
                    if (blockImage == null) continue;

                    GameObject blockObj = new GameObject("ProgressBlock_" + i);
                    blockObj.transform.parent = loadingOverlay.transform;
                    blockObj.transform.position = PixelToWorld(
                        PROGRESS_BAR_X + i * PROGRESS_BLOCK_SIZE,
                        PROGRESS_BAR_Y, -6f);
                    blockObj.transform.localScale = new Vector3(scale, scale, 1);

                    SpriteRenderer blockRenderer = blockObj.AddComponent<SpriteRenderer>();
                    blockRenderer.sprite = Texture2DExtension.CreateSpriteFromImageData(blockImage, new Vector2(0, 1));
                    blockRenderer.sortingOrder = 52;

                    blockObj.SetActive(false);
                    progressBlocks.Add(blockObj);
                }
            }

            // Status text
            GameObject statusObj = new GameObject("LoadingStatus");
            statusObj.transform.parent = loadingOverlay.transform;
            statusObj.transform.position = PixelToWorld(400, 580, -6f);

            loadingStatusText = statusObj.AddComponent<TextMesh>();
            loadingStatusText.text = "";
            loadingStatusText.fontSize = 18;
            loadingStatusText.characterSize = 0.08f * scale;
            loadingStatusText.color = UnityEngine.Color.white;
            loadingStatusText.anchor = TextAnchor.MiddleCenter;
            loadingStatusText.alignment = TextAlignment.Center;
            loadingStatusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusObj.GetComponent<MeshRenderer>().sortingOrder = 53;
        }

        private void UpdateProgressBar(float progress)
        {
            if (progressBlocks.Count == 0) return;

            int visibleCount = Mathf.FloorToInt(progress * progressBlocks.Count);
            visibleCount = Mathf.Clamp(visibleCount, 0, progressBlocks.Count);

            if (visibleCount != lastVisibleBlocks)
            {
                for (int i = lastVisibleBlocks; i < visibleCount; i++)
                {
                    progressBlocks[i].SetActive(true);
                }
                lastVisibleBlocks = visibleCount;
            }
        }

        private void UpdateLoadingStatus(string message)
        {
            if (loadingStatusText != null && !string.IsNullOrEmpty(message))
            {
                loadingStatusText.text = message;
            }
        }

        private void DestroyLoadingOverlay()
        {
            if (loadingOverlay != null)
            {
                Destroy(loadingOverlay);
                loadingOverlay = null;
                progressBlocks.Clear();
                lastVisibleBlocks = 0;
                loadingStatusText = null;
            }
        }

        #endregion
    }
}
