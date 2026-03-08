using UnityEngine;
using System;

namespace UnityClient.GUI.Mapping
{
    public class MapTile : MonoBehaviour
    {
        public int TileX { get; private set; }
        public int TileY { get; private set; }

        public static event Action<int, int> OnTileClicked;

        public void Initialize(int x, int y)
        {
            TileX = x;
            TileY = y;
        }

        void Start()
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        private void OnMouseUp()
        {
            OnTileClicked?.Invoke(TileX, TileY);
        }

        void Update()
        {
        }
    }
}
