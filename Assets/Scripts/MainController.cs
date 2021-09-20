using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Http;

public class MainController : MonoBehaviour
{
    public Button testButton;

    public GameObject map;

    public static readonly HttpClient client = new HttpClient();

    private static string mapboxAccessToken;
    public static string MapboxAccessToken { get { return mapboxAccessToken; } }

    private void Start()
    {
        TextAsset secretsFile = Resources.Load<TextAsset>("Config/secrets");
        mapboxAccessToken = secretsFile.text;

        testButton.onClick.AddListener(ButtonClicked);
    }

    private void ButtonClicked()
    {
        Logger.Log("Button pressed!");
        try
        {
            Vector2D origin = GlobalMercator.LatLonToMeters(38.706808, -9.136164);

            RenderingProperties terrainProperties = new RenderingProperties(origin, 16, 1, null, false);
            ILayer terrainLayer = new TerrainLayer(map, "mapbox.satellite", terrainProperties);
            //mapbox.satellite|danielflamino.b11llx44
            terrainLayer.Render();

            /*RenderingProperties geojsonProperties = new RenderingProperties(origin, 16, 1, null, false);
            ILayer geoJsonLayer = new GeoJsonLayer(map, "pracaComercioGeoJson", geojsonProperties);
            geoJsonLayer.Render();

            RenderingProperties shapefileProperties = new RenderingProperties(origin, 16, 1, "OBJECTID", false);
            ILayer shapefileLayer = new GeoJsonLayer(map, "arvoredo2", shapefileProperties);
            shapefileLayer.Render();

            RenderingProperties meshProperties = new RenderingProperties(origin, 16, 1, "model", true);
            ILayer meshLayer = new GeoJsonLayer(map, "Lisboa_data", meshProperties);
            meshLayer.Render();*/
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }

}