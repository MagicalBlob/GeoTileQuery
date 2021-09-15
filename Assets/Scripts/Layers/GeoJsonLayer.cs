using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : ILayer
{
    private List<Tile> tiles; // TODO

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

        tiles = new List<Tile>(); // TODO
    }

    public void Render()
    {
        //TODO
        Vector2Int tileCoords = GlobalMercator.MetersToTile(Properties.CenterX, Properties.CenterY, Properties.Zoom);
        Vector2Int googleTileCoords = GlobalMercator.GoogleTile(tileCoords.x, tileCoords.y, Properties.Zoom);
        for (int x = googleTileCoords.x - Properties.TileViewDistance; x <= googleTileCoords.x + Properties.TileViewDistance; x++)
        {
            for (int y = googleTileCoords.y - Properties.TileViewDistance; y <= googleTileCoords.y + Properties.TileViewDistance; y++)
            {
                tiles.Add(new Tile(this, x, y)); // Add to the list of tiles
            }
        }
    }
}