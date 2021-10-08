using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Represents a Terrain layer's tile
/// </summary>
public class TerrainTile : ITile
{
    public ILayer Layer { get; }

    public int Zoom { get { return Layer.Properties.Zoom; } }

    public int X { get; }

    public int Y { get; }

    public string Id { get { return $"{Zoom}/{X}/{Y}"; } }

    public string FullId { get { return $"{Layer.Id}/{Id}"; } }

    public Bounds Bounds { get; }

    public Vector2D Center { get { return Bounds.Center; } }

    public GameObject GameObject { get; }

    public TileState State { get; }

    private Texture2D tmpHeighmapThing; // TODO probably a better idea to only store the converted data

    /// <summary>
    /// Constructs a new Terrain tile
    /// </summary>
    /// <param name="layer">The layer where the tile belongs</param>
    /// <param name="x">Tile's X coordinate</param>
    /// <param name="y">Tile's Y coordinate</param>
    public TerrainTile(ILayer layer, int x, int y)
    {
        this.Layer = layer;
        this.X = x;
        this.Y = y;
        // Calculate tile bounds
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom);

        // Setup the gameobject
        GameObject = new GameObject($"{Id}");
        GameObject.transform.parent = Layer.GameObject.transform; // Set it as a child of the layer gameobject
        Vector2D relativeOrigin = Bounds.Min - Layer.Properties.Origin;
        GameObject.transform.localPosition = new Vector3((float)relativeOrigin.X, 0, (float)relativeOrigin.Y); // Set tile origin

