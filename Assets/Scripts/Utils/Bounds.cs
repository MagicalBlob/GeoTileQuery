/// <summary>
/// Represents a rectangular area with Min and Max as its two opposite corners
/// </summary>
public struct Bounds
{
    public Vector2D Min { get; }
    public Vector2D Max { get; }

    /// <summary>
    /// Get the center point of the bounds
    /// </summary>
    /// <returns>Center point vector</returns>
    public Vector2D Center
    {
        get
        {
            return new Vector2D((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);
        }
    }

    /// <summary>
    /// Get the width of the bounds (meters)
    /// </summary>
    public double Width { get { return Max.X - Min.X; } }

    /// <summary>
    /// Get the height of the bounds (meters)
    /// </summary>
    public double Height { get { return Max.Y - Min.Y; } }

    public Bounds(Vector2D min, Vector2D max)
    {
        this.Min = min;
        this.Max = max;
    }

    /// <summary>
    /// Check if the given point is inside the bounds
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <returns>True if the point is inside the bounds</returns>
    /// <remarks>Point is considered to be inside the bounds if it is on the bounds' edge</remarks>
    public bool Contains(Vector2D point)
    {
        return point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;
    }

    /// <summary>
    /// String representation of the Bounds
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{Min}, {Max}]";
    }
}