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
    /// The layer's renderer
    /// </summary>
    ILayerRenderer Renderer { get; }
}
