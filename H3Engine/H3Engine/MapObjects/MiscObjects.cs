using H3Engine.Common;
using H3Engine.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.MapObjects
{
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


