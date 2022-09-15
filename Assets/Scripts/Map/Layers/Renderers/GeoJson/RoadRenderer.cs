using UnityEngine;
using System;

/// <summary>
/// Road GeoJSON renderer
/// </summary>
public class RoadRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Road height offset
    /// </summary>
    private double roadHeightOffset = 0.05;

    /// <summary>
    /// Road width
    /// </summary>
    private double roadWidth = 5;

    public void RenderNode(GeoJsonTileLayer tileLayer, Feature feature, Position coordinates)
    {
        Debug.LogWarning($"[RoadRenderer] {tileLayer.FullId}: Tried to render a Node!");
    }

    public void RenderEdge(GeoJsonTileLayer tileLayer, Feature feature, Position[] coordinates)
    {
        // Check for empty coordinates array
        if (coordinates.Length == 0)
        {
            Debug.LogWarning($"[RoadRenderer] {tileLayer.FullId}/{feature.GameObject.name}: Tried to render an Edge with no coordinates");
            return;
        }

        // Setup the gameobject
        GameObject edge = new GameObject("Edge - Road"); // Create Edge gameobject
        edge.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        edge.transform.localPosition = Vector3.zero; // Set origin
        edge.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        // Setup the mesh components
        MeshRenderer meshRenderer = edge.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = edge.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Setup vertices and triangles
        int numSegments = coordinates.Length - 1; // Number of segments in the line
        Vector3[] vertices = new Vector3[numSegments * 5]; // Needs 5 vertices per line segment
        int[] triangles = new int[numSegments * 9]; // numSegments * 3 * 3
        for (int segment = 0; segment < numSegments; segment++)
        {
            // Point A
            Vector2D a = new Vector2D(coordinates[segment].GetRelativeX(tileLayer.Tile.Bounds.Min.X), coordinates[segment].GetRelativeY(tileLayer.Tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
            double ay = coordinates[segment].GetRelativeZ() + roadHeightOffset; // GeoJSON uses z for height, while Unity uses y

            // Point B
            Vector2D b = new Vector2D(coordinates[segment + 1].GetRelativeX(tileLayer.Tile.Bounds.Min.X), coordinates[segment + 1].GetRelativeY(tileLayer.Tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
            double by = coordinates[segment + 1].GetRelativeZ() + roadHeightOffset; // GeoJSON uses z for height, while Unity uses y

            // Calculate AB
            Vector2D ab = b - a;

            // Normalize AB
            Vector2D abNorm = ab.Normalized;

            // Calculate AB⟂ with given width
            Vector2D abPerp = Vector2D.Perpendicular(abNorm);
            abPerp *= (roadWidth / 2);

            // Add vertices
            vertices[(segment * 5) + 0] = new Vector3((float)(a.X - abPerp.X), (float)ay, (float)(a.Y - abPerp.Y));
            vertices[(segment * 5) + 1] = new Vector3((float)(a.X + abPerp.X), (float)ay, (float)(a.Y + abPerp.Y));
            vertices[(segment * 5) + 2] = new Vector3((float)(b.X - abPerp.X), (float)by, (float)(b.Y - abPerp.Y));
            vertices[(segment * 5) + 3] = new Vector3((float)(b.X + abPerp.X), (float)by, (float)(b.Y + abPerp.Y));

            // Add triangles
            triangles[(segment * 9) + 0] = (segment * 5) + 0; // tri 01
            triangles[(segment * 9) + 1] = (segment * 5) + 1; // tri 01
            triangles[(segment * 9) + 2] = (segment * 5) + 3; // tri 01
            triangles[(segment * 9) + 3] = (segment * 5) + 0; // tri 02
            triangles[(segment * 9) + 4] = (segment * 5) + 3; // tri 02
            triangles[(segment * 9) + 5] = (segment * 5) + 2; // tri 02

            // Bevel joins
            if (segment + 1 < numSegments)
            {
                // Point C
                Vector2D c = new Vector2D(coordinates[segment + 2].GetRelativeX(tileLayer.Tile.Bounds.Min.X), coordinates[segment + 2].GetRelativeY(tileLayer.Tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
                double cy = coordinates[segment + 2].GetRelativeZ() + roadHeightOffset; // GeoJSON uses z for height, while Unity uses y

                // Calculate BC
                Vector2D bc = c - b;

                // Normalize BC
                Vector2D bcNorm = bc.Normalized;

                // Calculate tangent
                Vector2D tangent = bcNorm + abNorm;
                tangent.Normalize();

                // Calculate normal
                Vector2D normal = Vector2D.Perpendicular(tangent);

                // Calculate CB
                Vector2D cb = b - c;

                // Calculate the direction of the bend
                int direction = Math.Sign(Vector2D.Dot(ab + cb, normal));

                // Add vertices and triangles
                vertices[(segment * 5) + 4] = new Vector3((float)b.X, (float)by, (float)b.Y);
                if (direction < 0)
                {
                    triangles[(segment * 9) + 6] = (segment * 5) + 4; // tri 03
                    triangles[(segment * 9) + 7] = ((segment + 1) * 5) + 0; // tri 03
                    triangles[(segment * 9) + 8] = (segment * 5) + 2; // tri 03
                }
                else
                {
                    triangles[(segment * 9) + 6] = ((segment + 1) * 5) + 1; // tri 03
                    triangles[(segment * 9) + 7] = (segment * 5) + 4; // tri 03
                    triangles[(segment * 9) + 8] = (segment * 5) + 3; // tri 03
                }
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Road"); // TODO use Addressables instead?
        meshFilter.mesh = mesh;
    }

    public void RenderArea(GeoJsonTileLayer tileLayer, Feature feature, Position[][] coordinates)
    {
        Debug.LogWarning($"[RoadRenderer] {tileLayer.FullId}: Tried to render an Area!");
    }
}
