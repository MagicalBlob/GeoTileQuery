/// <summary>
/// Represents a map layer
/// </summary>
public interface ILayer
{
    /// <summary>
    /// The layer's id
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Whether the layer is visible
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// The layer's renderer
    /// </summary>
    ILayerRenderer Renderer { get; }
}
