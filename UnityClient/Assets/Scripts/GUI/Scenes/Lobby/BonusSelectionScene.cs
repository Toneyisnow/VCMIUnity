using System;
using System.Collections.Generic;
using UnityEngine;

using H3Engine.DataAccess;
using H3Engine.Common;
using H3Engine.Campaign;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Mapping;
using UnityEngine.SceneManagement;
using UnityClient.Components.Data;
using UnityClient.GUI.GameControls;
using UnityClient.GUI.Rendering;

namespace UnityClient.GUI.Scenes.Lobby
{
    /// <summary>
    /// Bonus selection scene corresponding to CBonusSelection in VCMI (client/lobby/CBonusSelection.cpp).
    ///
    /// VCMI coordinate system: 800x600 pixels, top-left origin.
    /// All pixel positions from CBonusSelection constructor.
    ///
    /// Layout:
    ///   Left (0-455):   Campaign region map background ({prefix}_BG.BMP) - TODO
    ///   Right (456+):   CAMPBRF.BMP at (456, 6)
    ///
    /// Right panel:
    ///   (481, 28)   campaignName       FONT_BIG YELLOW
    ///   (481, 63)   "Description:"     FONT_SMALL YELLOW
    ///   (480, 86)   campaignDescription 286x117
    ///   (481, 219)  mapName            FONT_BIG YELLOW
    ///   (481, 253)  "Map Description:" FONT_SMALL YELLOW
    ///   (480, 276)  mapDescription     286x112
    ///   (475, 432)  "Choose a bonus:"  FONT_SMALL WHITE
    ///   (475+i*68, 455) bonus buttons  campaignBonusSelection.DEF
    ///   (724, 432)  "Difficulty"        FONT_MEDIUM WHITE
    ///   (709, 455)  difficulty icons   GSPBUT3-7.DEF
    ///   (694, 508)  difficultyLeft     SCNRBLF.DEF
    ///   (738, 508)  difficultyRight    SCNRBRT.DEF
    ///   (475, 536)  buttonStart        CBBEGIB.DEF
    ///   (624, 536)  buttonBack         CBCANCB.DEF
    ///   (705, 214)  buttonVideo        CBVIDEB.DEF
    /// </summary>
    public class BonusSelectionScene : MonoBehaviour
    {
        private const float SCREEN_W = 800f;
        private const float SCREEN_H = 600f;
        private const float PPU = 100f;

        private float scale = 1f;

        private H3DataAccess dataAccess = null;
        private H3Campaign campaign = null;
        private int selectedScenarioIndex = 0;
        private int selectedBonusIndex = -1;
        private int selectedDifficulty = 1; // 0-4, default Normal

        // Bonus button highlight tracking
        private List<GameObject> bonusButtons = new List<GameObject>();
        private List<SpriteRenderer> bonusHighlights = new List<SpriteRenderer>();

        // Difficulty icon tracking
        private List<SpriteRenderer> difficultyIcons = new List<SpriteRenderer>();

        void Start()
        {
            Camera cam = Camera.main;
            float viewHeight = cam.orthographicSize * 2f;
            scale = viewHeight / (SCREEN_H / PPU);
            Debug.Log(string.Format("[BonusSelection] Camera ortho={0}, scale={1}", cam.orthographicSize, scale));

            ECampaignVersion campaignVersion = CrossSceneData.SelectedCampaignSet;
            int campaignIndex = CrossSceneData.SelectedCampaignIndex;

            string campaignFileName = CrossSceneData.GetCampaignFileName(campaignVersion, campaignIndex);
            if (string.IsNullOrEmpty(campaignFileName))
            {
                Debug.LogError("[BonusSelection] No campaign file: version=" + campaignVersion + " index=" + campaignIndex);
                return;
            }

            dataAccess = H3DataAccess.GetInstance();
            campaign = dataAccess.RetrieveCampaign(campaignFileName);

            if (campaign == null)
            {
                Debug.LogError("[BonusSelection] Failed to load campaign: " + campaignFileName);
                return;
            }

            Debug.Log(string.Format("[BonusSelection] Campaign: {0}, Scenarios: {1}",
                campaign.Header.Name, campaign.Scenarios.Count));

            selectedScenarioIndex = 0;

            BuildUI();
        }

