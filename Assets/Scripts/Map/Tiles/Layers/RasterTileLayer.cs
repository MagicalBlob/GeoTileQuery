using System;
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
        GameObject.transform.rotation = Tile.GameObject.transform.rotation; // Match tile rotation with the tile
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Check that the current zoom level is within the layer's zoom range
        if (Tile.Zoom < Layer.MinZoom || Tile.Zoom > Layer.MaxZoom)
        {
            State = TileLayerState.Rendered;
            return;
        }

        // Request raster texture
        DateTime loadCalled = DateTime.Now;
        await MainController.networkSemaphore.WaitAsync(); // Wait for the semaphore so we don't overload the client with too many requests
        DateTime afterSemaphore = DateTime.Now;
        using UnityWebRequest rasterReq = UnityWebRequestTexture.GetTexture(string.Format(Layer.Url, Tile.Id));
        UnityWebRequestAsyncOperation rasterOp = rasterReq.SendWebRequest();

        // Wait for the request
        while (!rasterOp.isDone)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Cancel the request
                rasterReq.Abort();
                MainController.networkSemaphore.Release(); // Release the semaphore
                State = TileLayerState.Unloaded;
                return;
            }
            await Task.Yield();
        }

        if (cancellationToken.IsCancellationRequested)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            State = TileLayerState.Unloaded;
            return;
        }

        // If the gameobject was destroyed before the request finished, we're done here
        if (GameObject == null)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            return;
        }

        // Render the layer if the request was successful
        if (rasterReq.result == UnityWebRequest.Result.Success)
        {
            rasterTexture = DownloadHandlerTexture.GetContent(rasterReq);
            MainController.networkSemaphore.Release(); // Release the semaphore
            DateTime afterRequest = DateTime.Now;
            rasterTexture.wrapMode = TextureWrapMode.Clamp;
            State = TileLayerState.Loaded;

            // Render the tile
            ((IRasterRenderer)Layer.Renderer).Render(this, rasterTexture);
            State = TileLayerState.Rendered;
            DateTime afterRender = DateTime.Now;

            // TODO: Remove this and timers when done testing performance
            if (MainController.DebugMetrics)
            {
                double semaphoreTime = (afterSemaphore - loadCalled).TotalMilliseconds;
                double requestTime = (afterRequest - afterSemaphore).TotalMilliseconds;
                double renderTime = (afterRender - afterRequest).TotalMilliseconds;
                double totalTime = (afterRender - loadCalled).TotalMilliseconds;
                double totalWithoutSemaphore = totalTime - semaphoreTime;

                Debug.Log($"[PERF] {Tile.Id} > Layer ({Layer.Id}): {totalTime} ms [wo/Semaphore: {totalWithoutSemaphore} ms] (Semaphore: {semaphoreTime} ms | Request: {requestTime} ms | Render: {renderTime} ms)");
            }
        }
        else if (rasterReq.responseCode == 404)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            // Render the tile
            ((IRasterRenderer)Layer.Renderer).Render(this, null);
            State = TileLayerState.Rendered;
        }
        else if (rasterReq.responseCode == 503)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            // Render the tile
            ((IRasterRenderer)Layer.Renderer).Render(this, null);
            State = TileLayerState.Rendered;
        }
        else
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
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

    public async Task ApplyTerrainAsync(int count)
    {
        if (State != TileLayerState.Rendered) { return; } // Can't apply terrain if the layer isn't rendered

        // Update the state
        State = TileLayerState.Loaded;

        // HACK: Since we can't destroy/create Gameobjects outside the main thread, we do a pseudo async to not block the UI but not really. This is trash and should think of a better way to do this
        await Task.Delay(250 * count);

        // Destroy the gameobject. If the gameobject was destroyed before then the tile is probably gone, we're done here
        if (GameObject != null) { GameObject.Destroy(GameObject); } else { return; }

        // Re-render the tile
        SetupGameObject();
        ((IRasterRenderer)Layer.Renderer).Render(this, rasterTexture);
        State = TileLayerState.Rendered;
    }
}