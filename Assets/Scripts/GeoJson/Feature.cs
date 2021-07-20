using System.Text;
using System.Collections.Generic;

/// <summary>
/// Represents a GeoJSON Feature
/// </summary>
public class Feature : IGeoJsonObject
{
    /// <summary>
    /// The geometry of the feature or NULL if feature is unlocated
    /// </summary>
    public IGeometryObject geometry;

    /// <summary>
    /// Properties associated with the feature
    /// </summary>
    public Dictionary<string, object> properties;

    /// <summary>
    /// Feature identifier
    /// </summary>
    public string id;

    /// <summary>
    /// Constructs a new Feature with given geometry, properties and id
    /// </summary>
    /// <param name="geometry">The Feature's geometry</param>
    /// <param name="properties">The Feature's properties</param>
    /// <param name="id">The Feature's ID</param>
    public Feature(IGeometryObject geometry, Dictionary<string, object> properties, string id)
    {
        this.geometry = geometry;
        this.properties = properties;
        if (id == null || id.Length == 0)
        {
            this.id = $"GeneratedGUID_{System.Guid.NewGuid()}";
        }
        else
        {
            this.id = id;
        }
    }

    /// <summary>
    /// Returns a string representation of the Feature
    /// </summary>
    /// <returns>A string representation of the Feature</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append($"Feature: [Geometry: {geometry}, Properties: ");
        foreach (KeyValuePair<string, object> property in properties)
        {
            builder.Append($"<`{property.Key}`: {property.Value}>, ");
        }
        builder.Append($", ID: {id}]");

        return builder.ToString();
    }
}