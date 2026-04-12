using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.Campaign;
using H3Engine.Mapping;

namespace UnityClient.Components.Data
{
    /// <summary>
    /// Static data shared across scenes.
    /// Replaces the server-based state management (GAME->server()) in VCMI
    /// with simple local state for single-player mode.
    /// </summary>
    public static class CrossSceneData
    {
        /// <summary>
        /// The campaign set selected in MainMenuScene (SOD, ROE, AB).
        /// Corresponds to the campaignSet parameter in CCampaignScreen.
        /// </summary>
        public static ECampaignVersion SelectedCampaignSet { get; set; }

        /// <summary>
        /// The campaign index selected in CampaignSelectScene (1-based flag from CampaignIcon).
        /// Corresponds to CCampaignButton::campFile selection in VCMI.
        /// </summary>
        public static int SelectedCampaignIndex { get; set; }

        /// <summary>
        /// The loaded campaign object, set by BonusSelectionScene before entering GameMapScene.
        /// Replaces GAME->server().si->campState in VCMI.
        /// </summary>
        public static H3Campaign CurrentCampaign { get; set; }

        /// <summary>
        /// The scenario index within the campaign to play.
        /// Corresponds to GAME->server().campaignMap in VCMI.
        /// </summary>
        public static int SelectedScenarioIndex { get; set; }

        /// <summary>
        /// Pre-loaded map data from the loading screen, so GameMapScene can skip re-loading.
        /// Set by BonusSelectionScene's loading coroutine, consumed and cleared by GameMapScene.
        /// </summary>
        public static H3Map LoadedMap { get; set; }

        /// <summary>
        /// Legacy property kept for backward compatibility.
        /// </summary>
        public static ECampaignVersion SelectedCampaign
        {
            get { return SelectedCampaignSet; }
            set { SelectedCampaignSet = value; }
        }

        /// <summary>
        /// Map campaign version + campaign index to the .h3c filename.
        /// Based on the campaign file list from VCMI's campaignSets config.
        /// </summary>
        public static string GetCampaignFileName(ECampaignVersion version, int campaignIndex)
        {
            switch (version)
            {
                case ECampaignVersion.SOD:
                    return GetSODCampaignFileName(campaignIndex);
                case ECampaignVersion.ROE:
                    return GetROECampaignFileName(campaignIndex);
                case ECampaignVersion.AB:
                    return GetABCampaignFileName(campaignIndex);
                default:
                    return null;
            }
        }

        private static string GetSODCampaignFileName(int index)
        {
            switch (index)
            {
                case 1: return "final.h3c";
                default: return null;
            }
        }

        private static string GetROECampaignFileName(int index)
        {
            switch (index)
            {
                case 1: return "good1.h3c";
                case 2: return "evil1.h3c";
                case 3: return "good2.h3c";
                case 4: return "neutral.h3c";
                case 5: return "evil2.h3c";
                case 6: return "good3.h3c";
                default: return null;
            }
        }

        private static string GetABCampaignFileName(int index)
        {
            switch (index)
            {
                case 1: return "ab.h3c";
                case 2: return "blood.h3c";
                case 3: return "slayer.h3c";
                case 4: return "festival.h3c";
                case 5: return "fire.h3c";
                case 6: return "fool.h3c";
                default: return null;
            }
        }
    }
}
