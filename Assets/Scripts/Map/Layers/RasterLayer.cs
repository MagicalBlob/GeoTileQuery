/// <summary>
/// Represents a Raster layer
/// </summary>
public class RasterLayer : ILayer
{
    public string Id { get; }

    public ILayerRenderer Renderer { get; }

    /// <summary>
    /// Url to fetch raster tiles
    /// </summary>
    public string RasterUrl { get; }

    /// <summary>
    /// Construct a new RasterLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="rasterUrl">Url to fetch raster tiles</param>
    public RasterLayer(string id, IRasterRenderer renderer, string rasterUrl)
    {
        this.Id = id;
        this.Renderer = renderer;
        this.RasterUrl = rasterUrl;
    }
}