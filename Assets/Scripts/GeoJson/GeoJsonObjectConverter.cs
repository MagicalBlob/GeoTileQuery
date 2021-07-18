using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Converts a GeoJSON Object to and from JSON
/// </summary>
public class GeoJsonObjectConverter : JsonConverter
{
    /// <summary>
    ///  Determines whether this instance can convert the specified object type
    /// </summary>
    /// <param name="objectType">Type of the object</param>
    /// <returns>true if this instance can convert the specified object type; otherwise, false</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(IGeoJsonObject);
    }

    /// <summary>
    /// Reads the JSON representation of the object
    /// </summary>
    /// <param name="reader">The JsonReader to read from</param>
    /// <param name="objectType">Type of the object</param>
    /// <param name="existingValue">The existing value of object being read</param>
    /// <param name="serializer">The calling serializer</param>
    /// <returns>The object value</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);

        if (token.Type == JTokenType.Object)
        {
            // A GeoJSON object is a JSON object.
            JObject jsonObject = (JObject)token;

            if (jsonObject["type"] != null)
            {
                // A GeoJSON object has a member with the name "type".

                // TODO: A GeoJSON object MAY have a "bbox" member, the value of which MUST be a bounding box array (see Section 5).
                // TODO: A GeoJSON object MAY have other members (see Section 6).

                // The value of the member MUST be one of the GeoJSON types.
                switch ((String)jsonObject["type"])
                {
                    case "FeatureCollection":
                        throw new NotImplementedException(); // TODO FeatureCollection
                    case "Feature":
                        throw new NotImplementedException(); // TODO Feature
                    case "Point":
                        return jsonObject.ToObject<Point>(serializer);
                    case "LineString":
                        return jsonObject.ToObject<LineString>(serializer);
                    case "MultiPoint":
                        return jsonObject.ToObject<MultiPoint>(serializer);
                    case "Polygon":
                        throw new NotImplementedException(); // TODO Polygon
                    case "MultiLineString":
                        throw new NotImplementedException(); // TODO MultiLineString
                    case "MultiPolygon":
                        throw new NotImplementedException(); // TODO MultiPolygon
                    case "GeometryCollection":
                        throw new NotImplementedException(); // TODO GeometryCollection
                    default:
                        // Invalid GeoJSON: Unknown type, GeoJSON types are not extensible
                        throw new InvalidGeoJsonException($"Unknown type ({jsonObject["type"]}), GeoJSON types are not extensible");
                }
            }
            else
            {
                // Invalid GeoJSON: Missing type
                throw new InvalidGeoJsonException("Missing type");
            }
        }
        else
        {
            // Invalid GeoJSON: Not a JSON object
            throw new InvalidGeoJsonException("Not a JSON Object");
        }


        throw new JsonSerializationException("Failed to parse as a GeoJSON Object");
    }

    /// <summary>
    /// Writes the JSON representation of the object
    /// </summary>
    /// <param name="writer">The JsonWriter to write to</param>
    /// <param name="value">The value</param>
    /// <param name="serializer">The calling serializer</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException(); // TODO Implement this?
    }
}