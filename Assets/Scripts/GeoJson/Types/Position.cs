using System;

/// <summary>
/// Represents a GeoJSON Position
/// </summary>
public struct Position
{
    /// <summary>
    /// x coordinate of the position (WGS84)
    /// </summary>
    public double x;
    /// <summary>
    /// y coordinate of the position (WGS84)
    /// </summary>
    public double y;
    /// <summary>
    /// z coordinate of the position (WGS84)
    /// </summary>
    public double z;

    /// <summary>
    /// x coordinate of the position (Meters)
    /// </summary>
    private double metersX;

    /// <summary>
    /// y coordinate of the position (Meters)
    /// </summary>
    private double metersY;

    /// <summary>
    /// z coordinate of the position (Meters)
    /// </summary>
    private double metersZ;

    /// <summary>
    /// Number of dimensions used by the position
    /// </summary>
    public int Dimensions { get { return 3; } }

    /// <summary>
    /// Constructs a new position with given x, y, z coordinate
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="z">z coordinate</param>
    public Position(double x, double y, double z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;

        // Convert from WGS84 to Meters
        Vector2D meters = GlobalMercator.LatLonToMeters(y, x);
        this.metersX = meters.X;
        this.metersY = meters.Y;
        this.metersZ = z;
    }

    /// <summary>
    /// Get the X coordinate in the Unity scene (in Meters relative to the layer origin)
    /// </summary>
    /// <param name="originX">The layer's origin X coordinate</param>
    /// <returns>X coordinate in the Unity scene</returns>
    public double GetRelativeX(double originX)
    {
        return metersX - originX;
    }

    /// <summary>
    /// Get the Y coordinate in the Unity scene (in Meters relative to the layer origin)
    /// </summary>
    /// <param name="originY">The layer's origin Y coordinate</param>
    /// <returns>Y coordinate in the Unity scene</returns>
    public double GetRelativeY(double originY)
    {
        return metersY - originY;
    }

    /// <summary>
    /// Get the Z coordinate in the Unity scene (in Meters relative to the layer origin)
    /// </summary>
    /// <returns>Z coordinate in the Unity scene</returns>
    public double GetRelativeZ()
    {
        return metersZ;
    }

    /// <summary>
    /// Check if this Position is equal to the given object
    /// </summary>
    /// <param name="obj">The object to compare</param>
    /// <returns>true if given object is also a Position with the same coordinates</returns>
    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != typeof(Position))
        {
            return false;
        }

        Position other = (Position)obj;

        return (this.x == other.x && this.y == other.y && this.z == other.z);
    }

    /// <summary>
    /// Get the position hash code
    /// </summary>
    /// <returns>A hash code for the current position</returns>
    public override int GetHashCode()
    {
        int prime = 31;
        return (int)(prime * x + prime * y - prime * z);
    }

    /// <summary>
    /// Returns a string representation of the position
    /// </summary>
    /// <returns>String representation of the position</returns>
    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}