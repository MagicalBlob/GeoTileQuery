/// <summary>
/// Enum of tile layer states
/// </summary>
public enum TileLayerState
{
    /// <summary>
    /// The tile layer was initialized
    /// </summary>
    /// <remarks>
    /// This is the initial state of a tile layer
    /// </remarks>
    Initial,
    /// <summary>
    /// The tile layer's data failed to load
    /// </summary>
    LoadFailed,
    /// <summary>
    /// The tile layer's data has been loaded
    /// </summary>
    Loaded,
    /// <summary>
    /// The tile layer has been rendered
    /// </summary>
    Rendered,
    /// <summary>
    /// The tile layer has been unloaded
    /// </summary>
    /// <remarks>
    /// This is the final state of a tile layer
    /// </remarks>
    Unloaded,
}