using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Campaign
{

    public class CampaignHeader
    {
        public ECampaignVersion Version
        {
            get; set;
        }

        /// <summary>
        /// CampText.txt's format
        /// </summary>
        public int MapVersion
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public string FileName
        {
            get; set;
        }

        public int DifficultyChoosenByPlayer
        {
            get; set;
        }

        public int Music
        {
            get; set;
        }

    }
}
