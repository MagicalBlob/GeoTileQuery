using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a map's tile
/// </summary>
public class Tile
{
    /// <summary>
    /// The map to which the tile belongs
    /// </summary>
    public Map Map { get; }

    /// <summary>
    /// The tile's zoom level
    /// </summary>
    public int Zoom { get; }

    /// <summary>
    /// The tile's X coordinate
    /// </summary>
    public int X { get; }

    /// <summary>
    /// The tile's Y coordinate
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// The tile ID
    /// </summary>
    public string Id { get { return $"{Zoom}/{X}/{Y}"; } }

    /// <summary>
    /// The bounds of the tile (meters)
    /// </summary>
    public Bounds Bounds { get; }

    /// <summary>
    /// The center of the tile (meters)
    /// </summary>
    public Vector2D Center { get { return Bounds.Center; } }

    /// <summary>
    /// The tile's GameObject representation
    /// </summary>
    public GameObject GameObject { get; }

    /// <summary>
    /// The tile state //TODO
    /// </summary>
    public TileState State { get; private set; }

    /// <summary>
    /// The data layers
    /// </summary>
    private Dictionary<string, ITileLayer> layers;

    /// <summary>
    /// Constructs a new tile
    /// </summary>
    /// <param name="map">The map to which the tile belongs</param>
    /// <param name="origin">The map's origin in the scene (Meters)</param>
    /// <param name="zoom">The tile's zoom level</param>
    /// <param name="x">The tile's X coordinate</param>
    /// <param name="y">The tile's Y coordinate</param>
    public Tile(Map map, Vector2D origin, int zoom, int x, int y)
    {
        this.Map = map;
        this.Zoom = zoom;
        this.X = x;
        this.Y = y;
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom); // Calculate tile bounds
        layers = new Dictionary<string, ITileLayer>();

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.GameObject.transform; // Set it as a child of the map gameobject
        Vector2D relativeOrigin = Bounds.Min - origin;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin
        GameObject.transform.rotation = map.GameObject.transform.rotation; // Match tile rotation with the layer
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    public void Load()
    {
        // Load the heightmap
        // TODO: Load the heightmap

        // Load the layers
        foreach (ILayer layer in Map.Layers.Values)
        {
            ITileLayer tileLayer;
            switch (layer)
            {
                case GeoJsonLayer geojsonLayer:
                    tileLayer = new GeoJsonTileLayer(this, geojsonLayer);
                    break;
                case TerrainLayer terrainLayer:
                    tileLayer = new TerrainTileLayer(this, terrainLayer);
                    break;
                default:
                    throw new Exception($"Unknown layer type: {layer.GetType()}");
            }
            tileLayer.Load();
            layers.Add(layer.Id, tileLayer);
        }
    }
}