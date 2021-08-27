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
    public double metersX;

    /// <summary>
    /// y coordinate of the position (Meters)
    /// </summary>
    public double metersY;

    /// <summary>
    /// z coordinate of the position (Meters)
    /// </summary>
    public double metersZ;

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
        Tuple<double, double> metersXY = GlobalMercator.LatLonToMeters(y, x);
        this.metersX = metersXY.Item1;
        this.metersY = metersXY.Item2;
        this.metersZ = z;
    }

    /// <summary>
    /// Access the x, y or z component using [0], [1] or [2] respectively.
    /// </summary>
    /// <param name="index">The component index</param>
    /// <returns></returns>
    public double this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return this.x;
                case 1:
                    return this.y;
                case 2:
                    return this.z;
                default:
                    throw new System.IndexOutOfRangeException("Index was outside the bounds of the array.");
            }
        }
        set
        {
            switch (index)
            {
                case 0:
                    this.x = value;
                    break;
                case 1:
                    this.y = value;
                    break;
                case 2:
                    this.z = value;
                    break;
                default:
                    throw new System.IndexOutOfRangeException("Index was outside the bounds of the array.");
            }
        }
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