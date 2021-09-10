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
            Tuple<double, double> centerXY = GlobalMercator.LatLonToMeters(38.708059, -9.139218); // TODO: we really need a Vector2D, this Tuple<double, double> everywhere is ridiculous
            RenderingProperties properties = new RenderingProperties(16, centerXY.Item1, centerXY.Item2, 0, 1);

            ILayer geoJsonLayer = new GeoJsonLayer(map, "pracaComercioGeoJson", properties);
            geoJsonLayer.Load();

            ILayer shapefileLayer = new GeoJsonLayer(map, "arvoredo2", properties);
            shapefileLayer.Load();

            /*ILayer nothingLayer = new GeoJsonLayer(map, "nothing", properties);
            nothingLayer.Load();*/
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }
}