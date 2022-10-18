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
    /// Feature identifier
    /// </summary>
    public string Id { get => id; }

    /// <summary>
    /// Feature identifier (including tile layer)
    /// </summary>
    public string FullId { get { return $"{TileLayer.FullId}/{Id}"; } }

    /// <summary>
    /// The tile layer that this feature belongs to
    /// </summary>
    public GeoJsonTileLayer TileLayer { get; private set; }

    /// <summary>
    /// Reference to the GameObject representation of this feature
    /// </summary>
    public GameObject GameObject { get; private set; }

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
    /// Get the value for the Feature's property with given key as a nullable string
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a string, null otherwise</returns>
    public string GetPropertyAsNullableString(string key)
    {
        string value = null;

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                value = obj.ToString();
            }
        }
        else
        {
            Debug.LogWarning($"Unable to get property value for {key}");
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
        string value = GetPropertyAsNullableString(key);
        return value == null ? string.Empty : value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a nullable double
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a double, null otherwise</returns>
    public double? GetPropertyAsNullableDouble(string key)
    {
        double? value = null;

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
                    Debug.LogWarning($"The {obj.GetType().Name} value {obj} is not recognized as a valid Double value.");
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"Conversion of the {obj.GetType().Name} value {obj} to a Double is not supported.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Unable to get property value for {key}");
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
        double? value = GetPropertyAsNullableDouble(key);
        return value == null ? 0 : value.Value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a nullable integer
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as an integer, null otherwise</returns>
    public int? GetPropertyAsNullableInt(string key)
    {
        int? value = null;

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                try
                {
                    value = Convert.ToInt32(obj);
                }
                catch (FormatException)
                {
                    Debug.LogWarning($"The {obj.GetType().Name} value {obj} is not recognized as a valid Integer value.");
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"Conversion of the {obj.GetType().Name} value {obj} to an Integer is not supported.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Unable to get property value for {key}");
        }

        return value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as an integer
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as an integer, 0 otherwise</returns>
    public int GetPropertyAsInt(string key)
    {
        int? value = GetPropertyAsNullableInt(key);
        return value == null ? 0 : value.Value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a nullable boolean
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a boolean, null otherwise</returns>
    public bool? GetPropertyAsNullableBool(string key)
    {
        bool? value = null;

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                try
                {
                    value = Convert.ToBoolean(obj);
                }
                catch (FormatException)
                {
                    Debug.LogWarning($"The {obj.GetType().Name} value {obj} is not recognized as a valid Boolean value.");
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"Conversion of the {obj.GetType().Name} value {obj} to a Boolean is not supported.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Unable to get property value for {key}");
        }

        return value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a boolean
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a boolean, false otherwise</returns>
    public bool GetPropertyAsBool(string key)
    {
        bool? value = GetPropertyAsNullableBool(key);
        return value == null ? false : value.Value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a nullable datetime
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a datetime, null otherwise</returns>
    public DateTime? GetPropertyAsNullableDateTime(string key)
    {
        DateTime? value = null;

        object obj;
        // Check if we have properties and if there's an entry with given key
        if (properties != null && properties.TryGetValue(key, out obj))
        {
            // Check if value isn't null
            if (obj != null)
            {
                try
                {
                    value = Convert.ToDateTime(obj);
                }
                catch (FormatException)
                {
                    Debug.LogWarning($"The {obj.GetType().Name} value {obj} is not recognized as a valid DateTime value.");
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"Conversion of the {obj.GetType().Name} value {obj} to a DateTime is not supported.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"Unable to get property value for {key}");
        }

        return value;
    }

    /// <summary>
    /// Get the value for the Feature's property with given key as a datetime
    /// </summary>
    /// <param name="key">Property key</param>
    /// <returns>The value for the feature's property if it exists as a datetime, DateTime.MinValue otherwise</returns>
    public DateTime GetPropertyAsDateTime(string key)
    {
        DateTime? value = GetPropertyAsNullableDateTime(key);
        return value == null ? DateTime.MinValue : value.Value;
    }

    /// <summary>
    /// Render the Feature
    /// </summary>
    /// <param name="tileLayer">The Feature's tile layer</param>
    public void Render(GeoJsonTileLayer tileLayer)
    {
        if (id == null || id.Length == 0)
        {
            // There wasn't an ID set on the feature
            if (((GeoJsonLayer)tileLayer.Layer).IdPropertyName != null)
            {
                // A property name was given to try to use as an alternative to the Id and there is a value that matches that property name for this feature
                id = GetPropertyAsNullableString(((GeoJsonLayer)tileLayer.Layer).IdPropertyName);
                if (id == null)
                {
                    // There wasn't a value for the given property
                    id = $"UnknownID_{System.Guid.NewGuid()}";
                }
            }
            else
            {
                id = $"UnknownID_{System.Guid.NewGuid()}";
            }
        };

        TileLayer = tileLayer;

        // Setup the gameobject
        GameObject = new GameObject(id);
        GameObject.transform.parent = tileLayer.GameObject.transform;
        GameObject.transform.localPosition = Vector3.zero;
        GameObject.transform.rotation = tileLayer.GameObject.transform.rotation;
        GameObject.AddComponent<FeatureBehaviour>().Feature = this;

        // Check if the feature has a Geometry object, otherwise it's unlocated and there's nothing to render
        if (geometry != null)
        {
            geometry.Render(tileLayer, this);
        }
        else
        {
            Debug.LogWarning($"Unable to render the feature {tileLayer.FullId}/{id} as it has no geometry assigned (Unlocated feature)");
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