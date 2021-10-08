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

    public void Render(GeoJsonTile tile, Feature feature)
    {
        foreach (IGeometryObject geometry in geometries)
        {
            geometry.Render(tile, feature);
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