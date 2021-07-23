using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// The GeoJSON base class
/// </summary>
public class GeoJson
{
    /// <summary>
    /// Reads a JSON text and turns it into a GeoJSON object
    /// </summary>
    /// <param name="json">JSON text input that is a valid GeoJSON text</param>
    /// <returns>The GeoJSON Object represented in the JSON text input</returns>
    public static IGeoJsonObject Parse(string json)
    {
        try
        {
            JsonConverter[] converters = new JsonConverter[] { new GeoJsonObjectConverter(), new PositionConverter() };
            return JsonConvert.DeserializeObject<IGeoJsonObject>(json, converters);
        }
        catch (JsonException e)
        {
            throw new InvalidGeoJsonException("Failed to parse GeoJSON", e);
        }
    }

    /// <summary>
    /// Render a node with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The node coordinates</param>
    public static void RenderNode(GameObject feature, Position coordinates)
    {
        // Setup the gameobject
        GameObject node = new GameObject("Node"); // Create Node gameobject
        node.transform.parent = feature.transform; // Set it as a child of the Feature gameobject
        node.transform.position = new Vector3((float)coordinates.x, (float)coordinates.z, (float)coordinates.y); // Set origin to first coordinate

        // Setup the mesh components
        MeshRenderer meshRenderer = node.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = node.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        // Setup vertices
        float height = 1;
        float radius = 0.25f;
        Vector3[] vertices = new Vector3[5]
        {
            new Vector3(0, 0, 0),
            new Vector3(radius, height, radius),
            new Vector3(-radius, height, radius),
            new Vector3(-radius, height, -radius),
            new Vector3(radius, height, -radius)
        };
        mesh.vertices = vertices;

        // Setup triangles
        int[] triangles = new int[18]
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

    /// <summary>
    /// Render an edge with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The edge coordinates</param>
    public static void RenderEdge(GameObject feature, Position[] coordinates)
    {
        // Setup the gameobject
        GameObject edge = new GameObject("Edge"); // Create Node gameobject
        edge.transform.parent = feature.transform; // Set it as a child of the Feature gameobject
        edge.transform.position = new Vector3((float)coordinates[0].x, (float)coordinates[0].z, (float)coordinates[0].y); // Set origin to first coordinate

        // Render the geometry
        //TODO
    }

    /// <summary>
    /// Render an area with given coordinates as a child of given feature object
    /// </summary>
    /// <param name="feature">The parent feature</param>
    /// <param name="coordinates">The area coordinates</param>
    public static void RenderArea(GameObject feature, Position[][] coordinates)
    {
        // Setup the gameobject
        GameObject area = new GameObject("Area"); // Create Node gameobject
        area.transform.parent = feature.transform; // Set it as a child of the Feature gameobject
        area.transform.position = new Vector3((float)coordinates[0][0].x, (float)coordinates[0][0].z, (float)coordinates[0][0].y); // Set origin to first coordinate

        // Render the geometry
        //TODO
    }
}