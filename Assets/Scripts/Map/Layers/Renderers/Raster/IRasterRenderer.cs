using UnityEngine;

/// <summary>
/// Methods related to rendering a Raster layer
/// </summary>
public interface IRasterRenderer : ILayerRenderer
{
    /// <summary>
    /// Render the tile
    /// </summary>
    /// <param name="tileLayer">The raster tile layer</param>
    /// <param name="texture">The raster texture</param>
    void Render(RasterTileLayer tileLayer, Texture2D texture);
}