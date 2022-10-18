/// <summary>
/// Represents a tile's filterable layer
/// </summary>
public interface IFilterableTileLayer : ITileLayer
{
    /// <summary>
    /// Applies the layer's filters to the tile
    /// </summary>
    void ApplyFilters();
}