        #region Coordinate Conversion

        private Vector3 PixelToWorld(float px, float py, float z = 0f)
        {
            float halfW = SCREEN_W / PPU * scale / 2f;
            float halfH = SCREEN_H / PPU * scale / 2f;
            return new Vector3(px / PPU * scale - halfW, halfH - py / PPU * scale, z);
        }

        #endregion

        #region UI Construction

        private void BuildUI()
        {
            // ===== LEFT SIDE: Campaign region background =====
            // VCMI: setBackground(getCampaign()->getRegions().getBackgroundName())
            // Background is {prefix}_BG.BMP (e.g. KR_BG.BMP for Festival of Life)
            // TODO: determine prefix from campaign data dynamically
            CreateImage("ab_bg.bmp", 0, 0, 0.5f, "RegionBackground", new Vector2(0, 1));

            // ===== RIGHT PANEL =====
            // Line 79: panelBackground CAMPBRF.BMP at (456, 6)
            CreateImage("CAMPBRF.BMP", 456, 6, 0.1f, "PanelBackground", new Vector2(0, 1));

            // --- Campaign Info Section ---

            // Line 95: campaignName at (481, 28)
            CreateLabel("CampaignName", 481, 28, campaign.Header.Name ?? "Campaign", 24, UnityEngine.Color.yellow);

            // Line 99: "Campaign Description:" at (481, 63)
            CreateLabel("LabelCampaignDesc", 481, 63, "Campaign Description:", 14, UnityEngine.Color.yellow);

            // Line 100: campaignDescription at (480, 86), 286x117
            CreateLabel("CampaignDesc", 480, 86, campaign.Header.Description ?? "", 12, UnityEngine.Color.white);

            // --- Map Info Section ---

            // Line 104: mapName at (481, 219)
            CreateLabel("MapName", 481, 219, GetCurrentScenarioName(), 20, UnityEngine.Color.yellow);

            // Line 105: "Map Description:" at (481, 253)
            CreateLabel("LabelMapDesc", 481, 253, "Map Description:", 14, UnityEngine.Color.yellow);

            // Line 106: mapDescription at (480, 276), 286x112
            CreateLabel("MapDesc", 480, 276, GetCurrentScenarioRegionText(), 12, UnityEngine.Color.white);

            // --- Bonus Selection Section ---
            BuildBonusSelection();

            // --- Difficulty Section ---
            BuildDifficultySelection();

            // --- Buttons ---

            // Line 88: buttonStart CBBEGIB.DEF at (475, 536)
            CreateButton("CBBEGIB.def", 475, 536, "ButtonStart", OnStartClicked);

            // Line 93: buttonBack CBCANCB.DEF at (624, 536)
            CreateButton("CBCANCB.def", 624, 536, "ButtonBack", OnBackClicked);

            // Line 92: buttonVideo CBVIDEB.DEF at (705, 214) - placeholder
            CreateButton("CBVIDEB.def", 705, 214, "ButtonVideo", OnVideoClicked);
        }

