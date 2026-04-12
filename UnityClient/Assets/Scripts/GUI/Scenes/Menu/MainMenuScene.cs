using H3Engine;
using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.FileSystem;
using H3Engine.GUI;
using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityClient.Components;
using UnityClient.Components.Data;
using UnityClient.GUI.Widgets;
using UnityClient.GUI.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityClient.GUI.Scenes.Menu
{
    /// <summary>
    /// Main menu scene corresponding to CMainMenu + CMenuScreen in VCMI.
    /// Manages three states: CommandSelection, GameTypeSelection, CampaignSetSelection.
    /// </summary>
    public class MainMenuScene : MonoBehaviour
    {
        public enum MenuState
        {
            CommandSelection,       // new, load, highscores, credit, exit
            GameTypeSelection,      // single, multi, campaign, tutorial, back
            CampaignSetSelection    // campaign_set_1 (SOD), campaign_set_2 (ROE), campaign_set_3 (AB), back
        }

        public GameObject gameMenuItemPrefab = null;

        public Vector3 menuItem1Position = Vector3.zero;
        public Vector3 menuItem2Position = Vector3.zero;
        public Vector3 menuItem3Position = Vector3.zero;
        public Vector3 menuItem4Position = Vector3.zero;
        public Vector3 menuItem5Position = Vector3.zero;

        private H3Engine.DataAccess.H3DataAccess h3Engine = null;

        private MenuState currentState = MenuState.CommandSelection;
        private Stack<MenuState> stateHistory = new Stack<MenuState>();

        // CommandSelection menu items
        private GameObject menuItemNewGame = null;
        private GameObject menuItemLoadGame = null;
        private GameObject menuItemHighScore = null;
        private GameObject menuItemCredit = null;
        private GameObject menuItemQuit = null;

        // GameTypeSelection menu items
        private GameObject menuItemNewSingle = null;
        private GameObject menuItemNewMulti = null;
        private GameObject menuItemNewCampaign = null;
        private GameObject menuItemNewTutor = null;
        private GameObject menuItemNewBack = null;

        // CampaignSetSelection menu items
        private GameObject menuItemCampaignSOD = null;
        private GameObject menuItemCampaignROE = null;
        private GameObject menuItemCampaignAB = null;
        private GameObject menuItemCampaignCustom = null;
        private GameObject menuItemCampaignBack = null;

        void Start()
        {
            h3Engine = H3Engine.DataAccess.H3DataAccess.GetInstance();
            h3Engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3bitmap.lod"));
            h3Engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3sprite.lod"));
            h3Engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_spr.lod"));
            h3Engine.LoadArchiveFile(H3DataUtil.GetGameDataFilePath("H3ab_bmp.lod"));

            LoadBackground();
            LoadMenuItems();

            SwitchToState(MenuState.CommandSelection);
        }

        void LoadBackground()
        {
            ImageData image = h3Engine.RetrieveImage("gamselBK.pcx");
            Texture2D texture = Texture2DExtension.LoadFromData(image);
            Sprite sprite = Texture2DExtension.CreateSpriteFromTexture(texture, new Vector2(0.5f, 0.5f));

            GameObject go = new GameObject("BackgroundSprite");
            go.transform.parent = transform;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
        }

        void LoadMenuItems()
        {
            // CommandSelection: new, load, highscores, credit, exit
            menuItemNewGame = CreateMenuItem("MMENUNG.def", menuItem1Position, OnNewGameClicked);
            menuItemLoadGame = CreateMenuItem("MMENULG.def", menuItem2Position, OnLoadGameClicked);
            menuItemHighScore = CreateMenuItem("MMENUHS.def", menuItem3Position, OnHighScoreClicked);
            menuItemCredit = CreateMenuItem("MMENUCR.def", menuItem4Position, OnCreditClicked);
            menuItemQuit = CreateMenuItem("MMENUQT.def", menuItem5Position, OnQuitClicked);

            // GameTypeSelection: single, multi, campaign, tutorial, back
            menuItemNewSingle = CreateMenuItem("GTSINGL.def", menuItem1Position, OnSingleClicked);
            menuItemNewMulti = CreateMenuItem("GTMULTI.def", menuItem2Position, OnMultiClicked);
            menuItemNewCampaign = CreateMenuItem("GTCAMPN.def", menuItem3Position, OnCampaignClicked);
            menuItemNewTutor = CreateMenuItem("GTTUTOR.def", menuItem4Position, OnTutorialClicked);
            menuItemNewBack = CreateMenuItem("GTBACK.def", menuItem5Position, OnBackClicked);

            // CampaignSetSelection: SOD, ROE, AB, custom, back
            menuItemCampaignSOD = CreateMenuItem("csssod.def", menuItem1Position, OnCampaignSetSODClicked);
            menuItemCampaignROE = CreateMenuItem("cssroe.def", menuItem2Position, OnCampaignSetROEClicked);
            menuItemCampaignAB = CreateMenuItem("cssarm.def", menuItem3Position, OnCampaignSetABClicked);
            menuItemCampaignCustom = CreateMenuItem("csscus.def", menuItem4Position, OnCampaignSetCustomClicked);
            menuItemCampaignBack = CreateMenuItem("cssexit.def", menuItem5Position, OnBackClicked);
        }

        private GameObject CreateMenuItem(string defFileName, Vector3 position, Action callback)
        {
            BundleImageDefinition bundleDefinition = h3Engine.RetrieveBundleImage(defFileName);
            GameObject menuItem = Instantiate(gameMenuItemPrefab, transform);
            menuItem.transform.position = position;
            GameMenuItem gameMenuItem = menuItem.GetComponent<GameMenuItem>();
            gameMenuItem.Initialize(bundleDefinition, callback);
            menuItem.SetActive(false);
            return menuItem;
        }

        #region State Machine

        private void SwitchToState(MenuState newState)
        {
            HideAllMenus();
            currentState = newState;

            switch (newState)
            {
                case MenuState.CommandSelection:
                    ShowCommandSelection(true);
                    break;
                case MenuState.GameTypeSelection:
                    ShowGameTypeSelection(true);
                    break;
                case MenuState.CampaignSetSelection:
                    ShowCampaignSetSelection(true);
                    break;
            }
        }

        private void PushState(MenuState newState)
        {
            stateHistory.Push(currentState);
            SwitchToState(newState);
        }

        private void PopState()
        {
            if (stateHistory.Count > 0)
            {
                MenuState previousState = stateHistory.Pop();
                SwitchToState(previousState);
            }
        }

        private void HideAllMenus()
        {
            ShowCommandSelection(false);
            ShowGameTypeSelection(false);
            ShowCampaignSetSelection(false);
        }

        private void ShowCommandSelection(bool value)
        {
            menuItemNewGame.SetActive(value);
            menuItemLoadGame.SetActive(value);
            menuItemHighScore.SetActive(value);
            menuItemCredit.SetActive(value);
            menuItemQuit.SetActive(value);
        }

        private void ShowGameTypeSelection(bool value)
        {
            menuItemNewSingle.SetActive(value);
            menuItemNewMulti.SetActive(value);
            menuItemNewCampaign.SetActive(value);
            menuItemNewTutor.SetActive(value);
            menuItemNewBack.SetActive(value);
        }

        private void ShowCampaignSetSelection(bool value)
        {
            menuItemCampaignSOD.SetActive(value);
            menuItemCampaignROE.SetActive(value);
            menuItemCampaignAB.SetActive(value);
            menuItemCampaignCustom.SetActive(value);
            menuItemCampaignBack.SetActive(value);
        }

        #endregion

        #region CommandSelection Handlers

        private void OnNewGameClicked()
        {
            PushState(MenuState.GameTypeSelection);
        }

        private void OnLoadGameClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Load Game - not implemented");
        }

        private void OnHighScoreClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] High Score - not implemented");
        }

        private void OnCreditClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Credit - not implemented");
        }

        private void OnQuitClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Quit");
            Application.Quit();
        }

        #endregion

        #region GameTypeSelection Handlers

        private void OnSingleClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Single Player - not implemented");
        }

        private void OnMultiClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Multiplayer - not implemented");
        }

        private void OnCampaignClicked()
        {
            PushState(MenuState.CampaignSetSelection);
        }

        private void OnTutorialClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Tutorial - not implemented");
        }

        #endregion

        #region CampaignSetSelection Handlers

        private void OnCampaignSetSODClicked()
        {
            OpenCampaignSelectScene(ECampaignVersion.SOD);
        }

        private void OnCampaignSetROEClicked()
        {
            OpenCampaignSelectScene(ECampaignVersion.ROE);
        }

        private void OnCampaignSetABClicked()
        {
            OpenCampaignSelectScene(ECampaignVersion.AB);
        }

        private void OnCampaignSetCustomClicked()
        {
            // Placeholder
            Debug.Log("[MainMenuScene] Custom Campaign - not implemented");
        }

        #endregion

        #region Common Handlers

        private void OnBackClicked()
        {
            PopState();
        }

        #endregion

        #region Scene Navigation

        /// <summary>
        /// Navigate to CampaignSelectScene (corresponding to CCampaignScreen in VCMI).
        /// </summary>
        private void OpenCampaignSelectScene(ECampaignVersion campaignVersion)
        {
            CrossSceneData.SelectedCampaignSet = campaignVersion;
            SceneManager.LoadScene("CampaignSelectScene", LoadSceneMode.Single);
        }

        #endregion
    }
}
