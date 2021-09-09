/// <summary>
/// Rendering properties for layers
/// </summary>
public class RenderingProperties
{
    /// <summary>
    /// Zoom level for the tiles
    /// </summary>
    public int zoom;

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
    /// Radius of tiles to be loaded (eg: if 3, it will load tiles from origin - 3 to origin + 3)
    /// </summary>
    public int tileRadius;

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
    /// <param name="zoom">Zoom level for the tiles</param>
    /// <param name="centerX">The X coordinate of the center of the scene (Meters)</param>
    /// <param name="centerY">The Y coordinate of the center of the scene (Meters)</param>
    /// <param name="centerZ">The Z coordinate of the center of the scene (Meters)</param>
    /// <param name="tileRadius">Radius of tiles to be loaded</param>
    public RenderingProperties(int zoom, double centerX, double centerY, double centerZ, int tileRadius)
    {
        this.zoom = zoom;
        this.centerX = centerX;
        this.centerY = centerY;
        this.centerZ = centerZ;
        this.tileRadius = tileRadius;
    }
}
