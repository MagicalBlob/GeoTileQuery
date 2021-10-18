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

    public Vector2D Origin { get; }

    public int Zoom { get; }

    public int TileViewDistance { get; }

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
    /// <param name="origin">The layer's origin in the scene (Meters)</param>
    /// <param name="zoom">Zoom level for the tiles</param>
    /// <param name="tileRadius">Radius of tiles to be loaded</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    /// <param name="renderer">The layer's renderer</param>
    public GeoJsonLayer(GameObject map, string id, Vector2D origin, int zoom, int tileRadius, string idPropertyName, IGeoJsonRenderer renderer)
    {
        this.Id = id;
        this.Origin = origin;
        this.Zoom = zoom;
        this.TileViewDistance = tileRadius;
        this.IdPropertyName = idPropertyName;
        this.Renderer = renderer;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.transform; // Set it as a child of the map gameobject
        GameObject.transform.localPosition = Vector3.zero;

        tiles = new Dictionary<string, GeoJsonTile>();
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
                    GeoJsonTile tile = new GeoJsonTile(this, x, y);
                    tiles.Add(tile.Id, tile);
                }
            }
        }
    }
}