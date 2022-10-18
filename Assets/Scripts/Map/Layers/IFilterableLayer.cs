/// <summary>
/// Represents a filterable map layer
/// </summary>
public interface IFilterableLayer : ILayer
{
    /// <summary>
    /// Whether the properties' filters are applied
    /// </summary>
    bool Filtered { get; set; }

    /// <summary>
    /// The layer's features' properties
    /// </summary>
    public IFeatureProperty[] FeatureProperties { get; }
}