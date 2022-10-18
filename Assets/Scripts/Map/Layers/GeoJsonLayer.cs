using System;

/// <summary>
/// Represents a GeoJSON layer
/// </summary>
public class GeoJsonLayer : IFilterableLayer
{
    public string Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public string Source { get; }

    public DateTime LastUpdate { get; }

    public bool Visible { get; set; }

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
    /// <param name="id">The layer id</param>
    /// <param name="displayName">The layer display name</param>
    /// <param name="description">The layer description</param>
    /// <param name="source">The layer source</param>
    /// <param name="lastUpdate">The layer last update date</param>
    /// <param name="visible">Whether the layer is visible</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="url">Url to fetch the tile data</param>
    /// <param name="idPropertyName">Name of the Feature's property that may be used as an Id as an alternative to the actual Feature Id if it doesn't exist</param>
    /// <param name="featureProperties">The GeoJSON layer's features' properties</param>
    public GeoJsonLayer(string id, string displayName, string description, string source, DateTime lastUpdate, bool visible, IGeoJsonRenderer renderer, string url, string idPropertyName, IFeatureProperty[] featureProperties)
    {
        this.Id = id;
        this.DisplayName = displayName;
        this.Description = description;
        this.Source = source;
        this.LastUpdate = lastUpdate;
        this.Visible = visible;
        this.Renderer = renderer;
        this.Url = url;
        this.Filtered = false;
        this.FeatureProperties = featureProperties;
        this.IdPropertyName = idPropertyName;
    }
}