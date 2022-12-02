/// <summary>
/// Describes a feature's Category property
/// </summary>
public struct CategoryFeatureProperty : IFeatureProperty
{
    public string Key { get; }

    public string DisplayName { get; }

    public string FormatString { get; }

    public bool Filtered { get; set; }

    /// <summary>
    /// The value used for filtering
    /// </summary>
    public int FilterValue { get; set; }

    /// <summary>
    /// The comparison type used for filtering
    /// </summary>
    public FilterOperator Filter { get; set; }

    /// <summary>
    /// The list of categories for this property
    /// </summary>
    public string[] Categories { get; }

    /// <summary>
    /// Constructs a new CategoryFeatureProperty
    /// </summary>
    /// <param name="key">The property's key</param>
    /// <param name="displayName">The property's display name</param>
    /// <param name="formatString">The property's display format string</param>
    /// <param name="categories">The list of categories for this property</param>
    public CategoryFeatureProperty(string key, string displayName, string formatString, string[] categories)
    {
        this.Key = key;
        this.DisplayName = displayName;
        this.FormatString = formatString;
        this.Filtered = false;
        this.FilterValue = 0;
        this.Filter = FilterOperator.Equals;
        this.Categories = categories;
    }

    public bool MatchesFilter(Feature feature)
    {
        if (Categories.Length == 0)
        {
            // No categories set which makes it impossible to filter without an IndexOutOfRange exception, so all features match
            return true;
        }
        else
        {
            string value = feature.GetPropertyAsNullableString(Key);
            switch (Filter)
            {
                case FilterOperator.Equals:
                    return value == null ? false : value == Categories[(int)FilterValue];
                case FilterOperator.NotEquals:
                    return value == null ? false : value != Categories[(int)FilterValue];
                case FilterOperator.IsEmpty:
                    return value == null ? true : value == string.Empty;
                case FilterOperator.IsNotEmpty:
                    return value == null ? false : value != string.Empty;
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