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

        layers.Add("mapbox.satellite", new TerrainLayer(GameObject, "mapbox.satellite", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        //mapbox.satellite|danielflamino.b11llx44
        layers.Add("Bikepaths", new GeoJsonLayer(GameObject, "Bikepaths", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false, null)));
        layers.Add("Buildings", new GeoJsonLayer(GameObject, "Buildings", new RenderingProperties(origin, zoom, tileRadius, "name", false, null)));
        layers.Add("BuildingsLOD3", new GeoJsonLayer(GameObject, "BuildingsLOD3", new RenderingProperties(origin, zoom, tileRadius, "id", true, null)));
        layers.Add("Closures", new GeoJsonLayer(GameObject, "Closures", new RenderingProperties(origin, zoom, tileRadius, "id", false, null)));
        layers.Add("Electrical_IP_especial", new GeoJsonLayer(GameObject, "Electrical_IP_especial", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Electrical_PS", new GeoJsonLayer(GameObject, "Electrical_PS", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Electrical_PTC", new GeoJsonLayer(GameObject, "Electrical_PTC", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Electrical_PTD", new GeoJsonLayer(GameObject, "Electrical_PTD", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Electrical_Subestacao", new GeoJsonLayer(GameObject, "Electrical_Subestacao", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Electrical_Troco_MT", new GeoJsonLayer(GameObject, "Electrical_Troco_MT-AT", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Environment", new GeoJsonLayer(GameObject, "Environment", new RenderingProperties(origin, zoom, tileRadius, "id", false, null)));
        layers.Add("Interventions", new GeoJsonLayer(GameObject, "Interventions", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false, null)));
        layers.Add("Lamps", new GeoJsonLayer(GameObject, "Lamps", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID_1", true, "Lamp")));
        layers.Add("Rails", new GeoJsonLayer(GameObject, "Rails", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", false, null)));
        layers.Add("Roads", new GeoJsonLayer(GameObject, "Roads", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID_1", false, null)));
        layers.Add("Sidewalks", new GeoJsonLayer(GameObject, "Sidewalks", new RenderingProperties(origin, zoom, tileRadius, null, false, null)));
        layers.Add("Signs", new GeoJsonLayer(GameObject, "Signs", new RenderingProperties(origin, zoom, tileRadius, "IdSV_Posic", false, null)));
        layers.Add("Trees", new GeoJsonLayer(GameObject, "Trees", new RenderingProperties(origin, zoom, tileRadius, "OBJECTID", true, "Tree")));
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
