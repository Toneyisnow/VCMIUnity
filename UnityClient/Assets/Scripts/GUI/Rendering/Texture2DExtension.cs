using H3Engine.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using HCommon = H3Engine.Common;

namespace UnityClient.GUI.Rendering
{
    public static class Texture2DExtension
    {
        /// <summary>
        /// Pixels per unit for all sprites. H3 tiles are 32x32 pixels,
        /// and we want each tile to occupy 0.32 world units, so PPU = 32 / 0.32 = 100.
        /// </summary>
        public const float PIXELS_PER_UNIT = 100f;

        public static Texture2D LoadFromData(ImageData imageData, byte rotateIndex = 0)
        {
            Texture2D texture = new Texture2D(imageData.Width, imageData.Height, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] color32s = new Color32[imageData.Width * imageData.Height];

            for (int j = 0; j < imageData.Height; j++)
            {
                int baseIndex = (imageData.Height - 1 - j) * imageData.Width;
                for (int i = 0; i < imageData.Width; i++)
                {
                    HCommon.Color sysColor = imageData.GetPixelColor(i, j, rotateIndex);
                    color32s[baseIndex + i] = new UnityEngine.Color32(sysColor.B, sysColor.G, sysColor.R, sysColor.A);

                    if (IsTransparentColor(sysColor))
                    {
                        color32s[baseIndex + i] = UnityEngine.Color.clear;
                    }
                }
            }

            texture.SetPixels32(0, 0, imageData.Width, imageData.Height, color32s);
            texture.Apply();

            return texture;
        }


        public static Color ToColor(HCommon.Color h3Color)
        {
            return new Color((float)h3Color.B / 255, (float)h3Color.G / 255, (float)h3Color.R / 255, (float)h3Color.A / 255);
        }

        public static bool IsTransparentColor(HCommon.Color h3Color)
        {
            return (h3Color.A == 0 && h3Color.R == 0 && h3Color.G == 255 && h3Color.B == 255);
        }

        public static Sprite CreateSpriteFromTexture(Texture2D texture, Vector2? anchorPoint = null)
        {
            if (texture == null)
            {
                return null;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), anchorPoint ?? new Vector2(0.5f, 0.5f), PIXELS_PER_UNIT);
            return sprite;
        }

        public static Sprite CreateSpriteFromImageData(ImageData image, Vector2? anchorPoint = null)
        {
            if (image == null)
            {
                return null;
            }

            Texture2D texture = LoadFromData(image);
            return CreateSpriteFromTexture(texture, anchorPoint);
        }

    }
}
