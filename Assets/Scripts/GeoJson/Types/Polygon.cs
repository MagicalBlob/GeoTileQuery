using UnityEngine;
using System;
using System.Text;

/// <summary>
/// Represents a GeoJSON Polygon
/// </summary>
public class Polygon : IGeometryObject, IGeoJsonObject
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
                else
                {
                    // Ring is a closed loop
                    Position firstUniquePosition = ring[0];
                    Position? secondUniquePosition = null;
                    bool foundThreeUniquePositions = false;
                    for (int i = 1; i < ring.Length - 1; i++)
                    {
                        if (secondUniquePosition == null)
                        {
                            // We haven't found it yet, so check if this position is the second unique one
                            if (!ring[i].Equals(firstUniquePosition))
                            {
                                secondUniquePosition = ring[i];
                            }
                        }
                        else
                        {
                            // We've found the second unique position, check if this position is the third unique one
                            if (!ring[i].Equals(firstUniquePosition) && !ring[i].Equals(secondUniquePosition))
                            {
                                // We found at least three unique positions, so this is a valid polygon
                                foundThreeUniquePositions = true;
                                break;
                            }
                        }
                    }
                    if (!foundThreeUniquePositions)
                    {
                        // We didn't find at least three unique positions, so this is not a valid polygon
                        if (secondUniquePosition == null)
                        {
                            throw new InvalidGeoJsonException("(Not part of GeoJSON spec) A Polygon ring should have at least three unique positions, but this ring only has one");
                        }
                        else
                        {
                            throw new InvalidGeoJsonException("(Not part of GeoJSON spec) A Polygon ring should have at least three unique positions, but this ring only has two");
                        }
                    }
                }
            }
            else
            {
                // Ring does not have at least four positions
                throw new InvalidGeoJsonException($"Every Polygon ring coordinates must have four or more positions. Found {ring.Length} instead!");
            }
        }
        this.coordinates = coordinates;
    }

    public void Render(GeoJsonTileLayer tile, Feature feature)
    {
        ((IGeoJsonRenderer)tile.Layer.Renderer).RenderArea(tile, feature, coordinates);
    }

    /// <summary>
    /// Returns a string representation of the Polygon
    /// </summary>
    /// <returns>String representation of the Polygon</returns>
    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("Polygon: ");

        foreach (Position[] ring in coordinates)
        {
            builder.Append("(");
            builder.Append(String.Join(", ", ring));
            builder.Append("), ");
        }

        return builder.ToString();
    }
}