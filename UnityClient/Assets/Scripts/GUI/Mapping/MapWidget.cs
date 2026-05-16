using System;
using UnityEngine;
using H3Engine.DataAccess;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.MapObjects;
using UnityClient.GUI.Rendering;

namespace UnityClient.GUI.Mapping
{
    /// <summary>
    /// Adventure map UI overlay widget.
    /// Corresponds to VCMI's AdventureMapWidget — builds the right-side panel,
    /// info bar, and other overlay elements from adventureMap.json layout data.
    ///
    /// KEY DESIGN — Screen-fixed UI vs. scrollable map:
    ///   MapCamera scrolls the map by moving Camera.main.transform.position.
    ///   Any world-space GameObject at a fixed world coordinate therefore drifts
    ///   off-screen when the player drags the map.
    ///
    ///   Solution: all UI GameObjects are created as children of Camera.main so
    ///   they move with the camera and always appear at the same screen position.
    ///   Positions are set via transform.localPosition using PixelToLocal(), which
    ///   converts 800×600 pixel references to camera-local world units.
    ///
    /// COORDINATE SYSTEM:
    ///   GameMapScene.Start() sets  Camera.main.orthographicSize = Screen.height / (2·PPU)
    ///   so that 1 screen pixel = 1/PPU world units in both axes.
    ///   PixelToLocal therefore uses Screen.width/Screen.height directly —
    ///   avoiding camera.aspect which may not be initialised until the next frame.
    ///
    ///   X — RIGHT-ANCHORED (fixed pixels from the right screen edge):
    ///     halfW = Screen.width  / (2·PPU)   — actual right-edge world coordinate
    ///     x     = halfW  −  (SCREEN_W − px) / PPU
    ///     e.g. px=800 → x=halfW (right edge); px=600 → x=halfW−2 (200px from right)
    ///
    ///   Y — TOP-ANCHORED (fixed pixels from the top screen edge):
    ///     halfH = Screen.height / (2·PPU)   — actual top-edge world coordinate
    ///     y     = halfH  −  py / PPU
    ///     e.g. py=0 → y=halfH (top edge); py=600 → y=halfH−6 (=−halfH at 600px)
    ///
    /// Layout constants below are derived from
    ///   Assets/Resources/config/widgets/adventureMap.json
    /// for the standard 800×600 base resolution.
    /// </summary>
    // Run before GameMapScene so UIClickConsumed is set before the map processes the same click.
    [DefaultExecutionOrder(-1)]
    public class MapWidget : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Layout constants (from adventureMap.json, 800×600 base)
        // -----------------------------------------------------------------------

        // Screen reference dimensions used by PixelToLocal()
        private const float SCREEN_W = 800f;
        private const float SCREEN_H = 600f;
        private const float PPU      = 100f;  // pixels per world unit (matches GameMapScene)

        // Right panel: rightmost 200 px, full height
        //   JSON: backgroundRightMinimap  area { right:0, top:0, width:199 }
        private const int RightPanelWidth = 200;
        private const int RightPanelLeft  = (int)SCREEN_W - RightPanelWidth; // 600

        // Underground / surface toggle button
        //   JSON: buttonsContainer  area { top:196, right:57, width:64 }
        //   toggle button inside: top=0, left=32, width=32, height=32
        //   Absolute pixel: left = 800 - 57 - 64 + 32 = 711, top = 196
        private const int ToggleBtnPixelX    = 711;
        private const int ToggleBtnPixelY    = 196;
        private const int ToggleBtnPixelSize = 32;

        // Info bar (inside container)
        //   JSON: infoBarContainer  area { bottom:0, right:0, width:199, height:210 }
        //   JSON: infoBar  area { bottom:44, right:19, width:175, height:168 }
        //   Absolute on screen: left = 601 + (199-19-175) = 606
        //                       top  = 600 - 44 - 168     = 388
        private const int InfoBarLeft   = 606;
        private const int InfoBarTop    = 388;
        private const int InfoBarWidth  = 175;
        private const int InfoBarHeight = 168;

        // Portrait (top portion of the info bar)
        private const int PortraitLeft = InfoBarLeft + 4;
        private const int PortraitTop  = InfoBarTop  + 4;
        private const int PortraitSize = 58;

