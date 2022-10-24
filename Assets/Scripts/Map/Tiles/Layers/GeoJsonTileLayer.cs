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
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        await MainController.networkSemaphore.WaitAsync(cancellationToken); // Wait for the semaphore so we don't overload the client with too many requests
        try
        {
            string geoJsonText = await Task<string>.Run(async () => await MainController.client.GetStringAsync(string.Format(Layer.Url, Tile.Id)), cancellationToken);
            MainController.networkSemaphore.Release(); // Release the semaphore

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

                // Check if it's a FeatureCollection
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

    public void ApplyTerrain()
    {
        if (State != TileLayerState.Rendered) { return; } // Can't apply terrain if the layer isn't rendered

        // Update the state
        State = TileLayerState.Loaded;

        // Destroy the gameobject
        if (GameObject != null) { GameObject.Destroy(GameObject); }

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