using System;
using System.Collections.Generic;
using UnityEngine;

using H3Engine.DataAccess;
using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.FileSystem;
using UnityEngine.SceneManagement;
using UnityClient.GUI.Rendering;
using UnityClient.Components.Data;

namespace UnityClient.GUI.Scenes.Menu
{
    /// <summary>
    /// Campaign selection screen corresponding to CCampaignScreen in VCMI.
    /// Displays available campaigns for the selected campaign set (SOD/ROE/AB).
    /// Selecting a campaign navigates to BonusSelectionScene.
    ///
    /// The background image (campback.PCX) is 800x600 pixels.
    /// Campaign icon images are ~200x116 pixels each.
    /// Positions are specified in pixel coordinates relative to top-left of the background,
    /// then converted to world coordinates.
    /// </summary>
    public class CampaignSelectScene : MonoBehaviour
    {
        private ECampaignVersion campaignVersion;

        // Background dimensions in pixels (campback.PCX)
        private const float BG_WIDTH_PX = 800f;
        private const float BG_HEIGHT_PX = 600f;
        private const float BG_SCALE = 1.0f;
        private const float PPU = 100f;

        private List<GameObject> campaignIcons = new List<GameObject>();

        void Start()
        {
            this.campaignVersion = CrossSceneData.SelectedCampaignSet;

            H3DataAccess h3Engine = H3DataAccess.GetInstance();

            // Load and display background
            ImageData imageData = h3Engine.RetrieveImage("campback.PCX");
            GameObject background = GameObject.Find("Background");
            var bgRenderer = background.GetComponent<SpriteRenderer>();
            bgRenderer.sprite = Texture2DExtension.CreateSpriteFromImageData(imageData, new Vector2(0.5f, 0.5f));
            background.transform.position = new Vector3(0, 0, 0.5f);
            background.transform.localScale = new Vector3(BG_SCALE, BG_SCALE, 1);

            // Create campaign icons based on version
            // Pixel positions are approximate slot centers on campback.PCX (800x600)
            if (campaignVersion == ECampaignVersion.ROE)
            {
                CreateCampaignIcon(110, 148, "campgd1s.PCX", 1);  // Long Live the Queen
                CreateCampaignIcon(410, 148, "campev1s.PCX", 2);  // Dungeons and Devils
                CreateCampaignIcon(110, 308, "campgd2s.PCX", 3);  // Long Live the King
                CreateCampaignIcon(410, 308, "campneus.PCX", 4);  // Seeds of Discontent
                CreateCampaignIcon(110, 468, "campev2s.PCX", 5);  // Spoils of War
                CreateCampaignIcon(410, 468, "campgd3s.PCX", 6);  // Song for the Father
            }
            else if (campaignVersion == ECampaignVersion.AB)
            {
                CreateCampaignIcon(110, 148, "campgd1s.PCX", 1);
                CreateCampaignIcon(410, 148, "campev1s.PCX", 2);
                CreateCampaignIcon(110, 308, "campgd2s.PCX", 3);
                CreateCampaignIcon(410, 308, "campneus.PCX", 4);
                CreateCampaignIcon(110, 468, "campev2s.PCX", 5);
                CreateCampaignIcon(410, 468, "campgd3s.PCX", 6);
            }
            else if (campaignVersion == ECampaignVersion.SOD)
            {
                CreateCampaignIcon(260, 308, "campgd1s.PCX", 1);  // Centered for single campaign
            }
        }

        /// <summary>
        /// Convert pixel coordinates (relative to background top-left) to world position,
        /// accounting for background scale and center pivot.
        /// </summary>
        private Vector3 PixelToWorld(float pixelX, float pixelY)
        {
            // Background is centered at origin with pivot (0.5, 0.5)
            // Convert pixel coords to normalized [0,1] then to world coords
            float normalizedX = pixelX / BG_WIDTH_PX - 0.5f;
            float normalizedY = 0.5f - pixelY / BG_HEIGHT_PX;

            float worldX = normalizedX * (BG_WIDTH_PX / PPU) * BG_SCALE;
            float worldY = normalizedY * (BG_HEIGHT_PX / PPU) * BG_SCALE;

            return new Vector3(worldX, worldY, 0f);
        }

        private void CreateCampaignIcon(float pixelX, float pixelY, string imageFileName, int flag)
        {
            H3DataAccess h3Engine = H3DataAccess.GetInstance();
            ImageData imageData = h3Engine.RetrieveImage(imageFileName);
            if (imageData == null)
            {
                Debug.LogWarning("[CampaignSelectScene] Image not found: " + imageFileName);
                return;
            }

            // Create a simple GameObject with SpriteRenderer
            GameObject iconObj = new GameObject("CampaignIcon_" + flag);
            iconObj.transform.parent = transform;
            iconObj.transform.position = PixelToWorld(pixelX, pixelY);

            // Scale the icon to match the background scale
            iconObj.transform.localScale = new Vector3(BG_SCALE, BG_SCALE, 1);

            SpriteRenderer renderer = iconObj.AddComponent<SpriteRenderer>();
            renderer.sprite = Texture2DExtension.CreateSpriteFromImageData(imageData, new Vector2(0.5f, 0.5f));
            renderer.sortingOrder = 1; // Above background

            // Add collider for click detection
            BoxCollider2D collider = iconObj.AddComponent<BoxCollider2D>();
            collider.size = renderer.sprite.bounds.size;

            // Add click handler
            CampaignIconClickHandler handler = iconObj.AddComponent<CampaignIconClickHandler>();
            handler.Initialize(flag, OnSelectedCampaign);

            campaignIcons.Add(iconObj);
        }

        private void OnSelectedCampaign(int campaignFlag)
        {
            Debug.Log("[CampaignSelectScene] OnSelectedCampaign: " + campaignFlag);
            CrossSceneData.SelectedCampaignIndex = campaignFlag;
            SceneManager.LoadScene("BonusSelectionScene", LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Simple click handler for campaign icons. Single click selects the campaign.
    /// </summary>
    public class CampaignIconClickHandler : MonoBehaviour
    {
        private int campaignFlag;
        private Action<int> callback;

        public void Initialize(int flag, Action<int> callback)
        {
            this.campaignFlag = flag;
            this.callback = callback;
        }

        private void OnMouseDown()
        {
            callback?.Invoke(campaignFlag);
        }
    }
}