        // Text positions within the info bar
        private const int HeroNameLeft  = InfoBarLeft + 4;
        private const int HeroNameTop   = InfoBarTop  + 68;
        private const int HeroStatsLeft = InfoBarLeft + 4;
        private const int HeroStatsTop  = InfoBarTop  + 90;

        // -----------------------------------------------------------------------
        // Private state
        // -----------------------------------------------------------------------

        private H3DataAccess dataAccess;
        private MapComponent mapComponent;

        // Root attached to Camera.main — makes all children screen-fixed.
        // Camera.main may be scrolled by MapCamera, but children always
        // appear at the same localPosition relative to the camera's viewport.
        private GameObject uiRoot;

        // Info bar sub-objects
        private GameObject     infoPanelRoot;
        private SpriteRenderer heroPortraitRenderer;
        private TextMesh       heroNameText;
        private TextMesh       heroStatsText;

        // Underground toggle button
        private GameObject     toggleBtnGO;
        private SpriteRenderer toggleBtnRenderer;
        private Sprite         spriteGoUnderground; // IAM010.DEF — shown on surface, click → go underground
        private Sprite         spriteGoSurface;     // IAM003.DEF — shown underground, click → go surface
        private bool           hasUnderground  = false; // true once underground map is ready
        private int            currentMapLevel = 0;     // 0 = surface, 1 = underground

        /// <summary>Fired when the underground/surface toggle button is clicked.</summary>
        public Action OnToggleMapLevel { get; set; }

        /// <summary>
        /// True during the frame a UI button consumed a mouse click.
        /// GameMapScene must check this and skip map-tile click processing when set.
        /// </summary>
        public bool UIClickConsumed { get; private set; }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initialise the widget. Call once after the map has been rendered so
        /// that hero DEF file names are already populated in MapComponent.
        /// </summary>
        public void Initialize(H3DataAccess dataAccess, MapComponent mapComponent)
        {
            this.dataAccess   = dataAccess;
            this.mapComponent = mapComponent;

            // All UI lives under a child of Camera.main so it moves with
            // the camera and is always screen-fixed, regardless of map scroll.
            uiRoot = new GameObject("MapWidgetUIRoot");
            uiRoot.transform.SetParent(Camera.main.transform, worldPositionStays: false);
            uiRoot.transform.localPosition = Vector3.zero;

            // Log key layout values once for diagnostics.
            float halfW = Screen.width  / (2f * PPU);
            float halfH = Screen.height / (2f * PPU);
            Debug.Log(string.Format(
                "[MapWidget] Init — Screen={0}×{1}, halfW={2:F3}, halfH={3:F3}, panelLocalX={4:F3}",
                Screen.width, Screen.height, halfW, halfH,
                halfW - RightPanelWidth / PPU));

            BuildRightPanelBackground();
            BuildInfoBar();
            BuildToggleButton();
            ShowEmptyInfo();
        }

        /// <summary>
        /// Display hero information in the info bar (called when a hero is selected).
        /// Corresponds to VCMI CInfoBar::showHeroSelection().
        /// </summary>
        public void ShowHeroInfo(HeroInstance hero)
        {
            if (infoPanelRoot == null || hero == null) return;

            infoPanelRoot.SetActive(true);

            string name = hero.Data?.Name;
            if (string.IsNullOrEmpty(name))
                name = "Hero #" + hero.Identifier;
            heroNameText.text = name;

            int cur = hero.GetCurrentMovePoint();
            int max = hero.GetEffectiveMovePoint();
            heroStatsText.text = string.Format("Move: {0} / {1}", cur, max);

            LoadHeroPortrait(hero);
        }

        /// <summary>
        /// Revert the info bar to its empty / default state.
        /// Corresponds to VCMI CInfoBar::popAll() + showGameStatus().
        /// </summary>
        public void ShowEmptyInfo()
        {
            if (infoPanelRoot == null) return;

            infoPanelRoot.SetActive(false);
            heroNameText.text  = string.Empty;
            heroStatsText.text = string.Empty;
            if (heroPortraitRenderer != null)
                heroPortraitRenderer.sprite = null;
        }

