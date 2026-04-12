using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;

using H3Engine.DataAccess;
using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.Campaign;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Mapping;
using UnityEngine.SceneManagement;
using UnityClient.Components.Data;
using UnityClient.GUI.Widgets;
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
    ///   (475+i*68, 455) bonus buttons  campaignBonusSelection.DEF + overlay icons
    ///   (724, 432)  "Difficulty"        FONT_MEDIUM WHITE
    ///   (709, 455)  difficulty icons   GSPBUT3-7.DEF (stacked, only selected visible)
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

        // Bonus button tracking
        private List<GameObject> bonusButtons = new List<GameObject>();
        private List<SpriteRenderer> bonusBaseRenderers = new List<SpriteRenderer>();
        private Sprite bonusUnselectedSprite = null;
        private Sprite bonusSelectedSprite = null;

        // Difficulty icon tracking: 5 sprites from GSPBUT3-7.DEF, single renderer
        private Sprite[] difficultySprites = new Sprite[5];
        private SpriteRenderer difficultyIconRenderer = null;


        // Loading overlay state
        private bool isLoading = false;
        private LoadProgress loadProgress = null;
        private H3Map loadedMap = null;
        private Exception loadException = null;
        private GameObject loadingOverlay = null;
        private TextMesh loadingStatusText = null;

        // Progress bar blocks (from loadprog.def, matching VCMI's CLoadingScreen)
        private const int PROGRESS_BLOCK_COUNT = 20;
        private const float PROGRESS_BLOCK_SIZE = 18f; // pixels
        private const float PROGRESS_BAR_X = 395f;     // pixels from left
        private const float PROGRESS_BAR_Y = 548f;     // pixels from top
        private List<GameObject> progressBlocks = new List<GameObject>();
        private int lastVisibleBlocks = 0;

        /// <summary>
        /// DEF name to overlay frame mapping per bonus type.
        /// Matches VCMI CBonusSelection::createBonusesIcons() bonusPics array.
        /// </summary>
        private static readonly string[] BonusOverlayDefs =
        {
            "SPELLBON.DEF",   // 0: SPELL
            "TWCRPORT.DEF",   // 1: MONSTER
            "",               // 2: BUILDING (uses BO*.PCX, handled specially)
            "ARTIFBON.DEF",   // 3: ARTIFACT
            "SPELLBON.DEF",   // 4: SPELL_SCROLL
            "PSKILBON.DEF",   // 5: PRIMARY_SKILL
            "SSKILBON.DEF",   // 6: SECONDARY_SKILL
            "BORES.DEF",      // 7: RESOURCE
            "",               // 8: HEROES_FROM_PREVIOUS_SCENARIO (portrait)
            "",               // 9: HERO (portrait or CBONN1A3.BMP)
        };

        /// <summary>
        /// Building ID (EBuildingId) to campaign bonus PCX image name mapping.
        /// Uses Castle faction images as default. Faction-specific images follow the
        /// pattern Bo[FactionCode][BuildingName].pcx (e.g., BoCsCas1 for Castle's Fort).
        /// Source: VCMI config/factions/castle.json "structures" → "campaignBonus" fields.
        /// </summary>
        private static readonly Dictionary<int, string> BuildingBonusImages = new Dictionary<int, string>
        {
            // Mage Guilds (0-4)
            { 0, "BoCsMag1.pcx" },  // MAGES_GUILD_1
            { 1, "BoCsMag2.pcx" },  // MAGES_GUILD_2
            { 2, "BoCsMag3.pcx" },  // MAGES_GUILD_3
            { 3, "BoCsMag4.pcx" },  // MAGES_GUILD_4
            // Castle has no Mage Guild 5
            // Common buildings (5-16)
            { 5, "BoCsTav1.pcx" },  // TAVERN
            { 6, "BoCsDock.pcx" },  // SHIPYARD
            { 7, "BoCsCas1.pcx" },  // FORT
            { 8, "BoCsCas2.pcx" },  // CITADEL
            { 9, "BoCsCas3.pcx" },  // CASTLE
            { 10, "BoCsHal1.pcx" }, // VILLAGE_HALL
            { 11, "BoCsHal2.pcx" }, // TOWN_HALL
            { 12, "BoCsHal3.pcx" }, // CITY_HALL
            { 13, "BoCsHal4.pcx" }, // CAPITOL
            { 14, "BoCsMrk1.pcx" }, // MARKETPLACE
            { 15, "BoCsMrk2.pcx" }, // RESOURCE_SILO
            { 16, "BoCsBlak.pcx" }, // BLACKSMITH
            // Special buildings (17-26)
            { 17, "BoCsLite.pcx" }, // SPECIAL_1 (Lighthouse)
            { 18, "BoCsGr1H.pcx" }, // HORDE_1
            { 19, "BoCsGr2H.pcx" }, // HORDE_1_UPGR
            { 21, "BoCsCv2S.pcx" }, // SPECIAL_2 (Stables)
            { 22, "BoCsTav2.pcx" }, // SPECIAL_3 (Brotherhood)
            { 26, "BoCsHoly.pcx" }, // GRAIL
            // Dwellings (30-36)
            { 30, "BoCsPik1.pcx" }, // DWELL_LVL_1 (Pikeman)
            { 31, "BoCsCrs1.pcx" }, // DWELL_LVL_2 (Archer)
            { 32, "BoCsGr1.pcx" },  // DWELL_LVL_3 (Griffin)
            { 33, "BoCsSwd1.pcx" }, // DWELL_LVL_4 (Swordsman)
            { 34, "BoCsMon1.pcx" }, // DWELL_LVL_5 (Monk)
            { 35, "BoCsCv1.pcx" },  // DWELL_LVL_6 (Cavalier)
            { 36, "BoCsAng1.pcx" }, // DWELL_LVL_7 (Angel)
            // Upgraded dwellings (37-43)
            { 37, "BoCsPik2.pcx" }, // DWELL_LVL_1_UP
            { 38, "BoCsCrs2.pcx" }, // DWELL_LVL_2_UP
            { 39, "BoCsGr2.pcx" },  // DWELL_LVL_3_UP
            { 40, "BoCsSwd2.pcx" }, // DWELL_LVL_4_UP
            { 41, "BoCsMon2.pcx" }, // DWELL_LVL_5_UP
            { 42, "BoCsCv2.pcx" },  // DWELL_LVL_6_UP
            { 43, "BoCsAng2.pcx" }, // DWELL_LVL_7_UP
        };

        void Start()
        {
            // Use scale=1 so H3 pixel coords map directly to world units at PPU=100,
            // consistent with MainMenuScene's native sprite rendering.
            scale = 1.0f;
            Debug.Log(string.Format("[BonusSelection] scale={0}", scale));

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
        /// Build bonus selection buttons using campaignBonusSelection DEF as toggle base
        /// with overlay icons from the appropriate DEF files (SPELLBON, TWCRPORT, etc.).
        /// VCMI reference: CBonusSelection::createBonusesIcons() in CBonusSelection.cpp.
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

            // Load campaignBonusSelection DEF for toggle button base
            // Group 0 = unselected frames, Group 1 = selected frames
            BundleImageDefinition toggleDef = dataAccess.RetrieveBundleImage("campaignBonusSelection.def");
            if (toggleDef != null)
            {
                ImageData unselImg = toggleDef.GetImageData(0, 0);
                bonusUnselectedSprite = Texture2DExtension.CreateSpriteFromImageData(unselImg, new Vector2(0.5f, 0.5f));

                ImageData selImg = toggleDef.GetImageData(1, 0);
                if (selImg != null)
                    bonusSelectedSprite = Texture2DExtension.CreateSpriteFromImageData(selImg, new Vector2(0.5f, 0.5f));
                else
                    bonusSelectedSprite = bonusUnselectedSprite;
            }

            var bonuses = scenario.TravelOptions.BonusChoices;
            for (int i = 0; i < bonuses.Count; i++)
            {
                float px = 475 + i * 68;
                float py = 455;
                int bonusIndex = i;

                // Get toggle button dimensions for positioning
                float btnW = 58f;
                float btnH = 64f;
                if (bonusUnselectedSprite != null)
                {
                    btnW = bonusUnselectedSprite.texture.width;
                    btnH = bonusUnselectedSprite.texture.height;
                }

                // Create the slot object centered on the button area
                GameObject slotObj = new GameObject("BonusSlot_" + i);
                slotObj.transform.parent = transform;
                slotObj.transform.position = PixelToWorld(px + btnW / 2f, py + btnH / 2f, -1f);
                slotObj.transform.localScale = new Vector3(scale, scale, 1);

                // Base toggle button image from campaignBonusSelection DEF
                SpriteRenderer baseRenderer = slotObj.AddComponent<SpriteRenderer>();
                baseRenderer.sortingOrder = 1;
                if (bonusUnselectedSprite != null)
                {
                    baseRenderer.sprite = bonusUnselectedSprite;
                }
                else
                {
                    // Fallback: colored rectangle if DEF not found
                    baseRenderer.sprite = CreateColoredSprite(58, 64, new UnityEngine.Color(0.3f, 0.3f, 0.3f, 0.8f));
                }
                bonusBaseRenderers.Add(baseRenderer);

                // Overlay icon based on bonus type
                CreateBonusOverlay(slotObj, bonuses[i], i);

                // Clickable collider
                BoxCollider2D collider = slotObj.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(btnW / PPU, btnH / PPU);

                BonusSlotClickHandler handler = slotObj.AddComponent<BonusSlotClickHandler>();
                handler.Initialize(bonusIndex, OnBonusSelected);

                bonusButtons.Add(slotObj);

                // Bonus description label below the button (VCMI icon size is 58x64)
                string bonusText = GetBonusDescription(bonuses[i]);
                CreateLabel("BonusLabel_" + i, px, py + btnH + 2, bonusText, 10, UnityEngine.Color.white);
            }

            // Auto-select first bonus if available
            if (bonuses.Count > 0)
            {
                OnBonusSelected(0);
            }
        }

        /// <summary>
        /// Create an overlay icon on top of a bonus toggle button.
        /// VCMI: bonusButton->setOverlay(CAnimImage(picName, picNumber)) or CPicture(picName).
        /// </summary>
        private void CreateBonusOverlay(GameObject parentSlot, ScenarioTravelBonus bonus, int index)
        {
            string overlayDefName;
            int overlayFrame;
            string overlayBmpName;
            GetBonusOverlayInfo(bonus, out overlayDefName, out overlayFrame, out overlayBmpName);

            Sprite overlaySprite = null;

            if (!string.IsNullOrEmpty(overlayDefName) && overlayFrame >= 0)
            {
                // Load from DEF file with specific frame
                BundleImageDefinition overlayDef = dataAccess.RetrieveBundleImage(overlayDefName);
                if (overlayDef != null)
                {
                    ImageData overlayImg = overlayDef.GetImageData(0, overlayFrame);
                    if (overlayImg != null)
                    {
                        overlaySprite = Texture2DExtension.CreateSpriteFromImageData(overlayImg, new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("[BonusSelection] Overlay frame {0}:{1} not found", overlayDefName, overlayFrame));
                    }
                }
                else
                {
                    Debug.LogWarning("[BonusSelection] Overlay DEF not found: " + overlayDefName);
                }
            }
            else if (!string.IsNullOrEmpty(overlayBmpName))
            {
                // Load from BMP image file
                ImageData bmpImg = dataAccess.RetrieveImage(overlayBmpName);
                if (bmpImg != null)
                {
                    overlaySprite = Texture2DExtension.CreateSpriteFromImageData(bmpImg, new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogWarning("[BonusSelection] Overlay BMP not found: " + overlayBmpName);
                }
            }

            if (overlaySprite != null)
            {
                GameObject overlayObj = new GameObject("BonusOverlay_" + index);
                overlayObj.transform.parent = parentSlot.transform;
                overlayObj.transform.localPosition = Vector3.zero;

                SpriteRenderer overlayRenderer = overlayObj.AddComponent<SpriteRenderer>();
                overlayRenderer.sprite = overlaySprite;
                overlayRenderer.sortingOrder = 2;
            }
        }

        /// <summary>
        /// Determine overlay DEF name and frame number for a bonus type.
        /// Matches VCMI CBonusSelection::createBonusesIcons() logic.
        ///
        /// Returns either (defName + frame) for DEF-based overlays,
        /// or (bmpName) for single-image overlays.
        /// </summary>
        private void GetBonusOverlayInfo(ScenarioTravelBonus bonus, out string defName, out int frame, out string bmpName)
        {
            defName = null;
            frame = -1;
            bmpName = null;

            int typeIndex = (int)bonus.Type;
            if (typeIndex < 0 || typeIndex >= BonusOverlayDefs.Length)
                return;

            switch (bonus.Type)
            {
                case ScenarioTravelBonus.EBonusType.SPELL:
                    // SPELLBON.DEF, frame = spell ID (Info2)
                    defName = BonusOverlayDefs[typeIndex];
                    frame = bonus.Info2;
                    break;

                case ScenarioTravelBonus.EBonusType.MONSTER:
                    // TWCRPORT.DEF, frame = creature type (Info2) + 2
                    defName = BonusOverlayDefs[typeIndex];
                    frame = bonus.Info2 + 2;
                    break;

                case ScenarioTravelBonus.EBonusType.BUILDING:
                    // Building uses faction-specific PCX files. Default to Castle faction images.
                    // Info1 = building ID matching EBuildingId enum.
                    if (BuildingBonusImages.ContainsKey(bonus.Info1))
                    {
                        bmpName = BuildingBonusImages[bonus.Info1];
                    }
                    else
                    {
                        Debug.LogWarning("[BonusSelection] No building bonus image for building ID: " + bonus.Info1);
                    }
                    break;

                case ScenarioTravelBonus.EBonusType.ARTIFACT:
                    // ARTIFBON.DEF, frame = artifact ID (Info2)
                    defName = BonusOverlayDefs[typeIndex];
                    frame = bonus.Info2;
                    break;

                case ScenarioTravelBonus.EBonusType.SPELL_SCROLL:
                    // SPELLBON.DEF, frame = spell ID (Info2)
                    defName = BonusOverlayDefs[typeIndex];
                    frame = bonus.Info2;
                    break;

                case ScenarioTravelBonus.EBonusType.PRIMARY_SKILL:
                    // PSKILBON.DEF, frame = index of the leading (highest) skill
                    // Info2 is packed UInt32: 4 bytes for Attack, Defense, SpellPower, Knowledge
                    defName = BonusOverlayDefs[typeIndex];
                    frame = GetLeadingPrimarySkill(bonus.Info2);
                    break;

                case ScenarioTravelBonus.EBonusType.SECONDARY_SKILL:
                    // SSKILBON.DEF, frame = skill ID * 3 + mastery level - 1
                    defName = BonusOverlayDefs[typeIndex];
                    frame = bonus.Info2 * 3 + bonus.Info3 - 1;
                    break;

                case ScenarioTravelBonus.EBonusType.RESOURCE:
                    // BORES.DEF, frame = resource type
                    // Special: 0xFD (253) = wood+ore -> frame 7; 0xFE (254) = rare resources -> frame 8
                    defName = BonusOverlayDefs[typeIndex];
                    if (bonus.Info1 == 0xFD)
                        frame = 7;
                    else if (bonus.Info1 == 0xFE)
                        frame = 8;
                    else
                        frame = bonus.Info1;
                    break;

                case ScenarioTravelBonus.EBonusType.HEROES_FROM_PREVIOUS_SCENARIO:
                    // Portrait: would need hero lookup from previous scenario
                    // Skip overlay for now
                    break;

                case ScenarioTravelBonus.EBonusType.HERO:
                    // Info2 = hero ID; 0xFFFF = random hero -> CBONN1A3.BMP
                    if (bonus.Info2 == 0xFFFF)
                    {
                        bmpName = "CBONN1A3.BMP";
                    }
                    // For specific heroes, would need portrait lookup
                    break;
            }
        }

        /// <summary>
        /// Get the index of the highest primary skill from a packed UInt32.
        /// Byte 0=Attack, 1=Defense, 2=SpellPower, 3=Knowledge.
        /// </summary>
        private int GetLeadingPrimarySkill(int packedSkills)
        {
            int leading = 0;
            int maxVal = 0;
            for (int i = 0; i < 4; i++)
            {
                int val = (packedSkills >> (i * 8)) & 0xFF;
                if (val > maxVal)
                {
                    maxVal = val;
                    leading = i;
                }
            }
            return leading;
        }

        /// <summary>
        /// Build difficulty selection using GSPBUT3-7.DEF icons.
        /// VCMI: 5 CAnimImage at (709, 455), stacked; only the selected one is enabled.
        /// Left/right arrows at (694, 508) / (738, 508) to cycle.
        /// </summary>
        private void BuildDifficultySelection()
        {
            // Line 116: "Difficulty" label at (724, 432), TOPCENTER
            CreateLabel("LabelDifficulty", 695, 432, "Difficulty", 12, UnityEngine.Color.white);

            // Load all 5 difficulty icon DEFs: GSPBUT3.DEF (Easy) through GSPBUT7.DEF (Impossible)
            bool allLoaded = true;
            for (int i = 0; i < 5; i++)
            {
                string defName = "GSPBUT" + (i + 3) + ".DEF";
                BundleImageDefinition def = dataAccess.RetrieveBundleImage(defName);
                if (def != null)
                {
                    ImageData img = def.GetImageData(0, 0);
                    if (img != null)
                    {
                        difficultySprites[i] = Texture2DExtension.CreateSpriteFromImageData(img, new Vector2(0, 1));
                    }
                    else
                    {
                        Debug.LogWarning("[BonusSelection] No frame in: " + defName);
                        allLoaded = false;
                    }
                }
                else
                {
                    Debug.LogWarning("[BonusSelection] Difficulty DEF not found: " + defName);
                    allLoaded = false;
                }
            }

            // Create single icon renderer at (709, 455)
            // All 5 DEF icons share the same position; only the selected one is visible.
            GameObject iconObj = new GameObject("DifficultyIcon");
            iconObj.transform.parent = transform;
            iconObj.transform.position = PixelToWorld(709, 455, -1f);
            iconObj.transform.localScale = new Vector3(scale, scale, 1);

            difficultyIconRenderer = iconObj.AddComponent<SpriteRenderer>();
            difficultyIconRenderer.sortingOrder = 2;

            if (allLoaded && difficultySprites[selectedDifficulty] != null)
            {
                difficultyIconRenderer.sprite = difficultySprites[selectedDifficulty];
            }
            else
            {
                // Fallback: colored rectangle
                difficultyIconRenderer.sprite = CreateColoredSprite(32, 32, UnityEngine.Color.gray);
            }

            // Make the icon clickable to cycle difficulty
            BoxCollider2D iconCol = iconObj.AddComponent<BoxCollider2D>();
            if (difficultyIconRenderer.sprite != null)
            {
                float w = difficultyIconRenderer.sprite.texture.width;
                float h = difficultyIconRenderer.sprite.texture.height;
                iconCol.size = new Vector2(w / PPU, h / PPU);
                iconCol.offset = new Vector2(w / PPU / 2f, -h / PPU / 2f);
            }

            DifficultyClickHandler dh = iconObj.AddComponent<DifficultyClickHandler>();
            dh.Initialize(-1, OnDifficultyIconClicked); // -1 = cycle forward on click

            // Difficulty name below icon
            string[] diffNames = { "Easy", "Normal", "Hard", "Expert", "Impossible" };
            CreateLabel("DiffName", 695, 490, diffNames[selectedDifficulty], 10, UnityEngine.Color.white);

            // Line 129-130: left/right buttons at (694, 508) / (738, 508)
            CreateButton("SCNRBLF.def", 694, 508, "DiffLeft", OnDifficultyLeft);
            CreateButton("SCNRBRT.def", 738, 508, "DiffRight", OnDifficultyRight);
        }

        #endregion

        #region Data Helpers

        private string GetCurrentScenarioName()
        {
            if (selectedScenarioIndex < campaign.Scenarios.Count)
            {
                string mapName = campaign.Scenarios[selectedScenarioIndex].MapName ?? "Scenario";
                // MapName stores the .h3m filename; strip the extension for display
                return System.IO.Path.GetFileNameWithoutExtension(mapName);
            }
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

            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/fangzheng_black_gb18030_yolan SDF");
            if (font != null)
                tmp.font = font;
            else
                Debug.LogWarning("[BonusSelection] Chinese font not found at Resources/Fonts/fangzheng_black_gb18030_yolan SDF");

            tmp.text = text;
            // TMP fontSize in world units at PPU=100
            tmp.fontSize = fontSize * 0.1f * scale;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            // Width = 286px right panel, height = 200px (large enough to never clip)
            // Pivot top-left so position matches PixelToWorld top-left origin
            tmp.rectTransform.pivot = new Vector2(0f, 1f);
            tmp.rectTransform.sizeDelta = new Vector2(286f / PPU * scale, 2000f / PPU * scale);
            tmp.overflowMode = TextOverflowModes.Truncate;

            go.GetComponent<MeshRenderer>().sortingOrder = 3;
        }

        /// <summary>
        /// Create a simple colored rectangle sprite at runtime (fallback when DEF not found).
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

            // Update toggle button sprites: selected uses group 1, unselected uses group 0
            for (int i = 0; i < bonusBaseRenderers.Count; i++)
            {
                if (bonusUnselectedSprite != null && bonusSelectedSprite != null)
                {
                    bonusBaseRenderers[i].sprite = (i == index) ? bonusSelectedSprite : bonusUnselectedSprite;
                }
                else
                {
                    // Fallback: change color alpha
                    bonusBaseRenderers[i].color = (i == index)
                        ? new UnityEngine.Color(1f, 1f, 0.5f, 1f)
                        : UnityEngine.Color.white;
                }
            }

            Debug.Log("[BonusSelection] Bonus selected: " + index);
        }

        private void OnDifficultyIconClicked(int unused)
        {
            // Cycle forward on direct click
            selectedDifficulty = (selectedDifficulty + 1) % 5;
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

            // Swap the difficulty icon sprite
            if (difficultyIconRenderer != null && difficultySprites[selectedDifficulty] != null)
            {
                difficultyIconRenderer.sprite = difficultySprites[selectedDifficulty];
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
            if (isLoading) return;

            if (campaign == null || selectedScenarioIndex < 0 || selectedScenarioIndex >= campaign.Scenarios.Count)
            {
                Debug.LogError("[BonusSelection] Invalid scenario index: " + selectedScenarioIndex);
                return;
            }

            Debug.Log(string.Format("[BonusSelection] Starting scenario {0}: {1}, bonus={2}, difficulty={3}",
                selectedScenarioIndex, campaign.Scenarios[selectedScenarioIndex].MapName,
                selectedBonusIndex, selectedDifficulty));

            // Show loading overlay and start background map loading
            isLoading = true;
            loadProgress = new LoadProgress();
            loadedMap = null;
            loadException = null;

            ShowLoadingOverlay();

            // Capture values for the background thread
            H3Campaign campaignRef = campaign;
            int scenarioIdx = selectedScenarioIndex;

            Thread loadThread = new Thread(() =>
            {
                try
                {
                    loadedMap = H3CampaignLoader.LoadScenarioMap(campaignRef, scenarioIdx, loadProgress);
                }
                catch (Exception ex)
                {
                    loadException = ex;
                }
            });
            loadThread.IsBackground = true;
            loadThread.Start();
        }

        void Update()
        {
            if (!isLoading) return;

            // Check for loading error
            if (loadException != null)
            {
                Debug.LogError("[BonusSelection] Map loading failed: " + loadException.Message);
                isLoading = false;
                DestroyLoadingOverlay();
                return;
            }

            // Update progress bar
            if (loadProgress != null)
            {
                float progress = loadProgress.Progress;
                UpdateProgressBar(progress);
                UpdateLoadingStatus(loadProgress.StatusMessage);
            }

            // Check if loading is complete
            if (loadedMap != null)
            {
                isLoading = false;

                CrossSceneData.CurrentCampaign = campaign;
                CrossSceneData.SelectedScenarioIndex = selectedScenarioIndex;
                CrossSceneData.LoadedMap = loadedMap;

                SceneManager.LoadScene("GameMapScene", LoadSceneMode.Single);
            }
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

        #region Loading Overlay

        private void ShowLoadingOverlay()
        {
            loadingOverlay = new GameObject("LoadingOverlay");
            // Do not parent to scene transform — PixelToWorld computes world-space positions.

            // Background: "loadbar" PCX from H3 game data (800x600 loading screen)
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
                // Fallback: solid dark background if loadbar.pcx not found
                Debug.LogWarning("[BonusSelection] loadbar.pcx not found, using dark background");
                GameObject bgObj = new GameObject("LoadingBG");
                bgObj.transform.parent = loadingOverlay.transform;
                bgObj.transform.position = new Vector3(0, 0, -5f);

                SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
                Texture2D bgTex = new Texture2D(1, 1);
                bgTex.SetPixel(0, 0, new UnityEngine.Color(0, 0, 0, 0.85f));
                bgTex.Apply();
                bgRenderer.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                bgRenderer.sortingOrder = 50;

                Camera cam = Camera.main;
                float viewHeight = cam.orthographicSize * 2f;
                float viewWidth = viewHeight * cam.aspect;
                bgObj.transform.localScale = new Vector3(viewWidth, viewHeight, 1);
            }

            // Progress blocks: "loadprog" DEF from H3 game data
            // VCMI: 20 blocks, each 18px wide, at (395, 548)
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

                    blockObj.SetActive(false); // Start hidden, reveal as progress advances
                    progressBlocks.Add(blockObj);
                }
            }
            else
            {
                Debug.LogWarning("[BonusSelection] loadprog.def not found, progress bar will not show");
            }

            // Status text (shows current loading phase)
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

            // Only update if changed
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
