using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine.FileSystem;

namespace H3Engine.Campaign
{
    public class H3CampaignLoader
    {
        private H3Campaign campaignObject = null;

        private string h3cFileFullPath = null;

        public H3CampaignLoader(string h3cFileFullPath)
        {
            this.h3cFileFullPath = h3cFileFullPath;
        }

        public H3Campaign LoadCampaign()
        {
            campaignObject = new H3Campaign();

            using (FileStream file = new FileStream(h3cFileFullPath, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(file))
                {
                    campaignObject.Header = ReadHeader(reader);
                    campaignObject.Header.FileName = h3cFileFullPath;

                    int scenarioCount = 8;      // Read from CampText.txt, currently hard coded for AB.h3c

                    for(int g = 0; g < scenarioCount; g++)
                    {
                        CampaignScenario scenario = ReadScenario(reader);
                        campaignObject.PushScenario(scenario);
                    }

                    int scenarioId = 0;
                }
            }

            return campaignObject;
        }

        private CampaignHeader ReadHeader(BinaryReader reader)
        {
            CampaignHeader header = new CampaignHeader();

            header.Version = (ECampaignVersion)reader.ReadUInt32();
            header.MapVersion = reader.ReadByte() - 1;  //change range of it from [1, 20] to [0, 19]
            header.Name = reader.ReadStringToEnd();

            if (header.Version > ECampaignVersion.RoE)
            {
                header.DifficultyChoosenByPlayer = reader.ReadByte();
            }
            else
            {
                header.DifficultyChoosenByPlayer = 0;
            }

            header.Music = reader.ReadByte();

            return header;
        }

        private CampaignScenario ReadScenario(BinaryReader reader)
        {
            CampaignScenario scenario = new CampaignScenario();




            return scenario;
        }
    }
}
