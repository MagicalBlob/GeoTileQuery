using UnityEngine;
using System;
using System.Text;

/// <summary>
/// Represents a GeoJSON MultiPolygon
/// </summary>
public class MultiPolygon : IGeometryObject, IGeoJsonObject
{
    /// <summary>
    /// The coordinates for each point of each linear ring of each polygon
    /// </summary>
    public Position[][][] coordinates;

    /// <summary>
    /// Constructs a new MultiPolygon with given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates for each point of each linear ring of each polygon (every ring MUST have four or more positions)</param>
    public MultiPolygon(Position[][][] coordinates)
    {
        foreach (Position[][] polygon in coordinates)
        {
            foreach (Position[] ring in polygon)
            {
                if (ring.Length >= 4)
                {
                    // Ring has four or more positions
                    if (!ring[0].Equals(ring[ring.Length - 1]))
                    {
                        // Ring is not a closed loop
                        throw new InvalidGeoJsonException("The first and last positions of a MultiPolygon's polygon ring must be equivalent");
                    }
                }
                else
                {
                    // Ring does not have at least four positions
                    throw new InvalidGeoJsonException("Every MultiPolygon's polygon ring coordinates must have four or more positions");
                }
            }
        }
        this.coordinates = coordinates;
    }

    /// <summary>
    /// Renders the MultiPolygon as the geometry associated with the given Feature
    /// </summary>
    /// <param name="feature">The parent feature</param>
    public void Render(GameObject feature)
    {
        foreach (Position[][] polygon in coordinates)
        {
            GeoJsonRenderer.RenderArea(feature, polygon);
        }
    }

    /// <summary>
    /// Returns a string representation of the MultiPolygon
    /// </summary>
    /// <returns>String representation of the MultiPolygon</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("MultiPolygon: ");

        foreach (Position[][] polygon in coordinates)
        {
            builder.Append("(");

            foreach (Position[] ring in polygon)
            {
                builder.Append("(");
                builder.Append(String.Join(", ", ring));
                builder.Append("), ");
            }

            builder.Append("), ");
        }

        return builder.ToString();
    }
}