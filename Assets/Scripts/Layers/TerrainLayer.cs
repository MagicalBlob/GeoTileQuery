using System.Collections.Generic;
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

    public RenderingProperties Properties { get; }

    public GameObject GameObject { get; }

    /// <summary>
    /// Construct a new TerrainLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="id">The layer id</param>
    /// <param name="properties">The layer rendering properties</param>
    public TerrainLayer(GameObject map, string id, RenderingProperties properties)
    {
        this.Id = id;
        this.Properties = properties;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.transform; // Set it as a child of the map gameobject

        tiles = new Dictionary<string, TerrainTile>();
    }

    public void Render()
    {
        int tmpY = Properties.TileViewDistance;
        int tmpX;

        Vector2Int tileCoords = GlobalMercator.MetersToGoogleTile(Properties.CenterX, Properties.CenterY, Properties.Zoom);
        for (int y = tileCoords.y - Properties.TileViewDistance; y <= tileCoords.y + Properties.TileViewDistance; y++)
        {
            tmpX = -Properties.TileViewDistance;
            for (int x = tileCoords.x - Properties.TileViewDistance; x <= tileCoords.x + Properties.TileViewDistance; x++)
            {
                TerrainTile tile = new TerrainTile(this, x, y, tmpY, tmpX);
                tiles.Add(tile.Id, tile);
                tmpX++;
            }
            tmpY--;
        }
    }
}