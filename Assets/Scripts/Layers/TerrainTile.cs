using System.Net.Http;
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

    public Bounds Bounds { get; }

    public Vector2D Center { get; }

    public GameObject GameObject { get; }

    private int tmpX, tmpY;

    /// <summary>
    /// Constructs a new Terrain tile
    /// </summary>
    /// <param name="layer">The layer where the tile belongs</param>
    /// <param name="x">Tile's X coordinate</param>
    /// <param name="y">Tile's Y coordinate</param>
    public TerrainTile(ILayer layer, int x, int y, int tmpY, int tmpX)
    {
        this.Layer = layer;
        this.X = x;
        this.Y = y;
        // Calculate tile bounds and center
        this.Bounds = GlobalMercator.GoogleTileBounds(X, Y, Zoom);
        this.Center = Bounds.Center();

        this.tmpY = tmpY;
        this.tmpX = tmpX;

        // Setup the gameobject
        GameObject = new GameObject($"{Id}");
        GameObject.transform.parent = Layer.GameObject.transform; // Set it as a child of the layer gameobject

        // Load and render the tile
        string heightUrl = $"https://api.mapbox.com/v4/{Layer.Id}/{Zoom}/{x}/{y}.pngraw?access_token={MainController.MapboxAccessToken}";
        Load(heightUrl);
        //string rasterUrl = $"https://api.mapbox.com/v4/{Layer.Id}/{Zoom}/{x}/{y}.jpg70?access_token={MainController.MapboxAccessToken}";
        //Load(rasterUrl, tmpY, tmpX);
        /*
        GetTexture("terrain");
        GetTexture("satellite");
        BuildMesh(terrain);
        object material = satellite
        */
    }

    //TODO description
    async void Load(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                // Render the tile
                Render(texture);
            }
            else
            {
                Logger.LogError(request.error);
            }
        }
    }

    //TODO description
    private void Render(Texture2D texture)
    {
        RenderPlane(texture);
        //RenderFlatTerrain(texture);
        RenderElevatedTerrain(texture);
    }

    private void RenderPlane(Texture2D texture)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        gameObject.name = $"{tmpX}/{tmpY}";
        int step = Layer.Properties.TileViewDistance * 2 + 1;
        gameObject.transform.position = new Vector3(10 * tmpX, 1, 10 * tmpY);
        gameObject.transform.eulerAngles = new Vector3(0, -180, 0);
        gameObject.GetComponent<Renderer>().material.mainTexture = texture;
    }

    private void RenderFlatTerrain(Texture2D texture)
    {
        Logger.Log($"Google Coords: ({X},{Y}) | Google Bounds: {Bounds} | Center: {Center} | Unity: {Center - new Vector2D(Layer.Properties.CenterX, Layer.Properties.CenterY)}");

        int step = Layer.Properties.TileViewDistance * 2 + 1;
        int width = GlobalMercator.TileSize;
        int height = GlobalMercator.TileSize;
        int pixelSize = 1;
        Vector2D originOffset = new Vector2D(width * tmpX, height * tmpY);

        GameObject.transform.position = new Vector3((float)originOffset.X, 2, (float)originOffset.Y);

        // Setup the mesh components
        MeshRenderer meshRenderer = GameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = GameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[width * height * 4];
        int[] triangles = new int[width * height * 6]; // 2 * 3
        Vector2[] uvs = new Vector2[width * height * 4];

        for (int squareY = 0; squareY < height; squareY++)
        {
            for (int squareX = 0; squareX < width; squareX++)
            {
                int currentSquare = (squareY * width) + squareX;
                int squareXOrigin = squareX * pixelSize;
                int squareYOrigin = squareY * pixelSize;

                // Setup vertices
                vertices[(currentSquare * 4) + 0] = new Vector3(squareXOrigin, 0, squareYOrigin);
                vertices[(currentSquare * 4) + 1] = new Vector3(squareXOrigin + pixelSize, 0, squareYOrigin);
                vertices[(currentSquare * 4) + 2] = new Vector3(squareXOrigin + pixelSize, 0, squareYOrigin + pixelSize);
                vertices[(currentSquare * 4) + 3] = new Vector3(squareXOrigin, 0, squareYOrigin + pixelSize);

                // Setup triangles
                triangles[(currentSquare * 6) + 0] = (currentSquare * 4) + 0;
                triangles[(currentSquare * 6) + 1] = (currentSquare * 4) + 2;
                triangles[(currentSquare * 6) + 2] = (currentSquare * 4) + 1;
                triangles[(currentSquare * 6) + 3] = (currentSquare * 4) + 0;
                triangles[(currentSquare * 6) + 4] = (currentSquare * 4) + 3;
                triangles[(currentSquare * 6) + 5] = (currentSquare * 4) + 2;

                // Setup uvs
                uvs[(currentSquare * 4) + 0] = new Vector2((squareXOrigin) / (float)width, (squareYOrigin) / (float)height);
                uvs[(currentSquare * 4) + 1] = new Vector2((squareXOrigin + pixelSize) / (float)width, (squareYOrigin) / (float)height);
                uvs[(currentSquare * 4) + 2] = new Vector2((squareXOrigin + pixelSize) / (float)width, (squareYOrigin + pixelSize) / (float)height);
                uvs[(currentSquare * 4) + 3] = new Vector2((squareXOrigin) / (float)width, (squareYOrigin + pixelSize) / (float)height);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        meshFilter.mesh = mesh;

        GameObject.GetComponent<Renderer>().material.mainTexture = texture;
        double terrainHeight = MapboxHeightFromColor(texture.GetPixel(0, 0));
        Logger.Log(terrainHeight);
        Logger.Log($"Texture is {texture.width} x {texture.height}");
    }

    //TODO
    private void RenderElevatedTerrain(Texture2D texture)
    {
        int step = Layer.Properties.TileViewDistance * 2 + 1;
        int width = GlobalMercator.TileSize / Layer.Properties.TerrainTileDivisions;
        int height = GlobalMercator.TileSize / Layer.Properties.TerrainTileDivisions;
        int pixelSize = 1;
        Vector2D originOffset = new Vector2D(width * tmpX, height * tmpY);

        // Setup the gameobject
        GameObject terrainTileGameObject = new GameObject($"{tmpX}/{tmpY}");
        terrainTileGameObject.transform.parent = GameObject.transform; // Set it as a child of the layer gameobject

        terrainTileGameObject.transform.position = new Vector3((float)originOffset.X, 2, (float)originOffset.Y);

        // Setup the mesh components
        MeshRenderer meshRenderer = terrainTileGameObject.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = terrainTileGameObject.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[width * height * 4];
        int[] triangles = new int[width * height * 6]; // 2 * 3
        Vector2[] uvs = new Vector2[width * height * 4];

        for (int squareY = 0; squareY < height; squareY++)
        {
            for (int squareX = 0; squareX < width; squareX++)
            {
                int currentSquare = (squareY * width) + squareX;
                int squareXOrigin = squareX * pixelSize;
                int squareYOrigin = squareY * pixelSize;

                // Setup vertices
                vertices[(currentSquare * 4) + 0] = new Vector3(squareXOrigin, 0, squareYOrigin);
                vertices[(currentSquare * 4) + 1] = new Vector3(squareXOrigin + pixelSize, 0, squareYOrigin);
                vertices[(currentSquare * 4) + 2] = new Vector3(squareXOrigin + pixelSize, 0, squareYOrigin + pixelSize);
                vertices[(currentSquare * 4) + 3] = new Vector3(squareXOrigin, 0, squareYOrigin + pixelSize);

                // Setup triangles
                triangles[(currentSquare * 6) + 0] = (currentSquare * 4) + 0;
                triangles[(currentSquare * 6) + 1] = (currentSquare * 4) + 2;
                triangles[(currentSquare * 6) + 2] = (currentSquare * 4) + 1;
                triangles[(currentSquare * 6) + 3] = (currentSquare * 4) + 0;
                triangles[(currentSquare * 6) + 4] = (currentSquare * 4) + 3;
                triangles[(currentSquare * 6) + 5] = (currentSquare * 4) + 2;

                // Setup uvs
                uvs[(currentSquare * 4) + 0] = new Vector2((squareXOrigin) / (float)width, (squareYOrigin) / (float)height);
                uvs[(currentSquare * 4) + 1] = new Vector2((squareXOrigin + pixelSize) / (float)width, (squareYOrigin) / (float)height);
                uvs[(currentSquare * 4) + 2] = new Vector2((squareXOrigin + pixelSize) / (float)width, (squareYOrigin + pixelSize) / (float)height);
                uvs[(currentSquare * 4) + 3] = new Vector2((squareXOrigin) / (float)width, (squareYOrigin + pixelSize) / (float)height);
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Assign mesh
        mesh.RecalculateNormals();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
        meshFilter.mesh = mesh;

        terrainTileGameObject.GetComponent<Renderer>().material.mainTexture = texture;
        double terrainHeight = MapboxHeightFromColor(texture.GetPixel(0, 0));
        Logger.Log(terrainHeight);
        Logger.Log($"Texture is {texture.width} x {texture.height}");
    }

    private double MapboxHeightFromColor(Color color)
    {
        // Convert from 0..1 to 0..255
        float R = color.r * 255;
        float G = color.g * 255;
        float B = color.b * 255;

        return -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1);
    }
}