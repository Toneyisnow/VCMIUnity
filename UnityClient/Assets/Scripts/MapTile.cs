using UnityEngine;
using System.Collections;

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

    // Update is called once per frame
    void Update()
    {

    }
}
