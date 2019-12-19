﻿using Assets.Scripts.Components;
using H3Engine.FileSystem;
using H3Engine.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleImageSheet
{
    private TextureSheet textureSheet = null;

    private H3Engine.Engine h3Engine = H3Engine.Engine.GetInstance();

    private static string GetTextureKey(string defFileName, int index)
    {
        return string.Format(@"{0}-{1}", defFileName, index);
    }

    public BundleImageSheet()
    {
        textureSheet = new TextureSheet();
    }

    public void AddBundleImage(string defFileName)
    {
        BundleImageDefinition bundleImageDefinition = h3Engine.RetrieveBundleImage(defFileName);

        int animationIndex = 0;
        for (int group = 0; group < bundleImageDefinition.Groups.Count; group++)
        {
            var groupObj = bundleImageDefinition.Groups[group];
            for (int frame = 0; frame < groupObj.Frames.Count; frame++)
            {
                ImageData imageData = bundleImageDefinition.GetImageData(group, frame);

                Texture2D texture = Texture2DExtension.LoadFromData(imageData);
                string key = GetTextureKey(defFileName, animationIndex++);
                textureSheet.AddImageData(key, texture);
            }
        }
    }

    public Sprite[] LoadSprites(string defFileName)
    {
        if (textureSheet == null)
        {
            return null;
        }

        List<Sprite> sprites = new List<Sprite>();

        int animationIndex = 0;

        Sprite sprite = null;
        while ((sprite = textureSheet.RetrieveSprite(GetTextureKey(defFileName, animationIndex))) != null)
        {
            sprites.Add(sprite);
            animationIndex++;
        }

        return sprites.ToArray();
    }

    public void PackTextures()
    {
        this.textureSheet.PackTextures();
    }
}
