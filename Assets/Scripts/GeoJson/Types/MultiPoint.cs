using UnityEngine;
using System;

/// <summary>
/// Represents a GeoJSON MultiPoint
/// </summary>
public class MultiPoint : IGeometryObject, IGeoJsonObject
{
    /// <summary>
    /// The points coordinates
    /// </summary>
    public Position[] coordinates;

    /// <summary>
    /// Constructs a new MultiPoint with given coordinates
    /// </summary>
    /// <param name="coordinates">The points coordinates</param>
    public MultiPoint(Position[] coordinates)
    {
        this.coordinates = coordinates;
    }

    /// <summary>
    /// Renders the MultiPoint as the geometry associated with the given Feature
    /// </summary>
    /// <param name="feature">The parent feature</param>
    /// <param name="renderingProperties">The layer rendering properties</param>
    public void Render(GameObject feature, RenderingProperties renderingProperties)
    {
        foreach (Position position in coordinates)
        {
            GeoJsonRenderer.RenderNode(feature, position, renderingProperties);
        }
    }

    /// <summary>
    /// Returns a string representation of the multipoint
    /// </summary>
    /// <returns>String representation of the multipoint</returns>
    public override string ToString()
    {
        return $"MultiPoint: ({String.Join(", ", coordinates)})";
    }
}