using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine.DataAccess;
using H3Engine.Common;
using H3Engine.Campaign;
using H3Engine.Mapping;
using UnityEngine.SceneManagement;
using UnityClient.Components.Data;

namespace UnityClient.GUI.Scenes.Lobby
{
    /// <summary>
    /// Campaign info screen corresponding to CCampaignInfoScreen / CBonusSelection in VCMI.
    /// Displays campaign name, description, and scenario list.
    /// The player selects a scenario and starts the game (navigates to GameMapScene).
    ///
    /// Since the original VCMI uses GAME->server() for multiplayer state management,
    /// this single-player version loads campaign data locally.
    /// </summary>
    public class CampaignInfoScene : MonoBehaviour
    {
        private H3Campaign campaign = null;
        private ECampaignVersion campaignVersion;
        private int campaignIndex;
        private int selectedScenarioIndex = 0;

        void Start()
        {
            campaignVersion = CrossSceneData.SelectedCampaignSet;
            campaignIndex = CrossSceneData.SelectedCampaignIndex;

            string campaignFileName = CrossSceneData.GetCampaignFileName(campaignVersion, campaignIndex);
            if (string.IsNullOrEmpty(campaignFileName))
            {
                Debug.LogError("[CampaignInfoScene] No campaign file for version=" + campaignVersion + " index=" + campaignIndex);
                return;
            }

            H3DataAccess dataAccess = H3DataAccess.GetInstance();
            campaign = dataAccess.RetrieveCampaign(campaignFileName);

            if (campaign == null)
            {
                Debug.LogError("[CampaignInfoScene] Failed to load campaign: " + campaignFileName);
                return;
            }

            Debug.Log(string.Format("[CampaignInfoScene] Campaign loaded: {0}, Name: {1}, Scenarios: {2}",
                campaignFileName, campaign.Header.Name, campaign.Scenarios.Count));

            // Select the first available (unconquered) scenario
            selectedScenarioIndex = 0;

            DisplayCampaignInfo();
        }

        private void DisplayCampaignInfo()
        {
            // TODO: Build full UI with campaign background, region map, bonus selection
            // For now, log the campaign info and auto-select the first scenario
            Debug.Log(string.Format("[CampaignInfoScene] Campaign: {0}", campaign.Header.Name));
            Debug.Log(string.Format("[CampaignInfoScene] Description: {0}", campaign.Header.Description));

            for (int i = 0; i < campaign.Scenarios.Count; i++)
            {
                var scenario = campaign.Scenarios[i];
                Debug.Log(string.Format("[CampaignInfoScene]   Scenario {0}: {1} (Difficulty: {2})",
                    i, scenario.MapName, scenario.Difficulty));
            }
        }

        void Update()
        {
            // Temporary: click to start the selected scenario
            if (Input.GetMouseButtonDown(0) && campaign != null)
            {
                StartSelectedScenario();
            }
        }

        /// <summary>
        /// Start the game with the selected scenario.
        /// In VCMI this goes through GAME->server().sendStartGame() after validation.
        /// Here we simply pass the campaign data to GameMapScene via CrossSceneData.
        /// </summary>
        private void StartSelectedScenario()
        {
            if (selectedScenarioIndex < 0 || selectedScenarioIndex >= campaign.Scenarios.Count)
            {
                Debug.LogError("[CampaignInfoScene] Invalid scenario index: " + selectedScenarioIndex);
                return;
            }

            CrossSceneData.CurrentCampaign = campaign;
            CrossSceneData.SelectedScenarioIndex = selectedScenarioIndex;

            Debug.Log(string.Format("[CampaignInfoScene] Starting scenario {0}: {1}",
                selectedScenarioIndex, campaign.Scenarios[selectedScenarioIndex].MapName));

            SceneManager.LoadScene("GameMapScene", LoadSceneMode.Single);
        }
    }
}
