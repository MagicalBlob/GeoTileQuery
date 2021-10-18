using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a GeoJson layer's tile
/// </summary>
public class GeoJsonTile : ITile
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

    public TileState State { get; private set; }

    /// <summary>
    /// The tile's GeoJSON
    /// </summary>
    private IGeoJsonObject geoJson;

    /// <summary>
    /// Constructs a new GeoJson tile
    /// </summary>
    /// <param name="layer">The layer where the tile belongs</param>
    /// <param name="x">Tile's X coordinate</param>
    /// <param name="y">Tile's Y coordinate</param>
    public GeoJsonTile(ILayer layer, int x, int y)
    {
        this.State = TileState.Initial;
        this.Layer = layer;
        this.X = x;
        this.Y = y;
        // Calculate tile bounds and center
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom);

        // Setup the gameobject
        GameObject = new GameObject($"{Id}");
        GameObject.transform.parent = Layer.GameObject.transform; // Set it as a child of the layer gameobject
        Vector2D relativeOrigin = Bounds.Min - Layer.Origin;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin
        GameObject.transform.rotation = Layer.GameObject.transform.rotation; // Match tile rotation with the layer

        // Load the tile data
        Load();
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    private async void Load()
    {

        DateTime loadCalled = DateTime.Now;
        await MainController.networkSemaphore.WaitAsync(); // Wait for the semaphore so we don't overload the client with too many requests
        try
        {
            DateTime afterSemaphore = DateTime.Now;
            string geoJsonText = await MainController.client.GetStringAsync($"https://tese.flamino.eu/api/tiles/{Layer.Id}/{Zoom}/{X}/{Y}.geojson");
            MainController.networkSemaphore.Release(); // Release the semaphore
            DateTime afterRequest = DateTime.Now;
            try
            {
                // Parse the GeoJSON text
                geoJson = GeoJson.Parse(geoJsonText);
                DateTime afterParse = DateTime.Now;
                State = TileState.Loaded;
                // Render the tile
                Render();
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

    /// <summary>
    /// Render the tile
    /// </summary>
    /// <exception cref="InvalidGeoJsonException">Can't render as a tile, if root object isn't a FeatureCollection</exception>
    private void Render()
    {
        // Check if data has been loaded
        if (State == TileState.Loaded)
        {
            // Check if it's a FeatureCollection
            if (geoJson.GetType() == typeof(FeatureCollection))
            {
                // Render the GeoJSON
                ((FeatureCollection)geoJson).Render(this);
                State = TileState.Rendered;
            }
            else
            {
                // Can't render the tile. GeoJSON root isn't a FeatureCollection
                throw new InvalidGeoJsonException("Can't render the tile. GeoJSON root isn't a FeatureCollection");
            }
        }
        else
        {
            // Can't render the tile. Data hasn't been loaded yet
            Logger.LogError("Tried to render tile before it was loaded!");
        }
    }
}