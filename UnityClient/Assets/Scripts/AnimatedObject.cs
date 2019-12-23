using Assets.Scripts.Components;
using H3Engine.FileSystem;
using H3Engine.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HCommon = H3Engine.Common;


public class AnimatedObject : MonoBehaviour
{
    private List<Sprite> mainSprites;

    private bool isInitialized = false;

    private int frameTickCount = 4;

    private int frameTick = 0;

    private int frameIndex = 0;

    private GameObject imageSprite;

    // Start is called before the first frame update
    void Start()
    {
        imageSprite = gameObject.transform.GetChild(0).gameObject;

    }

    public void Initialize(BundleImageDefinition animation)
    {
        mainSprites = new List<Sprite>();
        for (int g = 0; g < animation.Groups.Count; g++)
        {
            for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
            {
                ImageData image = animation.GetImageData(g, i);

                Texture2D texture = Texture2DExtension.LoadFromData(image);
                Sprite sprite = CreateSpriteFromTexture(texture);
                mainSprites.Add(sprite);
            }
        }

        isInitialized = true;
        frameTick = 0;
        frameIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized || mainSprites == null || mainSprites.Count == 0)
        {
            return;
        }

        if (frameTick++ > frameTickCount)
        {
            frameTick = 0;

            SpriteRenderer renderer = imageSprite.GetComponent<SpriteRenderer>();
            frameIndex = (frameIndex + 1) % mainSprites.Count;
            renderer.sprite = mainSprites[frameIndex];
        }
    }

    private Sprite CreateSpriteFromTexture(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        return sprite;
    }
}
