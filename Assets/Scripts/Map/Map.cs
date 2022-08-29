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
    /// The data layers
    /// </summary>
    public Dictionary<string, ILayer> Layers { get; private set; }

    /// <summary>
    /// The tiles in the map
    /// </summary>
    public Dictionary<string, Tile> Tiles { get; private set; }

    /// <summary>
    /// Constructs a new Map
    /// </summary>
    public Map()
    {
        Layers = new Dictionary<string, ILayer>();
        Tiles = new Dictionary<string, Tile>();

        // Setup the gameobject
        GameObject = new GameObject("Map");

        // Add the data layers        
        IRasterRenderer defaultRasterRenderer = new TerrainRenderer();
        IGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        Layers.Add("StamenWatercolor", new RasterLayer("StamenWatercolor", defaultRasterRenderer, "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new RasterLayer("StamenToner", defaultRasterRenderer, "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        Layers.Add("StamenTerrain", new RasterLayer("StamenTerrain", defaultRasterRenderer, "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        Layers.Add("OSMStandard", new RasterLayer("OSMStandard", defaultRasterRenderer, "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new RasterLayer("MapboxSatellite", defaultRasterRenderer, $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        Layers.Add("Bikepaths", new GeoJsonLayer("Bikepaths", defaultGeoJsonRenderer, "OBJECTID"));
        Layers.Add("Buildings", new GeoJsonLayer("Buildings", new BuildingRenderer(), "name"));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer("BuildingsLOD3", new PrefabRenderer(null), "id"));
        Layers.Add("Closures", new GeoJsonLayer("Closures", defaultGeoJsonRenderer, "id"));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer("Electrical_IP_especial", defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_PS", new GeoJsonLayer("Electrical_PS", defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_PTC", new GeoJsonLayer("Electrical_PTC", defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_PTD", new GeoJsonLayer("Electrical_PTD", defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer("Electrical_Subestacao", defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer("Electrical_Troco_MT-AT", defaultGeoJsonRenderer, null));
        Layers.Add("Environment", new GeoJsonLayer("Environment", defaultGeoJsonRenderer, "id"));
        Layers.Add("Interventions", new GeoJsonLayer("Interventions", defaultGeoJsonRenderer, "OBJECTID"));
        Layers.Add("Lamps", new GeoJsonLayer("Lamps", new PrefabRenderer("Lamp"), "OBJECTID_1"));
        Layers.Add("Rails", new GeoJsonLayer("Rails", defaultGeoJsonRenderer, "OBJECTID"));
        Layers.Add("Roads", new GeoJsonLayer("Roads", new RoadRenderer(), "OBJECTID_1"));
        Layers.Add("Sidewalks", new GeoJsonLayer("Sidewalks", new SidewalkRenderer(), null));
        Layers.Add("Signs", new GeoJsonLayer("Signs", defaultGeoJsonRenderer, "IdSV_Posic"));
        Layers.Add("Trees", new GeoJsonLayer("Trees", new PrefabRenderer("Tree"), "OBJECTID"));
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
        Vector2D nullIsland = GlobalMercator.LatLonToMeters(0, 0);
        Vector2D origin = baixa;

        int zoom = 16;

        Vector2Int originTile = GlobalMercator.MetersToGoogleTile(origin, zoom);
        int tileLoadDistance = 1;
        for (int y = originTile.y - tileLoadDistance; y <= originTile.y + tileLoadDistance; y++)
        {
            for (int x = originTile.x - tileLoadDistance; x <= originTile.x + tileLoadDistance; x++)
            {
                // Only load tiles that haven't been loaded already
                if (!Tiles.ContainsKey($"{zoom}/{x}/{y}"))
                {
                    Tile tile = new Tile(this, origin, zoom, x, y);
                    Tiles.Add(tile.Id, tile);
                    tile.Load();
                }
            }
        }
    }
}
