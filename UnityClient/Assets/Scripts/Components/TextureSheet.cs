using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Components
{
    public class TextureSheet
    {
        private List<int> textureKeys = null;

        private List<Texture2D> textures = null;
        
        private Dictionary<int, Rect> keysToRects = null;

        private Dictionary<int, Sprite> spriteCache = null;

        private int sheetWidth = 0;

        private int sheetHeight = 0;

        private Texture2D mainTextureSheet = null;


        public TextureSheet()
        {
            textureKeys = new List<int>();
            textures = new List<Texture2D>();

            spriteCache = new Dictionary<int, Sprite>();
        }

        public void AddImageData(int key, Texture2D texture)
        {
            textureKeys.Add(key);
            textures.Add(texture);
        }

        public void PackTextures()
        {
            // Calculate the Height and Width

            int totalCount = textures.Count;

            if (totalCount == 0)
            {
                return;
            }

            int unitHeight = textures[0].height;
            int unitWidth = textures[0].width;

            int multiFactor = (int)Math.Sqrt(totalCount) + 1;

            this.sheetWidth = unitWidth + multiFactor;
            this.sheetHeight = unitHeight + multiFactor;


            mainTextureSheet = new Texture2D(this.sheetWidth, this.sheetHeight);

            Rect[] rects = mainTextureSheet.PackTextures(textures.ToArray(), 0);

            keysToRects = new Dictionary<int, Rect>();
            for(int i = 0; i < rects.Length; i ++)
            {
                keysToRects[textureKeys[i]] = rects[i];
            }

            // Release the memory for the textures
            textures.Clear();
            textureKeys.Clear();
        }

        public Sprite RetrieveSprite(int key)
        {
            if (spriteCache.ContainsKey(key))
            {
                return spriteCache[key];
            }

            float totalWidth = mainTextureSheet.width;
            float totalHeight = mainTextureSheet.height;
            Rect rect = keysToRects[key];

            Sprite sprite = Sprite.Create(mainTextureSheet, new Rect(rect.xMin * totalWidth, rect.yMin * totalHeight, rect.width * totalWidth, rect.height * totalHeight), Vector2.zero);
            spriteCache[key] = sprite;

            return sprite;
        }
    }
}
