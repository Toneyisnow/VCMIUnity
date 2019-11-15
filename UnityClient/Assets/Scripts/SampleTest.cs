using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Utils;

public class SampleTest : MonoBehaviour
{
    private List<Sprite> mainSprites;

    private int spriteIndex = 0;
    private int frameCount = 0;

    private bool sceneLoaded = false;

    private GameObject gObject = null;

    // Start is called before the first frame update
    void Start()
    {
        mainSprites = new List<Sprite>();

        // LoadImage();

        LoadAnimation();

        sceneLoaded = true;
        frameCount = 0;
    }

    void LoadImage()
    {
        Engine h3Engine = Engine.GetInstance();
        h3Engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_bmp.lod");
        ImageData image = h3Engine.RetrieveImage("Bo53Muck.pcx");
        
        Sprite sprite = CreateSpriteFromBytes(image.GetPNGData());

        GameObject go = new GameObject("SampleSprite");
        /// go.transform.position = new Vector3(10, 20, 0);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    void LoadAnimation()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_spr.lod");

        
        AnimationDefinition animation = engine.RetrieveAnimation("AVG2ele.def");
        for (int g = 0; g < animation.Groups.Count; g++)
        {
            for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
            {
                ImageData image = animation.ComposeFrameImage(g, i);
                Sprite sprite = CreateSpriteFromBytes(image.GetPNGData());
                mainSprites.Add(sprite);
            }
        }

        gObject = new GameObject("SampleSprite2");
        /// gObject.transform.position = new Vector3(50, 50, 0);

        SpriteRenderer renderer = gObject.AddComponent<SpriteRenderer>();
        renderer.sprite = mainSprites[0];
    }

    private Sprite CreateSpriteFromBytes(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        texture.LoadImage(imageBytes);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        
        return sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!sceneLoaded || mainSprites.Count == 0)
        {
            return;
        }

        if (frameCount ++ > 6)
        {
            frameCount = 0;

            SpriteRenderer renderer = gObject.GetComponent<SpriteRenderer>();
            spriteIndex = (spriteIndex + 1) % mainSprites.Count;
            renderer.sprite = mainSprites[spriteIndex];
        }
    }
}
