using UnityEngine;

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
    /// The layer's origin in the scene (Meters)
    /// </summary>
    Vector2D Origin { get; }

    /// <summary>
    /// Zoom level for the layer's tiles
    /// </summary>
    int Zoom { get; }

    /// <summary>
    /// Radius of tiles to be loaded (eg: if 3, it will load tiles from origin - 3 to origin + 3 in both axis)
    /// </summary>
    int TileViewDistance { get; }

    /// <summary>
    /// The layer's renderer
    /// </summary>
    ILayerRenderer Renderer { get; }

    /// <summary>
    /// The layer's GameObject representation
    /// </summary>
    GameObject GameObject { get; }

    /// <summary>
    /// Render the layer
    /// </summary>
    void Render();
}
