using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the Map
/// </summary>
public class Map
{
    /// <summary>
    /// The Map's GameObject representation
    /// </summary>
    public GameObject GameObject { get; }

    /// <summary>
    /// The terrain layer
    /// </summary>
    public TerrainLayer Terrain { get; private set; }

    /// <summary>
    /// The data layers
    /// </summary>
    public Dictionary<string, ILayer> Layers { get; private set; }

    /// <summary>
    /// The loaded tiles in the map
    /// </summary>
    private Dictionary<string, Tile> tiles;

    /// <summary>
    /// Constructs a new Map
    /// </summary>
    public Map()
    {
        // Setup the gameobject
        GameObject = new GameObject("Map");

        // Create the layer dictionary
        Layers = new Dictionary<string, ILayer>();

        // Add the Terrain layers
        //TODO probably should separate the terrain height from the layer, since the heightmap is needed by all layers, and these are essentially raster layers
        Layers.Add("StamenWatercolor", new TerrainLayer("StamenWatercolor", new FlatTerrainRenderer(), "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new TerrainLayer("StamenToner", new FlatTerrainRenderer(), "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        Layers.Add("StamenTerrain", new TerrainLayer("StamenTerrain", new FlatTerrainRenderer(), "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        Layers.Add("OSMStandard", new TerrainLayer("OSMStandard", new FlatTerrainRenderer(), "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new TerrainLayer("MapboxSatellite", new FlatTerrainRenderer(), $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));

        // Add the GeoJSON layers
        DefaultGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        Layers.Add("Bikepaths", new GeoJsonLayer("Bikepaths", "OBJECTID", defaultGeoJsonRenderer));
        Layers.Add("Buildings", new GeoJsonLayer("Buildings", "name", new BuildingRenderer()));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer("BuildingsLOD3", "id", new PrefabRenderer(null)));
        Layers.Add("Closures", new GeoJsonLayer("Closures", "id", defaultGeoJsonRenderer));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer("Electrical_IP_especial", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_PS", new GeoJsonLayer("Electrical_PS", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_PTC", new GeoJsonLayer("Electrical_PTC", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_PTD", new GeoJsonLayer("Electrical_PTD", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer("Electrical_Subestacao", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer("Electrical_Troco_MT-AT", null, defaultGeoJsonRenderer));
        Layers.Add("Environment", new GeoJsonLayer("Environment", "id", defaultGeoJsonRenderer));
        Layers.Add("Interventions", new GeoJsonLayer("Interventions", "OBJECTID", defaultGeoJsonRenderer));
        Layers.Add("Lamps", new GeoJsonLayer("Lamps", "OBJECTID_1", new PrefabRenderer("Lamp")));
        Layers.Add("Rails", new GeoJsonLayer("Rails", "OBJECTID", defaultGeoJsonRenderer));
        Layers.Add("Roads", new GeoJsonLayer("Roads", "OBJECTID_1", new RoadRenderer()));
        Layers.Add("Sidewalks", new GeoJsonLayer("Sidewalks", null, new SidewalkRenderer()));
        Layers.Add("Signs", new GeoJsonLayer("Signs", "IdSV_Posic", defaultGeoJsonRenderer));
        Layers.Add("Trees", new GeoJsonLayer("Trees", "OBJECTID", new PrefabRenderer("Tree")));

        // Create the tile dictionary
        tiles = new Dictionary<string, Tile>();
    }

    /// <summary>
    /// Load the map  
    /// </summary>
    public void Load()
    {
        Vector2D baixa = GlobalMercator.LatLonToMeters(38.706808, -9.136164);
        Vector2D expo = GlobalMercator.LatLonToMeters(38.765514, -9.093839);
        Vector2D marques = GlobalMercator.LatLonToMeters(38.725249, -9.149994);
        Vector2D alta = GlobalMercator.LatLonToMeters(38.773310, -9.153689);
        Vector2D campolide = GlobalMercator.LatLonToMeters(38.733744, -9.160745);
        Vector2D origin = baixa;

        int zoom = 16;

        Vector2Int originTile = GlobalMercator.MetersToGoogleTile(origin, zoom);
        int tileLoadDistance = 1;
        for (int y = originTile.y - tileLoadDistance; y <= originTile.y + tileLoadDistance; y++)
        {
            for (int x = originTile.x - tileLoadDistance; x <= originTile.x + tileLoadDistance; x++)
            {
                // Only load tiles that haven't been loaded already
                if (!tiles.ContainsKey($"{zoom}/{x}/{y}"))
                {
                    Tile tile = new Tile(this, origin, zoom, x, y);
                    tile.Load();
                    tiles.Add(tile.Id, tile);
                }
            }
        }
    }
}
