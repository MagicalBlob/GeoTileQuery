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
    /// String representation of the Bounds
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{Min}, {Max}]";
    }
}