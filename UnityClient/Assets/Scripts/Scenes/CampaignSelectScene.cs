using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine;
using H3Engine.Common;
using UnityClient.Components;
using H3Engine.FileSystem;
using Assets.Scripts.Components;
using UnityClient.GameControls;

public class CampaignSelectScene : MonoBehaviour
{
    private ECampaignVersion campaignVersion;

    public GameObject CampaignIconPrefab = null;

    public Vector3 iconPosition1;
    public Vector3 iconPosition2;
    public Vector3 iconPosition3;
    public Vector3 iconPosition4;
    public Vector3 iconPosition5;
    public Vector3 iconPosition6;
    public Vector3 iconPosition7;


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
            CreateCampaignIcon(iconPosition1, "campev1s.PCX", "CEVIL1.mp4", 1);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void CreateCampaignIcon(Vector3 position, string imageFileName, string videoFileName, int flag)
    {
        GameObject campaignIcon = Instantiate(CampaignIconPrefab);
        campaignIcon.transform.position = position;

        CampaignIcon script = campaignIcon.GetComponent<CampaignIcon>();
        script.Initialize(imageFileName, videoFileName, flag, (f) => { this.OnSelectedCampaign(f); });

    }

    private void OnSelectedCampaign(int campaignFlag)
    {

    }

}
