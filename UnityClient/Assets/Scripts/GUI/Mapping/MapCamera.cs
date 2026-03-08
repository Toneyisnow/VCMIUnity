using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityClient.GUI.Mapping
{
    public class MapCamera : MonoBehaviour
    {
        private Vector3 Origin;
        private Vector3 Difference;

        private float anchorDistance = 0;
        private float lastScale = 0;

        private bool isStartZooming = false;

        // Click vs drag detection
        private const float DRAG_THRESHOLD = 5f; // pixels
        private Vector3 mouseDownScreenPos;
        private bool isDragging = false;

        /// <summary>
        /// True if the last mouse up was a click (not a drag). Reset after reading.
        /// </summary>
        public bool WasClick { get; private set; }

        /// <summary>
        /// The world position where the click happened.
        /// </summary>
        public Vector3 ClickWorldPosition { get; private set; }

        void Start()
        {
        }

        private void LateUpdate()
        {
            WasClick = false;

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
                    mouseDownScreenPos = Input.mousePosition;
                    isDragging = false;
                }
                else if (touch1.phase == TouchPhase.Moved)
                {
                    float screenDist = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
                    if (screenDist > DRAG_THRESHOLD)
                    {
                        isDragging = true;
                    }

                    if (isDragging)
                    {
                        Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
                        if (Difference != Vector3.zero)
                        {
                            Camera.main.transform.position = Origin - Difference;
                        }
                    }
                }
                else if (touch1.phase == TouchPhase.Ended)
                {
                    if (!isDragging)
                    {
                        WasClick = true;
                        ClickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    }
                }
            }
            else if (touchCount == 2)
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
                mouseDownScreenPos = Input.mousePosition;
                isDragging = false;
            }
            else if (Input.GetMouseButton(0))
            {
                float screenDist = Vector3.Distance(Input.mousePosition, mouseDownScreenPos);
                if (screenDist > DRAG_THRESHOLD)
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    Difference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
                    if (Difference != Vector3.zero)
                    {
                        Camera.main.transform.position = Origin - Difference;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (!isDragging)
                {
                    WasClick = true;
                    ClickWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            }
        }
    }
}
