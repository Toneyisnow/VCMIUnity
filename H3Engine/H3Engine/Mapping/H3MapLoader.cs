﻿using H3Engine.Common;
using H3Engine.Components;
using H3Engine.Core;
using H3Engine.FileSystem;
using H3Engine.MapObjects;
using H3Engine.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Mapping
{
    public class H3MapLoader
    {
        private H3Map mapObject = null;
        
        private byte[] mapData = null;

        private ILogger logger = null;

        public H3MapLoader(string h3mFileFullPath)
        {
            this.logger = LoggerInstance.GetLogger();
            using (FileStream file = new FileStream(h3mFileFullPath, FileMode.Open, FileAccess.Read))
            {
                byte[] rawData = StreamHelper.ReadToEnd(file);
                mapData = GZipStreamHelper.DecompressBytes(rawData);
            }
        }

        public H3MapLoader(byte[] rawData)
        {
            this.logger = LoggerInstance.GetLogger();
            mapData = GZipStreamHelper.DecompressBytes(rawData);
        }

        public byte[] MapData
        {
            get
            {
                return this.mapData;
            }
        }

        public H3Map LoadMap()
        {
            mapObject = new H3Map();

            using (MemoryStream streamData = new MemoryStream(mapData))
            {
                using (BinaryReader reader = new BinaryReader(streamData, Encoding.UTF8))
                {
                    ReadHeader(reader);

                    ReadPlayerInfo(reader);

                    ReadVictoryLossConditions(reader);

                    // 1 byte + N * 1
                    ReadTeamInfo(reader);

                    // 20 byte mask + placeholdercount + placeholder
                    ReadAllowedHeroes(reader);
                    
                    // 417 
                    ReadDisposedHeroes(reader);

                    // 18 bytes
                    ReadAllowedArtifacts(reader);

                    // 13 bytes
                    ReadAllowedSpellsAbilities(reader);

                    if (this.mapObject.Header.Version == EMapFormat.HOTA)
                    {
                        // Unknown
                        reader.Seek(17, SeekOrigin.Current);
                    }

                    ReadRumors(reader);

                    ReadPredefinedHeroes(reader);

                    //// reader.Seek(646, SeekOrigin.Begin);

                    ReadTerrain(reader);

                    ReadObjectTemplates(reader);

                    ReadObjects(reader);

                    ReadEvents(reader);

                    ConsolidateAndAdjustData();
                }
            }

            return mapObject;
        }
        
        private void ReadHeader(BinaryReader reader)
        {
            // Check map for validity
            // Note: disabled, causes decompression of the entire file ( = SLOW)
            //if(inputStream->getSize() < 50)
            //{
            //	throw std::runtime_error("Corrupted map file.");
            //}

            // Map version
            UInt32 byte1 = reader.ReadUInt32();

            logger.LogTrace("Map version:" + byte1);
            mapObject.Header.Version = (EMapFormat)byte1;


            mapObject.Header.AreAnyPlayers = reader.ReadBoolean();
            logger.LogTrace("AreAnyPlayers:" + mapObject.Header.AreAnyPlayers);

            if (mapObject.Header.Version == EMapFormat.HOTA)
            {
                reader.ReadUInt32();
                reader.ReadBytes(2);
            }

            mapObject.Header.Height = reader.ReadUInt32();
            mapObject.Header.Width = mapObject.Header.Height;
            logger.LogTrace("Map Height and Width:" + mapObject.Header.Height);

            mapObject.Header.IsTwoLevel = reader.ReadBoolean();
            logger.LogTrace("twoLevel:" + mapObject.Header.IsTwoLevel);


            mapObject.Header.Name = reader.ReadStringWithLength();
            logger.LogTrace("Name:" + mapObject.Header.Name);

            mapObject.Header.Description = reader.ReadStringWithLength();
            logger.LogTrace("Description:" + mapObject.Header.Description);

            mapObject.Header.Difficulty = reader.ReadByte();
            logger.LogTrace("Difficulty:" + mapObject.Header.Difficulty);

            int heroLevelLimit = reader.ReadByte();
            logger.LogTrace("HeroLevelLimit:" + heroLevelLimit);


        }


        private void ReadPlayerInfo(BinaryReader reader)
        {
            for (int i = 0; i < GameConstants.PLAYER_LIMIT_T; i++)
            {
                PlayerInfo playerInfo = new PlayerInfo();

                logger.LogTrace("Reading Player [" + i.ToString() + "]");

                playerInfo.CanHumanPlay = reader.ReadBoolean();
                playerInfo.CanComputerPlay = reader.ReadBoolean();
                logger.LogTrace("canHumanPlay: " + playerInfo.CanHumanPlay);
                logger.LogTrace("canComputerPlay: " + playerInfo.CanComputerPlay);

                if (!playerInfo.CanHumanPlay && !playerInfo.CanComputerPlay)
                {
                    switch (mapObject.Header.Version)
                    {
                        case EMapFormat.SOD:
                        case EMapFormat.WOG:
                        // case EMapFormat.HOTA:
                            reader.Skip(13);
                            break;
                        case EMapFormat.HOTA:
                            reader.Skip(15);
                            break;
                        case EMapFormat.AB:
                            reader.Skip(12);
                            break;
                        case EMapFormat.ROE:
                            reader.Skip(6);
                            break;
                    }
                    continue;
                }

                playerInfo.AiTactic = (EAiTactic)reader.ReadByte();
                logger.LogTrace("aiTactic:" + playerInfo.AiTactic);

                if (mapObject.Header.Version == EMapFormat.SOD 
                    || mapObject.Header.Version == EMapFormat.WOG
                    || mapObject.Header.Version == EMapFormat.HOTA)
                {
                    playerInfo.P7 = reader.ReadByte();
                }
                else
                {
                    playerInfo.P7 = -1;
                }

                logger.LogTrace("p7:" + playerInfo.P7);

                // Reading the Factions for Player
                playerInfo.AllowedFactions = new List<int>();
                int allowedFactionsMask = reader.ReadByte();
                logger.LogTrace("allowedFactionsMask:" + allowedFactionsMask);

                int totalFactionCount = GameConstants.F_NUMBER;
                if (mapObject.Header.Version != EMapFormat.ROE)
                    allowedFactionsMask += reader.ReadByte() << 8;
                else
                    totalFactionCount--; //exclude conflux for ROE
                
                for (int fact = 0; fact < totalFactionCount; ++fact)
                {
                    if ((allowedFactionsMask & (1 << fact)) > 0)
                    {
                        playerInfo.AllowedFactions.Add(fact);
                    }
                }

                playerInfo.IsFactionRandom = reader.ReadBoolean();
                playerInfo.HasMainTown = reader.ReadBoolean();
                logger.LogTrace("isFactionRandom:" + playerInfo.IsFactionRandom);
                logger.LogTrace("hasMainTown:" + playerInfo.HasMainTown);

                if (playerInfo.HasMainTown)
                {
                    /// Added in new version, not tested yet
                    if (mapObject.Header.Version != EMapFormat.ROE)
                    {
                        playerInfo.GenerateHeroAtMainTown = reader.ReadBoolean();
                        playerInfo.GenerateHero = reader.ReadBoolean();
                    }
                    else
                    {
                        playerInfo.GenerateHeroAtMainTown = true;
                        playerInfo.GenerateHero = false;
                    }

                    var townPosition = reader.ReadPosition();
                    logger.LogTrace(string.Format("Main Town Position: {0}, {1}, {2}", townPosition.PosX, townPosition.PosY, townPosition.Level));
                    playerInfo.MainTownPosition = townPosition;
                }

                playerInfo.HasRandomHero = reader.ReadBoolean();
                logger.LogTrace("hasRandomHero:" + playerInfo.HasRandomHero);

                playerInfo.MainCustomHeroId = reader.ReadByte();
                logger.LogTrace("mainCustomHeroId:" + playerInfo.MainCustomHeroId);

                if (playerInfo.MainCustomHeroId != 0xff)
                {
                    playerInfo.MainCustomHeroPortrait = reader.ReadByte();
                    if (playerInfo.MainCustomHeroPortrait == 0xff)
                    {
                        playerInfo.MainCustomHeroPortrait = -1;
                    }

                    playerInfo.MainCustomHeroName = reader.ReadStringWithLength();
                    logger.LogTrace("mainCustomHeroPortrait:" + playerInfo.MainCustomHeroPortrait);
                    logger.LogTrace("heroName:" + playerInfo.MainCustomHeroName);

                }
                else
                {
                    playerInfo.MainCustomHeroId = -1;
                }

                if (mapObject.Header.Version != EMapFormat.ROE)
                {
                    playerInfo.PowerPlaceHolders = reader.ReadByte();
                    int heroCount = reader.ReadByte();
                    reader.Skip(3);

                    playerInfo.HeroIds = new List<H3HeroId>();
                    for (int pp = 0; pp < heroCount; ++pp)
                    {
                        H3HeroId heroId = new H3HeroId();
                        heroId.Id = reader.ReadByte();
                        heroId.Name = reader.ReadStringWithLength();
                        playerInfo.HeroIds.Add(heroId);
                    }
                }
            }
        }
        
        private void ReadVictoryLossConditions(BinaryReader reader)
        {
            //// mapObject.Header.TrigggeredEvents = new List<TrigggeredEvent>();

            var vicCondition = (EVictoryConditionType)reader.ReadByte();
            logger.LogTrace("Victory Condition:" + vicCondition);

            if (vicCondition == EVictoryConditionType.WINSTANDARD)
            {
                // create normal condition
                
            }
            else
            {
                bool allowNormalVictory = reader.ReadBoolean();
                bool appliesToAI = reader.ReadBoolean();

                if (allowNormalVictory)
                {
                    int playersOnMap = 2;
                    if (playersOnMap == 1)
                    {
                        //// logGlobal->warn("Map %s has only one player but allows normal victory?", mapHeader->name);
                        allowNormalVictory = false; // makes sense? Not much. Works as H3? Yes!
                    }
                }

                switch (vicCondition)
                {
                    case EVictoryConditionType.ARTIFACT:
                        {
                            int objectType = reader.ReadByte();
                            if (mapObject.Header.Version != EMapFormat.ROE)
                            {
                                reader.ReadByte();
                            }
                            break;
                        }
                    case EVictoryConditionType.GATHERTROOP:
                        {
                            int objectType = reader.ReadByte();
                            uint value = reader.ReadUInt32();
                            if (mapObject.Header.Version != EMapFormat.ROE)
                            {
                                reader.ReadByte();
                            }
                            reader.ReadUInt32();
                            break;
                        }
                    case EVictoryConditionType.GATHERRESOURCE:
                        {
                            int objectType = reader.ReadByte();
                            uint value = reader.ReadUInt32();
                            break;
                        }
                    case EVictoryConditionType.BUILDCITY:
                        {
                            var pos = reader.ReadPosition();
                            int objectType = reader.ReadByte();
                            int objectType2 = reader.ReadByte();

                            break;
                        }
                    case EVictoryConditionType.BUILDGRAIL:
                        {
                            var pos = reader.ReadPosition();
                            break;
                        }
                    case EVictoryConditionType.BEATHERO:
                        {
                            var pos = reader.ReadPosition();
                            break;
                        }
                    case EVictoryConditionType.CAPTURECITY:
                        {
                            var pos = reader.ReadPosition();
                            break;
                        }
                    case EVictoryConditionType.BEATMONSTER:
                        {
                            var pos = reader.ReadPosition();
                            break;
                        }
                    case EVictoryConditionType.TAKEDWELLINGS:
                        {
                            break;
                        }
                    case EVictoryConditionType.TAKEMINES:
                        {
                            break;
                        }
                    case EVictoryConditionType.TRANSPORTITEM:
                        {
                            byte value = reader.ReadByte();
                            var pos = reader.ReadPosition();
                            break;
                        }
                    default:
                        break;
                }
            }

            ELossConditionType loseCondition = (ELossConditionType)reader.ReadByte();
            logger.LogTrace("Lose Condition:" + loseCondition);
            if (loseCondition != ELossConditionType.LOSSSTANDARD)
            {
                switch (loseCondition)
                {
                    case ELossConditionType.LOSSCASTLE:
                        {
                            var pos = reader.ReadPosition();
                            break;
                        }
                    case ELossConditionType.LOSSHERO:
                        {
                            var pos = reader.ReadPosition();
                            break;
                        }
                    case ELossConditionType.TIMEEXPIRES:
                        {
                            int val = reader.ReadUInt16();
                            break;
                        }
                    default:
                        break;
                }
            }
        }

        private void ReadTeamInfo(BinaryReader reader)
        {
            int howManyTeams = reader.ReadByte();
            logger.LogTrace("How Many Teams: " + howManyTeams);
            if (howManyTeams > 0)
            {
                for (int i = 0; i < GameConstants.PLAYER_LIMIT_T; i++)
                {
                    int team = reader.ReadByte();
                }
            }
            else
            {
                for (int i = 0; i < GameConstants.PLAYER_LIMIT_T; i++)
                {

                }
            }
        }


        private void ReadAllowedHeroes(BinaryReader reader)
        {
            // in HOTA, there seems a UINT32 here
            if (mapObject.Header.Version == EMapFormat.HOTA)
            {
                reader.ReadUInt32();
            }

            int byteCount = 20; //// mapHeader->version == EMapFormat::ROE ? 16 : 20;
            if (mapObject.Header.Version == EMapFormat.ROE)
            {
                byteCount = 16;
            }

            HashSet<int> allowedHeroSet = new HashSet<int>();
            reader.ReadBitMask(allowedHeroSet, byteCount, GameConstants.HEROES_QUANTITY, false);

            // in HOTA, there seems 3 bytes unknown
            if (mapObject.Header.Version == EMapFormat.HOTA)
            {
                reader.ReadBytes(3);
            }

            // Probably reserved for further heroes
            if (mapObject.Header.Version > EMapFormat.ROE)
            {
                uint placeholdersQty = reader.ReadUInt32();

                reader.Skip((int)placeholdersQty);

                //		std::vector<ui16> placeholdedHeroes;
                //
                //		for(int p = 0; p < placeholdersQty; ++p)
                //		{
                //			placeholdedHeroes.push_back(reader.ReadByte());
                //		}
            }

        }

        private void ReadDisposedHeroes(BinaryReader reader)
        {
            mapObject.DisposedHeroes = new List<DisposedHero>();

            if (mapObject.Header.Version >= EMapFormat.SOD)
            {
                int disp = reader.ReadByte();
                logger.LogTrace("ReadDisposedHeroes: Total=" + disp);
                for (int g = 0; g < disp; ++g)
                {
                    uint heroId = reader.ReadByte();
                    ushort portrait = reader.ReadByte();
                    string name = reader.ReadStringWithLength();
                    byte players = reader.ReadByte();
                    logger.LogTrace(string.Format("ReadDisposedHeroes: id={0} portrait={1} name={2} players={3}", heroId, portrait, name, players));

                    DisposedHero disHero = new DisposedHero();
                    disHero.HeroId = heroId;
                    disHero.Portrait = portrait;
                    disHero.Name = name;
                    disHero.Players = players;

                    mapObject.DisposedHeroes.Add(disHero);
                }
            }

            //omitting NULLS
            reader.Skip(31);
        }

        private void ReadAllowedArtifacts(BinaryReader reader)
        {
            if (mapObject.Header.Version != EMapFormat.ROE)
            {
                int bytes = (mapObject.Header.Version == EMapFormat.AB ? 17 : 18);

                HashSet<int> allowedList = new HashSet<int>();
                reader.ReadBitMask(allowedList, bytes, GameConstants.ARTIFACTS_QUANTITY);
            }

            if (mapObject.Header.Version == EMapFormat.ROE || mapObject.Header.Version == EMapFormat.AB)
            {

            }
        }

        private void ReadAllowedSpellsAbilities(BinaryReader reader)
        {
            if (mapObject.Header.Version >= EMapFormat.SOD)
            {
                HashSet<int> allowedSpells = new HashSet<int>();
                HashSet<int> allowedSkills = new HashSet<int>();

                // Reading allowed spells (9 bytes)
                const int spell_bytes = 9;
                reader.ReadBitMask(allowedSpells, spell_bytes, GameConstants.SPELLS_QUANTITY);
                logger.LogTrace("allowedSpells: " + JsonConvert.SerializeObject(allowedSpells));


                // Allowed hero's abilities (4 bytes)
                const int skill_bytes = 4;
                reader.ReadBitMask(allowedSkills, skill_bytes, GameConstants.SKILL_QUANTITY);
                logger.LogTrace("allowedSkills: " + JsonConvert.SerializeObject(allowedSkills));
            }
        }

        private void ReadRumors(BinaryReader reader)
        {
            uint rumNr = reader.ReadUInt32();
            logger.LogTrace("Rumor count: " + rumNr);

            mapObject.Rumors = new List<Rumor>();
            for (int it = 0; it < rumNr; it++)
            {
                string name = reader.ReadStringWithLength();
                string text = reader.ReadStringWithLength();
                logger.LogTrace(string.Format("Rumor: name={0} text={1}", name, text));

                Rumor rumor = new Rumor();
                rumor.Name = name;
                rumor.Text = text;
                mapObject.Rumors.Add(rumor);
            }
        }

        private void ReadPredefinedHeroes(BinaryReader reader)
        {
            mapObject.PredefinedHeroes = new List<HeroInstance>();
            if (mapObject.Header.Version == EMapFormat.WOG 
                || mapObject.Header.Version == EMapFormat.SOD
                || mapObject.Header.Version == EMapFormat.HOTA)
            {

                int heroCount = GameConstants.HEROES_QUANTITY;
                if (this.mapObject.Header.Version == EMapFormat.HOTA)
                {
                    heroCount += 19;
                }

                for (int z = 0; z < heroCount; z++)
                {
                    logger.LogTrace(string.Format("===Reading Predefined Hero [{0}]", z));

                    int custom = reader.ReadByte();
                    if (custom == 0)
                    {
                        logger.LogTrace("is not custom.");
                        continue;
                    }

                    // Create Hero
                    HeroInstance hero = new HeroInstance();
                    hero.ObjectType = EObjectType.HERO;
                    hero.SubId = z;

                    bool hasExp = reader.ReadBoolean();
                    if (hasExp)
                    {
                        hero.Data.Experience = (int)reader.ReadUInt32();
                        logger.LogTrace("Has exp:" + hero.Data.Experience);
                    }

                    hero.Data.SecondarySkills = new List<AbilitySkill>();
                    bool hasSecondSkills = reader.ReadBoolean();
                    if (hasSecondSkills)
                    {
                        uint howMany = reader.ReadUInt32();
                        logger.LogTrace("Has Second Skills count=" + howMany);

                        for (int yy = 0; yy < howMany; ++yy)
                        {
                            int first = reader.ReadByte();
                            int second = reader.ReadByte();
                            logger.LogTrace(string.Format("Skill First: {0} Second: {1}", first, second));
                            AbilitySkill skill = new AbilitySkill((ESecondarySkill)first, (ESecondarySkillLevel)second);
                            hero.Data.SecondarySkills.Add(skill);
                        }
                    }

                    // Set Artifacts
                    CGHeroReader.LoadArtifactsOfHero(reader, mapObject, hero);

                    bool hasCustomBio = reader.ReadBoolean();
                    if (hasCustomBio)
                    {
                        string biography = reader.ReadStringWithLength();
                        logger.LogTrace("biography: " + biography);

                        hero.Data.Biography = biography;
                    }

                    byte sex = reader.ReadByte();
                    logger.LogTrace("sex: " + sex);
                    hero.Data.Sex = sex;

                    // Spells
                    bool hasCustomSpells = reader.ReadBoolean();
                    if (hasCustomSpells)
                    {
                        HashSet<int> spells = new HashSet<int>();
                        reader.ReadBitMask(spells, 9, GameConstants.SPELLS_QUANTITY, false);
                        logger.LogTrace("Spells: " + JsonConvert.SerializeObject(spells));

                        hero.Data.Spells = new List<ESpellId>();
                        foreach (int spell in spells)
                        {
                            hero.Data.Spells.Add((ESpellId)spell);
                        }
                    }

                    bool hasCustomPrimSkills = reader.ReadBoolean();
                    if (hasCustomPrimSkills)
                    {
                        logger.LogTrace("Has Custom Primary Skills.");

                        hero.Data.PrimarySkills = new List<int>();
                        for (int xx = 0; xx < GameConstants.PRIMARY_SKILLS; xx++)
                        {
                            int value = reader.ReadByte();
                            logger.LogTrace("Primary Skills: " + value);
                            hero.Data.PrimarySkills.Add(value);
                        }
                    }

                    mapObject.PredefinedHeroes.Add(hero);
                }
            }
        }
        
        private void ReadTerrain(BinaryReader reader)
        {
            int mapLevel = mapObject.Header.IsTwoLevel ? 2 : 1;
            uint mapHeight = mapObject.Header.Height;
            uint mapWidth = mapObject.Header.Width;

            mapObject.TerrainTiles = new TerrainTile[mapLevel, mapWidth, mapHeight];
            for (int a = 0; a < mapLevel; a++)
            {
                for (int yy = 0; yy < mapHeight; yy++)
                {
                    for (int xx = 0; xx < mapWidth; xx++)
                    {
                        byte terrianType = reader.ReadByte();
                        byte terrianView = reader.ReadByte();
                        byte riverType = reader.ReadByte();
                        byte riverDir = reader.ReadByte();
                        byte roadType = reader.ReadByte();
                        byte roadDir = reader.ReadByte();
                        byte extTileFlags = reader.ReadByte();

                        TerrainTile tile = new TerrainTile();
                        tile.TerrainType = (ETerrainType)terrianType;
                        tile.TerrainView = terrianView;
                        tile.RiverType = (ERiverType)riverType;
                        tile.RiverDir = riverDir;
                        tile.RoadType = (ERoadType)roadType;
                        tile.RoadDir = roadDir;
                        tile.SetExtTileFlags(extTileFlags);

                        tile.IsBlocked = (tile.TerrainType == ETerrainType.ROCK || tile.TerrainType == ETerrainType.BORDER); //underground tiles are always blocked
                        tile.IsVisitable = false;

                        mapObject.TerrainTiles[a, xx, yy] = tile;
                    }
                }
            }
        }

        private void ReadObjectTemplates(BinaryReader reader)
        {
            uint templateCount = reader.ReadUInt32();
            logger.LogTrace("ReadObjectTemplates totally:" + (int)templateCount);

            this.mapObject.ObjectTemplates = new List<ObjectTemplate>((int)templateCount);

            List<string> values = new List<string>();
            
            // Read custom defs
            for (int idd = 0; idd < templateCount; ++idd)
            {
                ObjectTemplate objectTemplate = new ObjectTemplate();

                objectTemplate.AnimationFile = reader.ReadStringWithLength();
                //// logger.LogTrace("Object Animation File:" + objectTemplate.AnimationFile);

                if (objectTemplate.Type == EObjectType.TOWN)
                {
                    int here = 1;
                }

                objectTemplate.BlockMask = ReadTileMask(reader, 6, true);
                objectTemplate.VisitMask = ReadTileMask(reader, 6, false);
                if (objectTemplate.VisitMask.Length > 1)
                {
                    int here = 10;
                }

                reader.ReadUInt16();
                int terrainMask = reader.ReadUInt16();

                objectTemplate.Type = (EObjectType)reader.ReadUInt32();
                objectTemplate.SubId = (int)reader.ReadUInt32();

                //// logger.LogTrace(string.Format("Object Type: {0} SubId: {1}", objectTemplate.Type, objectTemplate.SubId));
                
                // This type is not the template type, used in isOnVisitableFromTopList
                int type = reader.ReadByte();
                int printPriority = reader.ReadByte() * 100;

                reader.Skip(16);

                this.mapObject.ObjectTemplates.Add(objectTemplate);
            }

            values.Sort();
            foreach(var value in values)
            {
                logger.LogTrace(value);
            }

        }

        private void ReadObjects(BinaryReader reader)
        {
            int objectCount = (int)reader.ReadUInt32();
            logger.LogTrace(string.Format("Totally {0} objects.", objectCount));

            this.mapObject.Objects = new List<CGObject>(objectCount);

            for (int ww = 0; ww < objectCount; ww ++)
            {
                int objectId = this.mapObject.Objects.Count();

                /*
                if (objectId >= 2351)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        byte[] data = reader.ReadBytes(10);
                        logger.LogTrace(StringUtils.ByteArrayToString(data));
                    }

                    reader.BaseStream.Seek(-200, SeekOrigin.Current);
                }
                */

                MapPosition objectPosition = reader.ReadPosition();
                if (objectPosition.Level != 0 && objectPosition.Level != 1)
                {
                    throw new InvalidDataException("MapPosition data is not correct.");
                }

                int objectTemplateIndex = (int)reader.ReadUInt32();

                ObjectTemplate objTemplate = mapObject.ObjectTemplates[objectTemplateIndex];
                reader.Skip(5);

                MapObjectReader objectReader = MapObjectReaderFactory.GetObjectReader(objTemplate.Type);
                if (objTemplate.Type == EObjectType.TOWN)
                {
                    int here = 1;
                }

                CGObject resultObject = null;
                if (objectReader != null)
                {
                    objectReader.Map = this.mapObject;
                    objectReader.MapHeader = this.mapObject.Header;
                    objectReader.ObjectTemplate = objTemplate;

                    resultObject = objectReader.ReadObject(reader, objectId, objectPosition);
                    if (resultObject == null)
                    {
                        continue;
                    }
                }
                else
                {
                    // Normal Object, load from JSON
                    resultObject = new CGObject();
                }

                resultObject.Position = objectPosition;
                resultObject.Identifier = (uint)objectId;
                resultObject.Template = objTemplate;

                if (resultObject.Template.Type != EObjectType.HERO && resultObject.Template.Type != EObjectType.HERO_PLACEHOLDER && resultObject.Template.Type != EObjectType.PRISON)
                {
                    resultObject.SubId = resultObject.Template.SubId;
                }

                resultObject.InstanceName = string.Format("{0}_{1}", resultObject.Identifier, resultObject.Template.Type);
                logger.LogTrace(string.Format(@"Readed object {0}, Position: [{1}, {2}, {3}]", resultObject.InstanceName, objectPosition.PosX, objectPosition.PosY, objectPosition.Level));
                //// Console.WriteLine(string.Format(@"Readed object {0}, Position: [{1}, {2}, {3}]", resultObject.InstanceName, objectPosition.PosX, objectPosition.PosY, objectPosition.Level));

                mapObject.Objects.Add(resultObject);
            }
        }
        
        private void ReadEvents(BinaryReader reader)
        {
            mapObject.Events = new List<MapEvent>();

            uint numberOfEvents = reader.ReadUInt32();
            for (int yyoo = 0; yyoo < numberOfEvents; ++yyoo)
            {
                MapEvent mEvent = new MapEvent();
                mEvent.Name = reader.ReadStringWithLength();
                mEvent.Message = reader.ReadStringWithLength();

                mEvent.Resources = MapObjectReader.ReadResources(reader);
                mEvent.Players = reader.ReadByte();
                if (mapObject.Header.Version > EMapFormat.AB)
                {
                    mEvent.HumanAffected = reader.ReadByte();
                }
                else
                {
                    mEvent.HumanAffected = 0xff;
                }

                mEvent.ComputerAffected = reader.ReadByte();
                mEvent.FirstOccurence = reader.ReadUInt16();
                mEvent.NextOccurence = reader.ReadByte();

                reader.Skip(17);

                logger.LogTrace(string.Format("Map Event: {0} Name={1} Message={2}", yyoo, mEvent.Name, mEvent.Message));

                mapObject.Events.Add(mEvent);
            }
        }

        /// <summary>
        /// Update part of the data according to data alignment and conflict
        /// </summary>
        private void ConsolidateAndAdjustData()
        {
            // 1. Town should not allow Spells that the Map doesn't allow


            // 2. Random artifact should not generate the ones that CQuest will give


        }

        /// <summary>
        /// Load the MASK file for each Object Template
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        private bool[] ReadTileMask(BinaryReader reader, int byteCount, bool negate)
        {
            int valueCount = byteCount * 8;

            bool[] blockMask = new bool[valueCount];
            reader.ReadBitMask(blockMask, byteCount, valueCount, negate);

            int firstIndex = 0;
            while(firstIndex < valueCount && blockMask[firstIndex] == false)
            {
                firstIndex++;
            }

            int count = valueCount - firstIndex;
            bool[] result = new bool[count];
            for(int i = 0; i < count; i++)
            {
                result[i] = blockMask[valueCount - 1 - i];
            }

            return result;
        }
    }
}
