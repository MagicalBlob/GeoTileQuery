using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Represents a Terrain layer's tile
/// </summary>
public class TerrainTile : ITile
{
    public ILayer Layer { get; }

    public int Zoom { get { return Layer.Zoom; } }

    public int X { get; }

    public int Y { get; }

    public string Id { get { return $"{Zoom}/{X}/{Y}"; } }

    public string FullId { get { return $"{Layer.Id}/{Id}"; } }

    public Bounds Bounds { get; }

    public Vector2D Center { get { return Bounds.Center; } }

    public GameObject GameObject { get; }

    public TileState State { get; }

    private Texture2D tmpHeighmapThing; // TODO probably a better idea to only store the converted data

    /// <summary>
    /// Constructs a new Terrain tile
    /// </summary>
    /// <param name="layer">The layer where the tile belongs</param>
    /// <param name="x">Tile's X coordinate</param>
    /// <param name="y">Tile's Y coordinate</param>
    public TerrainTile(ILayer layer, int x, int y)
    {
        this.Layer = layer;
        this.X = x;
        this.Y = y;
        // Calculate tile bounds
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom);

        // Setup the gameobject
        GameObject = new GameObject($"{Id}");
        GameObject.transform.parent = Layer.GameObject.transform; // Set it as a child of the layer gameobject
        Vector2D relativeOrigin = Bounds.Min - Layer.Origin;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin
        GameObject.transform.rotation = Layer.GameObject.transform.rotation; // Match tile rotation with the layer

        // Load and render the tile
        Load();
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    private async void Load()
    {
        // Request heightmap
        string heightmapUrl = $"https://api.mapbox.com/v4/mapbox.terrain-rgb/{Zoom}/{X}/{Y}.pngraw?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest heightmapReq = UnityWebRequestTexture.GetTexture(heightmapUrl);
        UnityWebRequestAsyncOperation heightmapOp = heightmapReq.SendWebRequest();

        // Request raster texture
        string rasterUrl = $"https://api.mapbox.com/v4/{Layer.Id}/{Zoom}/{X}/{Y}.jpg?access_token={MainController.MapboxAccessToken}";
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

            // Render the tile
            ((ITerrainRenderer)Layer.Renderer).RenderTerrain(this, rasterTexture);
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