        // Load and render the tile
        Load();
    }

    /// <summary>
    /// Load the tile
    /// </summary>
    private async void Load()
    {
        // Request heightmap
        string heightmapUrl = $"https://api.mapbox.com/v4/mapbox.terrain-rgb/{Zoom}/{X}/{Y}.pngraw?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest heightmapReq = UnityWebRequestTexture.GetTexture(heightmapUrl);
        UnityWebRequestAsyncOperation heightmapOp = heightmapReq.SendWebRequest();

        // Request raster texture
        string rasterUrl = $"https://api.mapbox.com/v4/{Layer.Id}/{Zoom}/{X}/{Y}.jpg?access_token={MainController.MapboxAccessToken}";
        using UnityWebRequest rasterReq = UnityWebRequestTexture.GetTexture(rasterUrl);
        UnityWebRequestAsyncOperation rasterOp = rasterReq.SendWebRequest();

        // Wait for the requests     
        while (!heightmapOp.isDone || !rasterOp.isDone)
        {
            await Task.Yield();
        }

        // Check for errors
        if (heightmapReq.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError(heightmapReq.error);
        }
        if (rasterReq.result != UnityWebRequest.Result.Success)
        {
            Logger.LogError(rasterReq.error);
        }

        // Render the tile if requests were successful
        if (heightmapReq.result == UnityWebRequest.Result.Success && (rasterReq.result == UnityWebRequest.Result.Success))
        {
            tmpHeighmapThing = DownloadHandlerTexture.GetContent(heightmapReq);
            tmpHeighmapThing.wrapMode = TextureWrapMode.Clamp; // TODO actually take care of the edges

            Texture2D rasterTexture = DownloadHandlerTexture.GetContent(rasterReq);
            rasterTexture.wrapMode = TextureWrapMode.Clamp;

            // Render the tile
            if (Layer.Properties.ElevatedTerrain)
            {
                RenderElevatedTerrain(rasterTexture);
            }
            else
            {
                RenderFlatTerrain(rasterTexture);
            }
        }
    }

    /// <summary>
    /// Render the terrain as a flat plane
    /// </summary>
    /// <param name="texture">The raster texture</param>
    private void RenderFlatTerrain(Texture2D texture)
    {
        double tileWidth = Bounds.Width;
        double tileHeight = Bounds.Height;
        int divisions = Layer.Properties.TerrainTileDivisions;
        double divisionWidth = tileWidth / divisions;
        double divisionHeight = tileHeight / divisions;

        // Setup the mesh components
        MeshRenderer meshRenderer = GameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = GameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[divisions * divisions * 4];
        int[] triangles = new int[divisions * divisions * 6]; // 2 * 3
        Vector2[] uvs = new Vector2[divisions * divisions * 4];

        float tmpOffset = 0.01f; // TODO remove this
        for (int divisionY = 0; divisionY < divisions; divisionY++)
        {
            for (int divisionX = 0; divisionX < divisions; divisionX++)
            {
                int currentDivision = (divisionY * divisions) + divisionX;
                double divisionXOrigin = divisionX * divisionWidth;
                double divisionYOrigin = divisionY * divisionHeight;

                // Setup vertices
                vertices[(currentDivision * 4) + 0] = new Vector3((float)(divisionXOrigin), 0 - tmpOffset, (float)(divisionYOrigin));
                vertices[(currentDivision * 4) + 1] = new Vector3((float)(divisionXOrigin + divisionWidth), 0 - tmpOffset, (float)(divisionYOrigin));
                vertices[(currentDivision * 4) + 2] = new Vector3((float)(divisionXOrigin + divisionWidth), 0 - tmpOffset, (float)(divisionYOrigin + divisionHeight));
                vertices[(currentDivision * 4) + 3] = new Vector3((float)(divisionXOrigin), 0 - tmpOffset, (float)(divisionYOrigin + divisionHeight));

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
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        GameObject.GetComponent<Renderer>().material.mainTexture = texture;
        meshFilter.mesh = mesh;
    }

    /// <summary>
    /// Render the terrain with elevation using the heightmap data
    /// </summary>
    /// <param name="texture">The raster texture</param>
    private void RenderElevatedTerrain(Texture2D texture)
    {
        double tileWidth = Bounds.Width;
        double tileHeight = Bounds.Height;
        int divisions = Layer.Properties.TerrainTileDivisions;
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
                divisionGameObject.transform.parent = GameObject.transform; // Set it as a child of the tile gameobject
                divisionGameObject.transform.localPosition = new Vector3((float)divisionXOrigin, 0, (float)divisionYOrigin);

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
                        vertices[(currentPixel * 4) + 0] = new Vector3((float)(pixelXOrigin), (float)GetHeight((divisionX * pixelsPerDivision) + pixelX, (divisionY * pixelsPerDivision) + pixelY), (float)(pixelYOrigin));
                        vertices[(currentPixel * 4) + 1] = new Vector3((float)(pixelXOrigin + pixelWidth), (float)GetHeight((divisionX * pixelsPerDivision) + pixelX + 1, (divisionY * pixelsPerDivision) + pixelY), (float)(pixelYOrigin));
                        vertices[(currentPixel * 4) + 2] = new Vector3((float)(pixelXOrigin + pixelWidth), (float)GetHeight((divisionX * pixelsPerDivision) + pixelX + 1, (divisionY * pixelsPerDivision) + pixelY + 1), (float)(pixelYOrigin + pixelHeight));
                        vertices[(currentPixel * 4) + 3] = new Vector3((float)(pixelXOrigin), (float)GetHeight((divisionX * pixelsPerDivision) + pixelX, (divisionY * pixelsPerDivision) + pixelY + 1), (float)(pixelYOrigin + pixelHeight));

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
                meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
                divisionGameObject.GetComponent<Renderer>().material.mainTexture = texture;
                meshFilter.mesh = mesh;
            }
        }
    }

    /// <summary>
    /// Get height at given pixel location
    /// </summary>
    /// <param name="pixelX">The pixel X coordinate</param>
    /// <param name="pixelY">The pixel Y coordinate</param>
    /// <returns>Height at given location (meters)</returns>
    private double GetHeight(int pixelX, int pixelY)
    {
        return MapboxHeightFromColor(tmpHeighmapThing.GetPixel(pixelX, pixelY));
    }

    /// <summary>
    /// Decode pixel values to height values. The height will be returned in meters
    /// </summary>
    /// <param name="color">The queried location's pixel</param>
    /// <returns>Height at location (meters)</returns>
    private double MapboxHeightFromColor(Color color)
    {
        // Convert from 0..1 to 0..255
        float R = color.r * 255;
        float G = color.g * 255;
        float B = color.b * 255;

        return -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1);
    }
}