using System;
using System.Text;

/// <summary>
/// Represents a GeoJSON Polygon
/// </summary>
public class Polygon : IGeoJsonObject
{
    /// <summary>
    /// The coordinates for each point of each linear ring
    /// </summary>
    public Position[][] coordinates;

    /// <summary>
    /// Constructs a new Polygon with given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates for each point of each linear ring (every ring MUST have four or more positions)</param>
    public Polygon(Position[][] coordinates)
    {
        foreach (Position[] ring in coordinates)
        {
            if (ring.Length >= 4)
            {
                // Ring has four or more positions
                if (!ring[0].Equals(ring[ring.Length - 1]))
                {
                    // Ring is not a closed loop
                    throw new InvalidGeoJsonException("The first and last positions of a Polygon ring must be equivalent");
                }
            }
            else
            {
                // Ring does not have at least four positions
                throw new InvalidGeoJsonException("Every Polygon ring coordinates must have four or more positions");
            }
        }
        this.coordinates = coordinates;
    }

    /// <summary>
    /// Returns a string representation of the Polygon
    /// </summary>
    /// <returns>String representation of the Polygon</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("Polygon:");

        foreach (Position[] ring in coordinates)
        {
            builder.Append("\n> ");
            builder.Append(String.Join(", ", ring));
        }

        return builder.ToString();
    }
}