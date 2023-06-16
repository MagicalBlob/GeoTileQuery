/// <summary>
/// A class with some pretty printing methods
/// </summary>
public class PrettyPrint
{

    /// <summary>
    /// Converts the given seconds to a human readable time
    /// </summary>
    /// <param name="seconds">The seconds to convert</param>
    /// <returns>The human readable time (hours, minutes, seconds if applicable)</returns>
    public static string HumanTime(float seconds)
    {
        int hours = (int)seconds / 3600;
        int minutes = ((int)seconds % 3600) / 60;
        int secs = (int)seconds % 60;
        if (hours > 0)
        {
            return $"{hours}h {minutes}m {secs}s";
        }
        else if (minutes > 0)
        {
            return $"{minutes}m {secs}s";
        }
        else
        {
            return $"{secs}s";
        }
    }

    /// <summary>
    /// Converts the given meters to a human readable distance
    /// </summary>
    /// <param name="meters">The meters to convert</param>
    /// <returns>The human readable distance (meters or kilometers if applicable)</returns>
    public static string HumanDistance(float meters)
    {
        if (meters > 1000)
        {
            return $"{meters / 1000:0.##}km";
        }
        else
        {
            return $"{meters:0.##}m";
        }
    }
}