        /// <summary>
        /// Signal that the underground map is ready. The toggle button becomes functional
        /// and its icon becomes active. Call once underground rendering is complete.
        /// </summary>
        public void EnableUndergroundToggle()
        {
            hasUnderground = true;
            UpdateToggleBtnSprite();
            Debug.Log("[MapWidget] Underground toggle enabled.");
        }

        /// <summary>
        /// Refresh the toggle button icon to match the currently active map level.
        /// 0 = surface (show "go underground" icon), 1 = underground (show "go surface" icon).
        /// </summary>
        public void SetMapLevel(int level)
        {
            currentMapLevel = level;
            UpdateToggleBtnSprite();
        }

        // -----------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------

        void Update()
        {
            UIClickConsumed = false;

            // The button is visible regardless; clicks only fire when underground exists.
            if (!Input.GetMouseButtonDown(0)) return;

            // Convert actual screen mouse position to 800×600 reference pixel space.
            // Input.mousePosition has y=0 at bottom; reference has y=0 at top.
            float refX = Input.mousePosition.x / Screen.width  * SCREEN_W;
            float refY = (1f - Input.mousePosition.y / Screen.height) * SCREEN_H;

            bool overButton = refX >= ToggleBtnPixelX &&
                              refX <= ToggleBtnPixelX + ToggleBtnPixelSize &&
                              refY >= ToggleBtnPixelY &&
                              refY <= ToggleBtnPixelY + ToggleBtnPixelSize;

            if (!overButton) return;

            UIClickConsumed = true; // prevent map click falling through

            if (!hasUnderground)
            {
                Debug.Log("[MapWidget] Toggle button clicked but map has no underground layer.");
                return;
            }

            Debug.Log("[MapWidget] Toggle button clicked — switching map level.");
            OnToggleMapLevel?.Invoke();
        }

        // -----------------------------------------------------------------------
        // Build helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Renders the right-side panel background (200 px wide, full height).
        ///
        /// The panel is RIGHT-ANCHORED: its right edge is always flush with the
        /// screen right edge, and its width is exactly RightPanelWidth pixels,
        /// regardless of the screen's actual resolution.
        ///
        /// The solid-colour quad is created first so the panel is always visible
        /// even if AdvMap.pcx is unavailable.  The PCX overlay is drawn on top.
        ///
        /// Z: Camera.main is at z=-10; children at localZ > 0 have world z > -10,
        /// placing them in front of the camera (above near-clip plane 0.3).
        /// SortingOrder controls which sprite appears on top within that space.
        /// </summary>
        private void BuildRightPanelBackground()
        {
            float worldW = RightPanelWidth / PPU;   // 2.0 wu = 200 reference px
            float worldH = SCREEN_H        / PPU;   // 6.0 wu = 600 reference px

            // ---- Solid dark-brown background (always visible) ----
            Texture2D solidTex = new Texture2D(1, 1);
            solidTex.SetPixel(0, 0, new Color(0.18f, 0.13f, 0.08f, 1f));
            solidTex.Apply();

            // pixelsPerUnit=1 → sprite is 1×1 wu; scale the GO to fill the panel.
            Sprite solidSprite = Sprite.Create(solidTex,
                new Rect(0, 0, 1, 1), new Vector2(0f, 1f), pixelsPerUnit: 1f);

            GameObject panelGO = new GameObject("RightPanelBackground");
            panelGO.transform.SetParent(uiRoot.transform, worldPositionStays: false);
            SpriteRenderer sr = panelGO.AddComponent<SpriteRenderer>();
            sr.sprite       = solidSprite;
            sr.sortingOrder = 109;
            panelGO.transform.localPosition = PixelToLocal(RightPanelLeft, 0f, 1f);
            panelGO.transform.localScale    = new Vector3(worldW, worldH, 1f);

            // ---- Optional AdvMap.pcx overlay ----
            ImageData pcxData = dataAccess?.RetrieveImage("AdvMap.pcx");
            if (pcxData == null)
            {
                Debug.Log("[MapWidget] AdvMap.pcx not found — using solid background.");
                return;
            }

            Texture2D pcxTex = Texture2DExtension.LoadFromData(pcxData);
            if (pcxTex == null || pcxTex.width < RightPanelWidth || pcxTex.height == 0)
            {
                Debug.LogWarning(string.Format(
                    "[MapWidget] AdvMap.pcx size {0}×{1} too small — skipping overlay.",
                    pcxTex?.width ?? 0, pcxTex?.height ?? 0));
                return;
            }

            // Clip the rightmost RightPanelWidth columns, full height.
            int    srcX     = pcxTex.width - RightPanelWidth;
            Rect   clipRect = new Rect(srcX, 0, RightPanelWidth, pcxTex.height);
            Sprite pcxSprite = Sprite.Create(pcxTex, clipRect, new Vector2(0f, 1f), PPU);

            // Natural world size of the clipped sprite at PPU
            float natW = RightPanelWidth / PPU;   // = 2.0 wu
            float natH = pcxTex.height   / PPU;

            GameObject pcxGO = new GameObject("RightPanelPCX");
            pcxGO.transform.SetParent(uiRoot.transform, worldPositionStays: false);
            SpriteRenderer pcxSr = pcxGO.AddComponent<SpriteRenderer>();
            pcxSr.sprite       = pcxSprite;
            pcxSr.sortingOrder = 110;
            pcxGO.transform.localPosition = PixelToLocal(RightPanelLeft, 0f, 0.9f);
            // Scale to fill worldW × worldH exactly
            pcxGO.transform.localScale = new Vector3(worldW / natW, worldH / natH, 1f);
        }

