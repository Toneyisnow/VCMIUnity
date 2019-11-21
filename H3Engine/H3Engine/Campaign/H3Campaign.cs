using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Campaign
{
    

    public class H3Campaign
    {
        public CampaignHeader Header
        {
            get; set;
        }

        public List<byte[]> CampaignMapBytes
        {
            get; set;
        }


        public List<CampaignScenario> Scenarios = null;

        public H3Campaign()
        {
            this.Scenarios = new List<CampaignScenario>();
        }

        public void PushScenario(CampaignScenario scenario)
        {
            this.Scenarios.Add(scenario);
        }

    }
}
