using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

/// <summary>
/// Controls the user interface
/// </summary>
public class UIController
{
    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map Map { get; }

    /// <summary>
    /// Layers screen
    /// </summary>
    private GameObject LayersScreen { get; }

    /// <summary>
    /// Debug screen
    /// </summary>
    private GameObject DebugScreen { get; }

    /// <summary>
    /// Debug text display
    /// </summary>
    private Text DebugTextDisplay { get; }

    /// <summary>
    /// Log display
    /// </summary>
    private GameObject Log { get; }

    private float update = 0.0f;

    private int numFrames = 0;
    private float totalFps = 0;

    private bool arMode = false;

    /// <summary>
    /// Constructs a new UI Controller
    /// <param name="map">The map</param>
    /// </summary>
    public UIController(Map map)
    {
        this.Map = map;

        // Layers screen
        LayersScreen = GameObject.Find("/UI/Screens/Layers");
        LayersScreen.SetActive(false); // Disabled by default, but we needed it active first to be able to find it
        GameObject.Find("/UI/Buttons/Layers").GetComponent<Button>().onClick.AddListener(ToggleLayers);
        UpdateMapLayersList();

        // Debug screen
        DebugTextDisplay = GameObject.Find("/UI/Screens/Debug/Panel/Debug Text Display").GetComponent<Text>();
        Log = GameObject.Find("/UI/Screens/Debug/Scroll View/Viewport/Log");
        Logger.Subscribe(UpdateLog); // Listen for new log messages to display
        DebugScreen = GameObject.Find("/UI/Screens/Debug");
        DebugScreen.SetActive(false); // Disabled by default, but we needed it active first to be able to find it
        GameObject.Find("/UI/Buttons/Debug").GetComponent<Button>().onClick.AddListener(ToggleDebug);

        // Navigation buttons
        GameObject.Find("/UI/Buttons/Navigation/Up").GetComponent<Button>().onClick.AddListener(MoveUp);
        GameObject.Find("/UI/Buttons/Navigation/Down").GetComponent<Button>().onClick.AddListener(MoveDown);
        GameObject.Find("/UI/Buttons/Navigation/Left").GetComponent<Button>().onClick.AddListener(MoveLeft);
        GameObject.Find("/UI/Buttons/Navigation/Right").GetComponent<Button>().onClick.AddListener(MoveRight);

        // Zoom buttons
        GameObject.Find("/UI/Buttons/Zoom/In").GetComponent<Button>().onClick.AddListener(ZoomIn);
        GameObject.Find("/UI/Buttons/Zoom/Out").GetComponent<Button>().onClick.AddListener(ZoomOut);

        // Other buttons
        GameObject.Find("/UI/Buttons/AR").GetComponent<Button>().onClick.AddListener(ToggleAR);
        GameObject.Find("/UI/Buttons/POI").GetComponent<Button>().onClick.AddListener(TogglePOI);
        GameObject.Find("/UI/Buttons/Test").GetComponent<Button>().onClick.AddListener(TestButtonClicked);
        GameObject.Find("/UI/Buttons/Query").GetComponent<Button>().onClick.AddListener(ToggleQuery);
        GameObject.Find("/UI/Buttons/Ray").GetComponent<Button>().onClick.AddListener(CastScreenCenterRay);
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        UpdateAverageFps();

        update += Time.unscaledDeltaTime;
        if (update > 1.0f)
        {
            // Only update debug text about once per second
            update = 0.0f;
            UpdateDebugTextDisplay();
        }

        ProcessInput();
    }

