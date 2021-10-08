/// <summary>
/// Rendering properties for layers
/// </summary>
public class RenderingProperties
{
    /// <summary>
    /// The layer's origin in the scene (Meters)
    /// </summary>
    public Vector2D Origin { get; }

    /// <summary>
    /// Zoom level for the tiles
    /// </summary>
    public int Zoom { get; }

    /// <summary>
    /// Radius of tiles to be loaded (eg: if 3, it will load tiles from origin - 3 to origin + 3 in both axis)
    /// </summary>
    public int TileViewDistance { get; }

    /// <summary>
    /// Name of the Feature's property that may be used as an id as an alternative to the actual Feature id if it doesn't exist
    /// </summary>
    public string IdPropertyName { get; }

    /// <summary>
    /// Whether to replace the default node rendering with a custom model
    /// </summary>
    public bool RenderModel { get; }

    /// <summary>
    /// Name of the model to render if renderModel is true. If null, it tries to find the value in the GeoJSON
    /// </summary>
    public string ModelName { get; }

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
    /// Number of divisions that each terrain tile will be split (eg: if 4, each terrain tile will be split into 4 x 4 subdivisions)
    /// </summary>
    public int TerrainTileDivisions { get { return 4; } }

    /// <summary>
    /// Render terrain mesh using elevation data
    /// </summary>
    public bool ElevatedTerrain { get { return false; } }

    /// <summary>
    /// Create a new rendering properties object
    /// </summary>
    /// <param name="origin">The layer's origin in the scene (Meters)</param>
    /// <param name="zoom">Zoom level for the tiles</param>
    /// <param name="tileRadius">Radius of tiles to be loaded</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    /// <param name="renderModel">Whether to replace the default node rendering with a custom model</param>
    /// <param name="modelName">Name of the model to render if renderModel is true. If null, it tries to find the value in the GeoJSON</param>
    public RenderingProperties(Vector2D origin, int zoom, int tileRadius, string idPropertyName, bool renderModel, string modelName)
    {
        this.Origin = origin;
        this.Zoom = zoom;
        this.TileViewDistance = tileRadius;
        this.IdPropertyName = idPropertyName;
        this.RenderModel = renderModel;
        this.ModelName = modelName;
    }
}
