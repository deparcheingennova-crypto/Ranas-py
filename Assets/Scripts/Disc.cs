using UnityEngine;
using System.Collections;
using TMPro;

public class Disc : MonoBehaviour
{
    public float speed = 10f;         // Speed of the disc
    public float height = 5f;         // Height of the parabola

    private bool moving = false;

    public Camera cam;

    private IEnumerator moveIEnumerator;

    [SerializeField] private bool worksWithMouse = false;

    private void Start()
    {
        moveIEnumerator = MoveAlongParabola(Vector3.zero);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && worksWithMouse)
        {
            if (GameMechanics.Instance.GetIsPaused())
            {
                return;
            }

            Vector3 mousePosition = Input.mousePosition;

            // Get the display-relative mouse position and display index
            Vector3 relativeMousePos = Display.RelativeMouseAt(mousePosition);
            int displayIndex = (relativeMousePos != Vector3.zero) ? (int)relativeMousePos.z : cam.targetDisplay;

            // Check if the display index is valid and matches the camera's target display
            if (displayIndex >= 0 && displayIndex < Display.displays.Length)
            {
                if (displayIndex == cam.targetDisplay)
                {
                    // Correct the mouse position to fit the display's resolution
                    float displayWidth = Display.displays[displayIndex].systemWidth;
                    float displayHeight = Display.displays[displayIndex].systemHeight;

                    // Ensure mouse position is clamped within valid display bounds
                    relativeMousePos.x = Mathf.Clamp(relativeMousePos.x, 0, displayWidth);
                    relativeMousePos.y = Mathf.Clamp(relativeMousePos.y, 0, displayHeight);
                    relativeMousePos.z = 0; // Clear the display index component

                    Ray ray = cam.ScreenPointToRay(relativeMousePos);

                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Vector3 targetPosition = hit.point;
                        MoveToPosition(targetPosition);
                    }
                }
            }
            else
            {
                // Handle fallback raycasting for the primary display
                Ray ray = cam.ScreenPointToRay(mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 targetPosition = hit.point;
                    MoveToPosition(targetPosition);
                }
            }
        }
    }


    public void MoveToPosition(Vector3 targetPosition)
    {
        if (!GameMechanics.Instance.canThrow)
            return;

        GameMechanics.Instance.canThrow = false;

        if (!moving)
        {
            moveIEnumerator = MoveAlongParabola(targetPosition);
            StartCoroutine(moveIEnumerator);

            GameMechanics.Instance.MakeThrowSound();
            GameMechanics.Instance.StartMoveCameraToThrowPosition();
        }
    }

    public IEnumerator MoveAlongParabola(Vector3 targetPosition)
    {
        // CHANGED If disc (self) is null, exit the coroutine
        if (this == null)
            yield break;

        moving = true;

        Vector3 startPosition = transform.position;
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (true)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;

            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            float parabolicHeight = height * Mathf.Sin(Mathf.Clamp01(fractionOfJourney) * Mathf.PI);
            currentPosition.y += parabolicHeight;

            // CHANGED If disc (self) is null, exit the coroutine
            if (this == null)
                yield break;
            transform.position = currentPosition;

            if (fractionOfJourney >= 1f)
            {
                break;
            }

            // Wait until the next frame
            yield return null;
        }

        moving = false;  // Finished moving
        
        // If you haven't scored in that throw, call failed shot
        //yield return new WaitForSeconds(0.1f);
        //GameMechanics.Instance.FailedShot();
    }

    void OnDisable()
    {
        //moving = false;
        //StopCoroutine(moveIEnumerator);
    }
}
