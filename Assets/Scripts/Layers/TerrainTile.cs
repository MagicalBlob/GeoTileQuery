using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Represents a Terrain layer's tile
/// </summary>
public class TerrainTile : ITile
{
    public ILayer Layer { get; }

    public int Zoom { get; }

    public int X { get; }

    public int Y { get; }

    public string Id { get { return $"{Zoom}/{X}/{Y}"; } }

    public string FullId { get { return $"{Layer.Id}/{Id}"; } }

    public Bounds Bounds { get; }

    public Vector2D Center { get { return Bounds.Center; } }

    public GameObject GameObject { get; }

    public TileState State { get; private set; }

    private Texture2D tmpHeighmapThing; // TODO probably a better idea to only store the converted data

    private string tileRasterUrl;

    /// <summary>
    /// Constructs a new Terrain tile
    /// </summary>
    /// <param name="layer">The layer where the tile belongs</param>
    /// <param name="layerOrigin">The layer's origin in the scene (Meters)</param>
    /// <param name="zoom">Tile's zoom level</param>
    /// <param name="x">Tile's X coordinate</param>
    /// <param name="y">Tile's Y coordinate</param>
    /// <param name="tileRasterUrl">Url to fetch raster tiles</param>
    public TerrainTile(ILayer layer, Vector2D layerOrigin, int zoom, int x, int y, string tileRasterUrl)
    {
        this.State = TileState.Initial;
        this.Layer = layer;
        this.Zoom = zoom;
        this.X = x;
        this.Y = y;
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom); // Calculate tile bounds
        this.tileRasterUrl = tileRasterUrl;

        // Setup the gameobject
        GameObject = new GameObject($"{Id}");
        GameObject.transform.parent = Layer.GameObject.transform; // Set it as a child of the layer gameobject
        Vector2D relativeOrigin = Bounds.Min - layerOrigin;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin
        GameObject.transform.rotation = Layer.GameObject.transform.rotation; // Match tile rotation with the layer
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    public async Task Load()
    {
        // Request heightmap
        string heightmapUrl = $"https://api.mapbox.com/v4/mapbox.terrain-rgb/{Zoom}/{X}/{Y}.pngraw?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest heightmapReq = UnityWebRequestTexture.GetTexture(heightmapUrl);
        UnityWebRequestAsyncOperation heightmapOp = heightmapReq.SendWebRequest();

        // Request raster texture
        string rasterUrl = string.Format(tileRasterUrl, Id);
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

            State = TileState.Loaded;

            // Render the tile
            ((ITerrainRenderer)Layer.Renderer).RenderTerrain(this, rasterTexture);
            State = TileState.Rendered;
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