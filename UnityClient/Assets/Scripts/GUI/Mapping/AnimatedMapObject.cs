using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityClient.GUI.Mapping
{

    public class AnimatedMapObject : MonoBehaviour
    {
        private List<Sprite> mainSprites;

        private bool isInitialized = false;

        private int frameTickCount = 2;

        private int frameTick = 0;

        private int frameIndex = 0;

        private GameObject imageSprite;

        // Start is called before the first frame update
        void Start()
        {
            ///imageSprite = gameObject.transform.GetChild(0).gameObject;
            ///imageSprite.transform.position = transform.position;
        }

        public void Initialize(Sprite[] sprites)
        {
            mainSprites = new List<Sprite>(sprites);

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

                SpriteRenderer renderer = this.GetComponent<SpriteRenderer>();
                frameIndex = (frameIndex + 1) % mainSprites.Count;
                renderer.sprite = mainSprites[frameIndex];
            }
        }
    }
}