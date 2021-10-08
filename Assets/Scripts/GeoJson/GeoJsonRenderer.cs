using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Methods related to rendering GeoJSON objects
/// </summary>
public class GeoJsonRenderer
{
    /// <summary>
    /// Render a node with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="tile">The feature's tile</param>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The node coordinates</param>
    public static void RenderNode(GeoJsonTile tile, Feature feature, Position coordinates)
    {
        if (tile.Layer.Properties.RenderModel)
        {
            // Render Node with an existing model instead
            double x = coordinates.GetRelativeX(tile.Bounds.Min.X);
            double y = coordinates.GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
            double z = coordinates.GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

            // Get model name
            string model;
            if (tile.Layer.Properties.ModelName != null)
            {
                model = tile.Layer.Properties.ModelName;
            }
            else
            {
                model = feature.GetPropertyAsString("model");
            }

            GameObject prefab = Resources.Load<GameObject>($"Prefabs/{model}"); // TODO Actual prefabs probably shouldn't be loaded with Resources.Load
            if (prefab != null)
            {
                GameObject node = GameObject.Instantiate(prefab);
                node.name = $"Node - {model}";
                node.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
                node.transform.localPosition = new Vector3((float)x, (float)y, (float)z);
            }
            else
            {
                Logger.LogWarning($"Unable to find resource 'Prefabs/{model}'");
            }
        }
        else
        {
            // Default Node Rendering

            // Setup the gameobject
            GameObject node = new GameObject("Node"); // Create Node gameobject
            node.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
            node.transform.localPosition = Vector3.zero; // Set origin

            // Setup the mesh components
            MeshRenderer meshRenderer = node.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = node.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();

            // Setup vertices
            double x = coordinates.GetRelativeX(tile.Bounds.Min.X);
            double y = coordinates.GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
            double z = coordinates.GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y
            Vector3[] vertices = new Vector3[5] // TODO we're being greedy with vertices here and that means the normals will be messed up since we're sharing vertices between differently oriented faces
            {
            new Vector3((float)x, (float)y, (float)z),
            new Vector3((float)(x + tile.Layer.Properties.NodeRadius), (float)(y + tile.Layer.Properties.NodeHeight), (float)(z + tile.Layer.Properties.NodeRadius)),
            new Vector3((float)(x - tile.Layer.Properties.NodeRadius), (float)(y + tile.Layer.Properties.NodeHeight), (float)(z + tile.Layer.Properties.NodeRadius)),
            new Vector3((float)(x - tile.Layer.Properties.NodeRadius), (float)(y + tile.Layer.Properties.NodeHeight), (float)(z - tile.Layer.Properties.NodeRadius)),
            new Vector3((float)(x + tile.Layer.Properties.NodeRadius), (float)(y + tile.Layer.Properties.NodeHeight), (float)(z - tile.Layer.Properties.NodeRadius))
            };
            mesh.vertices = vertices;

            // Setup triangles
            int[] triangles = new int[18] // 6 * 3
            {
            0,1,2,
            0,2,3,
            0,3,4,
            0,4,1,
            1,4,3,
            1,3,2
            };
            mesh.triangles = triangles;

            // Assign mesh
            mesh.RecalculateNormals();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
            meshFilter.mesh = mesh;
        }
    }

    /// <summary>
    /// Render an edge with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="tile">The feature's tile</param>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The edge coordinates</param>
    public static void RenderEdge(GeoJsonTile tile, Feature feature, Position[] coordinates)
    {
        // Setup the gameobject
        GameObject edge = new GameObject("Edge"); // Create Edge gameobject
        edge.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        edge.transform.localPosition = Vector3.zero; // Set origin

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
            double ax = coordinates[segment].GetRelativeX(tile.Bounds.Min.X);
            double ay = coordinates[segment].GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
            double az = coordinates[segment].GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

            // End point of segment AB
            double bx = coordinates[segment + 1].GetRelativeX(tile.Bounds.Min.X);
            double by = coordinates[segment + 1].GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
            double bz = coordinates[segment + 1].GetRelativeY(tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

            // Calculate AB
            double abX = bx - ax;
            double abZ = bz - az;
            double abMagnitude = Math.Sqrt(Math.Pow(abX, 2) + Math.Pow(abZ, 2));

            // AB⟂ with given width
            double abPerpX = (tile.Layer.Properties.EdgeWidth * -abZ) / abMagnitude;
            double abPerpZ = (tile.Layer.Properties.EdgeWidth * abX) / abMagnitude;

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
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Render an area with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="tile">The feature's tile</param>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The area coordinates</param>
    public static void RenderArea(GeoJsonTile tile, Feature feature, Position[][] coordinates)
    {
        // Setup the gameobject
        GameObject area = new GameObject("Area"); // Create Area gameobject
        area.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        area.transform.localPosition = Vector3.zero; // Set origin

        // Check for empty coordinates array
        if (coordinates.Length == 0)
        {
            Logger.LogWarning($"{feature.GameObject.name}: Tried to render an Area with no coordinates");
            return;
        }

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
        Vector3[] vertices = new Vector3[data.Vertices.Count / data.Dimensions];
        for (int i = 0; i < data.Vertices.Count / data.Dimensions; i++)
        {
            double x = data.Vertices[i * data.Dimensions];
            double y = data.Vertices[(i * data.Dimensions) + 2];   // GeoJSON uses z for height, while Unity uses y
            double z = data.Vertices[(i * data.Dimensions) + 1];   // GeoJSON uses z for height, while Unity uses y
            vertices[i] = new Vector3((float)x, (float)y, (float)z);
        }
        mesh.vertices = vertices;

        // Setup triangles
        triangles.Reverse();
        mesh.triangles = triangles.ToArray();

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        meshFilter.mesh = mesh;

        // TODO we're getting an extra vertex because GeoJSON polygon's line rings loop around, should we cut it?
        // Logger.Log($"Mesh>Vertices:{meshFilter.mesh.vertexCount},Triangles:{meshFilter.mesh.triangles.Length / 3},Normals:{meshFilter.mesh.normals.Length}");
    }
}
