using UnityEngine;
using UnityEngine.UI;
using System;

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

        string pracaComercioJson = @"{
        'type': 'FeatureCollection',
        'features': [
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.137524701654911,
                    38.70822673986912
                    ],
                    [
                    -9.136904776096344,
                    38.70838293255911
                    ],
                    [
                    -9.13705162703991,
                    38.70875392153375
                    ],
                    [
                    -9.137669876217842,
                    38.70860793315428
                    ],
                    [
                    -9.137524701654911,
                    38.70822673986912
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.136706292629242,
                    38.7084321187962
                    ],
                    [
                    -9.136089384555817,
                    38.708576537764905
                    ],
                    [
                    -9.13623422384262,
                    38.70895014200432
                    ],
                    [
                    -9.136851131916046,
                    38.70880153775076
                    ],
                    [
                    -9.136706292629242,
                    38.7084321187962
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.136893711984156,
                    38.70835860105698
                    ],
                    [
                    -9.136700257658958,
                    38.70840543265051
                    ],
                    [
                    -9.136744514107704,
                    38.708522903942814
                    ],
                    [
                    -9.13693930953741,
                    38.708473979396885
                    ],
                    [
                    -9.136893711984156,
                    38.70835860105698
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.135322272777557,
                    38.70736675826019
                    ],
                    [
                    -9.135255217552185,
                    38.70718047551433
                    ],
                    [
                    -9.134984314441681,
                    38.70725059323406
                    ],
                    [
                    -9.135003089904785,
                    38.707292454526495
                    ],
                    [
                    -9.13332000374794,
                    38.70769850779119
                    ],
                    [
                    -9.13366064429283,
                    38.70868432854417
                    ],
                    [
                    -9.135174751281738,
                    38.70864560760334
                    ],
                    [
                    -9.135185480117798,
                    38.70866967737982
                    ],
                    [
                    -9.13570448756218,
                    38.708542002820664
                    ],
                    [
                    -9.135248512029648,
                    38.70741594519637
                    ],
                    [
                    -9.135265946388245,
                    38.70740861948462
                    ],
                    [
                    -9.135255217552185,
                    38.707379316630096
                    ],
                    [
                    -9.135322272777557,
                    38.70736675826019
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.13747541606426,
                    38.7066922659028
                    ],
                    [
                    -9.137457981705666,
                    38.706647264634675
                    ],
                    [
                    -9.137209877371788,
                    38.706705347661476
                    ],
                    [
                    -9.137268215417862,
                    38.70685761915634
                    ],
                    [
                    -9.137351363897324,
                    38.706838781463226
                    ],
                    [
                    -9.137371480464935,
                    38.70689372472098
                    ],
                    [
                    -9.137382879853249,
                    38.706890061838436
                    ],
                    [
                    -9.137821421027184,
                    38.708033395346426
                    ],
                    [
                    -9.138952642679214,
                    38.70776077606471
                    ],
                    [
                    -9.138964042067528,
                    38.707791648549964
                    ],
                    [
                    -9.139851182699203,
                    38.7075786804568
                    ],
                    [
                    -9.139831066131592,
                    38.707549377671974
                    ],
                    [
                    -9.141013249754906,
                    38.70726602958848
                    ],
                    [
                    -9.140963964164257,
                    38.70714724806339
                    ],
                    [
                    -9.14102766662836,
                    38.7065635412698
                    ],
                    [
                    -9.140772521495819,
                    38.706543133684775
                    ],
                    [
                    -9.140683338046074,
                    38.707112450795115
                    ],
                    [
                    -9.139757975935936,
                    38.7073678047911
                    ],
                    [
                    -9.139739200472832,
                    38.707312338631816
                    ],
                    [
                    -9.139089435338974,
                    38.70746094598069
                    ],
                    [
                    -9.138838984072208,
                    38.706827269537186
                    ],
                    [
                    -9.137972295284271,
                    38.70703291411829
                    ],
                    [
                    -9.137808345258236,
                    38.706610897310114
                    ],
                    [
                    -9.13747541606426,
                    38.7066922659028
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.138264656066895,
                    38.70845932818943
                    ],
                    [
                    -9.138123840093613,
                    38.70809095398698
                    ],
                    [
                    -9.137682616710663,
                    38.70819246638563
                    ],
                    [
                    -9.137827455997467,
                    38.708563979605294
                    ],
                    [
                    -9.138264656066895,
                    38.70845932818943
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.13592241704464,
                    38.70862232270313
                    ],
                    [
                    -9.135234430432318,
                    38.70878688661045
                    ],
                    [
                    -9.135379269719124,
                    38.70915368747002
                    ],
                    [
                    -9.136067926883698,
                    38.70899043254094
                    ],
                    [
                    -9.135925769805908,
                    38.70862258433126
                    ],
                    [
                    -9.13592241704464,
                    38.70862232270313
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Polygon',
                'coordinates': [
                [
                    [
                    -9.136430025100708,
                    38.70674093003296
                    ],
                    [
                    -9.136403873562813,
                    38.70667709105974
                    ],
                    [
                    -9.1363475471735,
                    38.70660644950657
                    ],
                    [
                    -9.136275127530098,
                    38.70656772744038
                    ],
                    [
                    -9.13621410727501,
                    38.7065300518964
                    ],
                    [
                    -9.136173203587532,
                    38.70649813232281
                    ],
                    [
                    -9.136136323213577,
                    38.70645470074893
                    ],
                    [
                    -9.136099442839622,
                    38.70640551315147
                    ],
                    [
                    -9.136077985167502,
                    38.70636103497569
                    ],
                    [
                    -9.135943539440632,
                    38.706395047700816
                    ],
                    [
                    -9.135964997112751,
                    38.7064342931328
                    ],
                    [
                    -9.13597907871008,
                    38.70650885939421
                    ],
                    [
                    -9.135965667665005,
                    38.706582117399854
                    ],
                    [
                    -9.135947898030281,
                    38.70662842688615
                    ],
                    [
                    -9.135920405387878,
                    38.70667578288345
                    ],
                    [
                    -9.135902300477028,
                    38.70672366211958
                    ],
                    [
                    -9.13589995354414,
                    38.70676840170469
                    ],
                    [
                    -9.135913699865341,
                    38.70683851982856
                    ],
                    [
                    -9.135923758149147,
                    38.706864683289986
                    ],
                    [
                    -9.136430025100708,
                    38.70674093003296
                    ]
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'Point',
                'coordinates': [
                -9.13645014166832,
                38.70752478425401
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    -9.133104085922241,
                    38.70760745996593
                ],
                [
                    -9.140399694442749,
                    38.7058660171755
                ],
                [
                    -9.140748381614685,
                    38.70580322397492
                ],
                [
                    -9.141027331352234,
                    38.70580322397492
                ],
                [
                    -9.141274094581604,
                    38.70590787927858
                ],
                [
                    -9.141467213630676,
                    38.7061171894263
                ],
                [
                    -9.141697883605955,
                    38.706657206777756
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    -9.141699224710464,
                    38.70714698642987
                ],
                [
                    -9.13994237780571,
                    38.70758234330405
                ],
                [
                    -9.139808267354965,
                    38.70764408841531
                ],
                [
                    -9.138988852500916,
                    38.70784188171779
                ],
                [
                    -9.138676375150679,
                    38.70788583571071
                ],
                [
                    -9.137842208147049,
                    38.708074209659976
                ],
                [
                    -9.137564599514008,
                    38.70810874483017
                ],
                [
                    -9.135974049568176,
                    38.708495956202405
                ],
                [
                    -9.135712534189224,
                    38.70859851453893
                ],
                [
                    -9.135161340236662,
                    38.70872618899715
                ],
                [
                    -9.133598953485489,
                    38.70873874712828
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    -9.13776844739914,
                    38.70864560760334
                ],
                [
                    -9.137565270066261,
                    38.70810979135021
                ],
                [
                    -9.136993288993835,
                    38.706680230682714
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    -9.135398715734482,
                    38.707059600776304
                ],
                [
                    -9.135974720120428,
                    38.708495956202405
                ],
                [
                    -9.136177897453308,
                    38.70903124630818
                ]
                ]
            }
            },
            {
            'type': 'Feature',
            'properties': {},
            'geometry': {
                'type': 'LineString',
                'coordinates': [
                [
                    -9.135996848344803,
                    38.70849072363028
                ],
                [
                    -9.13599818944931,
                    38.708458281674496
                ],
                [
                    -9.136122912168503,
                    38.70838083952702
                ],
                [
                    -9.136236906051636,
                    38.70832956022104
                ],
                [
                    -9.136731773614883,
                    38.70821235024066
                ],
                [
                    -9.137205183506012,
                    38.708095140068124
                ],
                [
                    -9.137296378612518,
                    38.70808572138522
                ],
                [
                    -9.13739964365959,
                    38.70808781442599
                ],
                [
                    -9.137502908706663,
                    38.708105605269935
                ],
                [
                    -9.137544482946396,
                    38.70811397743026
                ]
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
            IGeoJsonObject geoJson = GeoJson.Parse(pracaComercioJson);

            Logger.Log(geoJson);

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