        /// <summary>
        /// Build bonus selection buttons.
        /// VCMI: Toggle buttons at (475 + i*68, 455) using campaignBonusSelection DEF.
        /// Each button shows an overlay icon for the bonus type.
        /// Simplified: show text buttons describing each bonus.
        /// </summary>
        private void BuildBonusSelection()
        {
            // Line 108: "Choose a bonus:" at (475, 432)
            CreateLabel("LabelChooseBonus", 475, 432, "Choose a bonus:", 12, UnityEngine.Color.white);

            CampaignScenario scenario = (selectedScenarioIndex < campaign.Scenarios.Count)
                ? campaign.Scenarios[selectedScenarioIndex]
                : null;

            if (scenario == null || scenario.TravelOptions == null || scenario.TravelOptions.BonusChoices.Count == 0)
            {
                CreateLabel("NoBonuses", 475, 455, "(No bonuses available)", 12, new UnityEngine.Color(0.7f, 0.7f, 0.7f));
                return;
            }

            var bonuses = scenario.TravelOptions.BonusChoices;
            for (int i = 0; i < bonuses.Count; i++)
            {
                float px = 475 + i * 68;
                float py = 455;

                // Create a clickable bonus slot
                string bonusText = GetBonusDescription(bonuses[i]);
                int bonusIndex = i;

                GameObject slotObj = new GameObject("BonusSlot_" + i);
                slotObj.transform.parent = transform;
                slotObj.transform.position = PixelToWorld(px + 29, py + 29, -1f); // center of 58x58 slot
                slotObj.transform.localScale = new Vector3(scale, scale, 1);

                // Background rectangle for the slot
                SpriteRenderer bgRenderer = slotObj.AddComponent<SpriteRenderer>();
                bgRenderer.sprite = CreateColoredSprite(58, 58, new UnityEngine.Color(0.3f, 0.3f, 0.3f, 0.8f));
                bgRenderer.sortingOrder = 1;

                // Highlight overlay (shown when selected)
                GameObject highlightObj = new GameObject("BonusHighlight_" + i);
                highlightObj.transform.parent = slotObj.transform;
                highlightObj.transform.localPosition = Vector3.zero;
                SpriteRenderer hlRenderer = highlightObj.AddComponent<SpriteRenderer>();
                hlRenderer.sprite = CreateColoredSprite(58, 58, new UnityEngine.Color(1f, 1f, 0f, 0.3f));
                hlRenderer.sortingOrder = 2;
                hlRenderer.enabled = false;
                bonusHighlights.Add(hlRenderer);

                // Clickable collider
                BoxCollider2D collider = slotObj.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(58f / PPU, 58f / PPU);

                BonusSlotClickHandler handler = slotObj.AddComponent<BonusSlotClickHandler>();
                handler.Initialize(bonusIndex, OnBonusSelected);

                bonusButtons.Add(slotObj);

                // Bonus type label below the slot
                CreateLabel("BonusLabel_" + i, px, py + 62, bonusText, 10, UnityEngine.Color.white);
            }

            // Auto-select first bonus if available
            if (bonuses.Count > 0)
            {
                OnBonusSelected(0);
            }
        }

        /// <summary>
        /// Build difficulty selection.
        /// VCMI: 5 difficulty icons GSPBUT3-7.DEF at (709, 455), with left/right arrows.
        /// Simplified: show 5 colored rectangles with labels, left/right buttons.
        /// </summary>
        private void BuildDifficultySelection()
        {
            // Line 116: "Difficulty" label at (724, 432), TOPCENTER
            CreateLabel("LabelDifficulty", 695, 432, "Difficulty", 12, UnityEngine.Color.white);

            string[] diffNames = { "Easy", "Normal", "Hard", "Expert", "Impossible" };
            UnityEngine.Color[] diffColors = {
                new UnityEngine.Color(0.2f, 0.8f, 0.2f), // Easy - green
                new UnityEngine.Color(0.3f, 0.5f, 0.8f), // Normal - blue
                new UnityEngine.Color(0.8f, 0.8f, 0.2f), // Hard - yellow
                new UnityEngine.Color(0.8f, 0.4f, 0.1f), // Expert - orange
                new UnityEngine.Color(0.8f, 0.1f, 0.1f), // Impossible - red
            };

            // Difficulty icons at (709, 455), each ~30px wide
            for (int i = 0; i < 5; i++)
            {
                float px = 695 + i * 18;
                float py = 455;
                int diffIndex = i;

                GameObject iconObj = new GameObject("DiffIcon_" + i);
                iconObj.transform.parent = transform;
                iconObj.transform.position = PixelToWorld(px + 8, py + 12, -1f);
                iconObj.transform.localScale = new Vector3(scale, scale, 1);

                SpriteRenderer iconRenderer = iconObj.AddComponent<SpriteRenderer>();
                UnityEngine.Color c = diffColors[i];
                c.a = (i == selectedDifficulty) ? 1.0f : 0.3f;
                iconRenderer.sprite = CreateColoredSprite(16, 24, c);
                iconRenderer.sortingOrder = 2;
                difficultyIcons.Add(iconRenderer);

                BoxCollider2D col = iconObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(16f / PPU, 24f / PPU);

                DifficultyClickHandler dh = iconObj.AddComponent<DifficultyClickHandler>();
                dh.Initialize(diffIndex, OnDifficultySelected);
            }

            // Difficulty name below icons
            CreateLabel("DiffName", 695, 480, diffNames[selectedDifficulty], 10, UnityEngine.Color.white);

            // Line 129-130: left/right buttons at (694, 508) / (738, 508)
            CreateButton("SCNRBLF.def", 694, 508, "DiffLeft", OnDifficultyLeft);
            CreateButton("SCNRBRT.def", 738, 508, "DiffRight", OnDifficultyRight);
        }

