using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a tile's layer
/// </summary>
public interface ITileLayer
{
    /// <summary>
    /// The tile this layer belongs to
    /// </summary>
    Tile Tile { get; }

    /// <summary>
    /// The map layer that this tile layer is a part of
    /// </summary>
    ILayer Layer { get; }

    /// <summary>
    /// The tile ID (including layer ID)
    /// </summary>
    string FullId { get; }

    /// <summary>
    /// The tile layer's GameObject representation
    /// </summary>
    GameObject GameObject { get; }

    /// <summary>
    /// The tile layer's state
    /// </summary>
    TileLayerState State { get; }

    /// <summary>
    /// Load the tile layer asynchronously
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation</param>
    Task LoadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Unload the tile layer
    /// </summary>
    void Unload();
}