    /// <summary>
    /// Processes input events
    /// </summary>
    public void ProcessInput()
    {
        // Query mode input
        if (queryMode)
        {
            // User clicked/tapped
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.touchCount > 0)
                {
                    // Touch input
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        CastScreenPointRay(touch.position);
                    }
                }
                else
                {
                    // Mouse input
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        CastScreenPointRay(Input.mousePosition);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Moves the map up
    /// </summary>
    private void MoveUp()
    {
        Map.MoveCenter(new Vector2D(0, 100));
    }

    /// <summary>
    /// Moves the map down
    /// </summary>
    private void MoveDown()
    {
        Map.MoveCenter(new Vector2D(0, -100));
    }

    /// <summary>
    /// Moves the map left
    /// </summary>
    private void MoveLeft()
    {
        Map.MoveCenter(new Vector2D(-100, 0));
    }

    /// <summary>
    /// Moves the map right
    /// </summary>
    private void MoveRight()
    {
        Map.MoveCenter(new Vector2D(100, 0));
    }

    /// <summary>
    /// Zooms the map in
    /// </summary>
    private void ZoomIn()
    {
        Map.Zoom(1);
    }

    /// <summary>
    /// Zooms the map out
    /// </summary>
    private void ZoomOut()
    {
        Map.Zoom(-1);
    }

    /// <summary>
    /// Toggles the display of the layers screen
    /// </summary>
    private void ToggleLayers()
    {
        LayersScreen.SetActive(!LayersScreen.activeSelf);
        Debug.Log("Toggled Layers screen");
    }

    /// <summary>
    /// Toggles the display of the debug screen
    /// </summary>
    private void ToggleDebug()
    {
        DebugScreen.SetActive(!DebugScreen.activeSelf);
    }

    /// <summary>
    /// Toggles AR mode
    /// </summary>
    private void ToggleAR()
    {
        if (arMode)
        {
            Map.SwitchTo2DMode();
            Debug.Log("Switched to 2D mode");
        }
        else
        {
            Map.SwitchToARMode();
            Debug.Log("Switched to AR mode");
        }
        arMode = !arMode;
    }

    int currentOrigin = 0;
    private void TogglePOI()
    {
        switch (currentOrigin)
        {
            case 0:
                Map.MoveCenter(38.711992, -9.140663);
                Debug.Log("Moved origin to carmo");
                currentOrigin = 1;
                break;
            case 1:
                Map.MoveCenter(38.765514, -9.093839);
                Debug.Log("Moved origin to expo");
                currentOrigin = 2;
                break;
            case 2:
                Map.MoveCenter(38.725249, -9.149994);
                Debug.Log("Moved origin to marques");
                currentOrigin = 3;
                break;
            case 3:
                Map.MoveCenter(38.773310, -9.153689);
                Debug.Log("Moved origin to alta");
                currentOrigin = 4;
                break;
            case 4:
                Map.MoveCenter(38.733744, -9.160745);
                Debug.Log("Moved origin to campolide");
                currentOrigin = 5;
                break;
            case 5:
                Map.MoveCenter(38.706808, -9.136164);
                Debug.Log("Moved origin to baixa");
                currentOrigin = 0;
                break;
        }
    }

    bool queryMode = false;
    private void ToggleQuery()
    {
        if (queryMode)
        {
            GameObject.Find("/UI/Buttons/Query/Text").GetComponent<Text>().text = "Start Query";
            Debug.Log("Stopped query");
        }
        else
        {
            GameObject.Find("/UI/Buttons/Query/Text").GetComponent<Text>().text = "Stop Query";
            Debug.Log("Started query");
        }
        queryMode = !queryMode;
    }

    int currentCameraAngle = 0;
    private void TestButtonClicked()
    {
        Debug.Log("Test button clicked!");
        switch (currentCameraAngle)
        {
            case 0:
                Map.Test2DCamera(new Vector3(350, 20, 235), new Vector3(20, 325, 0));
                currentCameraAngle = 1;
                break;
            case 1:
                Map.Test2DCamera(new Vector3(420, 120, 125), new Vector3(30, 310, 0));
                currentCameraAngle = 2;
                break;
            case 2:
                Map.Test2DCamera(new Vector3(600, 270, -60), new Vector3(40, 310, 0));
                currentCameraAngle = 3;
                break;
            case 3:
                Map.Test2DCamera(new Vector3(350, 760, -980), new Vector3(35, 330, 0));
                currentCameraAngle = 0;
                break;
        }
    }

    /// <summary>
    /// Casts a ray from the center of the screen
    /// </summary>
    private void CastScreenCenterRay()
    {
        // Get the screen center point
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        // Cast a ray from the screen center point
        CastScreenPointRay(screenCenter);
    }

    /// <summary>
    /// Casts a ray from the given screen point
    /// </summary>
    /// <param name="screenPoint">The screen point to cast the ray from</param>
    private void CastScreenPointRay(Vector2 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Hit something
            Vector2D hitLL = Map.WorldToLatLon(hit.point);
            Debug.Log($"Coordinates of raycast hit: {hit.point} | {hitLL}");

            // Check if the hit object is part of a map feature
            FeatureBehaviour featureBehaviour = hit.transform.GetComponentInParent<FeatureBehaviour>();
            if (featureBehaviour != null)
            {
                // Hit a map feature
                Feature feature = featureBehaviour.Feature;
                Debug.Log($"Raycast hit: {feature.FullId} | {feature}");
            }
            else
            {
                // Hit something else
                Debug.Log($"Raycast didn't hit a feature: {hit.transform.name}");
            }
        }
        else
        {
            // Didn't hit anything
            Debug.LogWarning("Raycast hit nothing");
        }
    }

