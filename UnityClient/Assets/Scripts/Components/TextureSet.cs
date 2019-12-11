using H3Engine;
using H3Engine.Common;
using H3Engine.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Components
{
    /// <summary>
    /// One TextureSet will be corresponding to the DEF file in the H3Engine, it might contain one or more TextureSheets depending on the Game Object Type
    /// </summary>
    public abstract class TextureSet
    {
        protected Engine h3Engine = null;



        public TextureSet()
        {
            h3Engine = Engine.GetInstance();
        }

        protected Texture2D GenerateTexture(ImageData imageData, byte rotateIndex = 0)
        {
            byte[] pngData = imageData.GetPNGData(rotateIndex);
            Texture2D texture = new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            texture.LoadImage(pngData);

            return texture;
        }

        public abstract Sprite RetrieveSprite(int key);

    }

    public enum ETileType
    {
        Terrain,
        Road,
        River
    }

    public class TileMapTextureSet : TextureSet
    {
        private ETileType tileType;

        private int subType;

        private TextureSheet textureSheet = null;

        public static int TextureKey(int index, byte rotate)
        {
            return index * 4 + rotate;
        }

        public TileMapTextureSet(ETileType tileType, int subType)
        {
            this.tileType = tileType;
            this.subType = subType;

            LoadTextures();
        }

        private void LoadTextures()
        {
            textureSheet = new TextureSheet();

            ImageData[] images = null;
            switch (tileType)
            {
                case ETileType.Terrain:
                    images = h3Engine.RetrieveAllTerrainImages((ETerrainType)subType);
                    break;
                case ETileType.Road:
                    images = h3Engine.RetrieveAllRoadImages((ERoadType)subType);
                    break;
                case ETileType.River:
                    images = h3Engine.RetrieveAllRiverImages((ERiverType)subType);
                    break;
                default:
                    break;
            }

            for (int i = 0; i < images.Length; i++)
            {
                for (byte rotate = 0; rotate < 4; rotate++)
                {
                    Texture2D texture =  GenerateTexture(images[i], rotate);
                    textureSheet.AddImageData(TextureKey(i, rotate), texture);
                }
            }

            textureSheet.PackTextures();
        }

        public override Sprite RetrieveSprite(int key)
        {
            return textureSheet.RetrieveSprite(key);
        }
    }
}
