using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public Button testButton;

    public GameObject testLayer;

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

        string multiLineStringJson = @"{
            'type': 'MultiLineString', 
            'coordinates': [
                [[10.0, 10.0], [20.0, 20.0], [10.0, 40.0]], 
                [[40.0, 40.0], [30.0, 30.0], [40.0, 20.0], [30.0, 10.0]]
            ]
        }";

        string polygonJson = @"{
            'type': 'Polygon', 
            'coordinates': [
                [[30.0, 10.0], [40.0, 40.0], [20.0, 40.0], [10.0, 20.0], [30.0, 10.0]]
            ]
        }";

        string polygonBrokenJson = @"{
            'type': 'Polygon', 
            'coordinates': [
                [[30.0, 10.0], [40.0, 40.0], [20.0, 40.0], [10.0, 20.0], [40.0, 10.0]]
            ]
        }";

        string polygonWithHoleJson = @"{
            'type': 'Polygon', 
            'coordinates': [
                [[35.0, 10.0], [45.0, 45.0], [15.0, 40.0], [10.0, 20.0], [35.0, 10.0]], 
                [[20.0, 30.0], [35.0, 35.0], [30.0, 20.0], [20.0, 30.0]]
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

        string featureJson = @"{
            'type': 'Feature',
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                    [
                        [
                            -1,
                            5
                        ],
                        [
                            -2,
                            3
                        ],
                        [
                            -2,
                            2.5
                        ],
                        [
                            -1,
                            5
                        ]
                    ]
                ]
            },
            'properties': {
                'name': 'This a feature name'
            }
        }";

        string featureUnlocatedJson = @"{
            'type': 'Feature',
            'geometry': null,
            'properties': {
                'description': 'This an unlocated feature'
            }
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

        string featuresJson = @"{
        'type': 'FeatureCollection',
        'features': [
            {
            'type': 'Feature',
            'id': 'Point',
            'properties': {},
            'geometry': {
                'type': 'Point',
                'coordinates': [
                1,
                0.5
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'MultiPoint',
            'properties': {},
            'geometry': {
                'type': 'MultiPoint',
                'coordinates': [
                [
                    0.1,
                    1.6
                ],
                [
                    0.3,
                    1.7
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'LineString',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    3,
                    4
                ],
                [
                    3.1,
                    4.1
                ],
                [
                    3.2,
                    4.2
                ],
                [
                    3.3,
                    4.3
                ],
                [
                    3.4,
                    4.4
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'MultiLineString',
            'properties': {},
            'geometry': {
                'type': 'MultiLineString',
                'coordinates': [
                [
                    [
                    2.5,
                    2.5
                    ],
                    [
                    2.4,
                    2.6
                    ],
                    [
                    2.3,
                    2.7
                    ],
                    [
                    2.2,
                    2.8
                    ],
                    [
                    2.1,
                    2.9
                    ],
                    [
                    2,
                    3
                    ]
                ],
                [
                    [
                    2.2,
                    2.8
                    ],
                    [
                    2.3,
                    2.8
                    ],
                    [
                    2.4,
                    2.8
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'Polygon',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -1.4,
                    -1
                    ],
                    [
                    0.7,
                    -1
                    ],
                    [
                    0.7,
                    0.2
                    ],
                    [
                    -1,
                    0
                    ],
                    [
                    -1.4,
                    -1
                    ]
                ],
                [
                    [
                    -0.5,
                    -0.5
                    ],
                    [
                    -0.5,
                    -0.7
                    ],
                    [
                    -0.7,
                    -0.7
                    ],
                    [
                    -0.7,
                    -0.5
                    ],
                    [
                    -0.5,
                    -0.5
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'MultiPolygon',
            'properties': {},
            'geometry': {
                'type': 'MultiPolygon',
                'coordinates': [
                [
                    [
                    [
                        1.7,
                        1.7
                    ],
                    [
                        1.9,
                        1.7
                    ],
                    [
                        1.9,
                        1.9
                    ],
                    [
                        1.7,
                        1.9
                    ],
                    [
                        1.7,
                        1.7
                    ]
                    ]
                ],
                [
                    [
                    [
                        1,
                        1
                    ],
                    [
                        1.5,
                        1
                    ],
                    [
                        1.5,
                        1.5
                    ],
                    [
                        1,
                        1.5
                    ],
                    [
                        1,
                        1
                    ]
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'GeometryCollection',
            'properties': {},
            'geometry': {
                'type': 'GeometryCollection',
                'geometries': [
                {
                    'type': 'Point',
                    'coordinates': [
                    0,
                    4.2
                    ]
                },
                {
                    'type': 'LineString',
                    'coordinates': [
                    [
                        0,
                        3
                    ],
                    [
                        0,
                        3.5
                    ],
                    [
                        0,
                        4
                    ]
                    ]
                }
                ]
            }
            }
        ]
        }";

        string featuresMetersJson = @"{
        'type': 'FeatureCollection',
        'features': [
            {
            'type': 'Feature',
            'id': 'Point',
            'properties': {},
            'geometry': {
                'type': 'Point',
                'coordinates': [
                0.000009,
                0.0000045
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'MultiPoint',
            'properties': {},
            'geometry': {
                'type': 'MultiPoint',
                'coordinates': [
                [
                    0.0000009,
                    0.0000144
                ],
                [
                    0.0000027,
                    0.0000153
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'LineString',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    0.000027,
                    0.0000359
                ],
                [
                    0.0000278,
                    0.0000368
                ],
                [
                    0.0000288,
                    0.0000377
                ],
                [
                    0.0000296,
                    0.0000386
                ],
                [
                    0.0000305,
                    0.0000395
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'MultiLineString',
            'properties': {},
            'geometry': {
                'type': 'MultiLineString',
                'coordinates': [
                [
                    [
                    0.0000225,
                    0.0000225
                    ],
                    [
                    0.0000216,
                    0.0000234
                    ],
                    [
                    0.0000207,
                    0.0000242
                    ],
                    [
                    0.0000198,
                    0.0000252
                    ],
                    [
                    0.0000189,
                    0.000026
                    ],
                    [
                    0.000018,
                    0.000027
                    ]
                ],
                [
                    [
                    0.0000198,
                    0.0000252
                    ],
                    [
                    0.0000207,
                    0.0000252
                    ],
                    [
                    0.0000216,
                    0.0000252
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'Polygon',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -0.0000126,
                    -0.000009
                    ],
                    [
                    0.0000063,
                    -0.000009
                    ],
                    [
                    0.0000063,
                    0.0000018
                    ],
                    [
                    -0.000009,
                    0
                    ],
                    [
                    -0.0000126,
                    -0.000009
                    ]
                ],
                [
                    [
                    -0.0000045,
                    -0.0000045
                    ],
                    [
                    -0.0000045,
                    -0.0000063
                    ],
                    [
                    -0.0000063,
                    -0.0000063
                    ],
                    [
                    -0.0000063,
                    -0.0000045
                    ],
                    [
                    -0.0000045,
                    -0.0000045
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'MultiPolygon',
            'properties': {},
            'geometry': {
                'type': 'MultiPolygon',
                'coordinates': [
                [
                    [
                    [
                        0.0000153,
                        0.0000153
                    ],
                    [
                        0.0000171,
                        0.0000153
                    ],
                    [
                        0.0000171,
                        0.0000171
                    ],
                    [
                        0.0000153,
                        0.0000171
                    ],
                    [
                        0.0000153,
                        0.0000153
                    ]
                    ]
                ],
                [
                    [
                    [
                        0.000009,
                        0.000009
                    ],
                    [
                        0.0000135,
                        0.000009
                    ],
                    [
                        0.0000135,
                        0.0000135
                    ],
                    [
                        0.000009,
                        0.0000135
                    ],
                    [
                        0.000009,
                        0.000009
                    ]
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'id': 'GeometryCollection',
            'properties': {},
            'geometry': {
                'type': 'GeometryCollection',
                'geometries': [
                {
                    'type': 'Point',
                    'coordinates': [
                    0,
                    0.0000377
                    ]
                },
                {
                    'type': 'LineString',
                    'coordinates': [
                    [
                        0,
                        0.000027
                    ],
                    [
                        0,
                        0.0000314
                    ],
                    [
                        0,
                        0.0000359
                    ]
                    ]
                }
                ]
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
            IGeoJsonObject geoJson = GeoJson.Parse(featuresMetersJson);

            Logger.Log(geoJson);

            if (geoJson.GetType() == typeof(FeatureCollection))
            {
                FeatureCollection collection = (FeatureCollection)geoJson;

                collection.Render(testLayer);
            }
            else
            {
                // Can't render as a layer. Root isn't a FeatureCollection
                throw new System.Exception("Can't render as a layer. Root isn't a FeatureCollection");
            }

            Logger.Log(GlobalMercator.MetersToLatLon(-1017065.574593, 4679862.580530));
            Logger.Log(GlobalMercator.LatLonToMeters(GlobalMercator.MetersToLatLon(-1017065.574593, 4679862.580530).Item1, GlobalMercator.MetersToLatLon(-1017065.574593, 4679862.580530).Item2));
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