using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using System.Text;

public class DebugUIController : MonoBehaviour
{
    private float update = 0.0f;
    public Text debugTextDisplay, debugLogDisplay;
    private StringBuilder log = new StringBuilder();
    private int numFrames = 0;
    private float totalFps = 0f;

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

    // Update debug text display
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

    // Add message to debug log
    public void Log(string message)
    {
        log.AppendLine(message);
        debugLogDisplay.text = log.ToString();
    }

    // Calculate FPS
    private float CalculateInstantFps()
    {
        return 1 / Time.unscaledDeltaTime;
    }

    // Calculate average FPS
    private float CalculateAverageFps(float instantFps)
    {
        totalFps += instantFps;
        numFrames++;
        return totalFps / numFrames;
    }

}