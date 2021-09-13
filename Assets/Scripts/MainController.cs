using UnityEngine;
using UnityEngine.UI;
using System;

public class MainController : MonoBehaviour
{
    public Button testButton;

    public GameObject map;

    private void Start() => testButton.onClick.AddListener(ButtonClicked);

    private void ButtonClicked()
    {
        Logger.Log("Button pressed!");
        try
        {
            Vector2D center = GlobalMercator.LatLonToMeters(38.706808, -9.136164);

            RenderingProperties geojsonProperties = new RenderingProperties(16, center.X, center.Y, 0, 1, null, false);
            ILayer geoJsonLayer = new GeoJsonLayer(map, "pracaComercioGeoJson", geojsonProperties);
            geoJsonLayer.Load();

            RenderingProperties shapefileProperties = new RenderingProperties(16, center.X, center.Y, 0, 1, "OBJECTID", false);
            ILayer shapefileLayer = new GeoJsonLayer(map, "arvoredo2", shapefileProperties);
            shapefileLayer.Load();

            RenderingProperties meshProperties = new RenderingProperties(16, center.X, center.Y, 0, 1, "model", true);
            ILayer meshLayer = new GeoJsonLayer(map, "Lisboa_data", meshProperties);
            meshLayer.Load();

            /*ILayer nothingLayer = new GeoJsonLayer(map, "nothing", properties);
            nothingLayer.Load();*/
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }
}