    /// <summary>
    /// Update the list of map layers
    /// </summary>
    private void UpdateMapLayersList()
    {
        // Get the layer toggle prefab
        GameObject togglePrefab = Resources.Load<GameObject>("UI/Layer Toggle");

        int currentPos = 0;
        foreach (ILayer layer in Map.Layers.Values)
        {
            // Instantiate the layer toggle prefabs as children of the layers screen
            GameObject toggleObj = GameObject.Instantiate(togglePrefab, LayersScreen.transform);
            toggleObj.name = layer.Id;
            toggleObj.GetComponentInChildren<Text>().text = layer.Id;
            toggleObj.transform.position = new Vector3(toggleObj.transform.position.x, toggleObj.transform.position.y + currentPos, toggleObj.transform.position.z);
            currentPos -= 45;

            // Setup the toggle
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.isOn = layer.Visible; // Set the initial state of the toggle to the layer's default visibility
            toggle.onValueChanged.AddListener(
                delegate
                {
                    // When the toggle is toggled, set the layer's visibility to the new state
                    layer.Visible = toggle.isOn;

                    // Update the layer visibility for all the tiles already in the map
                    foreach (Tile tile in Map.Tiles.Values)
                    {
                        tile.Layers[layer.Id].GameObject.SetActive(layer.Visible);
                    }

                    Debug.Log(toggle.isOn ? $"Enabled layer '{layer.Id}'" : $"Disabled layer '{layer.Id}'");
                });
        }
    }

    /// <summary>
    /// Updates log to render new messages
    /// </summary>
    private void UpdateLog(object sender, System.EventArgs e)
    {
        Logger.Render(Log);
    }

    /// <summary>
    /// Updates the debug text display
    /// </summary>
    private void UpdateDebugTextDisplay()
    {
        StringBuilder debugText = new StringBuilder();

        debugText.Append("Version: ");
        debugText.Append(Application.version);

        debugText.Append("\n\nInstant FPS: ");
        float instantFps = GetInstantFps();
        debugText.Append((int)instantFps);
        debugText.Append("\nAverage FPS: ");
        float averageFps = GetAverageFps();
        debugText.Append((int)averageFps);

        debugText.Append("\n\nSystem Memory: ");
        debugText.Append(SystemInfo.systemMemorySize);
        debugText.Append(" MB\nTotal Reserved: ");
        debugText.Append(Profiler.GetTotalReservedMemoryLong() / 1000000);
        debugText.Append(" MB\n- Allocated: ");
        debugText.Append(Profiler.GetTotalAllocatedMemoryLong() / 1000000);
        debugText.Append(" MB\n- Unused: ");
        debugText.Append(Profiler.GetTotalUnusedReservedMemoryLong() / 1000000);
        debugText.Append(" MB");

        debugText.Append("\n\nCPU: ");
        debugText.Append(SystemInfo.processorCount);
        debugText.Append("x ");
        debugText.Append(SystemInfo.processorType);
        debugText.Append(" @ ");
        debugText.Append(SystemInfo.processorFrequency / 1000f);
        debugText.Append(" GHz");

        debugText.Append("\n\nDisplay: ");
        debugText.Append(Screen.currentResolution);
        debugText.AppendLine();
        debugText.Append(SystemInfo.graphicsDeviceVendor);
        debugText.Append(" ");
        debugText.Append(SystemInfo.graphicsDeviceName);
        debugText.Append(" (");
        debugText.Append(SystemInfo.graphicsMemorySize);
        debugText.Append(" MB)\n");
        debugText.Append(SystemInfo.graphicsDeviceVersion);

        debugText.Append("\n\nDevice: ");
        debugText.Append(SystemInfo.operatingSystem);
        debugText.AppendLine();
        debugText.Append(SystemInfo.deviceModel);
        debugText.Append(" (");
        debugText.Append(SystemInfo.deviceType);
        debugText.Append(")\n");
        debugText.Append(SystemInfo.batteryLevel * 100);
        debugText.Append("% Battery (");
        debugText.Append(SystemInfo.batteryStatus);
        debugText.Append(")");

        debugText.Append("\n\nAvailable semaphore threads: ");
        debugText.Append(MainController.networkSemaphore.CurrentCount);
        debugText.Append("\nTiles: ");
        debugText.Append(Map.Tiles.Count);

        DebugTextDisplay.text = debugText.ToString();
    }

    /// <summary>
    /// Calculates the instantaneous FPS
    /// </summary>
    /// <returns>Instantaneous FPS</returns>
    private float GetInstantFps()
    {
        return 1 / Time.unscaledDeltaTime;
    }

    /// <summary>
    /// Updates the counters for the average FPS
    /// </summary>
    private void UpdateAverageFps()
    {
        totalFps += GetInstantFps();
        numFrames++;
    }

    /// <summary>
    /// Gets the average FPS since the last call to GetAverageFps()
    /// </summary>
    /// <returns>Average FPS</returns>
    private float GetAverageFps()
    {
        float averageFps = totalFps / numFrames;
        totalFps = 0;
        numFrames = 0;
        return averageFps;
    }
}