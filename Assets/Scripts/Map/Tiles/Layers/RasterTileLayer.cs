using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Represents a tile's Raster layer
/// </summary>
public class RasterTileLayer : ITileLayer
{
    public Tile Tile { get; }

    public ILayer Layer { get; }

    public string FullId { get { return $"{Tile.Id}/{Layer.Id}"; } }

    public GameObject GameObject { get; }

    public TileLayerState State { get; private set; }

    /// <summary>
    /// Constructs a new tile's Raster layer
    /// </summary>
    /// <param name="tile">The tile this layer belongs to</param>
    /// <param name="layer">The map layer that this tile layer is a part of</param>
    public RasterTileLayer(Tile tile, ILayer layer)
    {
        this.Tile = tile;
        this.Layer = layer;

        // Setup the gameobject
        GameObject = new GameObject(Layer.Id);
        GameObject.transform.parent = Tile.GameObject.transform; // Set it as a child of the tile gameobject
        GameObject.transform.localPosition = Vector3.zero;

        this.State = TileLayerState.Initial;
    }

    public async void Load()
    {
        // Request raster texture
        string rasterUrl = string.Format(((RasterLayer)Layer).RasterUrl, Tile.Id);
        using UnityWebRequest rasterReq = UnityWebRequestTexture.GetTexture(rasterUrl);
        UnityWebRequestAsyncOperation rasterOp = rasterReq.SendWebRequest();

        // Wait for the request
        while (!rasterOp.isDone)
        {
            await Task.Yield();
        }

        // Render the layer if the request was successful
        if (rasterReq.result == UnityWebRequest.Result.Success)
        {
            Texture2D rasterTexture = DownloadHandlerTexture.GetContent(rasterReq);
            rasterTexture.wrapMode = TextureWrapMode.Clamp;
            State = TileLayerState.Loaded;

            // Render the tile
            ((IRasterRenderer)Layer.Renderer).Render(this, rasterTexture);
            State = TileLayerState.Rendered;
        }
        else
        {
            Logger.LogError(rasterReq.error);
        }
    }
}