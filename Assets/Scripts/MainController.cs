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
            Tuple<double, double> centerXY = GlobalMercator.LatLonToMeters(38.706808, -9.136164); // TODO: we really need a Vector2D, this Tuple<double, double> everywhere is ridiculous

            RenderingProperties geojsonProperties = new RenderingProperties(16, centerXY.Item1, centerXY.Item2, 0, 1, null);
            ILayer geoJsonLayer = new GeoJsonLayer(map, "pracaComercioGeoJson", geojsonProperties);
            geoJsonLayer.Load();

            RenderingProperties shapefileProperties = new RenderingProperties(16, centerXY.Item1, centerXY.Item2, 0, 1, "OBJECTID");
            ILayer shapefileLayer = new GeoJsonLayer(map, "arvoredo2", shapefileProperties);
            shapefileLayer.Load();

            RenderingProperties meshProperties = new RenderingProperties(16, centerXY.Item1, centerXY.Item2, 0, 1, "model");
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