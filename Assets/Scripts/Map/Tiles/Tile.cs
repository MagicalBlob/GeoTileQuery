using System;
using System.Collections.Generic;
using System.Threading;
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

    /// <summary>
    /// The elevation data (meters)
    /// </summary>
    private double[,] heights;
    /// <summary>
    /// Whether we checked the north tile for elevation data
    /// </summary>
    private bool checkedNeighbourNorth;
    /// <summary>
    /// Whether we checked the east tile for elevation data
    /// </summary>
    private bool checkedNeighbourEast;
    /// <summary>
    /// Whether we checked the north-east tile for elevation data
    /// </summary>
    private bool checkedNeighbourNorthEast;

    /// <summary>
    /// The terrain data
    /// </summary>
    private TerrainData terrainData;

    private CancellationTokenSource _cancellationTokenSource;

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
        this.heights = new double[Map.TileSize + 1, Map.TileSize + 1]; // +1 to prevent seams
        _cancellationTokenSource = new CancellationTokenSource();

        // Setup the gameobject
        GameObject = new GameObject(Id);
        GameObject.transform.parent = map.GameObject.transform; // Set it as a child of the map gameobject
        Vector2D relativeOrigin = Bounds.Min - Map.Center;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin
        GameObject.transform.rotation = map.GameObject.transform.rotation; // Match tile rotation with the layer

        this.State = TileState.Initial;
    }

    /// <summary>
    /// Load the tile asynchronously
    /// </summary>
    public async Task LoadAsync()
    {
        // Load the terrain
        await LoadTerrainAsync();

        // Load the layers
        if (State == TileState.TerrainLoaded)
        {
            LoadLayers();

            // If we couldn't check all neighbours, schedule another check
            _ = CheckNeighboursAsync();
        }
        else if (State != TileState.Unloaded)
        {
            Debug.LogError($"Can't load layers for tile {Id} because the terrain is not loaded");
        }
    }

    /// <summary>
    /// Load the tile's terrain asynchronously
    /// </summary>
    private async Task LoadTerrainAsync()
    {
        // Request heightmap texture
        string heightmapUrl = $"https://api.mapbox.com/v4/mapbox.terrain-rgb/{Zoom}/{X}/{Y}.pngraw?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest heightmapReq = UnityWebRequestTexture.GetTexture(heightmapUrl);
        UnityWebRequestAsyncOperation heightmapOp = heightmapReq.SendWebRequest();

        // Wait for the request
        while (!heightmapOp.isDone)
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Cancel the request
                heightmapReq.Abort();
                State = TileState.Unloaded;
                return;
            }
            await Task.Yield();
        }

        if (_cancellationTokenSource.Token.IsCancellationRequested)
        {
            State = TileState.Unloaded;
            return;
        }

        // If the gameobject was destroyed before the request finished, we're done here
        if (GameObject == null) { return; }

        // Grab the heightmap if the request was successful
        if (heightmapReq.result == UnityWebRequest.Result.Success)
        {
            Texture2D heightmapTexture = DownloadHandlerTexture.GetContent(heightmapReq);
            Color[] pixels = heightmapTexture.GetPixels();

            // Convert the encoded image into the respective heights
            for (int x = 0; x < Map.TileSize; x++)
            {
                for (int y = 0; y < Map.TileSize; y++)
                {
                    heights[x, y] = MapboxHeightFromColor(pixels[x + y * Map.TileSize]);
                }
            }

            // Check the neighbours to prevent seams
            CheckNeighbours();

            // Clamp the north edge if we couldn't check the north tile
            if (!checkedNeighbourNorth)
            {
                for (int x = 0; x < Map.TileSize; x++)
                {
                    heights[x, Map.TileSize] = heights[x, Map.TileSize - 1];
                }
            }

            // Clamp the east edge if we couldn't check the east tile
            if (!checkedNeighbourEast)
            {
                for (int y = 0; y < Map.TileSize; y++)
                {
                    heights[Map.TileSize, y] = heights[Map.TileSize - 1, y];
                }
            }

            // Clamp the north-east corner if we couldn't check the north-east tile
            if (!checkedNeighbourNorthEast)
            {
                heights[Map.TileSize, Map.TileSize] = heights[Map.TileSize - 1, Map.TileSize - 1];
            }

            // Update the state
            State = TileState.TerrainLoaded;
        }
        else if (heightmapReq.responseCode == 404)
        {
            Debug.LogWarning($"Tile {Id} not found. Assuming it's all water...");
            State = TileState.TerrainLoaded;
        }
        else
        {
            Debug.LogError(heightmapReq.error);
            State = TileState.TerrainLoadFailed;
        }
    }

    /// <summary>
    /// Decode pixel values to height values. The height will be returned in meters
    /// </summary>
    /// <param name="color">The color with the encoded height</param>
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
    /// Checks the neighbours of this tile for elevation data to prevent seams
    /// </summary>
    private void CheckNeighbours()
    {
        // North tile
        if (!checkedNeighbourNorth)
        {
            if (Map.Tiles.TryGetValue($"{Zoom}/{X}/{Y - 1}", out Tile northTile) && northTile.State >= TileState.TerrainLoaded)
            {
                for (int x = 0; x < Map.TileSize; x++)
                {
                    heights[x, Map.TileSize] = northTile.heights[x, 0];
                }
                checkedNeighbourNorth = true;
            }
        }

        // East tile
        if (!checkedNeighbourEast)
        {
            if (Map.Tiles.TryGetValue($"{Zoom}/{X + 1}/{Y}", out Tile eastTile) && eastTile.State >= TileState.TerrainLoaded)
            {
                for (int y = 0; y < Map.TileSize; y++)
                {
                    heights[Map.TileSize, y] = eastTile.heights[0, y];
                }
                checkedNeighbourEast = true;
            }
        }

        // North-East tile
        if (!checkedNeighbourNorthEast)
        {
            if (Map.Tiles.TryGetValue($"{Zoom}/{X + 1}/{Y - 1}", out Tile northEastTile) && northEastTile.State >= TileState.TerrainLoaded)
            {
                heights[Map.TileSize, Map.TileSize] = northEastTile.heights[0, 0];
                checkedNeighbourNorthEast = true;
            }
        }
    }

    /// <summary>
    /// Schedules a check on the neighbours of this tile for elevation data asynchronously to prevent seams
    /// </summary>
    public async Task CheckNeighboursAsync()
    {
        if (checkedNeighbourNorth && checkedNeighbourEast && checkedNeighbourNorthEast)
        {
            return; // If we already checked all neighbours, don't need to do anything
        }

        // Wait for the neighbours to load
        await Task.Delay(30000);

        if (GameObject == null) { return; } // If the GameObject was destroyed in the meantime, we're done here

        bool checkedNeighbourNorthBefore = checkedNeighbourNorth;
        bool checkedNeighbourEastBefore = checkedNeighbourEast;
        bool checkedNeighbourNorthEastBefore = checkedNeighbourNorthEast;

        // Check the neighbours
        CheckNeighbours();

        // Update the terrain data if it exists and we have new data from the neighbours
        // North
        if (terrainData != null && checkedNeighbourNorthBefore != checkedNeighbourNorth)
        {
            float[,] northHeights = new float[1, Map.TileSize];
            for (int x = 0; x < Map.TileSize; x++)
            {
                northHeights[0, x] = (float)((heights[x, Map.TileSize] - Map.MinElevation) / (Map.MaxElevation - Map.MinElevation));
            }
            terrainData.SetHeights(0, Map.TileSize, northHeights);
        }
        // East
        if (terrainData != null && checkedNeighbourEastBefore != checkedNeighbourEast)
        {
            float[,] eastHeights = new float[Map.TileSize, 1];
            for (int y = 0; y < Map.TileSize; y++)
            {
                eastHeights[y, 0] = (float)((heights[Map.TileSize, y] - Map.MinElevation) / (Map.MaxElevation - Map.MinElevation));
            }
            terrainData.SetHeights(Map.TileSize, 0, eastHeights);
        }
        // North-East
        if (terrainData != null && checkedNeighbourNorthEastBefore != checkedNeighbourNorthEast)
        {
            float northEastHeight = (float)((heights[Map.TileSize, Map.TileSize] - Map.MinElevation) / (Map.MaxElevation - Map.MinElevation));
            terrainData.SetHeights(Map.TileSize, Map.TileSize, new float[1, 1] { { northEastHeight } });
        }
    }

    /// <summary>
    /// Get the Unity Terrain data
    /// </summary>
    /// <returns>The terrain data</returns>
    /// <remarks>Will return null if the terrain data is not loaded</remarks>
    public TerrainData GetTerrainData()
    {
        if (State < TileState.TerrainLoaded)
        {
            // The terrain data is not loaded
            Debug.LogWarning($"Terrain data for tile {Id} is not loaded");
            return null;
        }
        if (terrainData != null)
        {
            // Terrain data already exists, just return it
            return terrainData;
        }
        else
        {
            // Create the terrain data
            terrainData = new TerrainData();
            terrainData.GetHeight(0, 0);
            int terrainLength = Map.TileSize + 1;
            terrainData.heightmapResolution = terrainLength;
            terrainData.size = new Vector3((float)Bounds.Width, (float)(Map.MaxElevation - Map.MinElevation), (float)Bounds.Height);
            float[,] tileHeights = new float[terrainLength, terrainLength];
            for (int y = 0; y < terrainLength; y++)
            {
                for (int x = 0; x < terrainLength; x++)
                {
                    // Get the elevation value and scale it to the 0..1 range
                    tileHeights[y, x] = (float)((heights[x, y] - Map.MinElevation) / (Map.MaxElevation - Map.MinElevation));
                }
            }
            terrainData.SetHeights(0, 0, tileHeights);
            return terrainData;
        }
    }

    /// <summary>
    /// Get height at given pixel location
    /// </summary>
    /// <param name="pixelX">The pixel X coordinate</param>
    /// <param name="pixelY">The pixel Y coordinate</param>
    /// <returns>Height at given location (meters)</returns>
    /// <remarks>
    /// Returns 0 if the tile terrain isn't loaded or the edge clamped value if the pixel is outside the tile
    /// </remarks>
    public double GetHeight(int pixelX, int pixelY)
    {
        // Check tile state
        if (State == TileState.Initial || State == TileState.TerrainLoadFailed || State == TileState.Unloaded)
        {
            Debug.LogWarning($"Can't get height for tile {Id} because the terrain isn't loaded");
            return 0;
        }
        // Clamp pixel coordinates
        if (pixelX < 0)
        {
            pixelX = 0;
            Debug.LogWarning($"Can't get height for tile {Id} because the pixel coordinates ({pixelX}, {pixelY}) are out of bounds");
        }
        else if (pixelX > Map.TileSize)
        {
            pixelX = Map.TileSize;
            Debug.LogWarning($"Can't get height for tile {Id} because the pixel coordinates ({pixelX}, {pixelY}) are out of bounds");
        }
        if (pixelY < 0)
        {
            pixelY = 0;
            Debug.LogWarning($"Can't get height for tile {Id} because the pixel coordinates ({pixelX}, {pixelY}) are out of bounds");
        }
        else if (pixelY > Map.TileSize)
        {
            pixelY = Map.TileSize;
            Debug.LogWarning($"Can't get height for tile {Id} because the pixel coordinates ({pixelX}, {pixelY}) are out of bounds");
        }
        // Return the height value
        return heights[pixelX, pixelY];
    }

    /// <summary>
    /// Get height at given location
    /// </summary>
    /// <param name="point">The point on the map (WGS84)</param>
    /// <returns>Height at given location (meters)</returns>
    /// <remarks>
    /// Returns the edge clamped value if the point is outside the tile
    /// </remarks>
    public double GetHeight(Vector2D point)
    {
        // Get the pixel coordinates
        int pixelX = (int)(((point.X - Bounds.Min.X) * Map.TileSize) / Bounds.Width);
        int pixelY = (int)(((point.Y - Bounds.Min.Y) * Map.TileSize) / Bounds.Height);

        // Get the height
        return GetHeight(pixelX, pixelY);
    }

    /// <summary>
    /// Load the tile layers
    /// </summary>
    private void LoadLayers()
    {
        List<Task> layerLoadTasks = new List<Task>();
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
            layerLoadTasks.Add(tileLayer.LoadAsync(_cancellationTokenSource.Token));
        }

        // Wait for all layers to finish loading
        Task.WhenAll(layerLoadTasks).ContinueWith((task) =>
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                // The operation was cancelled
                State = TileState.Unloaded;
                return;
            }

            // Check if all layers loaded successfully
            foreach (ITileLayer tileLayer in Layers.Values)
            {
                if (tileLayer.State != TileLayerState.Rendered)
                {
                    // A layer failed to load
                    State = TileState.LayersLoadFailed;
                }
            }

            // If all layers loaded successfully, set the tile state to loaded
            if (State != TileState.LayersLoadFailed)
            {
                State = TileState.Loaded;
            }
        });
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
    public void Unload()
    {
        // Check if the tile can be unloaded
        if (State == TileState.Initial || State == TileState.TerrainLoaded)
        {
            // Abort loading terrain or tile layers
            _cancellationTokenSource.Cancel();
        }

        // Update the state
        State = TileState.Unloaded;

        // Destroy the gameobject
        GameObject.Destroy(GameObject);

        // Unload the layers
        foreach (ITileLayer tileLayer in Layers.Values)
        {
            tileLayer.Unload();
        }
        Layers.Clear();
    }
}