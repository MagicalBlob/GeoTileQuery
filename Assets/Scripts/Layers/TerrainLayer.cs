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

    public Vector2D Origin { get; }

    public int Zoom { get; }

    public int TileViewDistance { get; }

    public ILayerRenderer Renderer { get; }

    public GameObject GameObject { get; }

    /// <summary>
    /// Construct a new TerrainLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="id">The layer id</param>
    /// <param name="origin">The layer's origin in the scene (Meters)</param>
    /// <param name="zoom">Zoom level for the tiles</param>
    /// <param name="tileRadius">Radius of tiles to be loaded</param>
    /// <param name="renderer">The layer's renderer</param>
    public TerrainLayer(GameObject map, string id, Vector2D origin, int zoom, int tileRadius, ITerrainRenderer renderer)
    {
        this.Id = id;
        this.Origin = origin;
        this.Zoom = zoom;
        this.TileViewDistance = tileRadius;
        this.Renderer = renderer;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.transform; // Set it as a child of the map gameobject
        GameObject.transform.localPosition = Vector3.zero;

        tiles = new Dictionary<string, TerrainTile>();
    }

    public void Render()
    {
        Vector2Int tileCoords = GlobalMercator.MetersToGoogleTile(Origin, Zoom);
        for (int y = tileCoords.y - TileViewDistance; y <= tileCoords.y + TileViewDistance; y++)
        {
            for (int x = tileCoords.x - TileViewDistance; x <= tileCoords.x + TileViewDistance; x++)
            {
                if (!tiles.ContainsKey($"{Zoom}/{x}/{y}"))
                {
                    // Only render tiles that haven't been rendered already
                    TerrainTile tile = new TerrainTile(this, x, y);
                    tiles.Add(tile.Id, tile);
                }
            }
        }
    }
}