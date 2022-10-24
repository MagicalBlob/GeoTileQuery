using System;

/// <summary>
/// Represents a map layer
/// </summary>
public interface ILayer
{
    /// <summary>
    /// The map to which the layer belongs
    /// </summary>
    public Map Map { get; }

    /// <summary>
    /// The layer's id
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The layer's display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The layer's description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The layer's data source
    /// </summary>
    string Source { get; }

    /// <summary>
    /// The layer's latest update date
    /// </summary>
    DateTime LastUpdate { get; }

    /// <summary>
    /// Whether the layer is visible
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// The layer's renderer
    /// </summary>
    ILayerRenderer Renderer { get; }

    /// <summary>
    /// Url to fetch the tile data
    /// </summary>
    string Url { get; }
}