        /// <summary>
        /// Creates the info bar panel (background + hero portrait + text labels).
        /// Corresponds to the adventureInfobar widget in adventureMap.json.
        /// </summary>
        private void BuildInfoBar()
        {
            infoPanelRoot = new GameObject("InfoBarPanel");
            infoPanelRoot.transform.SetParent(uiRoot.transform, worldPositionStays: false);

            // ---- Dark background ----
            Texture2D bgTex   = new Texture2D(InfoBarWidth, InfoBarHeight);
            Color     bgColor = new Color(0.08f, 0.06f, 0.04f, 0.92f);
            Color[]   pixels  = new Color[InfoBarWidth * InfoBarHeight];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = bgColor;
            bgTex.SetPixels(pixels);
            bgTex.Apply();

            Sprite bgSprite = Sprite.Create(bgTex,
                new Rect(0, 0, InfoBarWidth, InfoBarHeight),
                new Vector2(0f, 1f), PPU);

            GameObject bgGO = new GameObject("InfoBarBg");
            bgGO.transform.SetParent(infoPanelRoot.transform, worldPositionStays: false);
            SpriteRenderer bgSr = bgGO.AddComponent<SpriteRenderer>();
            bgSr.sprite       = bgSprite;
            bgSr.sortingOrder = 112;
            bgGO.transform.localPosition = PixelToLocal(InfoBarLeft, InfoBarTop, 0.8f);

            // ---- Hero portrait ----
            GameObject portraitGO = new GameObject("HeroPortrait");
            portraitGO.transform.SetParent(infoPanelRoot.transform, worldPositionStays: false);
            heroPortraitRenderer = portraitGO.AddComponent<SpriteRenderer>();
            heroPortraitRenderer.sortingOrder = 115;
            portraitGO.transform.localPosition = PixelToLocal(PortraitLeft, PortraitTop, 0.6f);

            // ---- Hero name ----
            GameObject nameGO = new GameObject("HeroName");
            nameGO.transform.SetParent(infoPanelRoot.transform, worldPositionStays: false);
            heroNameText               = nameGO.AddComponent<TextMesh>();
            heroNameText.fontSize      = 14;
            heroNameText.characterSize = 0.08f;
            heroNameText.color         = Color.white;
            heroNameText.anchor        = TextAnchor.UpperLeft;
            heroNameText.alignment     = TextAlignment.Left;
            heroNameText.font          = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameGO.GetComponent<MeshRenderer>().sortingOrder = 116;
            nameGO.transform.localPosition = PixelToLocal(HeroNameLeft, HeroNameTop, 0.6f);

            // ---- Hero stats ----
            GameObject statsGO = new GameObject("HeroStats");
            statsGO.transform.SetParent(infoPanelRoot.transform, worldPositionStays: false);
            heroStatsText               = statsGO.AddComponent<TextMesh>();
            heroStatsText.fontSize      = 12;
            heroStatsText.characterSize = 0.065f;
            heroStatsText.color         = new Color(0.8f, 0.8f, 0.5f);
            heroStatsText.anchor        = TextAnchor.UpperLeft;
            heroStatsText.alignment     = TextAlignment.Left;
            heroStatsText.font          = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statsGO.GetComponent<MeshRenderer>().sortingOrder = 116;
            statsGO.transform.localPosition = PixelToLocal(HeroStatsLeft, HeroStatsTop, 0.6f);

            infoPanelRoot.SetActive(false);
        }

