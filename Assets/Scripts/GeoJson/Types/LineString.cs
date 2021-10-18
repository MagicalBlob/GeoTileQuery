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
            this.coordinates = coordinates;
        }
        else
        {
            throw new InvalidGeoJsonException("Line coordinates must have two or more positions");
        }
    }

    public void Render(GeoJsonTile tile, Feature feature)
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