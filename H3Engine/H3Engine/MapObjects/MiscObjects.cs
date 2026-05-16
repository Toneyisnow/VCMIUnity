using H3Engine.Common;
using H3Engine.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.MapObjects
{
    /// <summary>
    /// Reward tier for a treasure chest: pairs of gold and experience amounts.
    /// Mirrors VCMI rewardablePickable.json tiers for TREASURE_CHEST.
    /// </summary>
    public struct TreasureChestReward
    {
        public int Gold;
        public int Experience;
    }

    /// <summary>
    /// Treasure chest on the adventure map (EObjectType.TREASURE_CHEST = 101).
    ///
    /// When a hero visits the chest the player is shown a dialog to choose
    /// either a gold reward or an experience reward.  The chest disappears
    /// after the player makes their choice.
    ///
    /// Reward tiers (matching VCMI rewardablePickable.json weights):
    ///   Tier 1 – 1 000 gold  or  500 experience
    ///   Tier 2 – 1 500 gold  or  1 000 experience
    ///   Tier 3 – 2 000 gold  or  1 500 experience
    ///
    /// Corresponds to VCMI CRewardableObject / Rewardable::Interface with
    /// selectMode = SELECT_PLAYER.
    /// </summary>
    public class CGTreasureChest : CGObject
    {
        private static readonly TreasureChestReward[] RewardTiers = new TreasureChestReward[]
        {
            new TreasureChestReward { Gold = 1000, Experience = 500  },
            new TreasureChestReward { Gold = 1500, Experience = 1000 },
            new TreasureChestReward { Gold = 2000, Experience = 1500 },
        };

        /// <summary>
        /// True once the chest has been picked up; prevents double-pickup.
        /// </summary>
        public bool IsPickedUp { get; set; }

        /// <summary>
        /// Gold the player may choose when opening this chest.
        /// </summary>
        public int RewardGold { get; private set; }

        /// <summary>
        /// Experience the player may choose when opening this chest.
        /// </summary>
        public int RewardExperience { get; private set; }

        /// <param name="identifier">
        /// CGObject identifier used as PRNG seed so the same chest always
        /// offers the same tier on every game load.
        /// </param>
        public CGTreasureChest(uint identifier)
        {
            // Hero visits from an adjacent tile — chest tile blocks movement.
            BlockVisit = true;

            // Deterministic tier selection seeded by object identifier.
            int tier = (int)(identifier % (uint)RewardTiers.Length);
            RewardGold       = RewardTiers[tier].Gold;
            RewardExperience = RewardTiers[tier].Experience;
        }
    }


    public class ITeamVisited : CGObject
    {

    }

    public class CGSignBottle : CGObject
    {
        public string Message
        {
            get; set;
        }

    }

    public class CGWitchHut : CGObject
    {
        public CGWitchHut()
        {
            this.AllowedAbilities = new List<int>();
        }

        public List<int> AllowedAbilities
        {
            get; set;
        }

    }

    public class CGScholar : CGObject
    {
        public enum EBonusType
        {
            PrimarySkill = 0,
            SecondarySkill = 1,
            Spell = 2,
            Random = 255
        }

        public EBonusType BonusType
        {
            get; set;
        }

        public int BonusId
        {
            get; set;
        }
    }

    public class CGGarrison : CGObject
    {
        public bool RemovableUnits
        {
            get; set;
        }
    }

    public class CGArtifact : ArmedInstance
    {
        /// <summary>
        /// The artifact type placed on the map.
        /// Corresponds to CGArtifact::storedArtifact (ArtifactInstanceID).
        /// </summary>
        public EArtifactId ArtifactId
        {
            get; set;
        }

        /// <summary>
        /// Set to true once the artifact has been picked up; prevents double-pickup.
        /// </summary>
        public bool IsPickedUp
        {
            get; set;
        }

        public CGArtifact(EArtifactId artId)
        {
            ArtifactId = artId;
            // Artifacts are visited from an adjacent tile — hero never steps on the artifact tile.
            // Mirrors VCMI CGArtifact::initObj() setting blockVisit = true.
            BlockVisit = true;
        }

        /// <summary>
        /// Called when a hero visits (steps adjacent to) this artifact.
        /// Adds the artifact to the hero's backpack and marks it as picked up.
        /// The artifact object must be removed from the map by the caller.
        ///
        /// Placeholder for future event hooks (guards, triggered events, etc.).
        /// Corresponds to VCMI CGArtifact::onHeroVisit() → pick().
        /// </summary>
        public void OnHeroVisit(H3Engine.MapObjects.HeroInstance hero)
        {
            if (IsPickedUp) return;
            IsPickedUp = true;

            // TODO: placeholder for artifact-specific events (guards, messages, etc.)

            // Add to hero's backpack
            if (hero?.Data?.Artifacts != null)
            {
                hero.Data.Artifacts.AddToBackpack(ArtifactId);
            }
        }
    }

    public class CGResource : ArmedInstance
    {
        public int Amount
        {
            get; set;
        }

        /// <summary>
        /// Set to true once the resource has been picked up; prevents double-pickup.
        /// </summary>
        public bool IsPickedUp
        {
            get; set;
        }

        public CGResource()
        {
            // Resources are visited from an adjacent tile — hero never steps on the resource tile.
            // Mirrors VCMI CGResource::initObj() setting blockVisit = true.
            BlockVisit = true;
        }

        /// <summary>
        /// The resource type derived from the object template's SubId (set after map loading).
        /// </summary>
        public EResourceType ResourceType =>
            Template != null ? (EResourceType)Template.SubId : EResourceType.GOLD;

        /// <summary>
        /// Called when a hero visits (steps adjacent to) this resource.
        /// Adds the resource amount to the hero's collected resources and marks it as picked up.
        /// The resource object must be removed from the map by the caller.
        ///
        /// Corresponds to VCMI CGResource::onHeroVisit() → collectRes().
        /// </summary>
        public void OnHeroVisit(H3Engine.MapObjects.HeroInstance hero)
        {
            if (IsPickedUp) return;
            IsPickedUp = true;

            if (hero?.Data != null)
            {
                hero.Data.Resources.AddAmount(ResourceType, Amount);
            }
        }
    }

    public class CGShrine : ITeamVisited
    {
        public ESpellId SpellId
        {
            get; set;
        }

    }

    public class CGShipyard : CGObject
    {

    }

    public class CGMine : ArmedInstance
    {

    }

    public class TeleportChannel
    {

    }

    public class CGTeleport : CGObject
    {

    }

    public class CGMonolith : CGTeleport
    {

    }

    public class CGSubterraneanGate : CGMonolith
    {

    }

    public class CGWhirlpool : CGMonolith
    {

    }

    public class CGMagicWell : CGObject
    {

    }

    public class CGSirens : CGObject
    {

    }

    public class CGObservatory : CGObject
    {

    }

    public class CGBoat : CGObject
    {
       


    }
    public class CGMagi : CGObject
    {

    }

    public class CGCartographer : ITeamVisited
    {

    }

    public class CGDenOfthieves : CGObject
    {

    }

    public class CGObelisk : ITeamVisited
    {

    }

    public class CGLighthouse : CGObject
    {

    }

    public class CGGrail : CGObject
    {
        public uint Radius
        {
            get; set;
        }
    }

}


