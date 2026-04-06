// Migrated from VCMI lib/entities/hero/CHero.h
// Represents the static hero type definition (template / blueprint).
// HeroSpecialty and InitialArmyStack moved here from H3Hero.cs.

using H3Engine.Common;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// Describes a hero's specialty (e.g. creature mastery, resource production).
    /// Migrated from H3Hero.cs; corresponds to CHero::specialty in VCMI.
    /// </summary>
    public class HeroSpecialty
    {
        public ESpecialtyType Type
        {
            get; set;
        }

        public int SubType
        {
            get; set;
        }
    }

    /// <summary>
    /// One slot of a hero type's starting army.
    /// Corresponds to CHero::InitialArmyStack in VCMI.
    /// </summary>
    public class InitialArmyStack
    {
        public ECreatureId Creature
        {
            get; set;
        }

        public int MinAmount
        {
            get; set;
        }

        public int MaxAmount
        {
            get; set;
        }
    }

    /// <summary>
    /// Static hero type definition – loaded once from config/h3m data and never mutated
    /// during gameplay.  Corresponds to VCMI's CHero class.
    ///
    /// Runtime hero state (experience, skills, army, etc.) lives in H3Hero / HeroInstance.
    /// </summary>
    public class HeroType
    {
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

        public string Biography
        {
            get; set;
        }

        /// <summary>
        /// Index into the hero portrait / image table.
        /// Corresponds to CHero::imageIndex.
        /// </summary>
        public int ImageIndex
        {
            get; set;
        }

        /// <summary>
        /// Default gender for this hero type.
        /// A HeroInstance may override this with EHeroGender.MALE/FEMALE.
        /// </summary>
        public EHeroGender Gender
        {
            get; set;
        } = EHeroGender.MALE;

        /// <summary>
        /// Reference to this hero's class (e.g. Knight, Ranger, Warlock…).
        /// </summary>
        public HeroClass HeroClass
        {
            get; set;
        }

        /// <summary>
        /// Starting army composition.  Randomised between MinAmount and MaxAmount at game start.
        /// </summary>
        public List<InitialArmyStack> InitialArmy
        {
            get; set;
        }

        /// <summary>
        /// Secondary skills the hero begins the game with.
        /// </summary>
        public List<AbilitySkill> SecSkillsInit
        {
            get; set;
        }

        /// <summary>
        /// Hero's specialty (creature mastery, resource bonus, skill boost, etc.).
        /// </summary>
        public HeroSpecialty Specialty
        {
            get; set;
        }

        /// <summary>
        /// Spells the hero knows from the start (independent of spell book).
        /// </summary>
        public List<ESpellId> Spells
        {
            get; set;
        }

        public bool HaveSpellBook
        {
            get; set;
        }

        /// <summary>
        /// Campaign-only hero – will not appear in random maps or taverns unless placed explicitly.
        /// Corresponds to CHero::special.
        /// </summary>
        public bool IsSpecial
        {
            get; set;
        }

        // --- Graphics ---

        public string PortraitSmall
        {
            get; set;
        }

        public string PortraitLarge
        {
            get; set;
        }

        public string BattleImage
        {
            get; set;
        }
    }
}
