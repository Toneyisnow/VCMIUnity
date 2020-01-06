using UnityEngine;
using System.Collections;
using System;

namespace UnityClient.GUI.Mapping
{
    public class MapTile : MonoBehaviour
    {
        private void OnMouseUp()
        {
            print("OnMouseUp");
        }

        private void OnMouseDown()
        {
            print("OnMouseDown");
        }

        // Use this for initialization
        void Start()
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        public void Initialize(Vector2 location, Action callback)
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}