        /// <summary>
        /// Attempts to load and display the hero's walking DEF first frame as a portrait.
        /// </summary>
        private void LoadHeroPortrait(HeroInstance hero)
        {
            if (heroPortraitRenderer == null || mapComponent == null) return;

            if (!mapComponent.heroDefFileNames.TryGetValue(hero.Identifier, out string defFile))
            {
                heroPortraitRenderer.sprite = null;
                return;
            }

            BundleImageDefinition def = dataAccess.RetrieveBundleImage(defFile);
            if (def == null) { heroPortraitRenderer.sprite = null; return; }

            ImageData frame = def.GetImageData(0, 0);
            if (frame == null) { heroPortraitRenderer.sprite = null; return; }

            Texture2D tex = Texture2DExtension.LoadFromData(frame);
            heroPortraitRenderer.sprite = Texture2DExtension.CreateSpriteFromTexture(tex, new Vector2(0f, 1f));

            // Scale portrait to fit within PortraitSize × PortraitSize pixels.
            // 1 world unit = PPU screen pixels, so PortraitSize px = PortraitSize/PPU wu.
            float maxWorld = PortraitSize / PPU;
            float spriteW  = frame.Width  / PPU;
            float spriteH  = frame.Height / PPU;
            float fit      = Mathf.Min(maxWorld / spriteW, maxWorld / spriteH);
            heroPortraitRenderer.transform.localScale = new Vector3(fit, fit, 1f);
        }

        /// <summary>
        /// Creates the underground/surface toggle button at its fixed panel position.
        /// Loads IAM010.DEF (go-underground icon) and IAM003.DEF (go-surface icon).
        /// The button is always visible; it becomes functional once EnableUndergroundToggle()
        /// is called after the underground map has been rendered.
        /// </summary>
        private void BuildToggleButton()
        {
            spriteGoUnderground = LoadDefSprite("IAM010.DEF");
            spriteGoSurface     = LoadDefSprite("IAM003.DEF");

            if (spriteGoUnderground == null)
                Debug.LogWarning("[MapWidget] IAM010.DEF not found — toggle button will use fallback colour.");
            if (spriteGoSurface == null)
                Debug.LogWarning("[MapWidget] IAM003.DEF not found — toggle button will use fallback colour.");

            toggleBtnGO = new GameObject("ToggleUndergroundBtn");
            toggleBtnGO.transform.SetParent(uiRoot.transform, worldPositionStays: false);
            toggleBtnGO.transform.localPosition = PixelToLocal(ToggleBtnPixelX, ToggleBtnPixelY, 0.85f);

            toggleBtnRenderer              = toggleBtnGO.AddComponent<SpriteRenderer>();
            toggleBtnRenderer.sortingOrder = 111;

            // Always start visible so it can be seen/clicked even before underground is loaded.
            UpdateToggleBtnSprite();
        }

