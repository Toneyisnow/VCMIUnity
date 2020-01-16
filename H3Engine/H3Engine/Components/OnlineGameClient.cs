using H3Engine.Components.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components
{
    public class OnlineGameClient : IPriviledgedMapCallback, IGameCallback
    {
        public void changePrimSkill(object hero, object which, object val, bool abs = false)
        {
            throw new NotImplementedException();
        }

        public void changeSecSkill(object hero, object which, int val, bool abs = false)
        {
            throw new NotImplementedException();
        }

        public void changeSpells(object hero, bool give, object spells)
        {
            throw new NotImplementedException();
        }

        public void giveHero(object heroId, object playerColor)
        {
            throw new NotImplementedException();
        }

        public void giveHeroArtifact(object h, object a, object pos)
        {
            throw new NotImplementedException();
        }

        public void giveHeroBonus(int bonus)
        {
            throw new NotImplementedException();
        }

        public void giveHeroNewArtifact(object h, object artType, object pos)
        {
            throw new NotImplementedException();
        }

        public void heroExchange(object heroId1, object heroId2)
        {
            throw new NotImplementedException();
        }

        public bool moveHero(object hid, object dst, bool teleporting, bool transit = false)
        {
            throw new NotImplementedException();
        }

        public void setManaPoints(object heroId, int points)
        {
            throw new NotImplementedException();
        }

        public void setMovePoints(object points)
        {
            throw new NotImplementedException();
        }
    }
}
