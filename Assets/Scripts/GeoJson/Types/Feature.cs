using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System;

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
    /// Get the value for the Feature's property with given key
    /// </summary>
    /// <typeparam name="ValueType">Type of the value</typeparam>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists, default value otherwise</returns>
    public ValueType GetProperty<ValueType>(string key)
    {
        ValueType value = default(ValueType);

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                // Check if value is of the requested type
                if (obj.GetType() == typeof(ValueType))
                {
                    value = (ValueType)obj;
                }
                else
                {
                    Logger.Log($"Property value for {key} is of type {obj.GetType()} and not {typeof(ValueType)}");
                }
            }
            else
            {
                Logger.LogWarning($"Property value for {key} is null");
            }
        }
        else
        {
            Logger.LogWarning($"Unable to get property value for {key}");
        }

        return value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a string
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a string, empty string otherwise</returns>
    public string GetPropertyAsString(string key)
    {
        string value = string.Empty;

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                value = obj.ToString();
            }
            else
            {
                Logger.LogWarning($"Property value for {key} is null");
            }
        }
        else
        {
            Logger.LogWarning($"Unable to get property value for {key}");
        }

        return value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a double
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a double, 0 otherwise</returns>
    public double GetPropertyAsDouble(string key)
    {
        double value = 0;

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                try
                {
                    value = Convert.ToDouble(obj);
                }
                catch (FormatException)
                {
                    Logger.LogWarning($"The {obj.GetType().Name} value {obj} is not recognized as a valid Double value.");
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine($"Conversion of the {obj.GetType().Name} value {obj} to a Double is not supported.");
                }
            }
            else
            {
                Logger.LogWarning($"Property value for {key} is null");
            }
        }
        else
        {
            Logger.LogWarning($"Unable to get property value for {key}");
        }

        return value;
    }

    /// <summary>
    /// Render the Feature
    /// </summary>
    /// <param name="tile">The Feature's tile</param>
    public void Render(GeoJsonTileLayer tile)
    {
        if (id == null || id.Length == 0)
        {
            // There wasn't an ID set on the feature
            if (((GeoJsonLayer)tile.Layer).IdPropertyName != null)
            {
                // A property name was given to try to use as an alternative to the Id and there is a value that matches that property name for this feature
                id = GetPropertyAsString(((GeoJsonLayer)tile.Layer).IdPropertyName);
            }
            else
            {
                id = $"UnknownID_{System.Guid.NewGuid()}";
            }
        };

        // Setup the gameobject
        gameObject = new GameObject(id);
        gameObject.transform.parent = tile.GameObject.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.rotation = tile.GameObject.transform.rotation;

        if (geometry != null)
        {
            geometry.Render(tile, this);
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