        private void UpdateToggleBtnSprite()
        {
            if (toggleBtnGO == null) return;

            // Choose icon: surface view → go-underground icon; underground view → go-surface icon.
            Sprite icon = (currentMapLevel == 0) ? spriteGoUnderground : spriteGoSurface;

            if (icon != null)
            {
                toggleBtnRenderer.sprite = icon;

                // Scale to fit exactly ToggleBtnPixelSize × ToggleBtnPixelSize in reference pixels.
                float targetWorld = ToggleBtnPixelSize / PPU;
                float spriteW     = icon.texture.width  / PPU;
                float spriteH     = icon.texture.height / PPU;
                float fit         = Mathf.Min(targetWorld / spriteW, targetWorld / spriteH);
                toggleBtnGO.transform.localScale = new Vector3(fit, fit, 1f);
            }
            else
            {
                // DEF not available — render a small coloured square as fallback.
                toggleBtnRenderer.sprite = MakeSolidSprite(
                    hasUnderground ? new Color(0.2f, 0.4f, 0.8f) : new Color(0.3f, 0.3f, 0.3f));
                float size = ToggleBtnPixelSize / PPU;
                toggleBtnGO.transform.localScale = new Vector3(size, size, 1f);
            }
        }

        /// <summary>Creates a 1×1 solid-colour sprite (pivot top-left) for use as a fallback.</summary>
        private static Sprite MakeSolidSprite(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 1f), pixelsPerUnit: 1f);
        }

        /// <summary>
        /// Load the first frame (group 0, frame 0) of a DEF file as a sprite with pivot (0, 1).
        /// Returns null if the file is not found or empty.
        /// </summary>
        private Sprite LoadDefSprite(string defFile)
        {
            BundleImageDefinition def = dataAccess?.RetrieveBundleImage(defFile);
            if (def == null)
            {
                Debug.LogWarning("[MapWidget] DEF not found: " + defFile);
                return null;
            }

            ImageData imgData = def.GetImageData(0, 0);
            if (imgData == null) return null;

            Texture2D tex = Texture2DExtension.LoadFromData(imgData);
            return Texture2DExtension.CreateSpriteFromTexture(tex, new Vector2(0f, 1f));
        }

        // -----------------------------------------------------------------------
        // Coordinate helper
        // -----------------------------------------------------------------------

        /// <summary>
        /// Convert pixel coordinates (top-left origin, 800×600 reference) to a
        /// localPosition inside Camera.main's coordinate space.
        ///
        /// Because Camera.main has no rotation, localPosition == world-offset from
        /// the camera's position.  Setting transform.localPosition with the value
        /// returned here keeps the object at a constant screen position even when
        /// the camera is scrolled by MapCamera.
        ///
        /// X — RIGHT-ANCHORED, fixed pixels from the actual right screen edge:
        ///   halfW = Screen.width / (2·PPU)          — right-edge world coord
        ///   x     = halfW − (SCREEN_W − px) / PPU
        ///   px=SCREEN_W → x = halfW  (right screen edge)
        ///   px=600      → x = halfW − 2.0  (200 px / PPU from right)
        ///
        /// Y — TOP-ANCHORED, fixed pixels from the actual top screen edge:
        ///   halfH = Screen.height / (2·PPU)          — top-edge world coord
        ///   y     = halfH − py / PPU
        ///   py=0        → y = halfH (top screen edge)
        ///   py=600      → y = halfH − 6.0 (= −halfH at Screen.height=600)
        ///
        /// Uses Screen.width/Screen.height instead of camera.aspect to avoid a
        /// Unity initialisation quirk where aspect is not yet set on a freshly
        /// created Camera component.
        /// </summary>
        private static Vector3 PixelToLocal(float px, float py, float z = -1f)
        {
            // Use the FIXED 800×600 reference dimensions — NOT Screen.width/height.
            //
            // The H3 game content is always laid out in an 800×600 pixel reference
            // space. In camera-local world units (PPU=100), this maps to:
            //   X: −4.0 (left edge) … +4.0 (right edge)  i.e. halfW = 4.0
            //   Y: −3.0 (bottom)    … +3.0 (top edge)    i.e. halfH = 3.0
            //
            // Using Screen.width/Screen.height instead would scale positions with
            // the actual Game View size, shifting the panel outside the game content
            // area on any screen that is not exactly 800×600.
            float halfW = SCREEN_W / (2f * PPU);   // 4.0 wu — fixed reference right edge
            float halfH = SCREEN_H / (2f * PPU);   // 3.0 wu — fixed reference top edge

            float x = halfW - (SCREEN_W - px) / PPU;   // right-anchored, fixed px
            float y = halfH - py / PPU;                 // top-anchored,   fixed px
            return new Vector3(x, y, z);
        }
    }
}
