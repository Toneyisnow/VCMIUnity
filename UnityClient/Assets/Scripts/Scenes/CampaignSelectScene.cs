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
        this.campaignVersion = ECampaignVersion.SOD;

        Engine h3Engine = Engine.GetInstance();
        ImageData imageData = h3Engine.RetrieveImage("campback.PCX");


        GameObject background = GameObject.Find("Background");
        var renderer = background.GetComponent<SpriteRenderer>();
        renderer.sprite = Texture2DExtension.CreateSpriteFromImageData(imageData, new Vector2(0.5f, 0.5f));
        background.transform.position = new Vector3(0, 0, 0.5f);
        background.transform.localScale = new Vector3(1.6f, 1.6f, 1);

        if (campaignVersion == ECampaignVersion.ROE)
        {
            CreateCampaignIcon(iconPosition1, "campgd1s.PCX", "CGOOD1.mp4", 1);
            CreateCampaignIcon(iconPosition2, "campev1s.PCX", "CEVIL1.mp4", 2);
            CreateCampaignIcon(iconPosition3, "campgd2s.PCX", "CGOOD2.mp4", 3);
            CreateCampaignIcon(iconPosition4, "campneus.PCX", "CNEUTRAL.mp4", 4);
            CreateCampaignIcon(iconPosition5, "campev2s.PCX", "CEVIL2.mp4", 5);
            CreateCampaignIcon(iconPosition6, "campgd3s.PCX", "CGOOD3.mp4", 6);
        }
        else if (campaignVersion == ECampaignVersion.AB)
        {
            CreateCampaignIcon(iconPosition1, "campgd1s.PCX", "CGOOD1.mp4", 1);
            CreateCampaignIcon(iconPosition2, "campev1s.PCX", "CEVIL1.mp4", 2);
            CreateCampaignIcon(iconPosition3, "campgd2s.PCX", "CGOOD2.mp4", 3);
            CreateCampaignIcon(iconPosition4, "campneus.PCX", "CNEUTRAL.mp4", 4);
            CreateCampaignIcon(iconPosition5, "campev2s.PCX", "CEVIL2.mp4", 5);
            CreateCampaignIcon(iconPosition6, "campgd3s.PCX", "CGOOD3.mp4", 6);
        }
        else if (campaignVersion == ECampaignVersion.SOD)
        {
            CreateCampaignIcon(iconPosition1, "campgd1s.PCX", "CGOOD1.mp4", 1);
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
        print("OnSelectedCampaign: " + campaignFlag);
    }

}
