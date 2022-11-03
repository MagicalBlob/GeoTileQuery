/// <summary>
/// Represents a Point of Interest on the map
/// </summary>
public struct PointOfInterest
{
    /// <summary>
    /// The name of the point of interest
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The coordinates of the point of interest (lat/lon)
    /// </summary>
    public Vector2D Coordinates { get; }

    /// <summary>
    /// Creates a new Point of Interest
    /// </summary>
    /// <param name="name">The name of the point of interest</param>
    /// <param name="coordinates">The coordinates of the point of interest (lat/lon)</param>
    public PointOfInterest(string name, Vector2D coordinates)
    {
        Name = name;
        Coordinates = coordinates;
    }
}