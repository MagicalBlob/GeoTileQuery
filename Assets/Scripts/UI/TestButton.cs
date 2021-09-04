using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.Networking;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    public GameObject testLayer;

    private void Start() => testButton.onClick.AddListener(ButtonClicked);

    private void ButtonClicked()
    {
        Logger.Log("Button pressed!");
        try
        {
            Tuple<double, double> centerXY = GlobalMercator.LatLonToMeters(52.157039, 5.410851); // TODO: we really need a Vector2D, this Tuple<double, double> everywhere is ridiculous
            RenderingProperties properties = new RenderingProperties(centerXY.Item1, centerXY.Item2, 0);

            StartCoroutine(LoadTile("test", 15, 16876, 10800, properties)); // 200 Tile
            StartCoroutine(LoadTile("test", 15, 16875, 10799, properties)); // 200 Tile
            StartCoroutine(LoadTile("test", 15, 16874, 10799, properties)); // 200 Tile
            //TODO this one blows up StartCoroutine(LoadTile("test", 15, 16874, 10800, properties)); // 200 Tile
            StartCoroutine(LoadTile("test", 15, 16876, 100, properties)); // 200 Empty Tile
            StartCoroutine(LoadTile("nothing", 15, 16876, 100, properties)); // 404 No Layer
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }

    /// <summary>
    /// Load and render the tile from the specified layer with given zoom level and coordinates
    /// </summary>
    /// <param name="layer">GeoJSON layer name</param>
    /// <param name="z">Zoom level</param>
    /// <param name="x">Tile X coordinate (Google)</param>
    /// <param name="y">Tile Y coordinate (Google)</param>
    /// <param name="properties">Layer rendering properties</param>
    private IEnumerator LoadTile(string layer, int z, long x, long y, RenderingProperties properties)
    {
        string uri = $"https://tese.flamino.eu/api/tiles?layer={layer}&z={z}&x={x}&y={y}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Logger.LogError($"{layer}/{z}/{x}/{y}: Connection Error when loading tile ({webRequest.error})");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Logger.LogError($"{layer}/{z}/{x}/{y}: Data Processing Error when loading tile ({webRequest.error})");
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Logger.LogError($"{layer}/{z}/{x}/{y}: HTTP Error when loading tile ({webRequest.error})");
                    break;
                case UnityWebRequest.Result.Success:
                    RenderTile(webRequest.downloadHandler.text, properties);
                    Logger.Log($"{layer}/{z}/{x}/{y}: Loaded tile!");
                    break;
            }
        }
    }

    /// <summary>
    /// Parse the tile's GeoJSON text and render it according to given properties
    /// </summary>
    /// <param name="geoJsonText">The tile's GeoJSON text</param>
    /// <param name="properties">The tile rendering properties</param>
    private void RenderTile(string geoJsonText, RenderingProperties properties)
    {
        try
        {
            IGeoJsonObject geoJson = GeoJson.Parse(geoJsonText);

            if (geoJson.GetType() == typeof(FeatureCollection))
            {
                FeatureCollection collection = (FeatureCollection)geoJson;
                collection.Render(testLayer, properties);
            }
            else
            {
                // Can't render as a layer. Root isn't a FeatureCollection
                throw new InvalidGeoJsonException("Can't render as a tile. Root isn't a FeatureCollection");
            }
        }
        catch (InvalidGeoJsonException e)
        {
            Logger.LogException(e);
        }
    }
}