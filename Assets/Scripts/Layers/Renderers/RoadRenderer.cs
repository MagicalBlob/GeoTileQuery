using UnityEngine;
using System;

/// <summary>
/// Road GeoJSON renderer
/// </summary>
public class RoadRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Edge width
    /// </summary>
    private double edgeWidth = 1; //0.025; //TODO check the rendering code but I think this might actually be rendering as half the width

    public void RenderNode(GeoJsonTile tile, Feature feature, Position coordinates)
    {
        Logger.LogWarning("[RoadRenderer] Tried to render a Node!");
    }

    public void RenderEdge(GeoJsonTile tile, Feature feature, Position[] coordinates)
    {
        // TODO an actual road renderer

        // Setup the gameobject
        GameObject edge = new GameObject("Edge - Road"); // Create Edge gameobject
        edge.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        edge.transform.localPosition = Vector3.zero; // Set origin
        edge.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        // Setup the mesh components
        MeshRenderer meshRenderer = edge.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = edge.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Setup vertices
        double tmpOffset = 0.05; // TODO remove this
        int numSegments = coordinates.Length - 1; // Number of segments in the line
        Vector3[] vertices = new Vector3[numSegments * 4]; // Needs 4 vertices per line segment
        for (int segment = 0; segment < numSegments; segment++)
        {
            // Start point of segment AB
            double ax = coordinates[segment].GetRelativeX(tile.Bounds.Min.X);
            double ay = coordinates[segment].GetRelativeZ() + tmpOffset; // GeoJSON uses z for height, while Unity uses y
            double az = coordinates[segment].GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

            // End point of segment AB
            double bx = coordinates[segment + 1].GetRelativeX(tile.Bounds.Min.X);
            double by = coordinates[segment + 1].GetRelativeZ() + tmpOffset; // GeoJSON uses z for height, while Unity uses y
            double bz = coordinates[segment + 1].GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

            // Calculate AB
            double abX = bx - ax;
            double abZ = bz - az;
            double abMagnitude = Math.Sqrt(Math.Pow(abX, 2) + Math.Pow(abZ, 2));

            // AB⟂ with given width
            double abPerpX = (edgeWidth * -abZ) / abMagnitude;
            double abPerpZ = (edgeWidth * abX) / abMagnitude;

            // Add vertices
            vertices[(segment * 4) + 0] = new Vector3((float)(ax - abPerpX), (float)ay, (float)(az - abPerpZ));
            vertices[(segment * 4) + 1] = new Vector3((float)(ax + abPerpX), (float)ay, (float)(az + abPerpZ));
            vertices[(segment * 4) + 2] = new Vector3((float)(bx - abPerpX), (float)by, (float)(bz - abPerpZ));
            vertices[(segment * 4) + 3] = new Vector3((float)(bx + abPerpX), (float)by, (float)(bz + abPerpZ));
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
        meshRenderer.sharedMaterial = new Material(Shader.Find("Mobile/Diffuse"));
        meshFilter.mesh = mesh;
    }

    public void RenderArea(GeoJsonTile tile, Feature feature, Position[][] coordinates)
    {
        Logger.LogWarning("[RoadRenderer] Tried to render an Area!");
    }
}
