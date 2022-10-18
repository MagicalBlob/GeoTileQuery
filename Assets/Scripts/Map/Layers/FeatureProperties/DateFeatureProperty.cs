using System;

/// <summary>
/// Describes a feature's Date property
/// </summary>
public struct DateFeatureProperty : IFeatureProperty
{
    public string Key { get; }

    public string DisplayName { get; }

    public string FormatString { get; }

    public bool Filtered { get; set; }

    /// <summary>
    /// The value used for filtering
    /// </summary>
    public DateTime? FilterValue { get; set; }

    /// <summary>
    /// The comparison type used for filtering
    /// </summary>
    public FilterOperator Filter { get; set; }

    /// <summary>
    /// Constructs a new DateFeatureProperty
    /// </summary>
    /// <param name="key">The property's key</param>
    /// <param name="displayName">The property's display name</param>
    /// <param name="formatString">The property's display format string</param>
    public DateFeatureProperty(string key, string displayName, string formatString)
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
            DateTime? value = feature.GetPropertyAsNullableDateTime(Key);
            switch (Filter)
            {
                case FilterOperator.Equals:
                    return value == null ? false : value.Value == FilterValue;
                case FilterOperator.NotEquals:
                    return value == null ? false : value.Value != FilterValue;
                case FilterOperator.LessThan:
                    return value == null ? false : value.Value < FilterValue;
                case FilterOperator.GreaterThan:
                    return value == null ? false : value.Value > FilterValue;
                case FilterOperator.LessOrEquals:
                    return value == null ? false : value.Value <= FilterValue;
                case FilterOperator.GreaterOrEquals:
                    return value == null ? false : value.Value >= FilterValue;
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
        /// Whether the feature's property value is less than the filter value
        /// </summary>
        LessThan,
        /// <summary>
        /// Whether the feature's property value is greater than the filter value
        /// </summary>
        GreaterThan,
        /// <summary>
        /// Whether the feature's property value is less than or equal to the filter value
        /// </summary>
        LessOrEquals,
        /// <summary>
        /// Whether the feature's property value is greater than or equal to the filter value
        /// </summary>
        GreaterOrEquals,
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