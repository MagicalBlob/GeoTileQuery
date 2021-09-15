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
    /// The layer's rendering properties
    /// </summary>
    RenderingProperties Properties { get; }

    /// <summary>
    /// The layer's GameObject representation
    /// </summary>
    GameObject GameObject { get; }

    /// <summary>
    /// Render the layer
    /// </summary>
    void Render();
}
