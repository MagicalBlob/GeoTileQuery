using UnityEngine;

/// <summary>
/// Represents a layer's tile
/// </summary>
public interface ITile
{
    /// <summary>
    /// The tile's layer
    /// </summary>
    ILayer Layer { get; }

    /// <summary>
    /// The tile's zoom level
    /// </summary>
    int Zoom { get; }

    /// <summary>
    /// The tile's X coordinate
    /// </summary>
    int X { get; }

    /// <summary>
    /// The tile's Y coordinate
    /// </summary>
    int Y { get; }

    /// <summary>
    /// The tile ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The bounds of the tile in meters (relative to the layer origin)
    /// </summary>
    Bounds Bounds { get; }

    /// <summary>
    /// The center of the tile in meters (relative to the layer origin)
    /// </summary>
    Vector2D Center { get; }

    /// <summary>
    /// The tile's GameObject representation
    /// </summary>
    GameObject GameObject { get; }
}
