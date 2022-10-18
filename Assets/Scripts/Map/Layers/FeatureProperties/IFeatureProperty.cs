/// <summary>
/// Describes a feature's property
/// </summary>
public interface IFeatureProperty
{
    /// <summary>
    /// The property's key
    /// </summary>
    string Key { get; }

    /// <summary>
    /// The property's display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The property's display format string
    /// </summary>
    string FormatString { get; }

    /// <summary>
    /// Whether the property's filter is applied
    /// </summary>
    bool Filtered { get; set; }

    /// <summary>
    /// Checks whether the given feature's property matches the filter
    /// </summary>
    /// <param name="feature">The feature to check</param>
    /// <returns>true if the feature's property matches the filter, false otherwise</returns>
    bool MatchesFilter(Feature feature);
}