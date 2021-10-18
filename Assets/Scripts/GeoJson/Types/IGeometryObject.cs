/// <summary>
/// Represents a GeoJSON Geometry Object
/// </summary>
public interface IGeometryObject : IGeoJsonObject
{
    /// <summary>
    /// Renders the Geometry Object as the geometry associated with the given Feature
    /// </summary>
    /// <param name="tile">The feature's tile</param>
    /// <param name="feature">The feature to which the geometry object belongs</param>
    void Render(GeoJsonTile tile, Feature feature);
}