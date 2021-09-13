/// <summary>
/// Rendering properties for layers
/// </summary>
public class RenderingProperties
{
    /// <summary>
    /// Zoom level for the tiles
    /// </summary>
    public int Zoom { get; }

    /// <summary>
    /// The X coordinate of the center of the scene (Meters)
    /// </summary>
    public double CenterX { get; }

    /// <summary>
    /// The Y coordinate of the center of the scene (Meters)
    /// </summary>
    public double CenterY { get; }

    /// <summary>
    /// The Z coordinate of the center of the scene (Meters)
    /// </summary>
    public double CenterZ { get; }

    /// <summary>
    /// Radius of tiles to be loaded (eg: if 3, it will load tiles from origin - 3 to origin + 3)
    /// </summary>
    public int TileRadius { get; }

    /// <summary>
    /// Name of the Feature's property that may be used as an id as an alternative to the actual Feature id if it doesn't exist
    /// </summary>
    public string IdPropertyName { get; }

    /// <summary>
    /// Whether to replace the default node rendering with a custom model
    /// </summary>
    public bool RenderModel { get; }

    /// <summary>
    /// Node height
    /// </summary>
    public double NodeHeight { get { return 5; } }

    /// <summary>
    /// Node radius
    /// </summary>
    public double NodeRadius { get { return 1; } }

    /// <summary>
    /// Edge width
    /// </summary>
    public double EdgeWidth { get { return 1; } } //0.025; //TODO check the rendering code but I think this might actually be rendering as half the width

    /// <summary>
    /// Create a new rendering properties object
    /// </summary>
    /// <param name="zoom">Zoom level for the tiles</param>
    /// <param name="centerX">The X coordinate of the center of the scene (Meters)</param>
    /// <param name="centerY">The Y coordinate of the center of the scene (Meters)</param>
    /// <param name="centerZ">The Z coordinate of the center of the scene (Meters)</param>
    /// <param name="tileRadius">Radius of tiles to be loaded</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    /// <param name="renderModel">Whether to replace the default node rendering with a custom model</param>
    public RenderingProperties(int zoom, double centerX, double centerY, double centerZ, int tileRadius, string idPropertyName, bool renderModel)
    {
        this.Zoom = zoom;
        this.CenterX = centerX;
        this.CenterY = centerY;
        this.CenterZ = centerZ;
        this.TileRadius = tileRadius;
        this.IdPropertyName = idPropertyName;
        this.RenderModel = renderModel;
    }
}
