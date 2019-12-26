using Assets.Scripts.Components;
using H3Engine;
using H3Engine.FileSystem;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameMenuScene : MonoBehaviour
{
    private static string GetGameDataFilePath(string filename)
    {
        return Path.Combine(Application.streamingAssetsPath, filename);
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadImage();
    }

    void LoadImage()
    {
        Engine h3Engine = Engine.GetInstance();
        h3Engine.LoadArchiveFile(GetGameDataFilePath("GameData/SOD.zh-cn/H3bitmap.lod"));
        ImageData image = h3Engine.RetrieveImage("gamselb1.pcx");


        Texture2D texture = Texture2DExtension.LoadFromData(image);
        Sprite sprite = Texture2DExtension.CreateSpriteFromTexture(texture, new Vector2(0.5f, 0.5f));

        GameObject go = new GameObject("BackgroundSprite");
        go.transform.parent = transform;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
