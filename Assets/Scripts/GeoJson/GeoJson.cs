using Newtonsoft.Json;

/// <summary>
/// The GeoJSON base class
/// </summary>
public class GeoJson
{
    /// <summary>
    /// Reads a JSON text and turns it into a GeoJSON object
    /// </summary>
    /// <param name="json">JSON text input that is a valid GeoJSON text</param>
    /// <returns>The GeoJSON Object represented in the JSON text input</returns>
    public static IGeoJsonObject Parse(string json)
    {
        try
        {
            JsonConverter[] converters = new JsonConverter[] { new GeoJsonObjectConverter(), new PositionConverter() };
            return JsonConvert.DeserializeObject<IGeoJsonObject>(json, converters);
        }
        catch (JsonException e)
        {
            throw new InvalidGeoJsonException("Failed to parse GeoJSON", e);
        }
    }
}