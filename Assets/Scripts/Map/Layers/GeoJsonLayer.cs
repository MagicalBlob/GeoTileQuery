using System;

/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : IFilterableLayer
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

    public bool Filtered { get; set; }

    public IFeatureProperty[] FeatureProperties { get; }

    /// <summary>
    /// Name of the Feature's property that may be used as an id as an alternative to the actual Feature id if it doesn't exist
    /// </summary>
    public string IdPropertyName { get; }

    /// <summary>
    /// Construct a new GeoJSONLayer
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
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    /// <param name="featureProperties">The GeoJSON layer's features' properties</param>
    public GeoJsonLayer(Map map, string id, string displayName, string description, string source, DateTime lastUpdate, bool visible, int minZoom, int maxZoom, IGeoJsonRenderer renderer, string url, string idPropertyName, IFeatureProperty[] featureProperties)
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
        this.Filtered = false;
        this.FeatureProperties = featureProperties;
        this.IdPropertyName = idPropertyName;
    }
}