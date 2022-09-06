/// <summary>
/// Enum of tile states
/// </summary>
public enum TileState
{
    /// <summary>
    /// The tile was initialized
    /// </summary>
    /// <remarks>
    /// This is the initial state of a tile
    /// </remarks>
    Initial,
    /// <summary>
    /// The tile's terrain data failed to load
    /// </summary>
    TerrainLoadFailed,
    /// <summary>
    /// The tile's terrain data has been loaded
    /// </summary>
    TerrainLoaded,
    /// <summary>
    /// The tile's layers failed to load
    /// </summary>
    LayersLoadFailed,
    /// <summary>
    /// The tile has been loaded
    /// <summary>
    Loaded,
    /// The tile has been unloaded
    /// </summary>
    /// <remarks>
    /// This is the final state of a tile
    /// </remarks>
    Unloaded,
}