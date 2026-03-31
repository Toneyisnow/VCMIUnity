using UnityEngine;

namespace UnityClient.GUI.Mapping
{
    /// <summary>
    /// Backward-compatibility MonoBehaviour kept on the "GameMap" scene GameObject.
    /// Ensures MapComponent is present on the same GameObject; all actual logic
    /// lives in MapComponent and MapTileRenderer.
    /// Once the Unity scene is updated to attach MapComponent directly, this class
    /// can be removed.
    /// </summary>
    [DisallowMultipleComponent]
    public class MapLoader : MonoBehaviour
    {
        void Awake()
        {
            if (GetComponent<MapComponent>() == null)
                gameObject.AddComponent<MapComponent>();
        }
    }
}
