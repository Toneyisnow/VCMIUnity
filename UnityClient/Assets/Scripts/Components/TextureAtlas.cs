using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine;
using H3Engine.Common;
using H3Engine.FileSystem;
using UnityEngine;

namespace Assets.Scripts.Components
{
    /// <summary>
    /// This is legacy, using TextureSet instead
    /// </summary>
    public abstract class TextureAtlas
    {
        protected Texture2D textureAtlas = null;

        protected Rect[] rects = null;

        protected Engine h3Engine = null;

        private Dictionary<int, Sprite> spriteCache = new Dictionary<int, Sprite>();

        public TextureAtlas()
        {
            h3Engine = Engine.GetInstance();
        }

        public abstract void LoadTextures();

        protected Texture2D GenerateTexture(ImageData imageData, byte rotateIndex = 0)
        {
            byte[] pngData = imageData.GetPNGData(rotateIndex);
            Texture2D texture = new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            texture.LoadImage(pngData);

            return texture;
        }

        public Sprite RetrieveSpriteAt(int textureIndex)
        {
            if (spriteCache.ContainsKey(textureIndex))
            {
                return spriteCache[textureIndex];
            }

            float totalWidth = textureAtlas.width;
            float totalHeight = textureAtlas.height;
            Rect rect = rects[textureIndex];

            Sprite sprite = Sprite.Create(textureAtlas, new Rect(rect.xMin * totalWidth, rect.yMin * totalHeight, rect.width * totalWidth, rect.height * totalHeight), Vector2.zero);
            
            spriteCache[textureIndex] = sprite;
            return sprite;
        }

    }

    public class TilemapTextureAtlas : TextureAtlas
    {
        private ETerrainType terrainType;

        private static Dictionary<ETerrainType, int> TerrainTileCount = null;

        static TilemapTextureAtlas()
        {
            TerrainTileCount = new Dictionary<ETerrainType, int>();

            TerrainTileCount.Add(ETerrainType.WATER, 32);
            TerrainTileCount.Add(ETerrainType.DIRT, 78);
            TerrainTileCount.Add(ETerrainType.GRASS, 32);
            TerrainTileCount.Add(ETerrainType.SAND, 32);

        }

        public static int GetTextureIndex(byte terrainIndex, byte rotate)
        {
            return terrainIndex * 4 + rotate;
        }


        public TilemapTextureAtlas(ETerrainType terrainType)
        {
            this.terrainType = terrainType;
        }

        public override void LoadTextures()
        {
            ImageData[] images = h3Engine.RetrieveAllTerrainImages(terrainType);

            int count = images.Count();
            List<Texture2D> textures = new List<Texture2D>();

            for (int i = 0; i < count; i++)
            {
                for(byte rotate = 0; rotate < 4; rotate++)
                {
                    Texture2D texture = GenerateTexture(images[i], rotate);
                    textures.Add(texture);
                }
            }
            
            textureAtlas = new Texture2D(2048, 2048);
            rects = textureAtlas.PackTextures(textures.ToArray(), 2);

            
        }

    }


}
