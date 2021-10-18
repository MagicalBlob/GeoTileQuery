using UnityEngine;

/// <summary>
/// Methods related to rendering a Terrain layer
/// </summary>
public interface ITerrainRenderer : ILayerRenderer
{
    /// <summary>
    /// Render the terrain
    /// </summary>
    /// <param name="tile">The terrain's tile</param>
    /// <param name="texture">The raster texture</param>
    void RenderTerrain(TerrainTile tile, Texture2D texture);
}