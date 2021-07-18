using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Converts a Position object to and from JSON
/// </summary>
public class PositionConverter : JsonConverter
{
    /// <summary>
    ///  Determines whether this instance can convert the specified object type
    /// </summary>
    /// <param name="objectType">Type of the object</param>
    /// <returns>true if this instance can convert the specified object type; otherwise, false</returns>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Position);
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

        if (token.Type == JTokenType.Array)
        {
            JArray array = (JArray)token;
            if (array.Count == 2 && array[0].Type == JTokenType.Float && array[1].Type == JTokenType.Float)
            {
                // Parsed a 2D position
                return new Position(array[0].Value<double>(), array[1].Value<double>());
            }
            else if (array.Count == 3 && array[0].Type == JTokenType.Float && array[1].Type == JTokenType.Float && array[2].Type == JTokenType.Float)
            {
                // Parsed a 3D position
                return new Position(array[0].Value<double>(), array[1].Value<double>(), array[2].Value<double>());
            }
        }

        throw new JsonSerializationException("Failed to parse as Position");
    }

    /// <summary>
    /// Writes the JSON representation of the object
    /// </summary>
    /// <param name="writer">The JsonWriter to write to</param>
    /// <param name="value">The value</param>
    /// <param name="serializer">The calling serializer</param>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Position position = (Position)value;
        writer.WriteStartArray();
        writer.WriteValue(position.x);
        writer.WriteValue(position.y);
        writer.WriteValue(position.z);
        writer.WriteEndArray();
    }
}