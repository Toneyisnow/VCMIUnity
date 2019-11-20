using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine.FileSystem;
using H3Engine.Mapping;
using H3Engine.Utils;

namespace H3Engine.Campaign
{
    public class H3CampaignLoader
    {
        private H3Campaign campaignObject = null;

        private string campaignFileName = null;

        private byte[] rawBytesData = null;

        private byte[] campaignHeaderBytes = null;

        private List<byte[]> campaignMapBytes = null;


        public H3CampaignLoader(string campaignFileName, byte[] rawBytesData)
        {
            this.campaignFileName = campaignFileName.ToLower();
            this.rawBytesData = rawBytesData;
        }

        public H3Campaign LoadCampaign()
        {
            campaignObject = new H3Campaign();

            ExtractRawBytesData();
            
            // Load the Campaign Data
            using (MemoryStream headerStream = new MemoryStream(campaignHeaderBytes))
            {
                using (BinaryReader reader = new BinaryReader(headerStream))
                {
                    campaignObject.Header = ReadHeader(reader);
                    campaignObject.Header.FileName = campaignFileName;

                    int scenarioCount = 8;      // Read from CampText.txt, currently hard coded for AB.h3c

                    for (int g = 0; g < 1; g++)
                    //// for (int g = 0; g < campaignMapBytes.Count; g++)
                    {
                        // Load the Scenario Configs
                        CampaignScenario scenario = ReadScenario(reader, campaignObject.Header.Version, campaignObject.Header.MapVersion);

                        try
                        {
                            // Load the H3M Map Data
                            H3MapLoader mapLoader = new H3MapLoader(campaignMapBytes[g]);
                            scenario.MapData = mapLoader.LoadMap();
                        }
                        catch(Exception ex)
                        {
                            int here = 0;
                        }

                        campaignObject.PushScenario(scenario);
                    }
                }
            }

            return campaignObject;
        }

        private void ExtractRawBytesData()
        {
            if (rawBytesData == null)
            {
                return;
            }

            campaignMapBytes = new List<byte[]>();
            List<byte> fileBytes = new List<byte>();
            bool isFirstSection = true;

            long index = 0;
            while (index < rawBytesData.Length)
            {
                if (rawBytesData[index] == 0x1F)
                {
                    if (rawBytesData[index + 1] == 0x8B)
                    {
                        if (rawBytesData[index + 2] == 0x08)
                        {
                            if (rawBytesData[index + 3] == 0x00)
                            {
                                if (fileBytes.Count > 0)
                                {
                                    byte[] data = GZipStreamHelper.DecompressBytes(fileBytes.ToArray());
                                    if (isFirstSection)
                                    {
                                        campaignHeaderBytes = data;
                                        isFirstSection = false;
                                    }
                                    else
                                    {
                                        // Save the current bytes into file
                                        campaignMapBytes.Add(data);
                                    }
                                }

                                fileBytes = new List<byte>() { 0x1F, 0x8B, 0x08, 0x00 };
                                index += 4;
                            }
                            else
                            {
                                fileBytes.Add(rawBytesData[index]);
                                fileBytes.Add(rawBytesData[index + 1]);
                                fileBytes.Add(rawBytesData[index + 2]);
                                index += 3;
                            }
                        }
                        else
                        {
                            fileBytes.Add(rawBytesData[index]);
                            fileBytes.Add(rawBytesData[index + 1]);
                            index += 2;
                        }
                    }
                    else
                    {
                        fileBytes.Add(rawBytesData[index]);
                        index++;
                    }
                }
                else
                {
                    fileBytes.Add(rawBytesData[index]);
                    index++;
                }
            }

            if (fileBytes.Count > 0)
            {
                // Save the current bytes into file
                byte[] data = GZipStreamHelper.DecompressBytes(fileBytes.ToArray());
                campaignMapBytes.Add(data);
            }
        }

