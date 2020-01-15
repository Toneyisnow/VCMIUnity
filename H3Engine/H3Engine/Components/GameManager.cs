using System.Collections;
using System.Collections.Generic;
using H3Engine.Components.Data;
using H3Engine.Components.MapProviders;
using H3Engine.Components.Protocols;
using H3Engine.Components.Queries;
using H3Engine.FileSystem;
using H3Engine.Mapping;

namespace H3Engine.Components
{
    public class GameManager : IGameCallback
    {
        private QueryManager queryManager = null;

        private GameData gameData = null;

        private GameMapProvider[] gameMapProviders = null;


        public GameManager()
        {
            this.queryManager = new QueryManager();

        }

        public void LoadFromH3Map(H3Map h3Map)
        {
            for(int i = 0; i < gameData.MapLevelCount; i++)
            {
                gameMapProviders[i] = new GameMapProvider(gameData.MapAtLevel(i));
            }

            
        }

        public void LoadGame(H3Save h3Save)
        {
        }

        public void SaveGame(string h3SaveFileName)
        {

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

        public bool moveHero(object hid, object dst, bool teleporting, bool transit = false)
        {
            int level = 0;
            List<MapPathNode> mapPath = gameMapProviders[level].GetHeroMovePath((int)hid, null);

            HeroMoveQuery query = new HeroMoveQuery((int)hid, mapPath);

            queryManager.AddQuery(query);

            return true;
        }

        public void setManaPoints(object heroId, int points)
        {
            throw new System.NotImplementedException();
        }

        public void setMovePoints(object points)
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }

}
