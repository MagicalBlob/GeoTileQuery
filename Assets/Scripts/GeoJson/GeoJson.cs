using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GeoJson
{
    public static IGeoJsonObject ParseGeoJson(string json)
    {
        try
        {
            JObject o = JObject.Parse(json);
            if (o["type"] != null)
            {
                switch (o["type"].ToString())
                {
                    case "Point":
                        return new Point(); //TODO this
                    default:
                        // Invalid GeoJSON: Unknown type, GeoJSON types are not extensible
                        throw new InvalidGeoJsonException($"Unknown type ({o["type"]})");
                }
            }
            else
            {
                // Invalid GeoJSON: Missing type
                throw new InvalidGeoJsonException("Missing type");
            }
        }
        catch (JsonReaderException e)
        {
            // Invalid GeoJSON: Failed to parse JSON
            throw new InvalidGeoJsonException("Failed to parse JSON", e);
        }
    }
}