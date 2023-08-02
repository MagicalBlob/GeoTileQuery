using Newtonsoft.Json.Linq;
using UnityEngine;

public class OSMServices
{

    /// <summary>
    /// Geocodes the given address if possible
    /// </summary>
    /// <param name="address">The address to geocode</param>
    /// <returns>The coordinates (lat/lon) of the address, or null if the address could not be geocoded</returns>
    /// <remarks>
    /// This method uses the Nominatim API to geocode the address.
    /// Only the first result is returned.
    /// </remarks>
    public static GeocodingQueryResult? GetCoordinates(string address)
    {
        // TODO: Currently hardcoded to country code PT and adds Lisboa as the city, make this configurable
        string nominatimUrl = $"https://nominatim.openstreetmap.org/search?format=json&countrycodes=pt&q={address}, Lisboa";
        try
        {
            string nominatimResponse = MainController.client.GetStringAsync(nominatimUrl).Result;
            JArray nominatimResults = JArray.Parse(nominatimResponse);
            if (nominatimResults.Count == 0)
            {
                return null;
            }
            if (nominatimResults[0]["display_name"] == null || nominatimResults[0]["lat"] == null || nominatimResults[0]["lon"] == null)
            {
                Debug.LogWarning($"Nominatim result for '{address}' is missing some fields. display_name: {nominatimResults[0]["display_name"]}, lat: {nominatimResults[0]["lat"]}, lon: {nominatimResults[0]["lon"]}");
                Debug.LogWarning(nominatimResults[0].ToString());
                return null;
            }
            //return new Vector2D(double.Parse(nominatimResults[0]["lat"].ToString()), double.Parse(nominatimResults[0]["lon"].ToString()));
            return new GeocodingQueryResult(nominatimResults[0]["display_name"].ToString(), new Vector2D(double.Parse(nominatimResults[0]["lat"].ToString()), double.Parse(nominatimResults[0]["lon"].ToString())));
        }
        catch (System.Net.Http.HttpRequestException e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    /// <summary>
    /// A geocoding query result
    /// </summary>
    public struct GeocodingQueryResult
    {
        /// <summary>
        /// The name of the result
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// The coordinates of the result
        /// </summary>
        public Vector2D Coordinates;

        /// <summary>
        /// Creates a new geocoding query result
        /// </summary>
        /// <param name="name">The name of the result</param>
        /// <param name="coordinates">The coordinates of the result</param>
        public GeocodingQueryResult(string name, Vector2D coordinates) : this()
        {
            this.DisplayName = name;
            this.Coordinates = coordinates;
        }
    }

    /// <summary>
    /// Reverse geocodes the given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates (lat/lon) to reverse geocode</param>
    /// <returns>The address at the coordinates</returns>
    /// <remarks>
    /// This method uses the Nominatim API to reverse geocode the coordinates.
    /// </remarks>
    public static string GetAddress(Vector2D coordinates)
    {
        string nominatimUrl = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={coordinates.X}&lon={coordinates.Y}&addressdetails=0";
        try
        {
            string nominatimResponse = MainController.client.GetStringAsync(nominatimUrl).Result;
            if (JObject.Parse(nominatimResponse)["display_name"] == null)
            {
                Debug.LogWarning($"Nominatim result for '{coordinates}' is missing display_name field");
                Debug.LogWarning(nominatimResponse);
                return "<i>Unable to get address</i>";
            }
            return JObject.Parse(nominatimResponse)["display_name"].ToString();
        }
        catch (System.Net.Http.HttpRequestException e)
        {
            Debug.LogException(e);
            return "<i>Unable to get address</i>";
        }
    }

    /// <summary>
    /// Fills the route with the route between the given coordinates, if possible
    /// </summary>
    /// <param name="route">The route to fill</param>
    /// <param name="from">Start coordinates</param>
    /// <param name="to">End coordinates</param>
    /// <returns>true if the route was found, false otherwise</returns>
    public static bool GetRoute(Route route, Vector2D from, Vector2D to)
    {
        string osrmUrlCar = $"https://router.project-osrm.org/route/v1/driving/{from.Y},{from.X};{to.Y},{to.X}?steps=true";
        //string osrmUrlFoot = $"https://routing.openstreetmap.de/routed-foot/route/v1/driving/{from.Y},{from.X};{to.Y},{to.X}?steps=true";
        try
        {
            string osrmResponse = MainController.client.GetStringAsync(osrmUrlCar).Result;
            JObject osrmResult = JObject.Parse(osrmResponse);

            // Check if the request was successful
            if (!osrmResult.HasValues || osrmResult["code"].ToString() != "Ok")
            {
                return false;
            }

            route.Distance = float.Parse(osrmResult["routes"][0]["legs"][0]["distance"].ToString());
            route.Duration = float.Parse(osrmResult["routes"][0]["legs"][0]["duration"].ToString());
            route.SetStart(from);
            route.SetEnd(to);
            foreach (JObject step in osrmResult["routes"][0]["legs"][0]["steps"])
            {
                route.AddNode(new Vector2D(double.Parse(step["maneuver"]["location"][1].ToString()), double.Parse(step["maneuver"]["location"][0].ToString())));
            }

            return true;
        }
        catch (System.Net.Http.HttpRequestException e)
        {
            Debug.LogException(e);
            return false;
        }
    }
}