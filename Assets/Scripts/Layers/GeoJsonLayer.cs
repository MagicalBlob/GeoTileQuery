using System.Collections.Generic;
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

    public RenderingProperties Properties { get; }

    public GameObject GameObject { get; }

    /// <summary>
    /// Construct a new GeoJSONLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="id">The layer id</param>
    /// <param name="properties">The layer rendering properties</param>
    public GeoJsonLayer(GameObject map, string id, RenderingProperties properties)
    {
        this.Id = id;
        this.Properties = properties;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.transform; // Set it as a child of the map gameobject
        GameObject.transform.localPosition = Vector3.zero;

        tiles = new Dictionary<string, GeoJsonTile>();
    }

    public void Render()
    {
        Vector2Int tileCoords = GlobalMercator.MetersToGoogleTile(Properties.Origin, Properties.Zoom);
        for (int y = tileCoords.y - Properties.TileViewDistance; y <= tileCoords.y + Properties.TileViewDistance; y++)
        {
            for (int x = tileCoords.x - Properties.TileViewDistance; x <= tileCoords.x + Properties.TileViewDistance; x++)
            {
                if (!tiles.ContainsKey($"{Properties.Zoom}/{x}/{y}"))
                {
                    // Only render tiles that haven't been rendered already
                    GeoJsonTile tile = new GeoJsonTile(this, x, y);
                    tiles.Add(tile.Id, tile);
                }
            }
        }
    }
}