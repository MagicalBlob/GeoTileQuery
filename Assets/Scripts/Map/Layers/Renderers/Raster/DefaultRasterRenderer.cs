using UnityEngine;

// Default Raster renderer
public class DefaultRasterRenderer : IRasterRenderer
{
    /// <summary>
    /// The default shader
    /// </summary>
    private Shader shader;

    /// <summary>
    /// The default material
    /// </summary>
    private Material material;

    /// <summary>
    /// Creates a new DefaultRasterRenderer
    /// </summary>
    public DefaultRasterRenderer()
    {
        // Load the default material
        this.shader = Shader.Find("Mobile/Diffuse");
        this.material = new Material(shader);
    }

    public void Render(RasterTileLayer tileLayer, Texture2D texture)
    {
        // Check if terrain is enabled on the map to choose which rendering strategy to use
        if (tileLayer.Tile.Map.ElevatedTerrain)
        {
            RenderElevatedTerrain(tileLayer, texture);
        }
        else
        {
            RenderFlat(tileLayer, texture);
        }
    }

    /// <summary>
    /// Renders the raster tile layer as a flat surface
    /// </summary>
    /// <param name="tileLayer">The raster tile layer</param>
    /// <param name="texture">The raster texture</param>
    public void RenderFlat(RasterTileLayer tileLayer, Texture2D texture)
    {
        double tileWidth = tileLayer.Tile.Bounds.Width;
        double tileHeight = tileLayer.Tile.Bounds.Height;

        // Setup the mesh components
        MeshRenderer meshRenderer = tileLayer.GameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = tileLayer.GameObject.AddComponent<MeshFilter>();
        MeshCollider meshCollider = tileLayer.GameObject.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        // Setup vertices
        Vector3[] vertices = new Vector3[]{
            new Vector3(0, 0, 0),
            new Vector3((float)tileWidth, 0, 0),
            new Vector3((float)tileWidth, 0, (float)tileHeight),
            new Vector3(0, 0, (float)tileHeight)
        };

        // Setup triangles
        int[] triangles = new int[] {
            0, 2, 1,
            0, 3, 2
        };

        // Setup uvs
        Vector2[] uvs = new Vector2[]{
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = material;
        tileLayer.GameObject.GetComponent<Renderer>().material.mainTexture = texture;
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Renders the raster tile layer according to the elevation data using Unity's Terrain component
    /// </summary>
    /// <param name="tileLayer">The raster tile layer</param>
    /// <param name="texture">The raster texture</param>
    public void RenderElevatedTerrain(RasterTileLayer tileLayer, Texture2D texture)
    {
        // Create the terrain
        Terrain terrain = Terrain.CreateTerrainGameObject(tileLayer.Tile.GetTerrainData()).GetComponent<Terrain>();
        terrain.name = "Terrain";
        terrain.transform.parent = tileLayer.GameObject.transform; // Set it as a child of the tile gameobject
        terrain.transform.localPosition = new Vector3(0, (float)tileLayer.Tile.Map.MinElevation, 0);
        terrain.transform.rotation = tileLayer.GameObject.transform.rotation;
        terrain.heightmapPixelError = 25;
        terrain.materialTemplate = new Material(shader);
        terrain.materialTemplate.mainTexture = texture;
    }
}