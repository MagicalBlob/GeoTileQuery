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

    private TerrainLayer terrain;

    private Dictionary<string, ILayer> layers;

    /// <summary>
    /// Constructs a new Map
    /// </summary>
    public Map()
    {
        // Setup the gameobject
        GameObject = new GameObject("Map");

        // Setup the map layers
        SetupLayers();
    }

    /// <summary>
    /// Setup the map layers
    /// </summary>
    private void SetupLayers()
    {
        // Create the layer dictionary
        layers = new Dictionary<string, ILayer>();

        // Add the Terrain layer
        terrain = new TerrainLayer(this.GameObject, "StamenWatercolor", new FlatTerrainRenderer(), "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg");
        //layers.Add("StamenToner", new TerrainLayer(this.GameObject, "StamenToner", new FlatTerrainRenderer(), "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        //layers.Add("StamenTerrain", new TerrainLayer(this.GameObject, "StamenTerrain", new FlatTerrainRenderer(), "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        //layers.Add("OSMStandard", new TerrainLayer(this.GameObject, "OSMStandard", new FlatTerrainRenderer(), "https://tile.openstreetmap.org/{0}.png"));
        //layers.Add("MapboxSatellite", new TerrainLayer(this.GameObject, "MapboxSatellite", new FlatTerrainRenderer(), $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));

        // Add the GeoJSON layers
        DefaultGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        layers.Add("Bikepaths", new GeoJsonLayer(this.GameObject, "Bikepaths", "OBJECTID", defaultGeoJsonRenderer));
        layers.Add("Buildings", new GeoJsonLayer(this.GameObject, "Buildings", "name", new BuildingRenderer()));
        layers.Add("BuildingsLOD3", new GeoJsonLayer(this.GameObject, "BuildingsLOD3", "id", new PrefabRenderer(null)));
        layers.Add("Closures", new GeoJsonLayer(this.GameObject, "Closures", "id", defaultGeoJsonRenderer));
        layers.Add("Electrical_IP_especial", new GeoJsonLayer(this.GameObject, "Electrical_IP_especial", null, defaultGeoJsonRenderer));
        layers.Add("Electrical_PS", new GeoJsonLayer(this.GameObject, "Electrical_PS", null, defaultGeoJsonRenderer));
        layers.Add("Electrical_PTC", new GeoJsonLayer(this.GameObject, "Electrical_PTC", null, defaultGeoJsonRenderer));
        layers.Add("Electrical_PTD", new GeoJsonLayer(this.GameObject, "Electrical_PTD", null, defaultGeoJsonRenderer));
        layers.Add("Electrical_Subestacao", new GeoJsonLayer(this.GameObject, "Electrical_Subestacao", null, defaultGeoJsonRenderer));
        layers.Add("Electrical_Troco_MT", new GeoJsonLayer(this.GameObject, "Electrical_Troco_MT-AT", null, defaultGeoJsonRenderer));
        layers.Add("Environment", new GeoJsonLayer(this.GameObject, "Environment", "id", defaultGeoJsonRenderer));
        layers.Add("Interventions", new GeoJsonLayer(this.GameObject, "Interventions", "OBJECTID", defaultGeoJsonRenderer));
        layers.Add("Lamps", new GeoJsonLayer(this.GameObject, "Lamps", "OBJECTID_1", new PrefabRenderer("Lamp")));
        layers.Add("Rails", new GeoJsonLayer(this.GameObject, "Rails", "OBJECTID", defaultGeoJsonRenderer));
        layers.Add("Roads", new GeoJsonLayer(this.GameObject, "Roads", "OBJECTID_1", new RoadRenderer()));
        layers.Add("Sidewalks", new GeoJsonLayer(this.GameObject, "Sidewalks", null, new SidewalkRenderer()));
        layers.Add("Signs", new GeoJsonLayer(this.GameObject, "Signs", "IdSV_Posic", defaultGeoJsonRenderer));
        layers.Add("Trees", new GeoJsonLayer(this.GameObject, "Trees", "OBJECTID", new PrefabRenderer("Tree")));
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
        await terrain.LoadTile(origin, zoom, x, y);

        // Load the other layers
        foreach (GeoJsonLayer layer in layers.Values)
        {
            layer.LoadTile(origin, zoom, x, y);
        }
    }
}
