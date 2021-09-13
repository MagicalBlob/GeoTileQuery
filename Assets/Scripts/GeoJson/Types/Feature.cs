using UnityEngine;
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
    private IGeometryObject geometry;

    /// <summary>
    /// Properties associated with the feature
    /// </summary>
    private Dictionary<string, object> properties;

    /// <summary>
    /// Feature identifier
    /// </summary>
    private string id;

    /// <summary>
    /// Reference to the GameObject representation of this feature
    /// </summary>
    public GameObject GameObject { get => gameObject; }
    private GameObject gameObject;

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
        this.id = id;
    }

    /// <summary>
    /// Renders the Feature as part of the given layer
    /// </summary>
    /// <param name="feature">The Feature's layer</param>
    /// <param name="renderingProperties">The layer rendering properties</param>
    public void Render(GameObject layer, RenderingProperties renderingProperties)
    {
        if (id == null || id.Length == 0)
        {
            // There wasn't an ID set on the feature
            object value;
            if (renderingProperties.IdPropertyName != null && renderingProperties.IdPropertyName.Length > 0 && properties != null && properties.TryGetValue(renderingProperties.IdPropertyName, out value))
            {
                // A property name was given to try to use as an alternative to the Id and there is a value that matches that property name for this feature
                id = value.ToString();
            }
            else
            {
                id = $"UnknownID_{System.Guid.NewGuid()}";
            }
        };

        gameObject = new GameObject(id);
        gameObject.transform.parent = layer.transform;

        if (geometry != null)
        {
            geometry.Render(this, renderingProperties);
        }
        else
        {
            Logger.LogWarning($"Unable to render the feature {id} as it has no geometry assigned (Unlocated feature)");
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