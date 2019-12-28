using System;

using H3Engine.GUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Components;
using H3Engine.FileSystem;

namespace UnityClient.GameControls
{

    public class GameMenuItem : MonoBehaviour
    {
        public enum MenuItemStatus
        {
            Idle = 0,
            Hover = 1,
            Clicked = 2
        }


        private Sprite idleSprite = null;

        private Sprite hoverSprite = null;

        private Sprite clickedSprite = null;

        private MenuItemStatus menuItemStatus = MenuItemStatus.Idle;

        private Action callbackAction = null;

        private bool isHovering = false;


        // Start is called before the first frame update
        void Start()
        {

        }

        public void Initialize(BundleImageDefinition bundleDefinition, Action callbackAction)
        {
            if (bundleDefinition == null)
            {
                throw new ArgumentNullException("BundleImageDefinition is null.");
            }

            this.callbackAction = callbackAction;

            idleSprite = CreateSpriteFromFrame(bundleDefinition, 0);
            clickedSprite = CreateSpriteFromFrame(bundleDefinition, 1);
            hoverSprite = CreateSpriteFromFrame(bundleDefinition, 3);

            menuItemStatus = (Input.touchSupported ? MenuItemStatus.Hover : MenuItemStatus.Idle);
            StatusUpdated();

            gameObject.AddComponent<BoxCollider2D>();

        }

        private void OnMouseDown()
        {
            menuItemStatus = MenuItemStatus.Clicked;
            StatusUpdated();
        }

        private void OnMouseUp()
        {
            isHovering = false;
            menuItemStatus = (Input.touchSupported ? MenuItemStatus.Hover : MenuItemStatus.Idle);
            StatusUpdated();

            callbackAction?.Invoke();
        }

        private void OnMouseOver()
        {
            if (isHovering)
            {
                return;
            }

            menuItemStatus = MenuItemStatus.Hover;
            isHovering = true;
            StatusUpdated();
        }

        private void OnMouseExit()
        {
            isHovering = false;
            menuItemStatus = (Input.touchSupported ? MenuItemStatus.Hover : MenuItemStatus.Idle);
            StatusUpdated();
        }

        private Sprite CreateSpriteFromFrame(BundleImageDefinition bundleDefinition, int frame)
        {
            ImageData image = bundleDefinition.GetImageData(0, frame);
            Texture2D texture = Texture2DExtension.LoadFromData(image);
            return Texture2DExtension.CreateSpriteFromTexture(texture, new Vector2(0.5f, 0.5f));
        }

        private void StatusUpdated()
        {
            var renderer = gameObject.GetComponent<SpriteRenderer>();

            if (menuItemStatus == MenuItemStatus.Idle)
            {
                renderer.sprite = idleSprite;
            }
            else if (menuItemStatus == MenuItemStatus.Hover)
            {
                renderer.sprite = hoverSprite;
            }
            else if (menuItemStatus == MenuItemStatus.Clicked)
            {
                renderer.sprite = clickedSprite;
            }
        }
    }
}