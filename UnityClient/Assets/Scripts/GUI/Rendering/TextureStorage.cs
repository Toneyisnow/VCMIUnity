using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using H3Engine.Common;
using H3Engine.FileSystem;

namespace UnityClient.GUI.Rendering
{
    /// <summary>
    /// This is legacy, using BundleImageSheet instead
    /// </summary>
    public class TextureStorage
    {
        private static TextureStorage instance = null;

        private Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

        private Dictionary<string, TextureAtlas> textureAtlases = new Dictionary<string, TextureAtlas>();

        private Dictionary<string, TextureSet> textureSets = new Dictionary<string, TextureSet>();

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

        public Texture2D LoadTextureFromImage(string textureKey, ImageData imageData)
        {
            Texture2D texture = GetTexture(textureKey);
            if (texture == null)
            {
                texture = Texture2DExtension.LoadFromData(imageData);
                textureDict[textureKey] = texture;
            }

            return texture;
        }

        //////////////// Public Methods for Usage ////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terrainType"></param>
        /// <param name="terrainIndex"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>

        public Sprite LoadTerrainSprite(ETerrainType terrainType, byte terrainIndex, byte rotation)
        {
            string textureSetKey = string.Format(@"TL{0}", terrainType.ToString());
            
            if (!textureSets.ContainsKey(textureSetKey))
            {
                textureSets[textureSetKey] = new MapTileTextureSet(ETileType.Terrain, terrainType.GetHashCode());
            }

            TextureSet textureSet = textureSets[textureSetKey];
            return textureSet.RetrieveSprite(MapTileTextureSet.TextureKey(terrainIndex, rotation));
        }

        public Sprite LoadRoadSprite(ERoadType roadType, byte roadIndex, byte rotation)
        {
            string textureSetKey = string.Format(@"RD{0}", roadType.ToString());

            if (!textureSets.ContainsKey(textureSetKey))
            {
                textureSets[textureSetKey] = new MapTileTextureSet(ETileType.Road, roadType.GetHashCode());
            }

            TextureSet textureSet = textureSets[textureSetKey];
            return textureSet.RetrieveSprite(MapTileTextureSet.TextureKey(roadIndex, rotation));
        }

        public Sprite LoadRiverSprite(ERiverType riverType, byte riverIndex, byte rotation)
        {
            string textureSetKey = string.Format(@"RV{0}", riverType.ToString());

            if (!textureSets.ContainsKey(textureSetKey))
            {
                textureSets[textureSetKey] = new MapTileTextureSet(ETileType.River, riverType.GetHashCode());
            }

            TextureSet textureSet = textureSets[textureSetKey];
            return textureSet.RetrieveSprite(MapTileTextureSet.TextureKey(riverIndex, rotation));
        }

        /// <summary>
        /// Each Artifact might have an animation sprites
        /// </summary>
        /// <param name="defFileName"></param>
        /// <returns></returns>
        public Sprite[] LoadMapArtifactSprites(string defFileName)
        {
            string textureSetKey = @"AVA";
            if (!textureSets.ContainsKey(textureSetKey))
            {
                textureSets[textureSetKey] = new MapArtifactTextureSet();
            }

            TextureSet textureSet = textureSets[textureSetKey];
            List<Sprite> sprites = new List<Sprite>();

            int animationIndex = 0;

            Sprite sprite = null;
            while ((sprite = textureSet.RetrieveSprite(MapArtifactTextureSet.TextureKey(defFileName, animationIndex))) != null)
            {
                sprites.Add(sprite);
                animationIndex++;
            }

            return sprites.ToArray();
        }

        public Sprite LoadMapDecorationSprite(string defFileName, int typeId)
        {
            return null;
        }
    }
}
