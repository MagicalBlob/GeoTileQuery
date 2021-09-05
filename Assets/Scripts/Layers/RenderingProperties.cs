/// <summary>
/// Rendering properties for layers
/// </summary>
public class RenderingProperties
{
    /// <summary>
    /// The X coordinate of the center of the scene (Meters)
    /// </summary>
    public double centerX;

    /// <summary>
    /// The Y coordinate of the center of the scene (Meters)
    /// </summary>
    public double centerY;

    /// <summary>
    /// The Z coordinate of the center of the scene (Meters)
    /// </summary>
    public double centerZ;

    /// <summary>
    /// Node height
    /// </summary>
    public double nodeHeight = 5;

    /// <summary>
    /// Node radius
    /// </summary>
    public double nodeRadius = 1;

    /// <summary>
    /// Edge width
    /// </summary>
    public double edgeWidth = 1;//0.025; //TODO check the rendering code but I think this might actually be rendering as half the width

    /// <summary>
    /// Create a new rendering properties object
    /// </summary>
    /// <param name="centerX">The X coordinate of the center of the scene (Meters)</param>
    /// <param name="centerY">The Y coordinate of the center of the scene (Meters)</param>
    /// <param name="centerZ">The Z coordinate of the center of the scene (Meters)</param>
    public RenderingProperties(double centerX, double centerY, double centerZ)
    {
        this.centerX = centerX;
        this.centerY = centerY;
        this.centerZ = centerZ;
    }
}
