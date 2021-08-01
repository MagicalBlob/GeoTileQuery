using UnityEngine;

/// <summary>
/// Represents a GeoJSON Point
/// </summary>
public class Point : IGeometryObject, IGeoJsonObject
{
    /// <summary>
    /// The point coordinates
    /// </summary>
    public Position coordinates;

    /// <summary>
    /// Constructs a new Point with given coordinates
    /// </summary>
    /// <param name="coordinates">The point coordinates</param>
    public Point(Position coordinates)
    {
        this.coordinates = coordinates;
    }

    /// <summary>
    /// Renders the Point as the geometry associated with the given Feature
    /// </summary>
    /// <param name="feature">The parent feature</param>
    public void Render(GameObject feature)
    {
        GeoJson.RenderNode(feature, coordinates);
    }

    /// <summary>
    /// Returns a string representation of the point
    /// </summary>
    /// <returns>String representation of the point</returns>
    public override string ToString()
    {
        return $"Point: {coordinates}";
    }
}