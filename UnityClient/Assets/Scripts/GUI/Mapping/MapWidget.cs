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
    ///   returns offsets relative to the camera's viewport centre — those are
    ///   constant regardless of where the camera is in world space.
    ///
    /// Layout constants below are derived from
    ///   Assets/Resources/config/widgets/adventureMap.json
    /// for the standard 800×600 base resolution.
    /// </summary>
    public class MapWidget : MonoBehaviour
    {
        // -----------------------------------------------------------------------
        // Layout constants (from adventureMap.json, 800×600 base)
        // -----------------------------------------------------------------------

        // Screen reference dimensions used by PixelToLocal()
        private const float SCREEN_W = 800f;
        private const float SCREEN_H = 600f;
        private const float PPU      = 100f;  // pixels per world unit

        // Right panel: 25 % of screen width, full height
        //   JSON: backgroundRightMinimap  area { right:0, top:0, width:199 }
        private const int RightPanelWidth = 200;                          // ~25 % of 800
        private const int RightPanelLeft  = (int)SCREEN_W - RightPanelWidth; // 600

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

        // Root attached to Camera.main — makes all children screen-fixed
        private GameObject uiRoot;

        // Info bar sub-objects
        private GameObject     infoPanelRoot;
        private SpriteRenderer heroPortraitRenderer;
        private TextMesh       heroNameText;
        private TextMesh       heroStatsText;

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

            BuildRightPanelBackground();
            BuildInfoBar();
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

            // Hero name
            string name = hero.Data?.Name;
            if (string.IsNullOrEmpty(name))
                name = "Hero #" + hero.Identifier;
            heroNameText.text = name;

            // Movement points
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

        // -----------------------------------------------------------------------
        // Build helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Renders the right-side panel background (25 % screen width, full height).
        ///
        /// A solid-colour quad is always created first so the panel is guaranteed
        /// to be visible even if AdvMap.pcx is unavailable or has unexpected
        /// dimensions.  The PCX is then overlaid on top when it can be loaded.
        ///
        /// Z NOTE: all camera-child objects must use a POSITIVE local z so their
        /// world z > camera world z (-10), keeping them inside the view frustum.
        /// We use localZ = 1 → worldZ = -9, which is in front of the camera and
        /// above the default near-clip plane (0.3).  SortingOrder still controls
        /// which sprite renders on top.
        /// </summary>
        private void BuildRightPanelBackground()
        {
            Camera cam      = Camera.main;
            float  viewH    = cam.orthographicSize * 2f;
            float  sc       = viewH / (SCREEN_H / PPU);   // 1.0 at 800×600
            float  worldW   = RightPanelWidth / PPU * sc; // 2.0 at 800×600
            float  worldH   = SCREEN_H        / PPU * sc; // 6.0 at 800×600

            // ---- Solid dark-brown background (always visible) ----
            Texture2D solidTex = new Texture2D(1, 1);
            solidTex.SetPixel(0, 0, new Color(0.18f, 0.13f, 0.08f, 1f));
            solidTex.Apply();

            // PPU = 1 → sprite is 1×1 world unit; scale the GO to fill the panel.
            Sprite solidSprite = Sprite.Create(solidTex,
                new Rect(0, 0, 1, 1), new Vector2(0f, 1f), pixelsPerUnit: 1f);

            GameObject panelGO = new GameObject("RightPanelBackground");
            panelGO.transform.SetParent(uiRoot.transform, worldPositionStays: false);
            SpriteRenderer sr   = panelGO.AddComponent<SpriteRenderer>();
            sr.sprite            = solidSprite;
            sr.sortingOrder      = 109;
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

            // Clip the right RightPanelWidth columns, full height.
            // LoadFromData y-flips the image (y=0 = texture bottom = visual top),
            // so Rect(x, 0, w, tex.height) covers the full strip correctly.
            int  srcX      = pcxTex.width - RightPanelWidth;
            Rect clipRect  = new Rect(srcX, 0, RightPanelWidth, pcxTex.height);
            Sprite pcxSprite = Sprite.Create(pcxTex, clipRect, new Vector2(0f, 1f), PPU);

            GameObject pcxGO = new GameObject("RightPanelPCX");
            pcxGO.transform.SetParent(uiRoot.transform, worldPositionStays: false);
            SpriteRenderer pcxSr = pcxGO.AddComponent<SpriteRenderer>();
            pcxSr.sprite       = pcxSprite;
            pcxSr.sortingOrder = 110;
            pcxGO.transform.localPosition = PixelToLocal(RightPanelLeft, 0f, 0.9f);
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

            // Group 0, frame 0 = standing south — used as portrait stand-in
            ImageData frame = def.GetImageData(0, 0);
            if (frame == null) { heroPortraitRenderer.sprite = null; return; }

            Texture2D tex = Texture2DExtension.LoadFromData(frame);
            heroPortraitRenderer.sprite = Texture2DExtension.CreateSpriteFromTexture(tex, new Vector2(0f, 1f));

            // Scale so the portrait fits within PortraitSize × PortraitSize pixels
            float viewH   = Camera.main.orthographicSize * 2f;
            float scale   = viewH / (SCREEN_H / PPU);
            float maxWorld = PortraitSize / PPU * scale;
            float spriteW  = frame.Width  / PPU;
            float spriteH  = frame.Height / PPU;
            float fit      = Mathf.Min(maxWorld / spriteW, maxWorld / spriteH);
            heroPortraitRenderer.transform.localScale = new Vector3(fit, fit, 1f);
        }

        // -----------------------------------------------------------------------
        // Coordinate helper
        // -----------------------------------------------------------------------

        /// <summary>
        /// Convert pixel coordinates (top-left origin, 800×600 reference) to a
        /// position in camera-local space.
        ///
        /// Because all UI GameObjects are children of Camera.main, setting
        /// transform.localPosition with the value returned here keeps the object
        /// at a constant screen position even when the camera moves.
        ///
        /// The maths is identical to the PixelToWorld() helper in GameMapScene —
        /// the key difference is that the result is used as localPosition (not
        /// world position), which is correct when the parent is Camera.main.
        /// </summary>
        private static Vector3 PixelToLocal(float px, float py, float z = -1f)
        {
            Camera cam       = Camera.main;
            float viewHeight = cam.orthographicSize * 2f;
            float scale      = viewHeight / (SCREEN_H / PPU);
            float halfW      = SCREEN_W / PPU * scale / 2f;
            float halfH      = SCREEN_H / PPU * scale / 2f;
            return new Vector3(px / PPU * scale - halfW, halfH - py / PPU * scale, z);
        }
    }
}