        #endregion

        #region Data Helpers

        private string GetCurrentScenarioName()
        {
            if (selectedScenarioIndex < campaign.Scenarios.Count)
                return campaign.Scenarios[selectedScenarioIndex].MapName ?? "Scenario";
            return "Scenario";
        }

        private string GetCurrentScenarioRegionText()
        {
            if (selectedScenarioIndex < campaign.Scenarios.Count)
            {
                string text = campaign.Scenarios[selectedScenarioIndex].RegionText;
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
            return "(No description available)";
        }

        private string GetBonusDescription(ScenarioTravelBonus bonus)
        {
            switch (bonus.Type)
            {
                case ScenarioTravelBonus.EBonusType.SPELL:
                    return "Spell";
                case ScenarioTravelBonus.EBonusType.MONSTER:
                    return string.Format("x{0}", bonus.Info3);
                case ScenarioTravelBonus.EBonusType.BUILDING:
                    return "Building";
                case ScenarioTravelBonus.EBonusType.ARTIFACT:
                    return "Artifact";
                case ScenarioTravelBonus.EBonusType.SPELL_SCROLL:
                    return "Scroll";
                case ScenarioTravelBonus.EBonusType.PRIMARY_SKILL:
                    return "Primary";
                case ScenarioTravelBonus.EBonusType.SECONDARY_SKILL:
                    return "SecSkill";
                case ScenarioTravelBonus.EBonusType.RESOURCE:
                    return string.Format("Res x{0}", bonus.Info2);
                case ScenarioTravelBonus.EBonusType.HEROES_FROM_PREVIOUS_SCENARIO:
                    return "Heroes";
                case ScenarioTravelBonus.EBonusType.HERO:
                    return "Hero";
                default:
                    return "Bonus";
            }
        }

        #endregion

        #region UI Factory Methods

        private void CreateImage(string imageFile, float px, float py, float z, string name, Vector2 pivot)
        {
            ImageData imageData = dataAccess.RetrieveImage(imageFile);
            if (imageData == null)
            {
                Debug.LogWarning("[BonusSelection] Image not found: " + imageFile);
                return;
            }

            GameObject go = new GameObject(name);
            go.transform.parent = transform;
            go.transform.position = PixelToWorld(px, py, z);
            go.transform.localScale = new Vector3(scale, scale, 1);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Texture2DExtension.CreateSpriteFromImageData(imageData, pivot);
            renderer.sortingOrder = (z > 0.1f) ? -1 : 0;
        }

        private void CreateButton(string defFileName, float px, float py, string name, Action callback)
        {
            BundleImageDefinition bundleDef = dataAccess.RetrieveBundleImage(defFileName);
            if (bundleDef == null)
            {
                Debug.LogWarning("[BonusSelection] DEF not found: " + defFileName);
                return;
            }

            ImageData firstFrame = bundleDef.GetImageData(0, 0);
            float halfW = (firstFrame != null) ? firstFrame.Width / 2f : 0;
            float halfH = (firstFrame != null) ? firstFrame.Height / 2f : 0;

            GameObject btnObj = new GameObject(name);
            btnObj.transform.parent = transform;
            btnObj.transform.position = PixelToWorld(px + halfW, py + halfH, -1f);
            btnObj.transform.localScale = new Vector3(scale, scale, 1);

            SpriteRenderer renderer = btnObj.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 2;

            GameMenuItem menuItem = btnObj.AddComponent<GameMenuItem>();
            menuItem.Initialize(bundleDef, callback);
        }

        private void CreateLabel(string name, float px, float py, string text, int fontSize, UnityEngine.Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = transform;
            go.transform.position = PixelToWorld(px, py, -0.5f);

            TextMesh textMesh = go.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.characterSize = 0.1f * scale;
            textMesh.color = color;
            textMesh.anchor = TextAnchor.UpperLeft;
            textMesh.alignment = TextAlignment.Left;

            textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            go.GetComponent<MeshRenderer>().sortingOrder = 3;
        }

        /// <summary>
        /// Create a simple colored rectangle sprite at runtime.
        /// </summary>
        private Sprite CreateColoredSprite(int width, int height, UnityEngine.Color color)
        {
            Texture2D tex = new Texture2D(width, height);
            tex.filterMode = FilterMode.Point;
            UnityEngine.Color[] pixels = new UnityEngine.Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), PPU);
        }

