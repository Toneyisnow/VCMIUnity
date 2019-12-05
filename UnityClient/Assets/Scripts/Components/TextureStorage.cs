using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using H3Engine.Common;

namespace Assets.Scripts.Components
{
    public class TextureStorage
    {
        private static TextureStorage instance = null;

        private Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

        private Dictionary<string, TextureAtlas> textureAtlases = new Dictionary<string, TextureAtlas>();


        public static TextureStorage GetInstance()
        {
            if (instance == null)
            {
                instance = new TextureStorage();
            }

            return instance;
        }

        private TextureStorage()
        {

        }

        public void SetTexture(string key, Texture2D texture)
        {
            textureDict[key] = texture;
        }

        public Texture2D GetTexture(string key)
        {
            if (textureDict.ContainsKey(key))
            {
                return textureDict[key];
            }

            return null;
        }

        public Texture2D LoadTextureFromPNGData(string textureKey, byte[] pngData)
        {
            Texture2D texture = GetTexture(textureKey);
            if (texture == null)
            {
                texture = new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                texture.LoadImage(pngData);

                textureDict[textureKey] = texture;
            }

            return texture;
        }

        public Texture2D LoadTerrainTexture(ETerrainType terrainType, byte terrainIndex, byte rotation, byte[] pngData)
        {
            string key = string.Format(@"TL-{0}-{1}-{2}", terrainType.GetHashCode(), terrainIndex, rotation);
            return LoadTextureFromPNGData(key, pngData);
        }

        public Sprite LoadTerrainSprite(ETerrainType terrainType, byte terrainIndex, byte rotation)
        {
            TextureAtlas textureAtlas = null;
            string atlasKey = terrainType.ToString();
            if (textureAtlases.ContainsKey(atlasKey))
            {
                textureAtlas = textureAtlases[atlasKey];
            }
            else
            {
                textureAtlas = new TilemapTextureAtlas(terrainType);
                textureAtlas.LoadTextures();

                textureAtlases[atlasKey] = textureAtlas;
            }

            return textureAtlas.RetrieveSpriteAt(TilemapTextureAtlas.GetTextureIndex(terrainIndex, rotation));
        }

        public Texture2D LoadRoadTexture(ERoadType roadType, byte roadIndex, byte rotation, byte[] pngData)
        {
            string key = string.Format(@"RD-{0}-{1}-{2}", roadType.GetHashCode(), roadIndex, rotation);
            return LoadTextureFromPNGData(key, pngData);
        }

        public Texture2D LoadRiverTexture(ERiverType riverType, byte riverIndex, byte rotation, byte[] pngData)
        {
            string key = string.Format(@"RVR-{0}-{1}-{2}", riverType.GetHashCode(), riverIndex, rotation);
            return LoadTextureFromPNGData(key, pngData);
        }
    }
}
