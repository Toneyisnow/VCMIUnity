using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Campaign
{

    public class ScenarioPrologEpilog
    {
        public bool HasPrologEpilog
        {
            get; set;
        }

        public byte VideoId
        {
            get; set;
        }

        public byte MusicId
        {
            get; set;
        }

        public string Text
        {
            get; set;
        }

    }

    public class ScenarioTravelOption
    {
        public static readonly int MONSTERS_KEPT_BY_HERO_COUNT = 19;
        public static readonly int ARTIFACTS_KEPT_BY_HERO_COUNT = 18;

        public ScenarioTravelOption()
        {
            this.BonusChoices = new List<ScenarioTravelBonus>();
        }

        public byte WhatHeroKeeps
        {
            get; set;
        }

        public byte[] MonstersKeptByHero
        {
            get; set;
        }

        public byte[] ArtifactsKeptByHero
        {
            get; set;
        }

        public byte StartOptions
        {
            get; set;
        }

        public byte PlayerColor
        {
            get; set;
        }

        public List<ScenarioTravelBonus> BonusChoices
        {
            get; set;
        }


    }

    public class ScenarioTravelBonus
    {
        public enum EBonusType
        {
            SPELL, MONSTER, BUILDING, ARTIFACT, SPELL_SCROLL, PRIMARY_SKILL, SECONDARY_SKILL, RESOURCE,
            HEROES_FROM_PREVIOUS_SCENARIO, HERO
        };

        public ScenarioTravelBonus()
        {

        }

        public EBonusType Type
        {
            get; set;
        }

        public int Info1
        {
            get; set;
        }

        public int Info2
        {
            get; set;
        }

        public int Info3
        {
            get; set;
        }

        public bool IsBonusForHero()
        {
            return false;
        }
        
    }


    public class CampaignScenario
    {
        public CampaignScenario()
        {

        }

        public bool Conquered
        {
            get;set;
        }

        public string MapName
        {
            get; set;
        }

        public uint PackedMapSize
        {
            get; set;
        }

        public int Difficulty
        {
            get; set;
        }

        public byte RegionColor
        {
            get; set;
        }

        public string RegionText
        {
            get; set;
        }

        public ScenarioPrologEpilog Prolog
        {
            get; set;
        }

        public ScenarioPrologEpilog Epilog
        {
            get; set;
        }

        public ScenarioTravelOption TravelOptions
        {
            get; set;
        }

        public void LoadPreconditionRegions(int count)
        {

        }
    }
}
