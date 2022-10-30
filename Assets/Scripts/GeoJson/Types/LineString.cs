using UnityEngine;
using System;

/// <summary>
/// Represents a GeoJSON LineString
/// </summary>
public class LineString : IGeometryObject, IGeoJsonObject
{
    /// <summary>
    /// The coordinates for each point in the line
    /// </summary>
    public Position[] coordinates;

    /// <summary>
    /// Constructs a new LineString with given coordinates
    /// </summary>
    /// <param name="coordinates">The coordinates for each point in the line (MUST have two or more positions)</param>
    public LineString(Position[] coordinates)
    {
        if (coordinates.Length >= 2)
        {
            Position firstUniquePosition = coordinates[0];
            bool secondUniquePositionFound = false;
            for (int i = 1; i < coordinates.Length; i++)
            {
                if (!secondUniquePositionFound && !coordinates[i].Equals(firstUniquePosition))
                {
                    secondUniquePositionFound = true;
                }
            }
            if (!secondUniquePositionFound)
            {
                // LineString does not have at least two unique positions, so this is not a valid LineString
                throw new InvalidGeoJsonException("(Not part of GeoJSON spec) A LineString should have at least two unique positions, but this LineString only has one");
            }
            this.coordinates = coordinates;
        }
        else
        {
            throw new InvalidGeoJsonException("Line coordinates must have two or more positions");
        }
    }

    public void Render(GeoJsonTileLayer tile, Feature feature)
    {
        ((IGeoJsonRenderer)tile.Layer.Renderer).RenderEdge(tile, feature, coordinates);
    }

    /// <summary>
    /// Returns a string representation of the LineString
    /// </summary>
    /// <returns>String representation of the LineString</returns>
    public override string ToString()
    {
        return $"LineString: ({String.Join(", ", coordinates)})";
    }
}