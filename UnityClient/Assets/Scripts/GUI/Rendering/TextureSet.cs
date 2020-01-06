using H3Engine;
using H3Engine.Common;
using H3Engine.FileSystem;
using H3Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using HCommon = H3Engine.Common;

namespace UnityClient.GUI.Rendering
{
    /// <summary>
    /// One TextureSet will be corresponding to the DEF file in the H3Engine, it might contain one or more TextureSheets depending on the Game Object Type
    /// 
    /// This is legacy, using BundleImageSheet instead
    /// </summary>
    public abstract class TextureSet
    {
        protected Engine h3Engine = null;
        
        public TextureSet()
        {
            h3Engine = Engine.GetInstance();
        }
        
        public abstract Sprite RetrieveSprite(string key);

    }

    public enum ETileType
    {
        Terrain,
        Road,
        River
    }

    public class MapTileTextureSet : TextureSet
    {
        private ETileType tileType;

        private int subType;

        private TextureSheet textureSheet = null;

        public static string TextureKey(int index, byte rotate)
        {
            return string.Format(@"{0}", index * 4 + rotate);
        }

        public MapTileTextureSet(ETileType tileType, int subType)
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
                    Texture2D texture = Texture2DExtension.LoadFromData(images[i], rotate);
                    textureSheet.AddImageData(TextureKey(i, rotate), texture);
                }
            }

            textureSheet.PackTextures();
        }

        public override Sprite RetrieveSprite(string key)
        {
            return textureSheet.RetrieveSprite(key);
        }
    }

    /// <summary>
    /// All of the MapObject that def file name starting with "AVA*"
    /// </summary>
    public class MapArtifactTextureSet : TextureSet
    {
        private TextureSheet textureSheet = null;

        public static string TextureKey(string defFileName, int animationIndex)
        {
            return defFileName.ToLower().Replace(@".def", string.Empty) + animationIndex.ToString();
        }

        public MapArtifactTextureSet()
        {
            this.LoadTextures();
        }

        private void LoadTextures()
        {
            textureSheet = new TextureSheet();

            List<string> mapArtifactFiles = h3Engine.SearchResourceFiles("AVA0*.def");
            foreach(string defFileName in mapArtifactFiles)
            {
                BundleImageDefinition bundleImageDefinition = h3Engine.RetrieveBundleImage(defFileName);

                int animationIndex = 0;
                for (int group = 0; group < bundleImageDefinition.Groups.Count; group ++)
                {
                    var groupObj = bundleImageDefinition.Groups[group];
                    for (int frame = 0; frame < groupObj.Frames.Count; frame++)
                    {
                        ImageData imageData = bundleImageDefinition.GetImageData(group, frame);

                        Texture2D texture = Texture2DExtension.LoadFromData(imageData);
                        string key = MapArtifactTextureSet.TextureKey(defFileName, animationIndex++);
                        textureSheet.AddImageData(key, texture);
                    }
                }
            }

            textureSheet.PackTextures();
        }


        public override Sprite RetrieveSprite(string key)
        {
            return textureSheet.RetrieveSprite(key);
        }
    }
}
