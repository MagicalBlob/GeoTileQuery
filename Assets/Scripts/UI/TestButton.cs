using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    public DebugUIController debugUI;

    private void Start() => testButton.onClick.AddListener(ButtonClicked);

    private void ButtonClicked()
    {
        string invalidJson = "{";

        string json = @"{
            'Name': 'Ninja',
            'AttackDamage': '40',
            'UndefinedProperty': 'This is some text',
            }";

        string altPointGeoJson = @"{
            'type': 'AltPoint', 
            'x': 30.0,
            'y': 10.0,
        }";

        string pointGeoJson = @"{
            'type': 'Point', 
            'coordinates': [30.0, 10.0]
        }";

        string point3DGeoJson = @"{
            'type': 'Point', 
            'coordinates': [30.0, 10.0, 5.0]
        }";

        string pointTooMuchDGeoJson = @"{
            'type': 'Point', 
            'coordinates': [30.0, 10.0, 5.0, 4.0]
        }";

        string pointEmptyCoordsGeoJson = @"{
            'type': 'Point', 
            'coordinates': []
        }";

        string multiPointJson = @"{
            'type': 'MultiPoint', 
            'coordinates': [
                [10.0, 40.0], [40.0, 30.0], [20.0, 20.0], [30.0, 10.0]
            ]
        }";

        string lineStringJson = @"{
            'type': 'LineString', 
            'coordinates': [
                [30.0, 10.0], [10.0, 30.0], [40.0, 40.0]
            ]
        }";

        string polygonJson = @"{
            'type': 'Polygon', 
            'coordinates': [
                [[30.0, 10.0], [40.0, 40.0], [20.0, 40.0], [10.0, 20.0], [30.0, 10.0]]
            ]
        }";

        string polygonWithHoleJson = @"{
            'type': 'Polygon', 
            'coordinates': [
                [[35.0, 10.0], [45.0, 45.0], [15.0, 40.0], [10.0, 20.0], [35.0, 10.0]], 
                [[20.0, 30.0], [35.0, 35.0], [30.0, 20.0], [20.0, 30.0]]
            ]
        }";

        string multiLineStringJson = @"{
            'type': 'MultiLineString', 
            'coordinates': [
                [[10.0, 10.0], [20.0, 20.0], [10.0, 40.0]], 
                [[40.0, 40.0], [30.0, 30.0], [40.0, 20.0], [30.0, 10.0]]
            ]
        }";

        string multiPolygonJson = @"{
            'type': 'MultiPolygon', 
            'coordinates': [
                [
                    [[30.0, 20.0], [45.0, 40.0], [10.0, 40.0], [30.0, 20.0]]
                ], 
                [
                    [[15.0, 5.0], [40.0, 10.0], [10.0, 20.0], [5.0, 10.0], [15.0, 5.0]]
                ]
            ]
        }";

        string multiPolygonWithHoleJson = @"{
            'type': 'MultiPolygon', 
            'coordinates': [
                [
                    [[40.0, 40.0], [20.0, 45.0], [45.0, 30.0], [40.0, 40.0]]
                ], 
                [
                    [[20.0, 35.0], [10.0, 30.0], [10.0, 10.0], [30.0, 5.0], [45.0, 20.0], [20.0, 35.0]], 
                    [[30.0, 20.0], [20.0, 15.0], [20.0, 25.0], [30.0, 20.0]]
                ]
            ]
        }";

        string geometryCollectionJson = @"{
            'type': 'GeometryCollection',
            'geometries': [
                {
                    'type': 'Point',
                    'coordinates': [40.0, 10.0]
                },
                {
                    'type': 'LineString',
                    'coordinates': [
                        [10.0, 10.0], [20.0, 20.0], [10.0, 40.0]
                    ]
                },
                {
                    'type': 'Polygon',
                    'coordinates': [
                        [[40.0, 40.0], [20.0, 45.0], [45.0, 30.0], [40.0, 40.0]]
                    ]
                }
            ]
        }";

        string exampleJson = @"{
            'type': 'FeatureCollection',
            'features': [
                {
                'type': 'Feature',
                'geometry': {
                    'type': 'Point',
                    'coordinates': [102.0, 0.5]
                },
                'properties': {
                    'prop0': 'value0'
                }
                },
                {
                'type': 'Feature',
                'geometry': {
                    'type': 'LineString',
                    'coordinates': [
                    [102.0, 0.0], [103.0, 1.0], [104.0, 0.0], [105.0, 1.0]
                    ]
                },
                'properties': {
                    'prop0': 'value0',
                    'prop1': 0.0
                }
                },
                {
                'type': 'Feature',
                'geometry': {
                    'type': 'Polygon',
                    'coordinates': [
                    [
                        [100.0, 0.0], [101.0, 0.0], [101.0, 1.0],
                        [100.0, 1.0], [100.0, 0.0]
                    ]
                    ]
                },
                'properties': {
                    'prop0': 'value0',
                    'prop1': { 'this': 'that' }
                }
                }
            ]
        }";

        string boundingBoxJson = @"{
            'type': 'Feature',
            'bbox': [-10.0, -10.0, 10.0, 10.0],
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                    [
                        [-10.0, -10.0],
                        [10.0, -10.0],
                        [10.0, 10.0],
                        [-10.0, -10.0]
                    ]
                ]
            }
        }";

        try
        {
            IGeoJsonObject geoJson = GeoJson.Parse(lineStringJson);
            Logger.Log("Type: " + geoJson.GetType());
            Logger.Log(geoJson);
        }
        catch (InvalidGeoJsonException e)
        {
            Logger.LogException(e);
        }
        catch (System.Exception e)
        {
            Logger.LogException(e);
        }
    }
}