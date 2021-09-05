using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : ILayer
{
    private GameObject layerGameObject; // TODO

    public string Name { get; }

    public RenderingProperties Properties { get; }

    /// <summary>
    /// Construct a new GeoJSONLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="name">The layer name</param>
    /// <param name="properties">The layer rendering properties</param>

    public GeoJsonLayer(GameObject map, string name, RenderingProperties properties)
    {
        this.Name = name;
        this.Properties = properties;

        // Setup the gameobject
        layerGameObject = new GameObject(Name); // Create Layer gameobject
        layerGameObject.transform.parent = map.transform; // Set it as a child of the Map gameobject
    }

    public void Load()
    {
        //TODO
        LoadTile(15, 16876, 10800);
        LoadTile(15, 16875, 10799);
        LoadTile(15, 16874, 10799);
        LoadTile(15, 16874, 10800);
    }

    private readonly HttpClient client = new HttpClient(); //TODO move to MainController

    /// <summary>
    /// Load and render the tile with given zoom level and coordinates
    /// </summary>
    /// <param name="z">Zoom level</param>
    /// <param name="x">Tile X coordinate (Google)</param>
    /// <param name="y">Tile Y coordinate (Google)</param>
    private async void LoadTile(int z, long x, long y)
    {
        try
        {
            var geoJsonText = await client.GetStringAsync($"https://tese.flamino.eu/api/tiles?layer={Name}&z={z}&x={x}&y={y}");
            try
            {
                // Parse the GeoJSON text
                IGeoJsonObject geoJson = GeoJson.Parse(geoJsonText);
                // Render the tile
                RenderTile(z, x, y, geoJson);
                Logger.Log($"{Name}/{z}/{x}/{y}: Loaded tile!");
            }
            catch (InvalidGeoJsonException e)
            {
                Logger.LogException(e);
            }
        }
        catch (HttpRequestException e)
        {
            Logger.LogException(e);
        }
        catch (TaskCanceledException e)
        {
            Logger.LogException(e);
        }
    }

    /// <summary>
    /// Render the tile
    /// </summary>
    /// <param name="z">Zoom level</param>
    /// <param name="x">Tile X coordinate (Google)</param>
    /// <param name="y">Tile Y coordinate (Google)</param>
    /// <param name="geoJson">The tile's GeoJSON</param>
    /// <exception cref="InvalidGeoJsonException">Can't render as a tile, if root object isn't a FeatureCollection</exception>
    private void RenderTile(int z, long x, long y, IGeoJsonObject geoJson)
    {
        // Check if it's a FeatureCollection
        if (geoJson.GetType() == typeof(FeatureCollection))
        {
            // Setup the tile gameobject
            GameObject tile = new GameObject($"{z}/{x}/{y}"); // Create Layer gameobject
            tile.transform.parent = layerGameObject.transform; // Set it as a child of the Map gameobject
            // Render the GeoJSON
            ((FeatureCollection)geoJson).Render(tile, Properties);
        }
        else
        {
            // Can't render as a layer. Root isn't a FeatureCollection
            throw new InvalidGeoJsonException("Can't render as a tile. Root isn't a FeatureCollection");
        }
    }
}