        #endregion

        #region Selection Handlers

        private void OnBonusSelected(int index)
        {
            selectedBonusIndex = index;
            for (int i = 0; i < bonusHighlights.Count; i++)
            {
                bonusHighlights[i].enabled = (i == index);
            }
            Debug.Log("[BonusSelection] Bonus selected: " + index);
        }

        private void OnDifficultySelected(int index)
        {
            selectedDifficulty = Mathf.Clamp(index, 0, 4);
            UpdateDifficultyDisplay();
        }

        private void OnDifficultyLeft()
        {
            if (selectedDifficulty > 0)
            {
                selectedDifficulty--;
                UpdateDifficultyDisplay();
            }
        }

        private void OnDifficultyRight()
        {
            if (selectedDifficulty < 4)
            {
                selectedDifficulty++;
                UpdateDifficultyDisplay();
            }
        }

        private void UpdateDifficultyDisplay()
        {
            string[] diffNames = { "Easy", "Normal", "Hard", "Expert", "Impossible" };
            for (int i = 0; i < difficultyIcons.Count; i++)
            {
                UnityEngine.Color c = difficultyIcons[i].sprite.texture.GetPixel(0, 0);
                c.a = (i == selectedDifficulty) ? 1.0f : 0.3f;
                difficultyIcons[i].sprite = CreateColoredSprite(16, 24, c);
            }

            // Update difficulty name label
            GameObject diffLabel = GameObject.Find("DiffName");
            if (diffLabel != null)
            {
                TextMesh tm = diffLabel.GetComponent<TextMesh>();
                if (tm != null) tm.text = diffNames[selectedDifficulty];
            }

            Debug.Log("[BonusSelection] Difficulty: " + diffNames[selectedDifficulty]);
        }

        #endregion

        #region Button Handlers

        private void OnStartClicked()
        {
            if (campaign == null || selectedScenarioIndex < 0 || selectedScenarioIndex >= campaign.Scenarios.Count)
            {
                Debug.LogError("[BonusSelection] Invalid scenario index: " + selectedScenarioIndex);
                return;
            }

            CrossSceneData.CurrentCampaign = campaign;
            CrossSceneData.SelectedScenarioIndex = selectedScenarioIndex;

            Debug.Log(string.Format("[BonusSelection] Starting scenario {0}: {1}, bonus={2}, difficulty={3}",
                selectedScenarioIndex, campaign.Scenarios[selectedScenarioIndex].MapName,
                selectedBonusIndex, selectedDifficulty));

            SceneManager.LoadScene("GameMapScene", LoadSceneMode.Single);
        }

        private void OnBackClicked()
        {
            SceneManager.LoadScene("CampaignSelectScene", LoadSceneMode.Single);
        }

        private void OnVideoClicked()
        {
            Debug.Log("[BonusSelection] Video button clicked (not implemented)");
        }

        #endregion
    }

    /// <summary>
    /// Click handler for bonus selection slots.
    /// </summary>
    public class BonusSlotClickHandler : MonoBehaviour
    {
        private int bonusIndex;
        private Action<int> callback;

        public void Initialize(int index, Action<int> callback)
        {
            this.bonusIndex = index;
            this.callback = callback;
        }

        private void OnMouseDown()
        {
            callback?.Invoke(bonusIndex);
        }
    }

    /// <summary>
    /// Click handler for difficulty icons.
    /// </summary>
    public class DifficultyClickHandler : MonoBehaviour
    {
        private int diffIndex;
        private Action<int> callback;

        public void Initialize(int index, Action<int> callback)
        {
            this.diffIndex = index;
            this.callback = callback;
        }

        private void OnMouseDown()
        {
            callback?.Invoke(diffIndex);
        }
    }
}
