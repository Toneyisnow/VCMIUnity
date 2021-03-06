﻿using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityClient.Components;
using UnityClient.Components.Data;
using UnityClient.GUI.GameControls;
using UnityClient.GUI.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityClient.GUI.Scenes
{
    public class GameMenuScene : MonoBehaviour
    {
        public GameObject gameMenuItemPrefab = null;


        private H3Engine.DataAccess.H3DataAccess h3Engine = null;

        public Vector3 menuItem1Position = Vector3.zero;
        public Vector3 menuItem2Position = Vector3.zero;
        public Vector3 menuItem3Position = Vector3.zero;
        public Vector3 menuItem4Position = Vector3.zero;
        public Vector3 menuItem5Position = Vector3.zero;

        private GameObject menuItemNewGame = null;
        private GameObject menuItemLoadGame = null;
        private GameObject menuItemHighScore = null;
        private GameObject menuItemCredit = null;
        private GameObject menuItemQuit = null;

        private GameObject menuItemNewSingle = null;
        private GameObject menuItemNewMulti = null;
        private GameObject menuItemNewCampaign = null;
        private GameObject menuItemNewTutor = null;
        private GameObject menuItemNewBack = null;

        private GameObject menuItemCampaignSOD = null;
        private GameObject menuItemCampaignROE = null;
        private GameObject menuItemCampaignAB = null;
        private GameObject menuItemCampaignCustom = null;
        private GameObject menuItemCampaignBack = null;


        private static string GetGameDataFilePath(string filename)
        {
            return Path.Combine(Application.streamingAssetsPath, filename);
        }

        // Start is called before the first frame update
        void Start()
        {
            h3Engine = H3Engine.DataAccess.H3DataAccess.GetInstance();
            h3Engine.LoadArchiveFile(GetGameDataFilePath("GameData/SOD.zh-cn/H3bitmap.lod"));
            h3Engine.LoadArchiveFile(GetGameDataFilePath("GameData/SOD.zh-cn/H3sprite.lod"));
            h3Engine.LoadArchiveFile(GetGameDataFilePath("GameData/SOD.zh-cn/H3ab_spr.lod"));
            h3Engine.LoadArchiveFile(GetGameDataFilePath("GameData/SOD.zh-cn/H3ab_bmp.lod"));

            LoadBackground();

            LoadMenuItems();

            ShowMainMenu(true);

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
            // New Game
            menuItemNewGame = CreateMenuItem("MMENUNG.def", menuItem1Position, () => { this.NewGameClicked(); });

            // Load Game
            menuItemLoadGame = CreateMenuItem("MMENULG.def", menuItem2Position, () => { this.LoadGameClicked(); });

            // High Score
            menuItemHighScore = CreateMenuItem("MMENUHS.def", menuItem3Position, () => { this.HighScoreClicked(); });

            // Credit
            menuItemCredit = CreateMenuItem("MMENUCR.def", menuItem4Position, () => { this.CreditClicked(); });

            // Quit
            menuItemQuit = CreateMenuItem("MMENUQT.def", menuItem5Position, () => { this.QuitGameClicked(); });


            // Single Game
            menuItemNewSingle = CreateMenuItem("GTSINGL.def", menuItem1Position, () => { this.NewSingleClicked(); });

            // Multi Game
            menuItemNewMulti = CreateMenuItem("GTMULTI.def", menuItem2Position, () => { this.NewMultiClicked(); });

            // Campaign Game
            menuItemNewCampaign = CreateMenuItem("GTCAMPN.def", menuItem3Position, () => { this.NewCampaignClicked(); });

            // Tutor Game
            menuItemNewTutor = CreateMenuItem("GTTUTOR.def", menuItem4Position, () => { this.NewTutorClicked(); });

            // Back
            menuItemNewBack = CreateMenuItem("GTBACK.def", menuItem5Position, () => { this.NewBackClicked(); });


            // Campaign SOD
            menuItemCampaignSOD = CreateMenuItem("csssod.def", menuItem1Position, () => { this.CampaignSODClicked(); });

            // Campaign ROE
            menuItemCampaignROE = CreateMenuItem("cssroe.def", menuItem2Position, () => { this.CampaignROEClicked(); });

            // Campaign AB
            menuItemCampaignAB = CreateMenuItem("cssarm.def", menuItem3Position, () => { this.CampaignABClicked(); });

            // Campaign Custom
            menuItemCampaignCustom = CreateMenuItem("csscus.def", menuItem4Position, () => { this.CampaignCusClicked(); });

            // Campaign SOD
            menuItemCampaignBack = CreateMenuItem("cssexit.def", menuItem5Position, () => { this.CampaignBackClicked(); });


        }

        void ShowMainMenu(bool value)
        {
            menuItemNewGame.SetActive(value);
            menuItemLoadGame.SetActive(value);
            menuItemHighScore.SetActive(value);
            menuItemCredit.SetActive(value);
            menuItemQuit.SetActive(value);
        }

        void ShowNewGameMenu(bool value)
        {
            menuItemNewSingle.SetActive(value);
            menuItemNewMulti.SetActive(value);
            menuItemNewCampaign.SetActive(value);
            menuItemNewTutor.SetActive(value);
            menuItemNewBack.SetActive(value);
        }

        void ShowCampaignMenu(bool value)
        {
            menuItemCampaignSOD.SetActive(value);
            menuItemCampaignROE.SetActive(value);
            menuItemCampaignAB.SetActive(value);
            menuItemCampaignCustom.SetActive(value);
            menuItemCampaignBack.SetActive(value);

        }

        // Update is called once per frame
        void Update()
        {

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


        private void NewGameClicked()
        {
            ShowNewGameMenu(true);
            ShowMainMenu(false);
        }

        private void LoadGameClicked()
        {
            print("LoadGameClicked");
        }

        private void HighScoreClicked()
        {

        }

        private void CreditClicked()
        {

        }

        private void QuitGameClicked()
        {

        }

        private void NewSingleClicked()
        {

        }

        private void NewMultiClicked()
        {

        }

        private void NewCampaignClicked()
        {
            ShowNewGameMenu(false);
            ShowCampaignMenu(true);
        }

        private void NewTutorClicked()
        {

        }

        private void NewBackClicked()
        {
            ShowNewGameMenu(false);
            ShowMainMenu(true);
        }

        private void CampaignSODClicked()
        {
            CrossSceneData.SelectedCampaign = H3Engine.Common.ECampaignVersion.SOD;
            SceneManager.LoadScene("CampaignSelectScene", LoadSceneMode.Single);
        }

        private void CampaignROEClicked()
        {
            CrossSceneData.SelectedCampaign = H3Engine.Common.ECampaignVersion.ROE;
            SceneManager.LoadScene("CampaignSelectScene", LoadSceneMode.Single);
        }

        private void CampaignABClicked()
        {
            CrossSceneData.SelectedCampaign = H3Engine.Common.ECampaignVersion.AB;
            SceneManager.LoadScene("CampaignSelectScene", LoadSceneMode.Single);
        }

        private void CampaignCusClicked()
        {

        }

        private void CampaignBackClicked()
        {
            ShowNewGameMenu(true);
            ShowCampaignMenu(false);
        }


    }
}