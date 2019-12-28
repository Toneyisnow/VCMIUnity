using H3Engine.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using HCommon = H3Engine.Common;

namespace Assets.Scripts.Components
{
    public static class Texture2DExtension
    {
        public static Texture2D LoadFromData1(ImageData imageData, byte rotateIndex = 0)
        {
            Texture2D texture = new Texture2D(imageData.Width, imageData.Height, TextureFormat.ARGB32, false);
            
            for (int j = 0; j < imageData.Height; j++)
            {
                for (int i = 0; i < imageData.Width; i++)
                {
                    HCommon.Color sysColor = imageData.GetPixelColor(i, j, rotateIndex);
                    Color clr = ToColor(sysColor);

                    if (IsTransparentColor(sysColor))
                    {
                        clr = UnityEngine.Color.clear;
                    }

                    texture.SetPixel(i, imageData.Height - j, clr);
                }
            }

            texture.Apply();

            return texture;
        }

        public static Texture2D LoadFromData(ImageData imageData, byte rotateIndex = 0)
        {
            Texture2D texture = new Texture2D(imageData.Width, imageData.Height, TextureFormat.ARGB32, false);

            Color[] colors = new Color[imageData.Width * imageData.Height];

            for (int j = 0; j < imageData.Height; j++)
            {
                int baseIndex = (imageData.Height - 1 - j) * imageData.Width;
                for (int i = 0; i < imageData.Width; i++)
                {
                    HCommon.Color sysColor = imageData.GetPixelColor(i, j, rotateIndex);
                    colors[baseIndex + i] = ToColor(sysColor);

                    if (IsTransparentColor(sysColor))
                    {
                        colors[baseIndex + i] = UnityEngine.Color.clear;
                    }
                }
            }

            texture.SetPixels(0, 0, imageData.Width, imageData.Height, colors);
            texture.Apply();

            return texture;
        }

        public static Texture2D LoadFromData3(ImageData imageData, byte rotateIndex = 0)
        {
            Texture2D texture = new Texture2D(imageData.Width, imageData.Height, TextureFormat.ARGB32, false);

            Color32[] color32s = new Color32[imageData.Width * imageData.Height];

            for (int j = 0; j < imageData.Height; j++)
            {
                int baseIndex = (imageData.Height - 1 - j) * imageData.Width;
                for (int i = 0; i < imageData.Width; i++)
                {
                    HCommon.Color sysColor = imageData.GetPixelColor(i, j, rotateIndex);
                    color32s[baseIndex + i] = new UnityEngine.Color32(sysColor.R, sysColor.G, sysColor.B, sysColor.A);

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

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), anchorPoint ?? new Vector2(0.5f, 0.5f));
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
