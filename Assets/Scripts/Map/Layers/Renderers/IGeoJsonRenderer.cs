/// <summary>
/// Methods related to rendering a GeoJSON layer
/// </summary>
public interface IGeoJsonRenderer : ILayerRenderer
{
    /// <summary>
    /// Render a node with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="tileLayer">The feature's tile layer</param>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The node coordinates</param>
    void RenderNode(GeoJsonTileLayer tileLayer, Feature feature, Position coordinates);

    /// <summary>
    /// Render an edge with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="tileLayer">The feature's tile layer</param>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The edge coordinates</param>
    void RenderEdge(GeoJsonTileLayer tileLayer, Feature feature, Position[] coordinates);

    /// <summary>
    /// Render an area with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="tileLayer">The feature's tile layer</param>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The area coordinates</param>
    void RenderArea(GeoJsonTileLayer tileLayer, Feature feature, Position[][] coordinates);
}