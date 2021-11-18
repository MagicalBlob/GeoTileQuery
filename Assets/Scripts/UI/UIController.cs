using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

/// <summary>
/// Controls the user interface
/// </summary>
public class UIController
{
    private GameObject layers;
    private GameObject debugInfo;

    private float update = 0.0f;

    private Text debugTextDisplay;
    private GameObject log;

    private int numFrames = 0;
    private float totalFps = 0;

    /// <summary>
    /// Constructs a new UI Controller
    /// </summary>
    public UIController()
    {
        // Layers screen
        layers = GameObject.Find("/UI/Layers");
        layers.SetActive(false); // Disabled by default, but we needed it active first to be able to find it
        GameObject.Find("/UI/Buttons/Layers").GetComponent<Button>().onClick.AddListener(ToggleLayers);

        // Debug info screen
        debugTextDisplay = GameObject.Find("/UI/Debug Info/Panel/Debug Text Display").GetComponent<Text>();
        log = GameObject.Find("/UI/Debug Info/Scroll View/Viewport/Log");
        Logger.Subscribe(UpdateLog); // Listen for new log messages to display
        debugInfo = GameObject.Find("/UI/Debug Info");
        debugInfo.SetActive(false); // Disabled by default, but we needed it active first to be able to find it
        GameObject.Find("/UI/Buttons/Debug Info").GetComponent<Button>().onClick.AddListener(ToggleDebugInfo);
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Render()
    {
        UpdateAverageFps();

        // Only update debug text about once per second
        update += Time.unscaledDeltaTime;
        if (update > 1.0f)
        {
            update = 0.0f;
            UpdateDebugTextDisplay();
        }
    }

    /// <summary>
    /// Toggles the display of the layers screen
    /// </summary>
    private void ToggleLayers()
    {
        layers.SetActive(!layers.activeSelf);
        Logger.Log("Toggled Layers screen");
    }

    /// <summary>
    /// Toggles the display of the debug info screen
    /// </summary>
    private void ToggleDebugInfo()
    {
        debugInfo.SetActive(!debugInfo.activeSelf);
    }

    /// <summary>
    /// Updates log to render new messages
    /// </summary>
    private void UpdateLog()
    {
        Logger.Render(log);
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

        debugTextDisplay.text = debugText.ToString();
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