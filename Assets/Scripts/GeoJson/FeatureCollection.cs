using System.Text;

/// <summary>
/// Represents a GeoJSON FeatureCollection
/// </summary>
public class FeatureCollection : IGeoJsonObject
{
    /// <summary>
    /// The features in the collection
    /// </summary>
    Feature[] features;

    /// <summary>
    /// Constructs a new FeatureCollection with the given features
    /// </summary>
    /// <param name="features">The features in the collection</param>
    public FeatureCollection(Feature[] features)
    {
        this.features = features;
    }

    /// <summary>
    /// Returns a string representation of the FeatureCollection
    /// </summary>
    /// <returns>A string representation of the FeatureCollection</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("FeatureCollection: ");

        foreach (Feature feature in features)
        {
            builder.Append($"\n\t> {feature}");
        }

        return builder.ToString();
    }
}