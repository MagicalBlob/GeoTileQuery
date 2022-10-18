using System;

/// <summary>
/// Represents a Raster layer
/// </summary>
public class RasterLayer : ILayer
{
    public string Id { get; }

    public string DisplayName { get; }

    public string Description { get; }

    public string Source { get; }

    public DateTime LastUpdate { get; }

    public bool Visible { get; set; }

    public ILayerRenderer Renderer { get; }

    public string Url { get; }

    /// <summary>
    /// Construct a new RasterLayer
    /// </summary>
    /// <param name="id">The layer id</param>
    /// <param name="displayName">The layer display name</param>
    /// <param name="description">The layer description</param>
    /// <param name="source">The layer source</param>
    /// <param name="lastUpdate">The layer last update date</param>
    /// <param name="visible">Whether the layer is visible</param>
    /// <param name="renderer">The layer's renderer</param>
    /// <param name="url">Url to fetch the tile data</param>
    public RasterLayer(string id, string displayName, string description, string source, DateTime lastUpdate, bool visible, IRasterRenderer renderer, string url)
    {
        this.Id = id;
        this.DisplayName = displayName;
        this.Description = description;
        this.Source = source;
        this.LastUpdate = lastUpdate;
        this.Visible = visible;
        this.Renderer = renderer;
        this.Url = url;
    }
}