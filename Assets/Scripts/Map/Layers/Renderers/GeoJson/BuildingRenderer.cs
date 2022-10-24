using UnityEngine;
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

    public void RenderNode(GeoJsonTileLayer tileLayer, Feature feature, Position coordinates)
    {
        Debug.LogWarning($"[BuildingRenderer] {tileLayer.FullId}: Tried to render a Node!");
    }

    public void RenderEdge(GeoJsonTileLayer tileLayer, Feature feature, Position[] coordinates)
    {
        Debug.LogWarning($"[BuildingRenderer] {tileLayer.FullId}: Tried to render an Edge!");
    }

    public void RenderArea(GeoJsonTileLayer tileLayer, Feature feature, Position[][] coordinates)
    {
        // Check for empty coordinates array
        if (coordinates.Length == 0)
        {
            //Debug.LogWarning($"[BuildingRenderer] {tileLayer.FullId}/{feature.GameObject.name}: Tried to render an Area with no coordinates"); TODO: Do we want to log this?
            return;
        }

        // Setup the gameobject
        GameObject building = new GameObject("Area - Building"); // Create Area gameobject
        building.transform.parent = feature.GameObject.transform; // Set it as a child of the Feature gameobject
        building.transform.localPosition = Vector3.zero; // Set origin
        building.transform.rotation = feature.GameObject.transform.rotation; // Match rotation

        double terrainHeightOffset = 0;
        if (tileLayer.Tile.Map.ElevatedTerrain)
        {
            // Use the Altiude property if it exists
            terrainHeightOffset = feature.GetPropertyAsDouble("Altiude");

            /* This would have been nice, but if a feature is across multiple tiles, the multiple parts will have different centroids and therefore different heights
            // If terrain's enabled, get the height at the building's centroid (ignoring holes). Based on https://en.wikipedia.org/wiki/Centroid#Of_a_polygon
            double centroidXSum = 0;
            double centroidYSum = 0;
            double areaSum = 0;
            for (int i = 0; i < coordinates[0].Length - 1; i++)
            {
                double xCurr = coordinates[0][i].GetRelativeX(tileLayer.Tile.Bounds.Min.X);
                double xNext = coordinates[0][i + 1].GetRelativeX(tileLayer.Tile.Bounds.Min.X);
                double yCurr = coordinates[0][i].GetRelativeY(tileLayer.Tile.Bounds.Min.Y);
                double yNext = coordinates[0][i + 1].GetRelativeY(tileLayer.Tile.Bounds.Min.Y);
                double areaCurr = (xCurr * yNext) - (xNext * yCurr);
                areaSum += areaCurr;
                centroidXSum += (xCurr + xNext) * areaCurr;
                centroidYSum += (yCurr + yNext) * areaCurr;
            }
            double area = areaSum / 2;
            double centroidX = centroidXSum / (6 * area);
            double centroidY = centroidYSum / (6 * area);
            centroidTerrainHeightOffset = tileLayer.Tile.GetHeight(GlobalMercator.LatLonToMeters(centroidY, centroidX)); */
        }

        // Get building height
        double height = feature.GetPropertyAsDouble("EXTRUDE");
        if (height == 0)
        {
            height = defaultHeight;
        }

        RenderWalls(tileLayer, building, coordinates, height, terrainHeightOffset);
        RenderRoof(tileLayer, building, coordinates, height, terrainHeightOffset);
    }

    /// <summary>
    /// Render the building walls
    /// </summary>
    /// <param name="tileLayer">The tile layer where the building is located</param>
    /// <param name="building">The building GameObject</param>
    /// <param name="coordinates">The building coordinates</param>
    /// <param name="height">The building height</param>
    /// <param name="terrainHeightOffset">The terrain height offset for the building</param>
    private void RenderWalls(GeoJsonTileLayer tileLayer, GameObject building, Position[][] coordinates, double height, double terrainHeightOffset)
    {
        // Setup the gameobject
        GameObject walls = new GameObject("Walls"); // Create Walls gameobject
        walls.transform.parent = building.transform; // Set it as a child of the Feature gameobject
        walls.transform.localPosition = Vector3.zero; // Set origin
        walls.transform.rotation = building.transform.rotation; // Match rotation

        // Setup the mesh components
        MeshRenderer meshRenderer = walls.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = walls.AddComponent<MeshFilter>();
        MeshCollider meshCollider = walls.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        // Setup vertices/triangles
        int numPoints = 0;
        foreach (Position[] ring in coordinates)
        {
            numPoints += ring.Length - 1;
        }
        Vector3[] vertices = new Vector3[numPoints * 4];
        int[] triangles = new int[numPoints * 6];
        int vertOffset = 0;
        int triOffset = 0;
        foreach (Position[] ring in coordinates)
        {
            for (int point = 0; point < ring.Length - 1; point++)
            {
                double x0 = ring[point].GetRelativeX(tileLayer.Tile.Bounds.Min.X);
                double y0 = ring[point].GetRelativeZ() + terrainHeightOffset; // GeoJSON uses z for height, while Unity uses y
                double z0 = ring[point].GetRelativeY(tileLayer.Tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

                double x1 = ring[point + 1].GetRelativeX(tileLayer.Tile.Bounds.Min.X);
                double y1 = ring[point + 1].GetRelativeZ() + terrainHeightOffset; // GeoJSON uses z for height, while Unity uses y
                double z1 = ring[point + 1].GetRelativeY(tileLayer.Tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

                vertices[vertOffset + 0] = new Vector3((float)x0, (float)y0, (float)z0);
                vertices[vertOffset + 1] = new Vector3((float)x1, (float)y1, (float)z1);
                vertices[vertOffset + 2] = new Vector3((float)x1, (float)(y1 + height), (float)z1);
                vertices[vertOffset + 3] = new Vector3((float)x0, (float)(y0 + height), (float)z0);

                triangles[triOffset + 0] = vertOffset + 0;
                triangles[triOffset + 1] = vertOffset + 1;
                triangles[triOffset + 2] = vertOffset + 2;
                triangles[triOffset + 3] = vertOffset + 0;
                triangles[triOffset + 4] = vertOffset + 2;
                triangles[triOffset + 5] = vertOffset + 3;

                vertOffset += 4;
                triOffset += 6;
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Wall"); // TODO use Addressables instead?
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Render the building roof
    /// </summary>
    /// <param name="tileLayer">The tile layer where the building is located</param>
    /// <param name="building">The building GameObject</param>
    /// <param name="coordinates">The building coordinates</param>
    /// <param name="height">The building height</param>
    /// <param name="terrainHeightOffset">The terrain height offset for the building</param>
    private void RenderRoof(GeoJsonTileLayer tileLayer, GameObject building, Position[][] coordinates, double height, double terrainHeightOffset)
    {
        // Setup the gameobject
        GameObject roof = new GameObject("Roof"); // Create Roof gameobject
        roof.transform.parent = building.transform; // Set it as a child of the Building gameobject
        roof.transform.localPosition = Vector3.zero; // Set origin
        roof.transform.rotation = building.transform.rotation; // Match rotation

        // Triangulate the polygon coordinates for the roof using Earcut
        EarcutLib.Data data = EarcutLib.Flatten(coordinates, tileLayer, false);
        List<int> triangles = EarcutLib.Earcut(data.Vertices, data.Holes, data.Dimensions);

        // Setup the mesh components
        MeshRenderer meshRenderer = roof.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = roof.AddComponent<MeshFilter>();
        MeshCollider meshCollider = roof.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        // Setup vertices
        List<Vector3> vertices = new List<Vector3>(data.Vertices.Count / data.Dimensions);
        for (int i = 0; i < data.Vertices.Count / data.Dimensions; i++)
        {
            double x = data.Vertices[i * data.Dimensions];
            double y = data.Vertices[(i * data.Dimensions) + 2] + terrainHeightOffset + height; // GeoJSON uses z for height, while Unity uses y
            double z = data.Vertices[(i * data.Dimensions) + 1];   // GeoJSON uses z for height, while Unity uses y
            vertices.Add(new Vector3((float)x, (float)y, (float)z));
        }
        mesh.vertices = vertices.ToArray();

        // Setup triangles
        triangles.Reverse();
        mesh.triangles = triangles.ToArray();

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Roof"); // TODO use Addressables instead?
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
