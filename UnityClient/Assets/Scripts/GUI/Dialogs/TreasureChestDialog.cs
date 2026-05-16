using UnityEngine;

namespace UnityClient.GUI.Dialogs
{
    /// <summary>
    /// Choice made by the player in the treasure-chest dialog.
    /// </summary>
    public enum TreasureChestChoice
    {
        None,
        Gold,
        Experience
    }

    /// <summary>
    /// Modal dialog shown when a hero steps adjacent to a treasure chest.
    /// Displays the gold and experience reward amounts and lets the player
    /// choose one.  The dialog is created and destroyed programmatically
    /// (no prefab required) using the same sprite/TextMesh pattern used
    /// elsewhere in the project.
    ///
    /// Usage:
    ///   1. Attach to (or AddComponent on) any persistent GameObject.
    ///   2. Call Show(goldAmount, expAmount).
    ///   3. Yield until IsAnswered == true in a coroutine.
    ///   4. Read SelectedChoice, then call Dismiss().
    ///
    /// Input handling: the dialog reads Input.GetMouseButtonDown(0) in
    /// Update() and hit-tests the two button world-space Rects.  The caller
    /// must prevent the underlying map from processing the same click (e.g.
    /// by checking IsAnswered before the normal click handler).
    /// </summary>
    public class TreasureChestDialog : MonoBehaviour
    {
        // ── Layout constants (in H3 "pixel" units at 100 PPU) ──────────────
        private const float SCREEN_W  = 800f;
        private const float SCREEN_H  = 600f;
        private const float PPU       = 100f;

        // Dialog panel size (pixels)
        private const float PANEL_W   = 420f;
        private const float PANEL_H   = 200f;

        // Button dimensions (pixels)
        private const float BTN_W     = 170f;
        private const float BTN_H     =  44f;

        // Sorting orders (rendered on top of everything)
        private const int ORDER_OVERLAY = 200;
        private const int ORDER_PANEL   = 201;
        private const int ORDER_TEXT    = 202;
        private const int ORDER_BTN     = 203;
        private const int ORDER_BTN_TXT = 204;

        // ── State ──────────────────────────────────────────────────────────
        public bool              IsAnswered    { get; private set; }
        public TreasureChestChoice SelectedChoice { get; private set; }

        // World-space hit bounds for each button (set during Show).
        private Rect goldButtonRect;
        private Rect expButtonRect;

        // Root for all dialog GameObjects so Dismiss() can destroy in one call.
        private GameObject dialogRoot;

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Builds and displays the dialog.  Call once before yielding.
        /// </summary>
        public void Show(int goldAmount, int expAmount)
        {
            IsAnswered     = false;
            SelectedChoice = TreasureChestChoice.None;

            dialogRoot = new GameObject("TreasureChestDialog");
            dialogRoot.transform.SetParent(transform, false);

            float scale = GetWorldScale();

            // ── Semi-transparent full-screen overlay ──────────────────────
            CreateColoredQuad(
                "Overlay",
                dialogRoot.transform,
                new Vector3(0f, 0f, -1f),
                new Vector2(SCREEN_W / PPU * scale * 2f, SCREEN_H / PPU * scale * 2f),
                new Color(0f, 0f, 0f, 0.55f),
                ORDER_OVERLAY);

            // ── Dialog panel (dark brown, centred) ────────────────────────
            Vector3 panelWorld = PixelToWorld(
                (SCREEN_W - PANEL_W) * 0.5f,
                (SCREEN_H - PANEL_H) * 0.5f);

            CreateColoredQuad(
                "Panel",
                dialogRoot.transform,
                panelWorld + new Vector3(0f, 0f, -0.5f),
                new Vector2(PANEL_W / PPU * scale, PANEL_H / PPU * scale),
                new Color(0.18f, 0.12f, 0.06f, 1f),
                ORDER_PANEL,
                pivot: new Vector2(0f, 1f));

            // ── Title text ────────────────────────────────────────────────
            Vector3 titleWorld = PixelToWorld(
                (SCREEN_W - PANEL_W) * 0.5f + 10f,
                (SCREEN_H - PANEL_H) * 0.5f + 12f);

            CreateText(
                "TitleText",
                dialogRoot.transform,
                titleWorld + new Vector3(0f, 0f, -1f),
                "You have found a Treasure Chest!",
                scale * 0.18f,
                Color.white,
                ORDER_TEXT,
                anchor: TextAnchor.UpperLeft);

            CreateText(
                "SubText",
                dialogRoot.transform,
                titleWorld + new Vector3(0f, -0.28f * scale, -1f),
                "What would you like to take?",
                scale * 0.15f,
                new Color(0.9f, 0.85f, 0.7f),
                ORDER_TEXT,
                anchor: TextAnchor.UpperLeft);

            // ── Gold button ───────────────────────────────────────────────
            float btnY  = (SCREEN_H - PANEL_H) * 0.5f + PANEL_H - BTN_H - 20f;
            float btnX1 = (SCREEN_W - PANEL_W) * 0.5f + 30f;
            float btnX2 = btnX1 + BTN_W + 20f;

            goldButtonRect = CreateButton(
                "GoldButton",
                dialogRoot.transform,
                btnX1, btnY,
                string.Format("Take Gold\n({0})", goldAmount),
                new Color(0.55f, 0.40f, 0.10f),
                new Color(1.00f, 0.85f, 0.30f),
                scale);

            expButtonRect = CreateButton(
                "ExpButton",
                dialogRoot.transform,
                btnX2, btnY,
                string.Format("Take Experience\n({0})", expAmount),
                new Color(0.10f, 0.30f, 0.55f),
                new Color(0.70f, 0.90f, 1.00f),
                scale);
        }

