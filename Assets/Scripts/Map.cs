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

    private Dictionary<string, ILayer> layers;

    /// <summary>
    /// Constructs a new Map
    /// </summary>
    public Map()
    {
        layers = new Dictionary<string, ILayer>();

        // Setup the gameobject
        GameObject = new GameObject("Map");

        LoadLayers();
    }

    private void LoadLayers()
    {
        Vector2D baixa = GlobalMercator.LatLonToMeters(38.706808, -9.136164);
        Vector2D expo = GlobalMercator.LatLonToMeters(38.765514, -9.093839);
        Vector2D marques = GlobalMercator.LatLonToMeters(38.725249, -9.149994);
        Vector2D alta = GlobalMercator.LatLonToMeters(38.773310, -9.153689);
        Vector2D campolide = GlobalMercator.LatLonToMeters(38.733744, -9.160745);
        Vector2D origin = baixa;
        int zoom = 16;
        int tileRadius = 1;

        DefaultGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();

        layers.Add("StamenWatercolor", new TerrainLayer(GameObject, "StamenWatercolor", origin, zoom, tileRadius, new FlatTerrainRenderer(), "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg"));
        //layers.Add("StamenToner", new TerrainLayer(GameObject, "StamenToner", origin, zoom, tileRadius, new FlatTerrainRenderer(), "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        //layers.Add("StamenTerrain", new TerrainLayer(GameObject, "StamenTerrain", origin, zoom, tileRadius, new FlatTerrainRenderer(), "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        //layers.Add("OSMStandard", new TerrainLayer(GameObject, "OSMStandard", origin, zoom, tileRadius, new FlatTerrainRenderer(), "https://tile.openstreetmap.org/{0}.png"));
        //layers.Add("MapboxSatellite", new TerrainLayer(GameObject, "MapboxSatellite", origin, zoom, tileRadius, new FlatTerrainRenderer(), $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        layers.Add("Bikepaths", new GeoJsonLayer(GameObject, "Bikepaths", origin, zoom, tileRadius, "OBJECTID", defaultGeoJsonRenderer));
        layers.Add("Buildings", new GeoJsonLayer(GameObject, "Buildings", origin, zoom, tileRadius, "name", new BuildingRenderer()));
        layers.Add("BuildingsLOD3", new GeoJsonLayer(GameObject, "BuildingsLOD3", origin, zoom, tileRadius, "id", new PrefabRenderer(null)));
        layers.Add("Closures", new GeoJsonLayer(GameObject, "Closures", origin, zoom, tileRadius, "id", defaultGeoJsonRenderer));
        layers.Add("Electrical_IP_especial", new GeoJsonLayer(GameObject, "Electrical_IP_especial", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Electrical_PS", new GeoJsonLayer(GameObject, "Electrical_PS", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Electrical_PTC", new GeoJsonLayer(GameObject, "Electrical_PTC", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Electrical_PTD", new GeoJsonLayer(GameObject, "Electrical_PTD", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Electrical_Subestacao", new GeoJsonLayer(GameObject, "Electrical_Subestacao", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Electrical_Troco_MT", new GeoJsonLayer(GameObject, "Electrical_Troco_MT-AT", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Environment", new GeoJsonLayer(GameObject, "Environment", origin, zoom, tileRadius, "id", defaultGeoJsonRenderer));
        layers.Add("Interventions", new GeoJsonLayer(GameObject, "Interventions", origin, zoom, tileRadius, "OBJECTID", defaultGeoJsonRenderer));
        layers.Add("Lamps", new GeoJsonLayer(GameObject, "Lamps", origin, zoom, tileRadius, "OBJECTID_1", new PrefabRenderer("Lamp")));
        layers.Add("Rails", new GeoJsonLayer(GameObject, "Rails", origin, zoom, tileRadius, "OBJECTID", defaultGeoJsonRenderer));
        layers.Add("Roads", new GeoJsonLayer(GameObject, "Roads", origin, zoom, tileRadius, "OBJECTID_1", defaultGeoJsonRenderer));
        layers.Add("Sidewalks", new GeoJsonLayer(GameObject, "Sidewalks", origin, zoom, tileRadius, null, defaultGeoJsonRenderer));
        layers.Add("Signs", new GeoJsonLayer(GameObject, "Signs", origin, zoom, tileRadius, "IdSV_Posic", defaultGeoJsonRenderer));
        layers.Add("Trees", new GeoJsonLayer(GameObject, "Trees", origin, zoom, tileRadius, "OBJECTID", new PrefabRenderer("Tree")));
    }

    /// <summary>
    /// Render the map
    /// </summary>
    public void Render()
    {
        foreach (ILayer layer in layers.Values)
        {
            layer.Render();
        }
    }
}
