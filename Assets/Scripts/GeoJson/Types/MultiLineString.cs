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
            else
            {
                if (line.Length == 2)
                {
                    // MultiLine Line has two positions, so check if they are the same
                    if (line[0].Equals(line[1]))
                    {
                        // MultiLine Line has two positions that are the same, so this is not a valid MultiLineString
                        throw new InvalidGeoJsonException("(Not part of GeoJSON spec) A MultiLineString Line should have at least two unique positions, but this Line only has one");
                    }
                }
            }
        }
        this.coordinates = coordinates;
    }

    public void Render(GeoJsonTileLayer tile, Feature feature)
    {
        foreach (Position[] line in coordinates)
        {
            ((IGeoJsonRenderer)tile.Layer.Renderer).RenderEdge(tile, feature, line);
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