/// <summary>
/// Describes a feature's String property
/// </summary>
public struct StringFeatureProperty : IFeatureProperty
{
    public string Key { get; }

    public string DisplayName { get; }

    public string FormatString { get; }

    public bool Filtered { get; set; }

    /// <summary>
    /// The value used for filtering
    /// </summary>
    public string FilterValue { get; set; }

    /// <summary>
    /// The comparison type used for filtering
    /// </summary>
    public FilterOperator Filter { get; set; }

    /// <summary>
    /// Constructs a new StringFeatureProperty
    /// </summary>
    /// <param name="key">The property's key</param>
    /// <param name="displayName">The property's display name</param>
    /// <param name="formatString">The property's display format string</param>
    public StringFeatureProperty(string key, string displayName, string formatString)
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
            string value = feature.GetPropertyAsNullableString(Key);
            switch (Filter)
            {
                case FilterOperator.Equals:
                    return value == null ? false : value.ToLower() == FilterValue.ToLower();
                case FilterOperator.NotEquals:
                    return value == null ? false : value.ToLower() != FilterValue.ToLower();
                case FilterOperator.Contains:
                    return value == null ? false : value.ToLower().Contains(FilterValue.ToLower());
                case FilterOperator.DoesNotContain:
                    return value == null ? false : !value.ToLower().Contains(FilterValue.ToLower());
                case FilterOperator.StartsWith:
                    return value == null ? false : value.ToLower().StartsWith(FilterValue.ToLower());
                case FilterOperator.EndsWith:
                    return value == null ? false : value.ToLower().EndsWith(FilterValue.ToLower());
                case FilterOperator.IsEmpty:
                    return value == null ? true : value.ToLower() == string.Empty;
                case FilterOperator.IsNotEmpty:
                    return value == null ? false : value.ToLower() != string.Empty;
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
        /// Whether the feature's property value contains the filter value
        /// </summary>
        Contains,
        /// <summary>
        /// Whether the feature's property value does not contain the filter value
        /// </summary>
        DoesNotContain,
        /// <summary>
        /// Whether the feature's property value starts with the filter value
        /// </summary>
        StartsWith,
        /// <summary>
        /// Whether the feature's property value ends with the filter value
        /// </summary>
        EndsWith,
        /// <summary>
        /// Whether the feature's property value is empty
        /// </summary>
        IsEmpty,
        /// <summary>
        /// Whether the feature's property value is not empty
        /// </summary>
        IsNotEmpty,
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