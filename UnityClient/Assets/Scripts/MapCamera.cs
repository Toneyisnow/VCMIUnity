using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private Vector3 anchorPosition = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anchorPosition = Input.mousePosition;
            lastPosition = this.transform.position;
            return;
        }

        if (Input.GetMouseButton(0))
        {
            // Dragging
            float deltaX = (Input.mousePosition.x - anchorPosition.x) / 100;
            float deltaY = (Input.mousePosition.y - anchorPosition.y) / 100;

            Vector3 newPosition = new Vector3(lastPosition.x - deltaX, lastPosition.y - deltaY, lastPosition.z);
            this.transform.position = newPosition;
        }

        if (Input.touchCount > 0)
        {
            int h = 1;
        }

    }
}
