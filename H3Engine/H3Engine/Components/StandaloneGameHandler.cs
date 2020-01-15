using H3Engine.Components.Protocols;
using H3Engine.Mapping;
using System.Collections;

namespace H3Engine.Components
{
    /// <summary>
    /// Handles the callbacks for standalone (Single Player) game.
    /// </summary>
    public class StandaloneGameHandler : IGameCallback, IPriviledgedMapCallback
    {
        private GameManager gameManager = null;

        public StandaloneGameHandler()
        {
            this.gameManager = new GameManager();
        }

        public void LoadFromH3Map(H3Map h3Map)
        {
            this.gameManager.LoadFromH3Map(h3Map);
        }

        /// <summary>
        /// Player decide to move hero to map position
        /// </summary>
        /// <param name="hid"></param>
        /// <param name="dst"></param>
        /// <param name="teleporting"></param>
        /// <param name="transit"></param>
        /// <returns></returns>
        public bool moveHero(object hid, object dst, bool teleporting, bool transit = false)
        {
            return gameManager.moveHero(hid, dst, teleporting, transit);
        }

        public void changePrimSkill(object hero, object which, object val, bool abs = false)
        {
            throw new System.NotImplementedException();
        }

        public void changeSecSkill(object hero, object which, int val, bool abs = false)
        {
            throw new System.NotImplementedException();
        }

        public void changeSpells(object hero, bool give, object spells)
        {
            throw new System.NotImplementedException();
        }

        public void giveHero(object heroId, object playerColor)
        {
            throw new System.NotImplementedException();
        }

        public void giveHeroArtifact(object h, object a, object pos)
        {
            throw new System.NotImplementedException();
        }

        public void giveHeroBonus(int bonus)
        {
            throw new System.NotImplementedException();
        }

        public void giveHeroNewArtifact(object h, object artType, object pos)
        {
            throw new System.NotImplementedException();
        }

        public void heroExchange(object heroId1, object heroId2)
        {
            throw new System.NotImplementedException();
        }

        

        public void setManaPoints(object heroId, int points)
        {
            throw new System.NotImplementedException();
        }

        public void setMovePoints(object points)
        {
            throw new System.NotImplementedException();
        }
    }
}
