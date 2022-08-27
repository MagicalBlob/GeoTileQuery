using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Represents a tile's Terrain layer
/// </summary>
public class TerrainTileLayer : ITileLayer
{
    public Tile Tile { get; }

    public ILayer Layer { get; }

    public string FullId { get { return $"{Tile.Id}/{Layer.Id}"; } }

    public GameObject GameObject { get; }

    private Texture2D tmpHeighmapThing; // TODO probably a better idea to only store the converted data

    private string tileRasterUrl;

    /// <summary>
    /// Constructs a new tile's Terrain layer
    /// </summary>
    /// <param name="tile">The tile this layer belongs to</param>
    /// <param name="layer">The map layer that this tile layer is a part of</param>
    public TerrainTileLayer(Tile tile, ILayer layer)
    {
        //this.State = TileState.Initial; TODO: State
        this.Tile = tile;
        this.Layer = layer;
        this.tileRasterUrl = ((TerrainLayer)layer).tileRasterUrl;// TODO this is a temporary hack to get the tile raster url from the map config

        // Setup the gameobject
        GameObject = new GameObject(Layer.Id);
        GameObject.transform.parent = Tile.GameObject.transform; // Set it as a child of the tile gameobject
        GameObject.transform.localPosition = Vector3.zero;
    }

    public async void Load()
    {
        // Request heightmap
        string heightmapUrl = $"https://api.mapbox.com/v4/mapbox.terrain-rgb/{Tile.Zoom}/{Tile.X}/{Tile.Y}.pngraw?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest heightmapReq = UnityWebRequestTexture.GetTexture(heightmapUrl);
        UnityWebRequestAsyncOperation heightmapOp = heightmapReq.SendWebRequest();

        // Request raster texture
        string rasterUrl = string.Format(tileRasterUrl, Tile.Id);
        using UnityWebRequest rasterReq = UnityWebRequestTexture.GetTexture(rasterUrl);
        UnityWebRequestAsyncOperation rasterOp = rasterReq.SendWebRequest();

        // Wait for the requests     
        while (!heightmapOp.isDone || !rasterOp.isDone)
        {
            await Task.Yield();
        }

        // Check for errors
        if (heightmapReq.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError(heightmapReq.error);
        }
        if (rasterReq.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError(rasterReq.error);
        }

        // Render the tile if requests were successful
        if (heightmapReq.result == UnityWebRequest.Result.Success && (rasterReq.result == UnityWebRequest.Result.Success))
        {
            tmpHeighmapThing = DownloadHandlerTexture.GetContent(heightmapReq);
            tmpHeighmapThing.wrapMode = TextureWrapMode.Clamp; // TODO actually take care of the edges

            Texture2D rasterTexture = DownloadHandlerTexture.GetContent(rasterReq);
            rasterTexture.wrapMode = TextureWrapMode.Clamp;

            //State = TileState.Loaded; TODO: State

            // Render the tile
            ((ITerrainRenderer)Layer.Renderer).RenderTerrain(this, rasterTexture);
            //State = TileState.Rendered; TODO: State
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
}