using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a Terrain layer
/// </summary>
public class TerrainLayer : ILayer
{
    /// <summary>
    /// The loaded tiles in the layer
    /// </summary>
    private Dictionary<string, TerrainTile> tiles;

    public string Id { get; }

    public ILayerRenderer Renderer { get; }

    public GameObject GameObject { get; }

    private string tileRasterUrl;

    /// <summary>
    /// Construct a new TerrainLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="id">The layer id</param>
    /// <param name="rasterUrl">Url to fetch raster tiles</param>
    /// <param name="renderer">The layer's renderer</param>
    public TerrainLayer(GameObject map, string id, ITerrainRenderer renderer, string rasterUrl)
    {
        this.Id = id;
        this.Renderer = renderer;
        this.tileRasterUrl = rasterUrl;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.transform; // Set it as a child of the map gameobject
        GameObject.transform.localPosition = Vector3.zero;

        tiles = new Dictionary<string, TerrainTile>();
    }

    /// <summary>
    /// Load the tile with given parameters
    /// </summary>
    /// <param name="origin">The layer's origin in the scene (Meters)</param>
    /// <param name="zoom">The tile's zoom level</param>
    /// <param name="x">The tile's X coordinate</param>
    /// <param name="y">The tile's Y coordinate</param>
    public async Task LoadTile(Vector2D origin, int zoom, int x, int y)
    {
        // Only load tiles that haven't been loaded already
        if (!tiles.ContainsKey($"{zoom}/{x}/{y}"))
        {
            TerrainTile tile = new TerrainTile(this, origin, zoom, x, y, tileRasterUrl);
            await tile.Load();
            tiles.Add(tile.Id, tile);
        }
    }
}