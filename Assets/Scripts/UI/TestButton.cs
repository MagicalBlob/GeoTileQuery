using UnityEngine;
using UnityEngine.UI;
using System;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    public GameObject testLayer;

    private void Start() => testButton.onClick.AddListener(ButtonClicked);

    private void ButtonClicked()
    {
        try
        {
            IGeoJsonObject geoJson = GeoJson.Parse(Resources.Load<TextAsset>("GeoJson/pracaComercioGeoJson").text);

            if (geoJson.GetType() == typeof(FeatureCollection))
            {
                FeatureCollection collection = (FeatureCollection)geoJson;

                Tuple<double, double> centerXY = GlobalMercator.LatLonToMeters(38.707524, -9.136456);
                RenderingProperties properties = new RenderingProperties(centerXY.Item1, centerXY.Item2, 0);

                collection.Render(testLayer, properties);
            }
            else
            {
                // Can't render as a layer. Root isn't a FeatureCollection
                throw new System.Exception("Can't render as a layer. Root isn't a FeatureCollection");
            }
        }
        catch (InvalidGeoJsonException e)
        {
            Logger.LogException(e);
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger
        }
    }
}