using UnityEngine;

/// <summary>
/// Represents a GeoJSON Geometry Object
/// </summary>
public interface IGeometryObject : IGeoJsonObject
{
    /// <summary>
    /// Renders the Geometry Object as the geometry associated with the given Feature
    /// </summary>
    /// <param name="feature">The parent feature</param>
    void Render(GameObject feature);
}