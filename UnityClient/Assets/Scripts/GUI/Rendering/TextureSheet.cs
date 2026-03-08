using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityClient.GUI.Rendering
{
    public class TextureSheet
    {
        /// <summary>
        /// Unity's recommended maximum atlas size per texture. Keep each atlas within
        /// this limit to avoid silent downscaling by PackTextures.
        /// </summary>
        private const int MAX_ATLAS_SIZE = 2048;

        /// <summary>
        /// Conservative packing efficiency estimate. 2D bin-packing rarely achieves
        /// 100% fill, so we assume 70% to decide how many atlases are needed.
        /// </summary>
        private const float PACKING_EFFICIENCY = 0.70f;

        private List<string> textureKeys = null;

        private List<Texture2D> textures = null;

        // After packing: one entry per atlas
        private List<Texture2D> packedAtlases = null;
        private List<Dictionary<string, Rect>> atlasKeyMaps = null;

        // Maps each key to the atlas index it lives in, for O(1) lookup
        private Dictionary<string, int> keyToAtlasIndex = null;

        private Dictionary<string, Sprite> spriteCache = null;

        /// <summary>Returns the first packed atlas, or null if not yet packed.</summary>
        public Texture2D AtlasTexture { get { return (packedAtlases != null && packedAtlases.Count > 0) ? packedAtlases[0] : null; } }

        public TextureSheet()
        {
            textureKeys = new List<string>();
            textures = new List<Texture2D>();
            spriteCache = new Dictionary<string, Sprite>();
        }

        public void AddImageData(string key, Texture2D texture)
        {
            if (textureKeys.Contains(key))
            {
                return;
            }

            textureKeys.Add(key);
            textures.Add(texture);
        }

        public void PackTextures()
        {
            int totalCount = textures.Count;
            if (totalCount == 0)
            {
                return;
            }

            packedAtlases = new List<Texture2D>();
            atlasKeyMaps = new List<Dictionary<string, Rect>>();
            keyToAtlasIndex = new Dictionary<string, int>();

            List<List<int>> batches = SplitIntoBatches();

            for (int b = 0; b < batches.Count; b++)
            {
                List<int> indices = batches[b];

                Texture2D[] batchTextures = new Texture2D[indices.Count];
                string[] batchKeys = new string[indices.Count];
                for (int i = 0; i < indices.Count; i++)
                {
                    batchTextures[i] = textures[indices[i]];
                    batchKeys[i] = textureKeys[indices[i]];
                }

                Texture2D atlas = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                Rect[] rects = atlas.PackTextures(batchTextures, 1, MAX_ATLAS_SIZE, false);
                atlas.filterMode = FilterMode.Point;
                atlas.wrapMode = TextureWrapMode.Clamp;

                Dictionary<string, Rect> keyMap = new();
                for (int i = 0; i < rects.Length; i++)
                {
                    keyMap[batchKeys[i]] = rects[i];
                    keyToAtlasIndex[batchKeys[i]] = b;
                }

                packedAtlases.Add(atlas);
                atlasKeyMaps.Add(keyMap);
            }

            for (int i = 0; i < packedAtlases.Count; i++)
            {
                // Calculate min/max/avg individual texture dimensions for this batch
                int minW = int.MaxValue, maxW = 0, minH = int.MaxValue, maxH = 0;
                long sumW = 0, sumH = 0;
                foreach (int idx in batches[i])
                {
                    int w = textures[idx].width, h = textures[idx].height;
                    if (w < minW) minW = w; if (w > maxW) maxW = w;
                    if (h < minH) minH = h; if (h > maxH) maxH = h;
                    sumW += w; sumH += h;
                }
                int cnt = batches[i].Count;
                MonoBehaviour.print(string.Format(
                    "TextureSheet: atlas[{0}] actual={1}x{2} ({3} textures) | per-tex W: min={4} max={5} avg={6} | H: min={7} max={8} avg={9}",
                    i, packedAtlases[i].width, packedAtlases[i].height, cnt,
                    minW, maxW, sumW / cnt, minH, maxH, sumH / cnt));
            }
            MonoBehaviour.print(string.Format("TextureSheet: packed {0} textures into {1} atlas(es).", totalCount, packedAtlases.Count));

            // Release staging memory
            textures.Clear();
            textureKeys.Clear();
        }

        public Sprite RetrieveSprite(string key)
        {
            if (spriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            if (keyToAtlasIndex == null || !keyToAtlasIndex.TryGetValue(key, out int atlasIndex))
            {
                return null;
            }

            Texture2D atlas = packedAtlases[atlasIndex];
            Rect rect = atlasKeyMaps[atlasIndex][key];

            float totalWidth = atlas.width;
            float totalHeight = atlas.height;

            Sprite sprite = Sprite.Create(
                atlas,
                new Rect(rect.xMin * totalWidth, rect.yMin * totalHeight, rect.width * totalWidth, rect.height * totalHeight),
                new Vector2(1, 0),
                Texture2DExtension.PIXELS_PER_UNIT);

            spriteCache[key] = sprite;
            return sprite;
        }

        /// <summary>
        /// Splits the staged textures into batches so that each batch's total pixel
        /// area fits within MAX_ATLAS_SIZE² (accounting for packing efficiency).
        /// Textures are distributed sequentially to keep related frames together.
        /// </summary>
        private List<List<int>> SplitIntoBatches()
        {
            long maxBatchArea = (long)(MAX_ATLAS_SIZE * MAX_ATLAS_SIZE * PACKING_EFFICIENCY);

            var batches = new List<List<int>>();
            var currentBatch = new List<int>();
            long currentArea = 0;

            for (int i = 0; i < textures.Count; i++)
            {
                long texArea = (long)textures[i].width * textures[i].height;

                // Start a new batch when adding this texture would exceed the limit,
                // but never leave a batch empty.
                if (currentBatch.Count > 0 && currentArea + texArea > maxBatchArea)
                {
                    batches.Add(currentBatch);
                    currentBatch = new List<int>();
                    currentArea = 0;
                }

                currentBatch.Add(i);
                currentArea += texArea;
            }

            if (currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
            }

            return batches;
        }
    }
}
