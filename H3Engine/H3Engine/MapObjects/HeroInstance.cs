using H3Engine.Components;
using H3Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.MapObjects
{
    public class CGHeroPlaceHolder : CGObject
    {
        public byte Power
        {
            get; set;
        }

    }

    public class HeroPatrol
    {
        public HeroPatrol()
        {

        }

        public bool IsPatrolling
        {
            get; set;
        }

        public MapPosition InitialPosition
        {
            get; set;
        }

        public uint PatrolRadius
        {
            get; set;
        }


    }

    public class HeroInstance : ArmedInstance
    {
        public HeroInstance(H3Hero data = null)
        {
            if (data == null)
            {
                this.Data = new H3Hero();
            }
            else
            {
                this.Data = data;
            }

            // Initialize rest move points to full effective move points
            RestMovePoint = -1; // -1 means "not yet initialized, use full"
        }

        public H3Hero Data
        {
            get; set;
        }

        /// <summary>
        /// Remaining movement points for the current turn.
        /// Reset to GetEffectiveMovePoint() at the start of each turn.
        /// Value of -1 means uninitialized (treat as full move points).
        /// </summary>
        public int RestMovePoint
        {
            get; set;
        }

        /// <summary>
        /// Get the effective (max) move points for this hero, delegating to H3Hero.
        /// </summary>
        public int GetEffectiveMovePoint()
        {
            return Data.GetEffectiveMovePoint();
        }

        /// <summary>
        /// Get the current remaining move points. If uninitialized, returns full effective move points.
        /// </summary>
        public int GetCurrentMovePoint()
        {
            if (RestMovePoint < 0)
            {
                return GetEffectiveMovePoint();
            }
            return RestMovePoint;
        }

        /// <summary>
        /// Reset move points to full at the start of a new turn.
        /// </summary>
        public void ResetMovePointForNewTurn()
        {
            RestMovePoint = GetEffectiveMovePoint();
        }

        public byte MoveDirection
        {
            get; set;
        }
        
        /*
        public int Mana
        {
            get; set;
        }

        public ArtifactSet ArtifactSet
        {
            get; set;
        }

        public List<H3Spell> Spells
        {
            get; set;
        }

        public HeroSpecialty Specialty
        {
            get; set;
        }
        */

        public HeroPatrol Patrol
        {
            get; set;
        }

        /// Visited
        /// 
        ///
        public List<TownInstance> VisitedTowns
        {
            get; set;
        }

        public List<CGObject> VisitedObjects
        {
            get; set;
        }



    }
}
