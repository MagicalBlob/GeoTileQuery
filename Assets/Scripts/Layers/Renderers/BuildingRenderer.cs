using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Building GeoJSON renderer
/// </summary>
public class BuildingRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Default building height
    /// </summary>
    private double defaultHeight = 5;

    public void RenderNode(GeoJsonTile tile, Feature feature, Position coordinates)
    {
        Logger.LogWarning("[BuildingRenderer] Tried to render a Node!");
    }

    public void RenderEdge(GeoJsonTile tile, Feature feature, Position[] coordinates)
    {
        Logger.LogWarning("[BuildingRenderer] Tried to render an Edge!");
    }

    public void RenderArea(GeoJsonTile tile, Feature feature, Position[][] coordinates)
    {
        // Setup the gameobject
        GameObject area = new GameObject("Area"); // Create Area gameobject
        area.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        area.transform.localPosition = Vector3.zero; // Set origin
        area.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        // Check for empty coordinates array
        if (coordinates.Length == 0)
        {
            Logger.LogWarning($"{feature.GameObject.name}: Tried to render an Area with no coordinates");
            return;
        }

        // Triangulate the polygon coordinates for the roof using Earcut
        EarcutLib.Data data = EarcutLib.Flatten(coordinates, tile);
        List<int> triangles = EarcutLib.Earcut(data.Vertices, data.Holes, data.Dimensions);
        triangles.Reverse();

        // Setup the mesh components
        MeshRenderer meshRenderer = area.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = area.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Get building height
        double height = feature.GetPropertyAsDouble("EXTRUDE");
        if (height == 0)
        {
            height = defaultHeight;
        }

        List<Vector3> vertices = new List<Vector3>((data.Vertices.Count / data.Dimensions) + coordinates[0].Length);
        // Add roof vertices
        for (int i = 0; i < data.Vertices.Count / data.Dimensions; i++)
        {
            double x = data.Vertices[i * data.Dimensions];
            double y = data.Vertices[(i * data.Dimensions) + 2] + height; // GeoJSON uses z for height, while Unity uses y
            double z = data.Vertices[(i * data.Dimensions) + 1];   // GeoJSON uses z for height, while Unity uses y
            vertices.Add(new Vector3((float)x, (float)y, (float)z));
        }

        // Add wall vertices/triangles
        foreach (Position[] ring in coordinates)
        {
            for (int point = 0; point < ring.Length - 1; point++)
            {
                int vertOffset = vertices.Count;

                double x0 = ring[point].GetRelativeX(tile.Bounds.Min.X);
                double y0 = ring[point].GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
                double z0 = ring[point].GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

                double x1 = ring[point + 1].GetRelativeX(tile.Bounds.Min.X);
                double y1 = ring[point + 1].GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
                double z1 = ring[point + 1].GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

                vertices.Add(new Vector3((float)x0, (float)y0, (float)z0));
                vertices.Add(new Vector3((float)x1, (float)y1, (float)z1));
                vertices.Add(new Vector3((float)x1, (float)(y1 + height), (float)z1));
                vertices.Add(new Vector3((float)x0, (float)(y0 + height), (float)z0));

                triangles.Add(vertOffset + 0);
                triangles.Add(vertOffset + 1);
                triangles.Add(vertOffset + 2);
                triangles.Add(vertOffset + 0);
                triangles.Add(vertOffset + 2);
                triangles.Add(vertOffset + 3);
            }
        }

        // Setup vertices
        mesh.vertices = vertices.ToArray();

        // Setup triangles
        mesh.triangles = triangles.ToArray();

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        meshFilter.mesh = mesh;
    }
}
