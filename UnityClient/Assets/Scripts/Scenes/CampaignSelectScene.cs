using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine;
using H3Engine.Common;
using UnityClient.Components;
using H3Engine.FileSystem;
using Assets.Scripts.Components;

public class CampaignSelectScene : MonoBehaviour
{
    private ECampaignVersion campaignVersion;

    // Start is called before the first frame update
    void Start()
    {
        this.campaignVersion = CrossSceneData.SelectedCampaign;

        Engine h3Engine = Engine.GetInstance();
        ImageData imageData = h3Engine.RetrieveImage("campback.PCX");


        GameObject background = GameObject.Find("Background");
        var renderer = background.GetComponent<SpriteRenderer>();
        
        renderer.sprite = Texture2DExtension.CreateSpriteFromImageData(imageData, new Vector2(0.5f, 0.5f));

        if (campaignVersion == ECampaignVersion.SOD)
        {

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