        /// <summary>
        /// Destroys all dialog GameObjects.  Call after reading SelectedChoice.
        /// </summary>
        public void Dismiss()
        {
            if (dialogRoot != null)
            {
                Destroy(dialogRoot);
                dialogRoot = null;
            }
        }

        // ── Unity message ──────────────────────────────────────────────────

        private void Update()
        {
            if (IsAnswered || dialogRoot == null) return;

            if (!Input.GetMouseButtonDown(0)) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f;

            if (goldButtonRect.Contains(new Vector2(worldPos.x, worldPos.y)))
            {
                SelectedChoice = TreasureChestChoice.Gold;
                IsAnswered     = true;
            }
            else if (expButtonRect.Contains(new Vector2(worldPos.x, worldPos.y)))
            {
                SelectedChoice = TreasureChestChoice.Experience;
                IsAnswered     = true;
            }
            // Clicks outside buttons are swallowed (dialog is modal).
        }

        // ── Helpers ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the uniform world-scale factor that maps H3 pixel coords to
        /// Unity world units.  Mirrors PixelToWorld in GameMapScene.
        /// </summary>
        private static float GetWorldScale()
        {
            Camera cam = Camera.main;
            float viewHeight = cam.orthographicSize * 2f;
            return viewHeight / (SCREEN_H / PPU);
        }

        /// <summary>
        /// Converts H3 screen-pixel coordinates (origin top-left) to Unity
        /// world coordinates.  Mirrors PixelToWorld in GameMapScene.
        /// </summary>
        private static Vector3 PixelToWorld(float px, float py, float z = 0f)
        {
            Camera cam = Camera.main;
            float viewHeight = cam.orthographicSize * 2f;
            float scale      = viewHeight / (SCREEN_H / PPU);
            float halfW      = SCREEN_W / PPU * scale / 2f;
            float halfH      = SCREEN_H / PPU * scale / 2f;
            return new Vector3(px / PPU * scale - halfW, halfH - py / PPU * scale, z);
        }

        /// <summary>
        /// Creates a solid-colour quad child GameObject.
        /// </summary>
        private static void CreateColoredQuad(
            string goName,
            Transform parent,
            Vector3 worldPos,
            Vector2 worldSize,
            Color color,
            int sortingOrder,
            Vector2? pivot = null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();

            Vector2 p = pivot ?? new Vector2(0.5f, 0.5f);
            Sprite spr = Sprite.Create(tex, new Rect(0, 0, 1, 1), p, 1f);

            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.transform.position   = worldPos;
            go.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.sortingOrder = sortingOrder;
        }

        /// <summary>
        /// Creates a TextMesh child GameObject.
        /// </summary>
        private static void CreateText(
            string goName,
            Transform parent,
            Vector3 worldPos,
            string text,
            float charSize,
            Color color,
            int sortingOrder,
            TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject go = new GameObject(goName);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;

            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text      = text;
            tm.fontSize  = 24;
            tm.characterSize = charSize;
            tm.color     = color;
            tm.anchor    = anchor;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = sortingOrder;
        }

        /// <summary>
        /// Creates a button (coloured background + text label) and returns the
        /// world-space Rect that represents its clickable area.
        /// </summary>
        private Rect CreateButton(
            string goName,
            Transform parent,
            float pixelX, float pixelY,
            string label,
            Color bgColor,
            Color textColor,
            float scale)
        {
            Vector3 btnWorld = PixelToWorld(pixelX, pixelY);
            Vector2 btnWorldSize = new Vector2(BTN_W / PPU * scale, BTN_H / PPU * scale);

            // Background quad (pivot top-left)
            CreateColoredQuad(
                goName + "_BG",
                parent,
                btnWorld + new Vector3(0f, 0f, -0.2f),
                btnWorldSize,
                bgColor,
                ORDER_BTN,
                pivot: new Vector2(0f, 1f));

            // Border (slightly larger, behind background)
            CreateColoredQuad(
                goName + "_Border",
                parent,
                btnWorld + new Vector3(-0.01f * scale, 0.01f * scale, -0.1f),
                btnWorldSize + new Vector2(0.04f * scale, 0.04f * scale),
                new Color(0.9f, 0.8f, 0.5f),
                ORDER_BTN - 1,
                pivot: new Vector2(0f, 1f));

            // Label text
            Vector3 textOffset = new Vector3(
                btnWorldSize.x * 0.5f,
                -btnWorldSize.y * 0.25f,
                -0.5f);

            CreateText(
                goName + "_Label",
                parent,
                btnWorld + textOffset,
                label,
                scale * 0.14f,
                textColor,
                ORDER_BTN_TXT,
                anchor: TextAnchor.MiddleCenter);

            // World-space Rect for click detection (y grows downward in pixel space
            // but upward in world space, so bottom = btnWorld.y - btnWorldSize.y).
            return new Rect(
                btnWorld.x,
                btnWorld.y - btnWorldSize.y,
                btnWorldSize.x,
                btnWorldSize.y);
        }
    }
}
