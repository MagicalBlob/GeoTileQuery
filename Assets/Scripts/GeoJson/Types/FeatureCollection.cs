using UnityEngine;
using System.Text;

/// <summary>
/// Represents a GeoJSON FeatureCollection
/// </summary>
public class FeatureCollection : IGeoJsonObject
{
    /// <summary>
    /// The features in the collection
    /// </summary>
    private Feature[] features;

    /// <summary>
    /// Constructs a new FeatureCollection with the given features
    /// </summary>
    /// <param name="features">The features in the collection</param>
    public FeatureCollection(Feature[] features)
    {
        this.features = features;
    }

    /// <summary>
    /// Render the FeatureCollection
    /// </summary>
    /// <param name="tile">The FeatureCollection's tile</param>
    /// <param name="renderingProperties">The layer rendering properties</param>
    public void Render(GameObject tile, RenderingProperties renderingProperties)
    {
        foreach (Feature feature in features)
        {
            feature.Render(tile, renderingProperties);
        }
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