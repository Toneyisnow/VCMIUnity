using H3Engine.Common;
using H3Engine.Components;
using H3Engine.Components.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Core
{

    public class HeroSpecialty
    {
        public HeroSpecialty()
        {

        }

        public ESpecialtyType Type
        {
            get; set;
        }

        public int SubType
        {
            get; set;
        }

    }


    public class H3HeroId
    {
        public H3HeroId()
        {

        }

        public int Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }
    }
    

    public class HeroClass
    {
        public enum EClassAffinity
        {
            MIGHT,
            MAGIC
        };
        
        public EEthnicity Ethnicity
        {
            get; set;
        }

        public string Identifier
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }


    }


    public class H3Hero
    {

        public static H3Hero InitializeFromTemplate(int templateId)
        {

            return null;
        }


        public H3Hero()
        {

        }

        public string Identifier
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public int PortaitIndex
        {
            get; set;
        }

        public string Biography
        {
            get; set;
        }

        public byte Sex
        {
            get; set;
        }
    

        public long Experience
        {
            get; set;
        }

        public int Level
        {
            get; set;
        }

        public int MaxExperience
        {
            get; set;
        }

        public int MaxLevel
        {
            get; set;
        }

        /// <summary>
        /// Will constantly be 4 members: Attack, Defence, Power, Intelligence
        /// </summary>
        public List<int> PrimarySkills
        {
            get; set;
        }

        /// <summary>
        /// Key: Skill Id Value: Level of Skill - (1 - basic, 2 - adv., 3 - expert)
        /// </summary>
        public List<AbilitySkill> SecondarySkills
        {
            get; set;
        }

        public List<ESpellId> Spells
        {
            get; set;
        }

        public ArtifactSet Artifacts
        {
            get; set;
        }
        
        public HeroSpecialty Specialty
        {
            get; set;
        }

        public CreatureSet Army
        {
            get; set;
        }

        /// <summary>
        /// Base movement points per turn. Default 1500 (foot heroes).
        /// In H3 this depends on the slowest creature speed in army:
        ///   Speed 0-4: 1500, Speed 5: 1560, Speed 6: 1630, Speed 7: 1700,
        ///   Speed 8: 1760, Speed 9: 1830, Speed 10: 1900, Speed 11+: 2000
        /// </summary>
        public int MovePoint { get; set; } = 1500;

        /// <summary>
        /// Base movement point values indexed by lowest creature speed in hero's army.
        /// Source: VCMI config/gameConfig.json heroes.movementPoints.land
        /// </summary>
        private static readonly int[] MovePointsBySpeed = {
            1500, 1500, 1500, 1500, 1500,  // speed 0-4
            1560, 1630, 1700, 1760, 1830,  // speed 5-9
            1900, 2000                       // speed 10-11+
        };

        /// <summary>
        /// Calculate effective movement points after equipment, skills, and army speed adjustments.
        /// Reference: VCMI lib/pathfinder/TurnInfo.cpp
        /// Formula: baseMovePoints * (100 + logisticsBonus) / 100
        /// Logistics: Basic +10%, Advanced +20%, Expert +30%
        /// </summary>
        public int GetEffectiveMovePoint()
        {
            // Step 1: Determine base move points from army speed
            int baseMovePoints = MovePoint;

            if (Army != null && Army.Stacks != null && Army.Stacks.Count > 0)
            {
                int lowestSpeed = int.MaxValue;
                foreach (var stack in Army.Stacks)
                {
                    if (stack.Creature != null && stack.Creature.Speed > 0 && stack.Creature.Speed < lowestSpeed)
                    {
                        lowestSpeed = stack.Creature.Speed;
                    }
                }

                if (lowestSpeed < int.MaxValue)
                {
                    int speedIndex = Math.Min(lowestSpeed, MovePointsBySpeed.Length - 1);
                    if (speedIndex >= 0)
                    {
                        baseMovePoints = MovePointsBySpeed[speedIndex];
                    }
                }
            }

            // Step 2: Apply Logistics skill bonus (PERCENT_TO_BASE)
            int logisticsPercent = 0;
            if (SecondarySkills != null)
            {
                foreach (var skill in SecondarySkills)
                {
                    if (skill.Id == ESecondarySkill.LOGISTICS)
                    {
                        switch (skill.Level)
                        {
                            case ESecondarySkillLevel.BASIC:    logisticsPercent = 10; break;
                            case ESecondarySkillLevel.ADVANCED: logisticsPercent = 20; break;
                            case ESecondarySkillLevel.EXPERT:   logisticsPercent = 30; break;
                        }
                        break;
                    }
                }
            }

            // Step 3: TODO - Apply artifact bonuses (e.g., Boots of Speed +600 flat)
            // Step 4: TODO - Apply hero specialty bonuses

            int effectivePoints = baseMovePoints * (100 + logisticsPercent) / 100;
            return effectivePoints;
        }

    }
}