        private CampaignHeader ReadHeader(BinaryReader reader)
        {
            CampaignHeader header = new CampaignHeader();

            header.Version = (ECampaignVersion)reader.ReadUInt32();
            header.MapVersion = reader.ReadByte() - 1;  //change range of it from [1, 20] to [0, 19]
            header.Name = reader.ReadStringWithLength();
            header.Description = reader.ReadStringWithLength();

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

        private CampaignScenario ReadScenario(BinaryReader reader, ECampaignVersion version, int mapVersion)
        {
            CampaignScenario scenario = new CampaignScenario();

            scenario.Conquered = false;
            scenario.MapName = reader.ReadStringWithLength();
            scenario.PackedMapSize = reader.ReadUInt32();

            if (mapVersion == 18)
            {
                scenario.LoadPreconditionRegions(reader.ReadUInt16());
            }
            else
            {
                scenario.LoadPreconditionRegions(reader.ReadByte());
            }

            scenario.RegionColor = reader.ReadByte();
            scenario.Difficulty = reader.ReadByte();
            scenario.RegionText = reader.ReadStringWithLength();
            scenario.Prolog = ReadScenarioPrologEpilog(reader);
            scenario.Epilog = ReadScenarioPrologEpilog(reader);

            scenario.TravelOptions = ReadScenarioTravelOptions(reader, version);

            return scenario;
        }

        private ScenarioPrologEpilog ReadScenarioPrologEpilog(BinaryReader reader)
        {
            ScenarioPrologEpilog pelog = new ScenarioPrologEpilog();

            pelog.HasPrologEpilog = (reader.ReadByte() > 0);
            if (pelog.HasPrologEpilog)
            {
                pelog.VideoId = reader.ReadByte();
                pelog.MusicId = reader.ReadByte();
                pelog.Text = reader.ReadStringWithLength();
            }

            return pelog;
        }

        private ScenarioTravelOption ReadScenarioTravelOptions(BinaryReader reader, ECampaignVersion version)
        {
            ScenarioTravelOption travelOption = new ScenarioTravelOption();

            travelOption.WhatHeroKeeps = reader.ReadByte();
            travelOption.MonstersKeptByHero = reader.ReadBytes(ScenarioTravelOption.MONSTERS_KEPT_BY_HERO_COUNT);

            if (version < ECampaignVersion.SoD)
            {
                travelOption.ArtifactsKeptByHero = reader.ReadBytes(ScenarioTravelOption.ARTIFACTS_KEPT_BY_HERO_COUNT - 1);
            }
            else
            {
                travelOption.ArtifactsKeptByHero = reader.ReadBytes(ScenarioTravelOption.ARTIFACTS_KEPT_BY_HERO_COUNT);
            }

            travelOption.StartOptions = reader.ReadByte();
            byte numOfBonus = 0;
            travelOption.BonusChoices = new List<ScenarioTravelBonus>();

            switch (travelOption.StartOptions)
            {
                case 0:
                    // No Bonus, seems to be OK.
                    break;
                case 1: //reading of bonuses player can choose
                    travelOption.PlayerColor = reader.ReadByte();
                    numOfBonus = reader.ReadByte();
                    for (int g = 0; g < numOfBonus; g++)
                    {
                        ScenarioTravelBonus bonus = new ScenarioTravelBonus();
                        bonus.Type = (ScenarioTravelBonus.EBonusType)reader.ReadByte();

                        switch(bonus.Type)
                        {
                            case ScenarioTravelBonus.EBonusType.SPELL:
                                bonus.Info1 = reader.ReadUInt16();  //hero
                                bonus.Info2 = reader.ReadByte();    //spell ID
                                break;
                            case ScenarioTravelBonus.EBonusType.MONSTER:
                                bonus.Info1 = reader.ReadUInt16();  //hero
                                bonus.Info2 = reader.ReadUInt16();  //monster type
                                bonus.Info3 = reader.ReadUInt16();  //monster count
                                break;
                            case ScenarioTravelBonus.EBonusType.BUILDING:
                                bonus.Info1 = reader.ReadByte(); //building ID (0 - town hall, 1 - city hall, 2 - capitol, etc)
                                break;
                            case ScenarioTravelBonus.EBonusType.ARTIFACT:
                                bonus.Info1 = reader.ReadUInt16();  //hero
                                bonus.Info2 = reader.ReadUInt16();  //artifact ID
                                break;
                            case ScenarioTravelBonus.EBonusType.SPELL_SCROLL:
                                bonus.Info1 = reader.ReadUInt16();  //hero
                                bonus.Info2 = reader.ReadByte();    //spell ID
                                break;
                            case ScenarioTravelBonus.EBonusType.PRIMARY_SKILL:
                                bonus.Info1 = reader.ReadUInt16(); //hero
                                bonus.Info2 = (int)reader.ReadUInt32(); //bonuses (4 bytes for 4 skills)
                                break;
                            case ScenarioTravelBonus.EBonusType.SECONDARY_SKILL:
                                bonus.Info1 = reader.ReadUInt16(); //hero
                                bonus.Info2 = reader.ReadByte(); //skill ID
                                bonus.Info3 = reader.ReadByte(); //skill level
                                break;
                            case ScenarioTravelBonus.EBonusType.RESOURCE:
                                bonus.Info1 = reader.ReadByte(); //type
                                                                  //FD - wood+ore
                                                                  //FE - mercury+sulfur+crystal+gem
                                bonus.Info2 = (int)reader.ReadUInt32(); //count
                                break;
                            default:
                                break;
                        }

                        travelOption.BonusChoices.Add(bonus);
                    }
                    break;
                case 2:     //reading of players (colors / scenarios ?) player can choose
                    numOfBonus = reader.ReadByte();
                    for (int g = 0; g < numOfBonus; g++)
                    {
                        ScenarioTravelBonus bonus = new ScenarioTravelBonus();
                        bonus.Type = ScenarioTravelBonus.EBonusType.HEROES_FROM_PREVIOUS_SCENARIO;
                        bonus.Info1 = reader.ReadByte(); //player color
                        bonus.Info2 = reader.ReadByte(); //from what scenario

                        travelOption.BonusChoices.Add(bonus);
                    }
                    break;
                case 3:     //heroes player can choose between
                    numOfBonus = reader.ReadByte();
                    for (int g = 0; g < numOfBonus; g++)
                    {
                        ScenarioTravelBonus bonus = new ScenarioTravelBonus();
                        bonus.Type = ScenarioTravelBonus.EBonusType.HERO;
                        bonus.Info1 = reader.ReadByte(); //player color
                        bonus.Info2 = reader.ReadUInt16(); //hero, FF FF - random

                        travelOption.BonusChoices.Add(bonus);
                    }
                    break;
                default:
                    // Something wrong with the H3C file
                    break;
            }

            return travelOption;
        }

    }
}
