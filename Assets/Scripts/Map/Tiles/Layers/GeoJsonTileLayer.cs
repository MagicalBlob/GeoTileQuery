using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a tile's GeoJson layer
/// </summary>
public class GeoJsonTileLayer : IFilterableTileLayer
{
    public Tile Tile { get; }

    public ILayer Layer { get; }

    public string FullId { get { return $"{Tile.Id}/{Layer.Id}"; } }

    public GameObject GameObject { get; private set; }

    public TileLayerState State { get; private set; }

    /// <summary>
    /// The tile layer's GeoJSON
    /// </summary>
    private IGeoJsonObject geoJson;

    /// <summary>
    /// Constructs a new tile's GeoJson layer
    /// </summary>
    /// <param name="tile">The tile this layer belongs to</param>
    /// <param name="layer">The map layer that this tile layer is a part of</param>
    public GeoJsonTileLayer(Tile tile, ILayer layer)
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

        DateTime loadCalled = DateTime.Now;
        await MainController.networkSemaphore.WaitAsync(cancellationToken); // Wait for the semaphore so we don't overload the client with too many requests
        try
        {
            DateTime afterSemaphore = DateTime.Now;
            HttpResponseMessage response = await MainController.client.GetAsync(string.Format(Layer.Url, Tile.Id), cancellationToken);
            DateTime afterRequestA = DateTime.Now;
            string geoJsonText = await response.Content.ReadAsStringAsync();
            DateTime afterRequestB = DateTime.Now;
            MainController.networkSemaphore.Release(); // Release the semaphore
            DateTime afterRequest = DateTime.Now;

            if (cancellationToken.IsCancellationRequested)
            {
                State = TileLayerState.Unloaded;
                return;
            }

            // If the gameobject was destroyed before the request finished, we're done here
            if (GameObject == null) { return; }
            State = TileLayerState.Loaded;

            // Process the GeoJSON
            try
            {
                // Parse the GeoJSON text
                geoJson = GeoJson.Parse(geoJsonText);
                DateTime afterParse = DateTime.Now;

                // Check if it's a FeatureCollection
                if (geoJson.GetType() == typeof(FeatureCollection))
                {
                    // Render the GeoJSON
                    ((FeatureCollection)geoJson).Render(this);
                    State = TileLayerState.Rendered;
                    DateTime afterRender = DateTime.Now;

                    // TODO: Remove this and timers when done testing performance
                    bool showPerformance = false;
                    if (showPerformance && (Layer.Id == "Buildings" || Layer.Id == "Roads" || Layer.Id == "Trees"))
                    {
                        double semaphoreTime = (afterSemaphore - loadCalled).TotalSeconds;
                        double requestTimeA = (afterRequestA - afterSemaphore).TotalSeconds;
                        double requestTimeB = (afterRequestB - afterRequestA).TotalSeconds;
                        double requestTimeC = (afterRequest - afterRequestB).TotalSeconds;
                        double requestTime = (afterRequest - afterSemaphore).TotalSeconds;
                        double parseTime = (afterParse - afterRequest).TotalSeconds;
                        double renderTime = (afterRender - afterParse).TotalSeconds;
                        double totalTime = (afterRender - loadCalled).TotalSeconds;

                        // Check which of the three stages took the longest
                        if (requestTime > parseTime && requestTime > renderTime)
                        {
                            // Check which of the request stages took the longest
                            if (requestTimeA > requestTimeB && requestTimeA > requestTimeC)
                            {
                                Debug.Log($"{FullId} : Semaphore > {semaphoreTime} | [REQUEST] > {requestTime} ( [A] > {requestTimeA} | B > {requestTimeB} | C > {requestTimeC} ) | Parse > {parseTime} | Render > {renderTime} | TOTAL > {totalTime}");
                            }
                            else if (requestTimeB > requestTimeC)
                            {
                                Debug.Log($"{FullId} : Semaphore > {semaphoreTime} | [REQUEST] > {requestTime} ( A > {requestTimeA} | [B] > {requestTimeB} | C > {requestTimeC} ) | Parse > {parseTime} | Render > {renderTime} | TOTAL > {totalTime}");
                            }
                            else
                            {
                                Debug.Log($"{FullId} : Semaphore > {semaphoreTime} | [REQUEST] > {requestTime} ( A > {requestTimeA} | B > {requestTimeB} | [C] > {requestTimeC} ) | Parse > {parseTime} | Render > {renderTime} | TOTAL > {totalTime}");
                            }
                        }
                        else if (parseTime > renderTime)
                        {
                            Debug.Log($"{FullId} : Semaphore > {semaphoreTime} | Request > {requestTime} | [PARSE] > {parseTime} | Render > {renderTime} | TOTAL > {totalTime}");
                        }
                        else
                        {
                            Debug.Log($"{FullId} : Semaphore > {semaphoreTime} | Request > {requestTime} | Parse > {parseTime} | [RENDER] > {renderTime} | TOTAL > {totalTime}");
                        }
                    }

                    // Apply the filters
                    ApplyFilters();
                }
                else
                {
                    // Can't render the tile. GeoJSON root isn't a FeatureCollection
                    throw new InvalidGeoJsonException($"Can't render the tile {FullId}. GeoJSON root isn't a FeatureCollection");
                }
            }
            catch (InvalidGeoJsonException e)
            {
                Debug.LogException(e);
            }
            State = TileLayerState.Rendered;
        }
        catch (HttpRequestException e)
        {
            MainController.networkSemaphore.Release(); // Release the semaphore
            Debug.LogException(e);
            State = cancellationToken.IsCancellationRequested ? TileLayerState.Unloaded : TileLayerState.LoadFailed;
        }
        catch (TaskCanceledException)
        {
            // The request was cancelled
            MainController.networkSemaphore.Release(); // Release the semaphore
            State = TileLayerState.Unloaded;
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

        // Re-render the GeoJSON
        SetupGameObject();
        if (geoJson.GetType() == typeof(FeatureCollection))
        {
            // Render the GeoJSON
            ((FeatureCollection)geoJson).Render(this);
            State = TileLayerState.Rendered;

            // Apply the filters
            ApplyFilters();
        }
        else
        {
            // Can't render the tile. GeoJSON root isn't a FeatureCollection
            Debug.LogException(new InvalidGeoJsonException($"Can't render the tile {FullId}. GeoJSON root isn't a FeatureCollection"));
            State = TileLayerState.Rendered;
        }
    }

    public void ApplyFilters()
    {
        if (State != TileLayerState.Rendered) { return; } // Can't apply filters if the layer isn't rendered

        foreach (Feature feature in ((FeatureCollection)geoJson).Features)
        {
            // Can't apply filters to a feature that doesn't have a GameObject
            if (feature.GameObject == null) { continue; }

            // Check if the feature's properties match the filters
            bool hideFeature = false;
            foreach (IFeatureProperty property in ((IFilterableLayer)Layer).FeatureProperties)
            {
                if (property.Filtered && !property.MatchesFilter(feature))
                {
                    // The feature doesn't match the filter
                    hideFeature = true;
                    break;
                }
            }

            if (hideFeature)
            {
                // The feature doesn't match the filters, toggle its visibility according to the layer's filter setting
                feature.GameObject.SetActive(!((IFilterableLayer)Layer).Filtered);
            }
            else
            {
                // The feature matches the filters, it should be visible regardless of the layer's filter setting
                feature.GameObject.SetActive(true);
            }
        }
    }
}