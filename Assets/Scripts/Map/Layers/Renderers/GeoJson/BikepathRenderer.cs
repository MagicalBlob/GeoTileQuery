using UnityEngine;
using System;

/// <summary>
/// Bikepath GeoJSON renderer
/// </summary>
public class BikepathRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Bikepath height offset
    /// </summary>
    private double bikepathHeightOffset = 0.06;

    /// <summary>
    /// Bikepath width
    /// </summary>
    private double bikepathWidth = 2;

    /// <summary>
    /// The bikepath material
    /// </summary>
    private Material bikepathMaterial;

    /// <summary>
    /// Creates a new BikepathRenderer
    /// </summary>
    public BikepathRenderer()
    {
        this.bikepathMaterial = Resources.Load<Material>("Materials/Bikepath"); // TODO use Addressables instead?
    }

    public void RenderNode(GeoJsonTileLayer tileLayer, Feature feature, Position coordinates)
    {
        Debug.LogWarning($"[BikepathRenderer] {tileLayer.FullId}: Tried to render a Node!");
    }

    public void RenderEdge(GeoJsonTileLayer tileLayer, Feature feature, Position[] coordinates)
    {
        // Setup the gameobject
        GameObject edge = new GameObject("Edge - Bikepath"); // Create Edge gameobject
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
            Vector2D a = new Vector2D(coordinates[segment].GetRelativeX(tileLayer.Tile.Bounds.Min.X), coordinates[segment].GetRelativeY(tileLayer.Tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
            double ayTerrainHeightOffset = 0;
            if (tileLayer.Tile.Map.ElevatedTerrain)
            {
                // If we're using the elevation data, get the height at the position to offset it (assumes position's at sea level in dataset)
                ayTerrainHeightOffset = tileLayer.Tile.GetHeight(GlobalMercator.LatLonToMeters(coordinates[segment].y, coordinates[segment].x));
            }
            double ay = coordinates[segment].GetRelativeZ() + ayTerrainHeightOffset + bikepathHeightOffset; // GeoJSON uses z for height, while Unity uses y

            // End point of segment AB
            Vector2D b = new Vector2D(coordinates[segment + 1].GetRelativeX(tileLayer.Tile.Bounds.Min.X), coordinates[segment + 1].GetRelativeY(tileLayer.Tile.Bounds.Min.Y)); // GeoJSON uses z for height, while Unity uses y
            double byTerrainHeightOffset = 0;
            if (tileLayer.Tile.Map.ElevatedTerrain)
            {
                // If we're using the elevation data, get the height at the position to offset it (assumes position's at sea level in dataset)
                byTerrainHeightOffset = tileLayer.Tile.GetHeight(GlobalMercator.LatLonToMeters(coordinates[segment + 1].y, coordinates[segment + 1].x));
            }
            double by = coordinates[segment + 1].GetRelativeZ() + byTerrainHeightOffset + bikepathHeightOffset; // GeoJSON uses z for height, while Unity uses y

            // Calculate AB and AB⟂ with given width
            Vector2D ab = b - a;
            Vector2D abPerp = Vector2D.Perpendicular(ab);
            abPerp.Normalize();
            abPerp *= (bikepathWidth / 2);

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
        meshRenderer.sharedMaterial = bikepathMaterial;
        meshFilter.mesh = mesh;
        // Only add a mesh collider if the current zoom level is higher than 16 (Level of Detail)
        if (tileLayer.Tile.Zoom > 16)
        {
            MeshCollider meshCollider = edge.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }
    }

    public void RenderArea(GeoJsonTileLayer tileLayer, Feature feature, Position[][] coordinates)
    {
        Debug.LogWarning($"[BikepathRenderer] {tileLayer.FullId}: Tried to render an Area!");
    }
}
