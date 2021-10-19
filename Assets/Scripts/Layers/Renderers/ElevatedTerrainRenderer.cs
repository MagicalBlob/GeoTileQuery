using UnityEngine;

// Elevated Terrain renderer
public class ElevatedTerrainRenderer : ITerrainRenderer
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
        int pixelsPerDivision = GlobalMercator.TileSize / divisions;
        double pixelWidth = divisionWidth / pixelsPerDivision;
        double pixelHeight = divisionHeight / pixelsPerDivision;

        for (int divisionY = 0; divisionY < divisions; divisionY++)
        {
            for (int divisionX = 0; divisionX < divisions; divisionX++)
            {
                double divisionXOrigin = divisionX * divisionWidth;
                double divisionYOrigin = divisionY * divisionHeight;

                // Setup the gameobject
                GameObject divisionGameObject = new GameObject($"{divisionX}/{divisionY}");
                divisionGameObject.transform.parent = tile.GameObject.transform; // Set it as a child of the tile gameobject
                divisionGameObject.transform.localPosition = new Vector3((float)divisionXOrigin, 0, (float)divisionYOrigin);
                divisionGameObject.transform.rotation = tile.GameObject.transform.rotation;

                // Setup the mesh components
                MeshRenderer meshRenderer = divisionGameObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = divisionGameObject.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();

                Vector3[] vertices = new Vector3[pixelsPerDivision * pixelsPerDivision * 4];
                int[] triangles = new int[pixelsPerDivision * pixelsPerDivision * 6]; // 2 * 3
                Vector2[] uvs = new Vector2[pixelsPerDivision * pixelsPerDivision * 4];

                for (int pixelY = 0; pixelY < pixelsPerDivision; pixelY++)
                {
                    for (int pixelX = 0; pixelX < pixelsPerDivision; pixelX++)
                    {
                        int currentPixel = (pixelY * pixelsPerDivision) + pixelX;
                        double pixelXOrigin = pixelX * pixelWidth;
                        double pixelYOrigin = pixelY * pixelHeight;

                        // Setup vertices
                        vertices[(currentPixel * 4) + 0] = new Vector3((float)(pixelXOrigin), (float)tile.GetHeight((divisionX * pixelsPerDivision) + pixelX, (divisionY * pixelsPerDivision) + pixelY), (float)(pixelYOrigin));
                        vertices[(currentPixel * 4) + 1] = new Vector3((float)(pixelXOrigin + pixelWidth), (float)tile.GetHeight((divisionX * pixelsPerDivision) + pixelX + 1, (divisionY * pixelsPerDivision) + pixelY), (float)(pixelYOrigin));
                        vertices[(currentPixel * 4) + 2] = new Vector3((float)(pixelXOrigin + pixelWidth), (float)tile.GetHeight((divisionX * pixelsPerDivision) + pixelX + 1, (divisionY * pixelsPerDivision) + pixelY + 1), (float)(pixelYOrigin + pixelHeight));
                        vertices[(currentPixel * 4) + 3] = new Vector3((float)(pixelXOrigin), (float)tile.GetHeight((divisionX * pixelsPerDivision) + pixelX, (divisionY * pixelsPerDivision) + pixelY + 1), (float)(pixelYOrigin + pixelHeight));

                        // Setup triangles
                        triangles[(currentPixel * 6) + 0] = (currentPixel * 4) + 0;
                        triangles[(currentPixel * 6) + 1] = (currentPixel * 4) + 2;
                        triangles[(currentPixel * 6) + 2] = (currentPixel * 4) + 1;
                        triangles[(currentPixel * 6) + 3] = (currentPixel * 4) + 0;
                        triangles[(currentPixel * 6) + 4] = (currentPixel * 4) + 3;
                        triangles[(currentPixel * 6) + 5] = (currentPixel * 4) + 2;

                        // Setup uvs
                        uvs[(currentPixel * 4) + 0] = new Vector2((float)((divisionXOrigin + pixelXOrigin) / tileWidth), (float)((divisionYOrigin + pixelYOrigin) / tileHeight));
                        uvs[(currentPixel * 4) + 1] = new Vector2((float)((divisionXOrigin + pixelXOrigin + pixelWidth) / tileWidth), (float)((divisionYOrigin + pixelYOrigin) / tileHeight));
                        uvs[(currentPixel * 4) + 2] = new Vector2((float)((divisionXOrigin + pixelXOrigin + pixelWidth) / tileWidth), (float)((divisionYOrigin + pixelYOrigin + pixelHeight) / tileHeight));
                        uvs[(currentPixel * 4) + 3] = new Vector2((float)((divisionXOrigin + pixelXOrigin) / tileWidth), (float)((divisionYOrigin + pixelYOrigin + pixelHeight) / tileHeight));
                    }
                }

                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uvs;

                // Assign mesh
                mesh.RecalculateNormals();
                meshRenderer.sharedMaterial = new Material(Shader.Find("Mobile/Diffuse"));
                divisionGameObject.GetComponent<Renderer>().material.mainTexture = texture;
                meshFilter.mesh = mesh;
            }
        }
    }
}