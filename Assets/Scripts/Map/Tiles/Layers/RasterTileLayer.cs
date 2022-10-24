using System.Threading;
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

    public GameObject GameObject { get; private set; }

    public TileLayerState State { get; private set; }

    private Texture2D rasterTexture;

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
        SetupGameObject();

        this.State = TileLayerState.Initial;
    }

    /// <summary>
    /// Sets up the tile layer's GameObject
    /// </summary>
    private void SetupGameObject()
    {
        GameObject = new GameObject(Layer.Id);
        GameObject.SetActive(Layer.Visible);
        GameObject.transform.parent = Tile.GameObject.transform; // Set it as a child of the tile gameobject
        GameObject.transform.localPosition = Vector3.zero;
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Request raster texture
        using UnityWebRequest rasterReq = UnityWebRequestTexture.GetTexture(string.Format(Layer.Url, Tile.Id));
        UnityWebRequestAsyncOperation rasterOp = rasterReq.SendWebRequest();

        // Wait for the request
        while (!rasterOp.isDone)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Cancel the request
                rasterReq.Abort();
                State = TileLayerState.Unloaded;
                return;
            }
            await Task.Yield();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            State = TileLayerState.Unloaded;
            return;
        }

        // If the gameobject was destroyed before the request finished, we're done here
        if (GameObject == null) { return; }

        // Render the layer if the request was successful
        if (rasterReq.result == UnityWebRequest.Result.Success)
        {
            rasterTexture = DownloadHandlerTexture.GetContent(rasterReq);
            rasterTexture.wrapMode = TextureWrapMode.Clamp;
            State = TileLayerState.Loaded;

            // Render the tile
            ((IRasterRenderer)Layer.Renderer).Render(this, rasterTexture);
            State = TileLayerState.Rendered;
        }
        else
        {
            Debug.LogError($"Failed to load `{FullId}`: {rasterReq.error}");
            State = TileLayerState.LoadFailed;
        }
    }

    public void Unload()
    {
        // Update the state
        State = TileLayerState.Unloaded;

        // Destroy the gameobject
        GameObject.Destroy(GameObject);
    }

    public void ApplyTerrain()
    {
        if (State != TileLayerState.Rendered) { return; } // Can't apply terrain if the layer isn't rendered

        // Update the state
        State = TileLayerState.Loaded;

        // Destroy the gameobject
        if (GameObject != null) { GameObject.Destroy(GameObject); }

        // Re-render the tile
        SetupGameObject();
        ((IRasterRenderer)Layer.Renderer).Render(this, rasterTexture);
        State = TileLayerState.Rendered;
    }
}