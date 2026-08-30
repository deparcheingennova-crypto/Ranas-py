using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }
    private Vector2[] displayOffsets;

    private bool defaultDisplayScreens = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetScreens();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SwapDisplays();
        }
    }

    public void SetScreens()
    {
        Camera mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();
        Camera secondScreenCamera = GameObject.FindGameObjectsWithTag("SecondScreenCamera")[0].GetComponent<Camera>();

        if (mainCamera != null && secondScreenCamera != null)
        {
            Debug.Log("Cameras found!");
        }
        else
        {
            Debug.LogWarning("Cameras were not found! Prepare to have some erros (Solution: In every scene you need to have a camera tagged with MainCamera and a camera tagged with SecondScreenCamera)");
        }

        // Ensure each display is only activated if available and not already active.
        if (Display.displays.Length > 0 && !Display.displays[0].active)
        {
            Display.displays[0].Activate();
        }
        if (Display.displays.Length > 1 && !Display.displays[1].active)
        {
            Display.displays[1].Activate();
        }

        // Assign main camera to the primary display and second camera to the secondary display.
        mainCamera.targetDisplay = 0;  // Primary display
        if (Display.displays.Length > 1 && secondScreenCamera != null)
        {
            secondScreenCamera.targetDisplay = 1;  // Secondary display
        }


        defaultDisplayScreens = true;
    }

    public void SwapDisplays()
    {
        Camera mainCamera = GameObject.FindGameObjectsWithTag("MainCamera")[0].GetComponent<Camera>();
        Camera secondScreenCamera = GameObject.FindGameObjectsWithTag("SecondScreenCamera")[0].GetComponent<Camera>();

        if (mainCamera == null || secondScreenCamera == null)
        {
            Debug.LogWarning("Cameras were not found! Ensure you have cameras tagged as MainCamera and SecondScreenCamera in your scene.");
            return;
        }

        // Swap the target displays of the main and second cameras based on a boolean state
        if (defaultDisplayScreens == true)
        {
            // Swap target displays
            mainCamera.targetDisplay = 1;
            secondScreenCamera.targetDisplay = 0;
            // Swap all scene canvases to the other display
            foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
            {
                canvas.targetDisplay = (canvas.targetDisplay == 0) ? 1 : 0;
            }
            defaultDisplayScreens = false;
        }
        else
        {
            // Swap target displays back to default
            mainCamera.targetDisplay = 0;
            secondScreenCamera.targetDisplay = 1;
            foreach (Canvas canvas in FindObjectsOfType<Canvas>(true))
            {
                canvas.targetDisplay = (canvas.targetDisplay == 0) ? 1 : 0;
            }
            defaultDisplayScreens = true;
        }


        Debug.Log($"Displays swapped: Main Camera is now on Display {mainCamera.targetDisplay}, Second Camera is now on Display {secondScreenCamera.targetDisplay}");
    }
}