using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System.Text;

/// <summary>
/// Controls the Debug UI
/// </summary>
public class DebugUIController : MonoBehaviour
{
    private float update = 0.0f;
    public Text debugTextDisplay, debugLogDisplay;
    private int numFrames = 0;
    private float totalFps = 0f;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before any of the Update methods are called the first time
    /// </summary>
    private void Start()
    {
        // Listen for new log messages to display
        Logger.Subscribe(UpdateLog);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled
    /// </summary>
    private void Update()
    {
        // Only update debug text about once per second
        update += Time.unscaledDeltaTime;
        if (update > 1.0f)
        {
            update = 0.0f;
            UpdateDebugTextDisplay();
        }
    }

    /// <summary>
    /// Updates log to render new messages
    /// </summary>
    private void UpdateLog()
    {
        debugLogDisplay.text = Logger.Print();
    }

    /// <summary>
    /// Updates the debug text display
    /// </summary>
    private void UpdateDebugTextDisplay()
    {
        StringBuilder debugText = new StringBuilder();

        debugText.Append("Version: ");
        debugText.Append(Application.version);

        debugText.Append("\n\nFPS: ");
        float instantFps = CalculateInstantFps();
        debugText.Append((int)instantFps);
        debugText.Append("\nAverage FPS: ");
        float averageFps = CalculateAverageFps(instantFps);
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
    private float CalculateInstantFps()
    {
        return 1 / Time.unscaledDeltaTime;
    }

    /// <summary>
    /// Calculates the average FPS
    /// </summary>
    /// <param name="instantFps">Instantaneous FPS</param>
    /// <returns>Average FPS</returns>
    private float CalculateAverageFps(float instantFps)
    {
        totalFps += instantFps;
        numFrames++;
        return totalFps / numFrames;
    }

}