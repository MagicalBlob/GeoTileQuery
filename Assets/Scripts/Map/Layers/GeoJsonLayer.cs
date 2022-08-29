/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : ILayer
{
    public string Id { get; }

    public ILayerRenderer Renderer { get; }

    /// <summary>
    /// Name of the Feature's property that may be used as an id as an alternative to the actual Feature id if it doesn't exist
    /// </summary>
    public string IdPropertyName { get; }

    /// <summary>
    /// Construct a new GeoJSONLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    public GeoJsonLayer(string id, IGeoJsonRenderer renderer, string idPropertyName)
    {
        this.Id = id;
        this.Renderer = renderer;
        this.IdPropertyName = idPropertyName;
    }
}