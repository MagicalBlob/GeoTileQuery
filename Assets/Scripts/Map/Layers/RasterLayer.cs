using System;

/// <summary>
/// Represents a Raster layer
/// </summary>
public class RasterLayer : ILayer
{
    public Map Map { get; }

    public string Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public string Source { get; }

    public DateTime LastUpdate { get; }

    private bool _visible;
    public bool Visible
    {
        get
        {
            return _visible;
        }
        set
        {
            // Set the backing field
            _visible = value;

            // Update the layer visibility for all the tiles already in the map
            foreach (Tile tile in Map.Tiles.Values)
            {
                if (tile.Layers.TryGetValue(Id, out ITileLayer tileLayer))
                {
                    tileLayer.GameObject.SetActive(Visible);
                }
            }
        }
    }

    public int MinZoom { get; }

    public int MaxZoom { get; }

    public ILayerRenderer Renderer { get; }

    public string Url { get; }

    /// <summary>
    /// Construct a new RasterLayer
    /// </summary>
    /// <param name="map">The map</param>
    /// <param name="id">The layer id</param>
    /// <param name="displayName">The layer display name</param>
    /// <param name="description">The layer description</param>
    /// <param name="source">The layer source</param>
    /// <param name="lastUpdate">The layer last update date</param>
    /// <param name="visible">Whether the layer is visible</param>
    /// <param name="minZoom">The layer minimum zoom level</param>
    /// <param name="maxZoom">The layer maximum zoom level</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="url">Url to fetch the tile data</param>
    public RasterLayer(Map map, string id, string displayName, string description, string source, DateTime lastUpdate, bool visible, int minZoom, int maxZoom, IRasterRenderer renderer, string url)
    {
        this.Map = map;
        this.Id = id;
        this.DisplayName = displayName;
        this.Description = description;
        this.Source = source;
        this.LastUpdate = lastUpdate;
        this.Visible = visible;
        this.MinZoom = minZoom;
        this.MaxZoom = maxZoom;
        this.Renderer = renderer;
        this.Url = url;
    }
}