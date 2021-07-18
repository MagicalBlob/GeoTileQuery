/// <summary>
/// Represents a GeoJSON Position
/// </summary>
public struct Position
{
    /// <summary>
    /// x coordinate of the position
    /// </summary>
    public double x;
    /// <summary>
    /// y coordinate of the position
    /// </summary>
    public double y;
    /// <summary>
    /// z coordinate of the position
    /// </summary>
    public double z;

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
    /// Returns a string representation of the position
    /// </summary>
    /// <returns>String representation of the position</returns>
    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}