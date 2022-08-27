using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a tile's GeoJson layer
/// </summary>
public class GeoJsonTileLayer : ITileLayer
{
    public Tile Tile { get; }

    public ILayer Layer { get; }

    public string FullId { get { return $"{Tile.Id}/{Layer.Id}"; } }

    public GameObject GameObject { get; }

    /// <summary>
    /// The layer's GeoJSON
    /// </summary>
    private IGeoJsonObject geoJson;

    /// <summary>
    /// Constructs a new tile's GeoJson layer
    /// </summary>
    /// <param name="tile">The tile this layer belongs to</param>
    /// <param name="layer">The map layer that this tile layer is a part of</param>
    public GeoJsonTileLayer(Tile tile, ILayer layer)
    {
        //this.State = TileState.Initial; TODO: State
        this.Tile = tile;
        this.Layer = layer;

        // Setup the gameobject
        GameObject = new GameObject(Layer.Id);
        GameObject.transform.parent = Tile.GameObject.transform; // Set it as a child of the tile gameobject
        GameObject.transform.localPosition = Vector3.zero;
    }

    public async void Load()
    {
        DateTime loadCalled = DateTime.Now;
        await MainController.networkSemaphore.WaitAsync(); // Wait for the semaphore so we don't overload the client with too many requests
        try
        {
            DateTime afterSemaphore = DateTime.Now;
            string geoJsonText = await MainController.client.GetStringAsync($"https://tese.flamino.eu/api/tiles/{Layer.Id}/{Tile.Zoom}/{Tile.X}/{Tile.Y}.geojson");
            MainController.networkSemaphore.Release(); // Release the semaphore
            DateTime afterRequest = DateTime.Now;
            try
            {
                // Parse the GeoJSON text
                geoJson = GeoJson.Parse(geoJsonText);
                DateTime afterParse = DateTime.Now;
                //State = TileState.Loaded; TODO: State

                // Check if it's a FeatureCollection
                if (geoJson.GetType() == typeof(FeatureCollection))
                {
                    // Render the GeoJSON
                    ((FeatureCollection)geoJson).Render(this);
                    //State = TileState.Rendered; TODO: State
                }
                else
                {
                    // Can't render the tile. GeoJSON root isn't a FeatureCollection
                    throw new InvalidGeoJsonException("Can't render the tile. GeoJSON root isn't a FeatureCollection");
                }
                DateTime afterRender = DateTime.Now;
                Logger.Log($"{FullId} : Semaphore > {(afterSemaphore - loadCalled).TotalSeconds} | Request > {(afterRequest - afterSemaphore).TotalSeconds} | Parse > {(afterParse - afterRequest).TotalSeconds} | Render > {(afterRender - afterParse).TotalSeconds} | TOTAL > {(afterRender - loadCalled).TotalSeconds}");
            }
            catch (InvalidGeoJsonException e)
            {
                Logger.LogException(e);
            }
        }
        catch (HttpRequestException e)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            Logger.LogException(e);
        }
        catch (TaskCanceledException e)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            Logger.LogException(e);
        }
    }
}