using UnityEngine;
using System;
using System.Text;

/// <summary>
/// Represents a GeoJSON MultiLineString
/// </summary>
public class MultiLineString : IGeometryObject, IGeoJsonObject
{
    /// <summary>
    /// The coordinates for each point of each line
    /// </summary>
    public Position[][] coordinates;

    /// <summary>
    /// Constructs a new MultiLineString with given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates for each point of each line (every line MUST have two or more positions)</param>
    public MultiLineString(Position[][] coordinates)
    {
        foreach (Position[] line in coordinates)
        {
            if (line.Length < 2)
            {
                throw new InvalidGeoJsonException("Every MultiLine Line coordinates must have two or more positions");
            }
        }
        this.coordinates = coordinates;
    }

    /// <summary>
    /// Renders the MultiLineString as the geometry associated with the given Feature
    /// </summary>
    /// <param name="feature">The parent feature</param>
    /// <param name="properties">The layer rendering properties</param>
    public void Render(GameObject feature, RenderingProperties properties)
    {
        foreach (Position[] line in coordinates)
        {
            GeoJsonRenderer.RenderEdge(feature, line, properties);
        }
    }

    /// <summary>
    /// Returns a string representation of the MultiLineString
    /// </summary>
    /// <returns>String representation of the MultiLineString</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("MultiLineString: ");

        foreach (Position[] line in coordinates)
        {
            builder.Append("(");
            builder.Append(String.Join(", ", line));
            builder.Append("), ");
        }

        return builder.ToString();
    }
}