using UnityEngine;

// Flat Terrain renderer
public class FlatTerrainRenderer : ITerrainRenderer
{
    /// <summary>
    /// Number of divisions that each terrain tile will be split (eg: if 4, each terrain tile will be split into 4 x 4 subdivisions)
    /// </summary>
    private int terrainTileDivisions = 4;

    public void RenderTerrain(TerrainTile tile, Texture2D texture)
    {
        double tileWidth = tile.Bounds.Width;
        double tileHeight = tile.Bounds.Height;
        int divisions = terrainTileDivisions;
        double divisionWidth = tileWidth / divisions;
        double divisionHeight = tileHeight / divisions;

        // Setup the mesh components
        MeshRenderer meshRenderer = tile.GameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = tile.GameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[divisions * divisions * 4];
        int[] triangles = new int[divisions * divisions * 6]; // 2 * 3
        Vector2[] uvs = new Vector2[divisions * divisions * 4];

        for (int divisionY = 0; divisionY < divisions; divisionY++)
        {
            for (int divisionX = 0; divisionX < divisions; divisionX++)
            {
                int currentDivision = (divisionY * divisions) + divisionX;
                double divisionXOrigin = divisionX * divisionWidth;
                double divisionYOrigin = divisionY * divisionHeight;

                // Setup vertices
                vertices[(currentDivision * 4) + 0] = new Vector3((float)(divisionXOrigin), 0, (float)(divisionYOrigin));
                vertices[(currentDivision * 4) + 1] = new Vector3((float)(divisionXOrigin + divisionWidth), 0, (float)(divisionYOrigin));
                vertices[(currentDivision * 4) + 2] = new Vector3((float)(divisionXOrigin + divisionWidth), 0, (float)(divisionYOrigin + divisionHeight));
                vertices[(currentDivision * 4) + 3] = new Vector3((float)(divisionXOrigin), 0, (float)(divisionYOrigin + divisionHeight));

                // Setup triangles
                triangles[(currentDivision * 6) + 0] = (currentDivision * 4) + 0;
                triangles[(currentDivision * 6) + 1] = (currentDivision * 4) + 2;
                triangles[(currentDivision * 6) + 2] = (currentDivision * 4) + 1;
                triangles[(currentDivision * 6) + 3] = (currentDivision * 4) + 0;
                triangles[(currentDivision * 6) + 4] = (currentDivision * 4) + 3;
                triangles[(currentDivision * 6) + 5] = (currentDivision * 4) + 2;

                // Setup uvs
                uvs[(currentDivision * 4) + 0] = new Vector2((float)((divisionXOrigin) / tileWidth), (float)((divisionYOrigin) / tileHeight));
                uvs[(currentDivision * 4) + 1] = new Vector2((float)((divisionXOrigin + divisionWidth) / tileWidth), (float)((divisionYOrigin) / tileHeight));
                uvs[(currentDivision * 4) + 2] = new Vector2((float)((divisionXOrigin + divisionWidth) / tileWidth), (float)((divisionYOrigin + divisionHeight) / tileHeight));
                uvs[(currentDivision * 4) + 3] = new Vector2((float)((divisionXOrigin) / tileWidth), (float)((divisionYOrigin + divisionHeight) / tileHeight));
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Mobile/Diffuse"));
        tile.GameObject.GetComponent<Renderer>().material.mainTexture = texture;
        meshFilter.mesh = mesh;
    }
}