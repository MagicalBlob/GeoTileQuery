/// <summary>
/// Represents a Raster layer
/// </summary>
public class RasterLayer : ILayer
{
    public string Id { get; }

    public bool Visible { get; set; }

    public ILayerRenderer Renderer { get; }

    public string Url { get; }

    /// <summary>
    /// Construct a new RasterLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="visible">Whether the layer is visible</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="url">Url to fetch the tile data</param>
    public RasterLayer(string id, bool visible, IRasterRenderer renderer, string url)
    {
        this.Id = id;
        this.Visible = visible;
        this.Renderer = renderer;
        this.Url = url;
    }
}