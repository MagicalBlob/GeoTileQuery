using UnityEngine;
using System.Text;

/// <summary>
/// Represents a GeoJSON GeometryCollection
/// </summary>
public class GeometryCollection : IGeometryObject, IGeoJsonObject
{
    /// <summary>
    /// The geometry objects in the collection
    /// </summary>
    public IGeometryObject[] geometries;

    /// <summary>
    /// Constructs a new GeometryCollection with given geometry objects
    /// </summary>
    /// <param name="geometries"></param>
    public GeometryCollection(IGeometryObject[] geometries)
    {
        this.geometries = geometries;
    }

    /// <summary>
    /// Renders the GeometryCollection as the geometry associated with the given Feature
    /// </summary>
    /// <param name="feature">The parent feature</param>
    /// <param name="properties">The layer rendering properties</param>
    public void Render(GameObject feature, RenderingProperties properties)
    {
        foreach (IGeometryObject geometry in geometries)
        {
            geometry.Render(feature, properties);
        }
    }

    /// <summary>
    /// Returns a string representation of the GeometryCollection
    /// </summary>
    /// <returns>A string representation of the GeometryCollection</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("GeometryCollection: ");

        foreach (IGeometryObject geometry in geometries)
        {
            builder.Append($"({geometry}), ");
        }

        return builder.ToString();
    }
}