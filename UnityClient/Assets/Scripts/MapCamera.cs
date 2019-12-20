using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private Vector3 anchorPosition = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;

    private float anchorDistance = 0;
    private float lastScale = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int touchCount = Input.touchCount;
        var cameraCom = this.GetComponent<Camera>();

        if (Input.GetMouseButtonDown(0))
        {
            if (touchCount == 1)
            {
                // Click on screen
                anchorPosition = Input.mousePosition;
                lastPosition = this.transform.position;
                return;
            }
            else if (touchCount == 2)
            {
                // Two touch on screen
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];

                anchorDistance = Mathf.Abs(touch1.position.x - touch2.position.x) + Mathf.Abs(touch1.position.y - touch2.position.y);

                lastScale = cameraCom.orthographicSize;
                return;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (touchCount == 1)
            {
                // Dragging
                float deltaX = (Input.mousePosition.x - anchorPosition.x) / 100 * cameraCom.orthographicSize;
                float deltaY = (Input.mousePosition.y - anchorPosition.y) / 100 * cameraCom.orthographicSize;

                Vector3 newPosition = new Vector3(lastPosition.x - deltaX, lastPosition.y - deltaY, lastPosition.z);
                this.transform.position = newPosition;
            }
            else if (touchCount == 2)
            {
                if (anchorDistance > 0)
                {
                    Touch touch1 = Input.touches[0];
                    Touch touch2 = Input.touches[1];

                    float currentDistance = Mathf.Abs(touch1.position.x - touch2.position.x) + Mathf.Abs(touch1.position.y - touch2.position.y);

                    
                    var currentScale = (anchorDistance / currentDistance) * lastScale;
                    currentScale = Mathf.Max(currentScale, 1.0f);
                    currentScale = Mathf.Min(currentScale, 6.0f);

                    cameraCom.orthographicSize = currentScale;

                }

                return;
            }

        }

    }
}
