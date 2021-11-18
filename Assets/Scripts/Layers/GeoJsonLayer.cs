using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : ILayer
{
    /// <summary>
    /// The loaded tiles in the layer
    /// </summary>
    private Dictionary<string, GeoJsonTile> tiles;

    public string Id { get; }

    /// <summary>
    /// Name of the Feature's property that may be used as an id as an alternative to the actual Feature id if it doesn't exist
    /// </summary>
    public string IdPropertyName { get; }

    public ILayerRenderer Renderer { get; }

    public GameObject GameObject { get; }

    /// <summary>
    /// Construct a new GeoJSONLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="id">The layer id</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    /// <param name="renderer">The layer's renderer</param>
    public GeoJsonLayer(GameObject map, string id, string idPropertyName, IGeoJsonRenderer renderer)
    {
        this.Id = id;
        this.IdPropertyName = idPropertyName;
        this.Renderer = renderer;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.transform; // Set it as a child of the map gameobject
        GameObject.transform.localPosition = Vector3.zero;

        tiles = new Dictionary<string, GeoJsonTile>();
    }

    /// <summary>
    /// Load the tile with given parameters
    /// </summary>
    /// <param name="origin">The layer's origin in the scene (Meters)</param>
    /// <param name="zoom">The tile's zoom level</param>
    /// <param name="x">The tile's X coordinate</param>
    /// <param name="y">The tile's Y coordinate</param>
    public void LoadTile(Vector2D origin, int zoom, int x, int y)
    {
        // Only load tiles that haven't been loaded already
        if (!tiles.ContainsKey($"{zoom}/{x}/{y}"))
        {
            GeoJsonTile tile = new GeoJsonTile(this, origin, zoom, x, y);
            tile.Load();
            tiles.Add(tile.Id, tile);
        }
    }
}