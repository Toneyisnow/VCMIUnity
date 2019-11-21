using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class TextureStorage
    {
        private static TextureStorage instance = null;

        private Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();


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

    }
}
