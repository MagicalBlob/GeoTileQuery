using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Represents a map tile
/// </summary>
public class Tile
{
    /// <summary>
    /// The tile's layer
    /// </summary>
    public ILayer Layer { get; }

    /// <summary>
    /// The tile's zoom level
    /// </summary>
    public int Zoom { get { return Layer.Properties.Zoom; } }

    /// <summary>
    /// The tile's X coordinate
    /// </summary>
    public int X { get; }

    /// <summary>
    /// The tile's Y coordinate
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// The tile ID
    /// </summary>
    public string Id { get { return $"{Zoom}/{X}/{Y}"; } }

    /// <summary>
    /// The tile's GameObject representation
    /// </summary>
    public GameObject GameObject { get; }

    /// <summary>
    /// Constructs a new map tile
    /// </summary>
    /// <param name="layer">The layer where the tile belongs</param>
    /// <param name="x">Tile's X coordinate</param>
    /// <param name="y">Tile's Y coordinate</param>
    public Tile(ILayer layer, int x, int y)
    {
        this.Layer = layer;
        this.X = x;
        this.Y = y;

        // Setup the gameobject
        GameObject = new GameObject($"{Id}");
        GameObject.transform.parent = Layer.GameObject.transform; // Set it as a child of the layer gameobject

        // Load and render the tile
        Load();
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    private async void Load()
    {
        try
        {
            string geoJsonText = await MainController.client.GetStringAsync($"https://tese.flamino.eu/api/tiles?layer={Layer.Id}&z={Zoom}&x={X}&y={Y}");
            try
            {
                // Parse the GeoJSON text
                IGeoJsonObject geoJson = GeoJson.Parse(geoJsonText);
                // Render the tile
                Render(geoJson);
            }
            catch (InvalidGeoJsonException e)
            {
                Logger.LogException(e);
            }
        }
        catch (HttpRequestException e)
        {
            Logger.LogException(e);
        }
        catch (TaskCanceledException e)
        {
            Logger.LogException(e);
        }
    }

    /// <summary>
    /// Render the tile
    /// </summary>
    /// <param name="geoJson">The tile's GeoJSON</param>
    /// <exception cref="InvalidGeoJsonException">Can't render as a tile, if root object isn't a FeatureCollection</exception>
    private void Render(IGeoJsonObject geoJson)
    {
        // Check if it's a FeatureCollection
        if (geoJson.GetType() == typeof(FeatureCollection))
        {
            // Render the GeoJSON
            ((FeatureCollection)geoJson).Render(GameObject, Layer.Properties);
        }
        else
        {
            // Can't render the tile. GeoJSON root isn't a FeatureCollection
            throw new InvalidGeoJsonException("Can't render the tile. GeoJSON root isn't a FeatureCollection");
        }
    }
}