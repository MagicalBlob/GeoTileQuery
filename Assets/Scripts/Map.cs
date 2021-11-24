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
    /// Constructs a new Map
    /// </summary>
    public Map()
    {
        // Setup the gameobject
        GameObject = new GameObject("Map");

        // Add the Terrain layer
        Terrain = new TerrainLayer(this.GameObject, "StamenWatercolor", new FlatTerrainRenderer(), "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg");
        //layers.Add("StamenToner", new TerrainLayer(this.GameObject, "StamenToner", new FlatTerrainRenderer(), "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        //layers.Add("StamenTerrain", new TerrainLayer(this.GameObject, "StamenTerrain", new FlatTerrainRenderer(), "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        //layers.Add("OSMStandard", new TerrainLayer(this.GameObject, "OSMStandard", new FlatTerrainRenderer(), "https://tile.openstreetmap.org/{0}.png"));
        //layers.Add("MapboxSatellite", new TerrainLayer(this.GameObject, "MapboxSatellite", new FlatTerrainRenderer(), $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));

        // Create the layer dictionary
        Layers = new Dictionary<string, ILayer>();

        // Add the GeoJSON layers
        DefaultGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        Layers.Add("Bikepaths", new GeoJsonLayer(this.GameObject, "Bikepaths", "OBJECTID", defaultGeoJsonRenderer));
        Layers.Add("Buildings", new GeoJsonLayer(this.GameObject, "Buildings", "name", new BuildingRenderer()));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer(this.GameObject, "BuildingsLOD3", "id", new PrefabRenderer(null)));
        Layers.Add("Closures", new GeoJsonLayer(this.GameObject, "Closures", "id", defaultGeoJsonRenderer));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer(this.GameObject, "Electrical_IP_especial", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_PS", new GeoJsonLayer(this.GameObject, "Electrical_PS", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_PTC", new GeoJsonLayer(this.GameObject, "Electrical_PTC", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_PTD", new GeoJsonLayer(this.GameObject, "Electrical_PTD", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer(this.GameObject, "Electrical_Subestacao", null, defaultGeoJsonRenderer));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer(this.GameObject, "Electrical_Troco_MT-AT", null, defaultGeoJsonRenderer));
        Layers.Add("Environment", new GeoJsonLayer(this.GameObject, "Environment", "id", defaultGeoJsonRenderer));
        Layers.Add("Interventions", new GeoJsonLayer(this.GameObject, "Interventions", "OBJECTID", defaultGeoJsonRenderer));
        Layers.Add("Lamps", new GeoJsonLayer(this.GameObject, "Lamps", "OBJECTID_1", new PrefabRenderer("Lamp")));
        Layers.Add("Rails", new GeoJsonLayer(this.GameObject, "Rails", "OBJECTID", defaultGeoJsonRenderer));
        Layers.Add("Roads", new GeoJsonLayer(this.GameObject, "Roads", "OBJECTID_1", new RoadRenderer()));
        Layers.Add("Sidewalks", new GeoJsonLayer(this.GameObject, "Sidewalks", null, new SidewalkRenderer()));
        Layers.Add("Signs", new GeoJsonLayer(this.GameObject, "Signs", "IdSV_Posic", defaultGeoJsonRenderer));
        Layers.Add("Trees", new GeoJsonLayer(this.GameObject, "Trees", "OBJECTID", new PrefabRenderer("Tree")));
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
                LoadTile(origin, zoom, x, y);
            }
        }
    }

    /// <summary>
    /// Load the tile with given parameters
    /// </summary>
    /// <param name="origin">The map's origin in the scene (Meters)</param>
    /// <param name="zoom">The tile's zoom level</param>
    /// <param name="x">The tile's X coordinate</param>
    /// <param name="y">The tile's Y coordinate</param>
    private async void LoadTile(Vector2D origin, int zoom, int x, int y)
    {
        // Load the terrain first
        await Terrain.LoadTile(origin, zoom, x, y);

        // Load the other layers
        foreach (GeoJsonLayer layer in Layers.Values)
        {
            layer.LoadTile(origin, zoom, x, y);
        }
    }
}
