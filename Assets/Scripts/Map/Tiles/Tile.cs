using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

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
    /// The data layers
    /// </summary>
    public Dictionary<string, ITileLayer> Layers;

    /// <summary>
    /// The tile state
    /// </summary>
    public TileState State { get; private set; }

    /// <summary>
    /// The tile's generation
    /// </summary>
    public uint Generation { get; set; }

    private Texture2D tmpHeighmapThing; // TODO probably a better idea to only store the converted data

    /// <summary>
    /// Constructs a new tile
    /// </summary>
    /// <param name="map">The map to which the tile belongs</param>
    /// <param name="zoom">The tile's zoom level</param>
    /// <param name="x">The tile's X coordinate</param>
    /// <param name="y">The tile's Y coordinate</param>
    /// <param name="generation">The tile's generation</param>
    public Tile(Map map, int zoom, int x, int y, uint generation)
    {
        this.Map = map;
        this.Zoom = zoom;
        this.X = x;
        this.Y = y;
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom); // Calculate tile bounds
        this.Layers = new Dictionary<string, ITileLayer>();
        this.Generation = generation;

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.GameObject.transform; // Set it as a child of the map gameobject
        Vector2D relativeOrigin = Bounds.Min - Map.Origin;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin
        GameObject.transform.rotation = map.GameObject.transform.rotation; // Match tile rotation with the layer

        this.State = TileState.Initial;
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    public async void Load()
    {
        // Load the heightmap
        await LoadHeightmap();

        // Load the layers
        if (State == TileState.LoadedTerrain)
        {
            LoadLayers();
        }
        else
        {
            Logger.LogError($"Can't load layers for tile {Id} because the heightmap hasn't been loaded yet");
        }
    }

    /// <summary>
    /// Load the tile's heightmap
    /// </summary>
    private async Task LoadHeightmap()
    {
        // Request heightmap texture
        string heightmapUrl = $"https://api.mapbox.com/v4/mapbox.terrain-rgb/{Zoom}/{X}/{Y}.pngraw?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest heightmapReq = UnityWebRequestTexture.GetTexture(heightmapUrl);
        UnityWebRequestAsyncOperation heightmapOp = heightmapReq.SendWebRequest();

        // Wait for the request
        while (!heightmapOp.isDone)
        {
            await Task.Yield();
        }

        // Grab the heightmap if the request was successful
        if (heightmapReq.result == UnityWebRequest.Result.Success)
        {
            tmpHeighmapThing = DownloadHandlerTexture.GetContent(heightmapReq);
            tmpHeighmapThing.wrapMode = TextureWrapMode.Clamp; // TODO actually take care of the edges
            State = TileState.LoadedTerrain;
        }
        else
        {
            Logger.LogError(heightmapReq.error);
            State = TileState.LoadFailed;
        }
    }

    /// <summary>
    /// Load the tile layers
    /// </summary>
    private void LoadLayers()
    {
        foreach (ILayer layer in Map.Layers.Values)
        {
            ITileLayer tileLayer;
            switch (layer)
            {
                case GeoJsonLayer geojsonLayer:
                    tileLayer = new GeoJsonTileLayer(this, geojsonLayer);
                    break;
                case RasterLayer rasterLayer:
                    tileLayer = new RasterTileLayer(this, rasterLayer);
                    break;
                default:
                    throw new Exception($"Unknown layer type: {layer.GetType()}");
            }
            Layers.Add(layer.Id, tileLayer);
            tileLayer.Load();
        }
    }

    /// <summary>
    /// Get height at given pixel location
    /// </summary>
    /// <param name="pixelX">The pixel X coordinate</param>
    /// <param name="pixelY">The pixel Y coordinate</param>
    /// <returns>Height at given location (meters)</returns>
    public double GetHeight(int pixelX, int pixelY)
    {
        return MapboxHeightFromColor(tmpHeighmapThing.GetPixel(pixelX, pixelY));
    }

    /// <summary>
    /// Decode pixel values to height values. The height will be returned in meters
    /// </summary>
    /// <param name="color">The queried location's pixel</param>
    /// <returns>Height at location (meters)</returns>
    private double MapboxHeightFromColor(Color color)
    {
        // Convert from 0..1 to 0..255
        float R = color.r * 255;
        float G = color.g * 255;
        float B = color.b * 255;

        return -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1);
    }

    /// <summary>
    /// Move the tile according to the given vector
    /// </summary>
    /// <param name="delta">The vector to move the tile by</param>
    internal void Move(Vector2D delta)
    {
        GameObject.transform.localPosition += new Vector3((float)delta.X, 0, (float)delta.Y);
    }

    /// <summary>
    /// Unload the tile
    /// </summary>
    /// <returns>Whether the tile was unloaded</returns>
    public bool Unload()
    {
        // Check if the tile can be unloaded
        if (State != TileState.LoadedTerrain && State != TileState.LoadFailed)
        {
            // Need to wait for the tile terrain to load or at least fail to load
            return false;
        }
        foreach (ITileLayer tileLayer in Layers.Values)
        {
            if (tileLayer.State != TileLayerState.Rendered && tileLayer.State != TileLayerState.LoadFailed)
            {
                // Need to wait for the tile layer to render or at least have fail to load
                return false;
            }
        }

        // Update the state
        State = TileState.Unloaded;

        // Unload the layers
        foreach (ITileLayer tileLayer in Layers.Values)
        {
            tileLayer.Unload();
        }
        Layers.Clear();

        // Destroy the gameobject
        GameObject.Destroy(GameObject);

        return true;
    }
}