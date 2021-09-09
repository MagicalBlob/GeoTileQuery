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
            Tuple<double, double> centerXY = GlobalMercator.LatLonToMeters(52.157039, 5.410851); // TODO: we really need a Vector2D, this Tuple<double, double> everywhere is ridiculous
            RenderingProperties properties = new RenderingProperties(15, centerXY.Item1, centerXY.Item2, 0, 2);

            ILayer testLayer = new GeoJsonLayer(map, "test", properties);
            testLayer.Load();

            /*ILayer nothingLayer = new GeoJsonLayer(map, "nothing", properties);
            nothingLayer.Load();*/
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }
}