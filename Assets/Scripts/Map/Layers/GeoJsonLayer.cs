/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : ILayer
{
    public string Id { get; }

    public bool Visible { get; set; }

    public ILayerRenderer Renderer { get; }

    /// <summary>
    /// Name of the Feature's property that may be used as an id as an alternative to the actual Feature id if it doesn't exist
    /// </summary>
    public string IdPropertyName { get; }


    /// <summary>
    /// Construct a new GeoJSONLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="visible">Whether the layer is visible</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    public GeoJsonLayer(string id, bool visible, IGeoJsonRenderer renderer, string idPropertyName)
    {
        this.Id = id;
        this.Visible = visible;
        this.Renderer = renderer;
        this.IdPropertyName = idPropertyName;
    }
}