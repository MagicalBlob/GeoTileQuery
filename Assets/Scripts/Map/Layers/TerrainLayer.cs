/// <summary>
/// Represents a Terrain layer
/// </summary>
public class TerrainLayer : ILayer
{
    public string Id { get; }

    public ILayerRenderer Renderer { get; }

    public string tileRasterUrl { get; } // TODO this is a temporary hack to get the tile raster url from the map config

    /// <summary>
    /// Construct a new TerrainLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="rasterUrl">Url to fetch raster tiles</param>
    public TerrainLayer(string id, ITerrainRenderer renderer, string rasterUrl)
    {
        this.Id = id;
        this.Renderer = renderer;
        this.tileRasterUrl = rasterUrl;
    }
}