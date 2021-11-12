using UnityEngine;
using System;

/// <summary>
/// Sidewalk GeoJSON renderer
/// </summary>
public class SidewalkRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Sidewalk height offset
    /// </summary>
    private double sidewalkHeightOffset = 0.05;

    /// <summary>
    /// Sidewalk width
    /// </summary>
    private double sidewalkWidth = 1;

    public void RenderNode(GeoJsonTile tile, Feature feature, Position coordinates)
    {
        Logger.LogWarning("[SidewalkRenderer] Tried to render a Node!");
    }

    public void RenderEdge(GeoJsonTile tile, Feature feature, Position[] coordinates)
    {
        // Setup the gameobject
        GameObject edge = new GameObject("Edge - Sidewalk"); // Create Edge gameobject
        edge.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        edge.transform.localPosition = Vector3.zero; // Set origin
        edge.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        // Setup the mesh components
        MeshRenderer meshRenderer = edge.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = edge.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Setup vertices
        int numSegments = coordinates.Length - 1; // Number of segments in the line
        Vector3[] vertices = new Vector3[numSegments * 4]; // Needs 4 vertices per line segment
        for (int segment = 0; segment < numSegments; segment++)
        {
            // Start point of segment AB
            Vector2D a = new Vector2D(coordinates[segment].GetRelativeX(tile.Bounds.Min.X), coordinates[segment].GetRelativeY(tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
            double ay = coordinates[segment].GetRelativeZ() + sidewalkHeightOffset; // GeoJSON uses z for height, while Unity uses y

            // End point of segment AB
            Vector2D b = new Vector2D(coordinates[segment + 1].GetRelativeX(tile.Bounds.Min.X), coordinates[segment + 1].GetRelativeY(tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
            double by = coordinates[segment + 1].GetRelativeZ() + sidewalkHeightOffset; // GeoJSON uses z for height, while Unity uses y

            // Calculate AB and AB⟂ with given width
            Vector2D ab = b - a;
            Vector2D abPerp = Vector2D.Perpendicular(ab);
            abPerp.Normalize();
            abPerp *= (sidewalkWidth / 2);

            // Add vertices
            vertices[(segment * 4) + 0] = new Vector3((float)(a.X - abPerp.X), (float)ay, (float)(a.Y - abPerp.Y));
            vertices[(segment * 4) + 1] = new Vector3((float)(a.X + abPerp.X), (float)ay, (float)(a.Y + abPerp.Y));
            vertices[(segment * 4) + 2] = new Vector3((float)(b.X - abPerp.X), (float)by, (float)(b.Y - abPerp.Y));
            vertices[(segment * 4) + 3] = new Vector3((float)(b.X + abPerp.X), (float)by, (float)(b.Y + abPerp.Y));
        }
        mesh.vertices = vertices;

        // Setup triangles
        int[] triangles = new int[numSegments * 6]; // numSegments * 2 * 3
        for (int segment = 0; segment < numSegments; segment++)
        {
            triangles[(segment * 6) + 0] = (segment * 4) + 0;
            triangles[(segment * 6) + 1] = (segment * 4) + 1;
            triangles[(segment * 6) + 2] = (segment * 4) + 3;

            triangles[(segment * 6) + 3] = (segment * 4) + 0;
            triangles[(segment * 6) + 4] = (segment * 4) + 3;
            triangles[(segment * 6) + 5] = (segment * 4) + 2;
        }
        mesh.triangles = triangles;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Sidewalk"); // TODO use Addressables instead?
        meshFilter.mesh = mesh;
    }

    public void RenderArea(GeoJsonTile tile, Feature feature, Position[][] coordinates)
    {
        Logger.LogWarning("[SidewalkRenderer] Tried to render an Area!");
    }
}
