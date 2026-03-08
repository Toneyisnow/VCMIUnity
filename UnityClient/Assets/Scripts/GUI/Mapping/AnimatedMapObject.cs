using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityClient.GUI.Mapping
{

    public class AnimatedMapObject : MonoBehaviour
    {
        private Sprite[] mainSprites;

        private SpriteRenderer cachedRenderer;

        private int frameTickCount = 18;

        private int frameTick = 0;

        private int frameIndex = 0;

        public void Initialize(Sprite[] sprites)
        {
            mainSprites = sprites;
            cachedRenderer = GetComponent<SpriteRenderer>();
            frameTick = 0;
            frameIndex = 0;

            // Set the first frame immediately
            if (mainSprites != null && mainSprites.Length > 0 && cachedRenderer != null)
            {
                cachedRenderer.sprite = mainSprites[0];
            }
        }

        void Update()
        {
            if (mainSprites == null || mainSprites.Length == 0)
            {
                return;
            }

            if (frameTick++ > frameTickCount)
            {
                frameTick = 0;
                frameIndex = (frameIndex + 1) % mainSprites.Length;
                cachedRenderer.sprite = mainSprites[frameIndex];
            }
        }
    }
}