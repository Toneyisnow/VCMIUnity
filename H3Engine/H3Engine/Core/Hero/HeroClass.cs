// Migrated from VCMI lib/entities/hero/CHeroClass.h
// Replaces the partial HeroClass definition previously in H3Hero.cs

using H3Engine.Common;
using System.Collections.Generic;

namespace H3Engine.Core
{
    public class HeroClass
    {
        public enum EClassAffinity
        {
            MIGHT,
            MAGIC
        }

        public int Id
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

        /// <summary>
        /// Associated faction (matches EEthnicity / ETownType).
        /// </summary>
        public EEthnicity Faction
        {
            get; set;
        }

        /// <summary>
        /// MIGHT or MAGIC affinity for this hero class.
        /// </summary>
        public EClassAffinity Affinity
        {
            get; set;
        }

        /// <summary>
        /// Default probability of this class appearing in a town tavern.
        /// Actual chance = sqrt(town.chance * heroClass.defaultTavernChance).
        /// </summary>
        public int DefaultTavernChance
        {
            get; set;
        }

        /// <summary>
        /// Initial primary skill values (4 entries: Attack, Defense, Power, Knowledge).
        /// Source: VCMI CHeroClass::primarySkillInitial
        /// </summary>
        public List<int> PrimarySkillInitial
        {
            get; set;
        }

        /// <summary>
        /// Probability (%) of gaining each primary skill on level-up for levels 1-10.
        /// Source: VCMI CHeroClass::primarySkillLowLevel
        /// </summary>
        public List<int> PrimarySkillLowLevel
        {
            get; set;
        }

        /// <summary>
        /// Probability (%) of gaining each primary skill on level-up for levels above 10.
        /// Source: VCMI CHeroClass::primarySkillHighLevel
        /// </summary>
        public List<int> PrimarySkillHighLevel
        {
            get; set;
        }

        /// <summary>
        /// Probability of gaining each secondary skill (out of 112), keyed by skill enum.
        /// Source: VCMI CHeroClass::secSkillProbability
        /// </summary>
        public Dictionary<ESecondarySkill, int> SecSkillProbability
        {
            get; set;
        }

        // --- Graphics ---

        public string ImageBattleMale
        {
            get; set;
        }

        public string ImageBattleFemale
        {
            get; set;
        }

        public string ImageMapMale
        {
            get; set;
        }

        public string ImageMapFemale
        {
            get; set;
        }
    }
}
