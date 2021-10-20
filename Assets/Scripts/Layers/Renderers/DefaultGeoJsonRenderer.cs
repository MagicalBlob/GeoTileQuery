using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Default GeoJSON renderer
/// </summary>
public class DefaultGeoJsonRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Node height
    /// </summary>
    private double nodeHeight = 5;

    /// <summary>
    /// Node radius
    /// </summary>
    private double nodeRadius = 1;

    /// <summary>
    /// Edge width
    /// </summary>
    private double edgeWidth = 1; //0.025; //TODO check the rendering code but I think this might actually be rendering as half the width

    public void RenderNode(GeoJsonTile tile, Feature feature, Position coordinates)
    {
        // Setup the gameobject
        GameObject node = new GameObject("Node"); // Create Node gameobject
        node.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        node.transform.localPosition = Vector3.zero; // Set origin
        node.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        // Setup the mesh components
        MeshRenderer meshRenderer = node.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = node.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Setup vertices
        double x = coordinates.GetRelativeX(tile.Bounds.Min.X);
        double y = coordinates.GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
        double z = coordinates.GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y
        Vector3[] vertices = new Vector3[16]
        {
            new Vector3((float)x, (float)y, (float)z),
            new Vector3((float)(x + nodeRadius), (float)(y + nodeHeight), (float)(z + nodeRadius)),
            new Vector3((float)(x - nodeRadius), (float)(y + nodeHeight), (float)(z + nodeRadius)),
            new Vector3((float)x, (float)y, (float)z),
            new Vector3((float)(x - nodeRadius), (float)(y + nodeHeight), (float)(z + nodeRadius)),
            new Vector3((float)(x - nodeRadius), (float)(y + nodeHeight), (float)(z - nodeRadius)),
            new Vector3((float)x, (float)y, (float)z),
            new Vector3((float)(x - nodeRadius), (float)(y + nodeHeight), (float)(z - nodeRadius)),
            new Vector3((float)(x + nodeRadius), (float)(y + nodeHeight), (float)(z - nodeRadius)),
            new Vector3((float)x, (float)y, (float)z),
            new Vector3((float)(x + nodeRadius), (float)(y + nodeHeight), (float)(z - nodeRadius)),
            new Vector3((float)(x + nodeRadius), (float)(y + nodeHeight), (float)(z + nodeRadius)),
            new Vector3((float)(x + nodeRadius), (float)(y + nodeHeight), (float)(z + nodeRadius)),
            new Vector3((float)(x - nodeRadius), (float)(y + nodeHeight), (float)(z + nodeRadius)),
            new Vector3((float)(x - nodeRadius), (float)(y + nodeHeight), (float)(z - nodeRadius)),
            new Vector3((float)(x + nodeRadius), (float)(y + nodeHeight), (float)(z - nodeRadius))
        };
        mesh.vertices = vertices;

        // Setup triangles
        int[] triangles = new int[18] // 6 * 3
        {
            0,1,2,
            3,4,5,
            6,7,8,
            9,10,11,
            12,15,14,
            12,14,13
        };
        mesh.triangles = triangles;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Mobile/Diffuse"));
        meshFilter.mesh = mesh;
    }

    public void RenderEdge(GeoJsonTile tile, Feature feature, Position[] coordinates)
    {
        // Setup the gameobject
        GameObject edge = new GameObject("Edge"); // Create Edge gameobject
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
        // Check for empty coordinates array
        if (coordinates.Length == 0)
        {
            Logger.LogWarning($"{feature.GameObject.name}: Tried to render an Area with no coordinates");
            return;
        }

        // Setup the gameobject
        GameObject area = new GameObject("Area"); // Create Area gameobject
        area.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        area.transform.localPosition = Vector3.zero; // Set origin
        area.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        // Triangulate the polygon coordinates using Earcut
        EarcutLib.Data data = EarcutLib.Flatten(coordinates, tile);
        List<int> triangles = EarcutLib.Earcut(data.Vertices, data.Holes, data.Dimensions);
        /*double deviation = EarcutLib.Deviation(data.Vertices, data.Holes, data.Dimensions, triangles);
        Logger.Log(deviation == 0 ? "The triangulation is fully correct" : $"Triangulation deviation: {Math.Round(deviation, 6)}"); TODO clear this */

        // Setup the mesh components
        MeshRenderer meshRenderer = area.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = area.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Setup vertices
        double tmpOffset = 0.05; // TODO remove this
        Vector3[] vertices = new Vector3[data.Vertices.Count / data.Dimensions];
        for (int i = 0; i < data.Vertices.Count / data.Dimensions; i++)
        {
            double x = data.Vertices[i * data.Dimensions];
            double y = data.Vertices[(i * data.Dimensions) + 2] + tmpOffset;   // GeoJSON uses z for height, while Unity uses y
            double z = data.Vertices[(i * data.Dimensions) + 1];   // GeoJSON uses z for height, while Unity uses y
            vertices[i] = new Vector3((float)x, (float)y, (float)z);
        }
        mesh.vertices = vertices;

        // Setup triangles
        triangles.Reverse();
        mesh.triangles = triangles.ToArray();

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Mobile/Diffuse"));
        meshFilter.mesh = mesh;

        // TODO we're getting an extra vertex because GeoJSON polygon's line rings loop around, should we cut it?
        // Logger.Log($"Mesh>Vertices:{meshFilter.mesh.vertexCount},Triangles:{meshFilter.mesh.triangles.Length / 3},Normals:{meshFilter.mesh.normals.Length}");
    }
}
