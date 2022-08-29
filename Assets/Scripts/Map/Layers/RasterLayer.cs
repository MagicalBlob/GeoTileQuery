/// <summary>
/// Represents a Raster layer
/// </summary>
public class RasterLayer : ILayer
{
    public string Id { get; }

    public bool Visible { get; set; }

    public ILayerRenderer Renderer { get; }

    /// <summary>
    /// Url to fetch raster tiles
    /// </summary>
    public string RasterUrl { get; }

    /// <summary>
    /// Construct a new RasterLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="visible">Whether the layer is visible</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="rasterUrl">Url to fetch raster tiles</param>
    public RasterLayer(string id, bool visible, IRasterRenderer renderer, string rasterUrl)
    {
        this.Id = id;
        this.Visible = visible;
        this.Renderer = renderer;
        this.RasterUrl = rasterUrl;
    }
}