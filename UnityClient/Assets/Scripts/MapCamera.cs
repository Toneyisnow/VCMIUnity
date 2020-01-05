using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private Vector3 Origin;
    private Vector3 Difference;

    private float anchorDistance = 0;
    private float lastScale = 0;

    private bool isStartZooming = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    private void LateUpdate()
    {
        if (Input.touchSupported)
        {
            HandleTouches();
        }
        else
        {
            HandleMouse();
        }
    }

    private void HandleTouches()
    {
        int touchCount = Input.touchCount;
        if (touchCount == 1)
        {
            Touch touch1 = Input.touches[0];
            if (touch1.phase == TouchPhase.Began)
            {
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else if (touch1.phase == TouchPhase.Moved)
            {
                Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;

                if (Difference != Vector3.zero)
                {
                    //// print("dragging.");
                    Camera.main.transform.position = Origin - Difference;
                }
            }
        }
        else if(touchCount == 2)
        {
            Touch touch1 = Input.touches[0];
            Touch touch2 = Input.touches[1];

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                anchorDistance = Mathf.Abs(touch1.position.x - touch2.position.x) + Mathf.Abs(touch1.position.y - touch2.position.y);
                lastScale = Camera.main.orthographicSize;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                if (anchorDistance > 0)
                {
                    float currentDistance = Mathf.Abs(touch1.position.x - touch2.position.x) + Mathf.Abs(touch1.position.y - touch2.position.y);
                    var currentScale = (anchorDistance / currentDistance) * lastScale;
                    currentScale = Mathf.Max(currentScale, 1.0f);
                    currentScale = Mathf.Min(currentScale, 6.0f);

                    Camera.main.orthographicSize = currentScale;
                }
            }
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;

            if (Difference != Vector3.zero)
            {
                Camera.main.transform.position = Origin - Difference;
            }
        }
    }

    private void LateUpdate2()
    {
        int touchCount = Input.touchCount;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!Input.touchSupported || touchCount == 1)
            {
                Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else if (touchCount == 2)
            {
                print("start zooming.");
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];

                anchorDistance = Mathf.Abs(touch1.position.x - touch2.position.x) + Mathf.Abs(touch1.position.y - touch2.position.y);
                lastScale = Camera.main.orthographicSize;

                isStartZooming = true;
            }
        }
        else if ((!Input.touchSupported && Input.GetMouseButton(0)) || touchCount >= 1)
        {
            if (!Input.touchSupported || touchCount == 1)
            {
                Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;

                if(Difference != Vector3.zero)
                {
                    print("dragging.");
                    Camera.main.transform.position = Origin - Difference;
                }
            }
            else if (touchCount == 2)
            {
                print("zooming/");
                if (anchorDistance > 0)
                {
                    print("do zooming.");
                    Touch touch1 = Input.touches[0];
                    Touch touch2 = Input.touches[1];

                    float currentDistance = Mathf.Abs(touch1.position.x - touch2.position.x) + Mathf.Abs(touch1.position.y - touch2.position.y);
                    var currentScale = (anchorDistance / currentDistance) * lastScale;
                    currentScale = Mathf.Max(currentScale, 1.0f);
                    currentScale = Mathf.Min(currentScale, 6.0f);

                    Camera.main.orthographicSize = currentScale;

                    isStartZooming = false;
                }
            }
        }
        else
        {
            if (!isStartZooming && anchorDistance > 0)
            {
                print("clearing.");
                anchorDistance = 0;
            }
        }
    }
}
