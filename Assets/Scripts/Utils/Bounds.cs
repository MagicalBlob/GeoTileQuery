public struct Bounds
{
    public Vector2D Min { get; }
    public Vector2D Max { get; }

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

    /// <summary>
    /// Get the center point of the bounds
    /// </summary>
    /// <returns>Center point vector</returns>
    public Vector2D Center()
    {
        return new Vector2D((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);
    }
}