using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine.DataAccess;
using UnityEngine.Video;
using System.IO;
using UnityClient.GUI.Rendering;

namespace UnityClient.GUI.GameControls
{
    public class CampaignIcon : MonoBehaviour
    {
        private Sprite imageFile = null;

        private int campaignFlag = 0;

        private Action<int> callback = null;

        private VideoPlayer videoPlayer = null;

        public void Initialize(string imageFileName, string videoFileName, int campaignFlag, Action<int> action)
        {
            H3DataAccess h3Engine = H3DataAccess.GetInstance();

            this.campaignFlag = campaignFlag;
            var imageData = h3Engine.RetrieveImage(imageFileName);
            var renderer = gameObject.GetComponent<SpriteRenderer>();
            renderer.sprite = Texture2DExtension.CreateSpriteFromImageData(imageData);
            gameObject.AddComponent<BoxCollider2D>();

            var canvasImage = GameObject.Find("RawImage");
            ///canvasImage.transform.position = gameObject.transform.position;

            this.callback = action;

            this.videoPlayer = gameObject.GetComponent<VideoPlayer>();
            this.videoPlayer.url = Path.Combine(Application.streamingAssetsPath, @"Videos\CampaignIcons\", videoFileName);
            this.videoPlayer.Stop();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnMouseDown()
        {
            if (this.videoPlayer == null)
            {
                return;
            }

            if (!this.videoPlayer.isPlaying)
            {
                this.videoPlayer.isLooping = true;
                this.videoPlayer.Play();
            }
            else
            {
                this.callback?.Invoke(campaignFlag);
            }
        }

        public void StopVideo()
        {
            this.videoPlayer.Stop();
        }
    }
}
