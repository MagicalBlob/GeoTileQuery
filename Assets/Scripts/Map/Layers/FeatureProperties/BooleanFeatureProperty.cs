/// <summary>
/// Describes a feature's Boolean property
/// </summary>
public struct BooleanFeatureProperty : IFeatureProperty
{
    public string Key { get; }

    public string DisplayName { get; }

    public string FormatString { get; }

    public bool Filtered { get; set; }

    /// <summary>
    /// The value used for filtering
    /// </summary>
    public bool? FilterValue { get; set; }

    /// <summary>
    /// The comparison type used for filtering
    /// </summary>
    public FilterOperator Filter { get; set; }

    /// <summary>
    /// Constructs a new BooleanFeatureProperty
    /// </summary>
    /// <param name="key">The property's key</param>
    /// <param name="displayName">The property's display name</param>
    /// <param name="formatString">The property's display format string</param>
    public BooleanFeatureProperty(string key, string displayName, string formatString)
    {
        this.Key = key;
        this.DisplayName = displayName;
        this.FormatString = formatString;
        this.Filtered = false;
        this.FilterValue = null;
        this.Filter = FilterOperator.Equals;
    }

    public bool MatchesFilter(Feature feature)
    {
        if (FilterValue == null)
        {
            // No filter value set, so all features match
            return true;
        }
        else
        {
            bool? value = feature.GetPropertyAsNullableBool(Key);
            switch (Filter)
            {
                case FilterOperator.Equals:
                    return value == null ? false : value == FilterValue;
                case FilterOperator.NotEquals:
                    return value == null ? false : value != FilterValue;
                case FilterOperator.IsNull:
                    return value == null;
                case FilterOperator.IsNotNull:
                    return value != null;
                default:
                    return true; // No filter set, so all features match
            }
        }
    }

    /// <summary>
    /// The operator to use for filtering
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>
        /// Whether the feature's property value is equal to the filter value
        /// </summary>
        Equals,
        /// <summary>
        /// Whether the feature's property value is not equal to the filter value
        /// </summary>
        NotEquals,
        /// <summary>
        /// Whether the feature's property value starts is null
        /// </summary>
        IsNull,
        /// <summary>
        /// Whether the feature's property value is not null
        /// </summary>
        IsNotNull
    }
}