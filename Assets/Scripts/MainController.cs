using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;

public class MainController : MonoBehaviour
{
    public Button testButton;

    public GameObject map;

    public static readonly HttpClient client = new HttpClient();
    public static readonly SemaphoreSlim networkSemaphore = new SemaphoreSlim(5, 5);

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
            Vector2D pracaDoComercio = GlobalMercator.LatLonToMeters(38.706808, -9.136164);
            Vector2D parqueDasNacoes = GlobalMercator.LatLonToMeters(38.765514, -9.093839);
            int zoom = 16;
            int tileRadius = 1;

            LoadLayers(pracaDoComercio, zoom, tileRadius);
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }

    private void LoadLayers(Vector2D origin, int zoom, int tileRadius)
    {
        List<ILayer> layers = new List<ILayer>();

        layers.Add(new TerrainLayer(map, "mapbox.satellite", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        //mapbox.satellite|danielflamino.b11llx44
        layers.Add(new GeoJsonLayer(map, "Bikepaths", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false)));
        layers.Add(new GeoJsonLayer(map, "Buildings", new RenderingProperties(origin, zoom, tileRadius, "name", false)));
        layers.Add(new GeoJsonLayer(map, "BuildingsLOD3", new RenderingProperties(origin, zoom, tileRadius, "id", true)));
        layers.Add(new GeoJsonLayer(map, "Closures", new RenderingProperties(origin, zoom, tileRadius, "id", false)));
        layers.Add(new GeoJsonLayer(map, "Electrical_IP_especial", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Electrical_PS", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Electrical_PTC", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Electrical_PTD", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Electrical_Subestacao", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Electrical_Troco_MT-AT", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Environment", new RenderingProperties(origin, zoom, tileRadius, "id", false)));
        layers.Add(new GeoJsonLayer(map, "Interventions", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false)));
        layers.Add(new GeoJsonLayer(map, "Lamps", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID_1", false)));
        layers.Add(new GeoJsonLayer(map, "Rails", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false)));
        layers.Add(new GeoJsonLayer(map, "Roads", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID_1", false)));
        layers.Add(new GeoJsonLayer(map, "Sidewalks", new RenderingProperties(origin, zoom, tileRadius, null, false)));
        layers.Add(new GeoJsonLayer(map, "Signs", new RenderingProperties(origin, zoom, tileRadius, "IdSV_Posic", false)));
        layers.Add(new GeoJsonLayer(map, "Trees", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false)));

        foreach (ILayer layer in layers)
        {
            layer.Render();
        }
    }
}