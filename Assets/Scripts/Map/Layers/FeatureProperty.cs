/// <summary>
/// Describes a feature's property
/// </summary>
public struct FeatureProperty
{

    /// <summary>
    /// The property's key
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The property's type
    /// </summary>
    public FeaturePropertyType Type { get; }

    /// <summary>
    /// The property's display name
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// The property's display format string
    /// </summary>
    public string FormatString { get; }

    /// <summary>
    /// Constructs a new FeatureProperty
    /// </summary>
    /// <param name="key">The property's key</param>
    /// <param name="type">The property's type</param>
    /// <param name="displayName">The property's display name</param>
    /// <param name="formatString">The property's display format string</param>
    public FeatureProperty(string key, FeaturePropertyType type, string displayName, string formatString)
    {
        this.Key = key;
        this.Type = type;
        this.DisplayName = displayName;
        this.FormatString = formatString;
    }
}

/// <summary>
/// Possible types of a feature's property
/// </summary>
public enum FeaturePropertyType
{
    String,
    Double,
    Int,
    Bool,
    Date
}