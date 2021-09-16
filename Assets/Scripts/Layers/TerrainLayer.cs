using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Represents a Terrain layer
/// </summary>
public class TerrainLayer : ILayer
{
    private List<Tile> tiles; // TODO

    public string Id { get; }

    public RenderingProperties Properties { get; }

    public GameObject GameObject { get; }

    /// <summary>
    /// Construct a new TerrainLayer
    /// </summary>
    /// <param name="map">The map to which the layer belongs</param>
    /// <param name="id">The layer id</param>
    /// <param name="properties">The layer rendering properties</param>
    public TerrainLayer(GameObject map, string id, RenderingProperties properties)
    {
        this.Id = id;
        this.Properties = properties;

        // Setup the gameobject
        GameObject = new GameObject(Id); // Create Layer gameobject
        GameObject.transform.parent = map.transform; // Set it as a child of the Map gameobject
    }

    public void Render()
    {
        //TODO
        int tmp = 0;

        Vector2Int tileCoords = GlobalMercator.MetersToTile(Properties.CenterX, Properties.CenterY, Properties.Zoom);
        Vector2Int googleTileCoords = GlobalMercator.GoogleTile(tileCoords.x, tileCoords.y, Properties.Zoom);
        for (int x = googleTileCoords.x - Properties.TileViewDistance; x <= googleTileCoords.x + Properties.TileViewDistance; x++)
        {
            for (int y = googleTileCoords.y - Properties.TileViewDistance; y <= googleTileCoords.y + Properties.TileViewDistance; y++)
            {
                string url = $"https://api.mapbox.com/v4/{Id}/{Properties.Zoom}/{x}/{y}.pngraw?access_token={MainController.MapboxAccessToken}";
                //string url = $"https://api.mapbox.com/v4/{Id}/{Properties.Zoom}/{x}/{y}.jpg70?access_token={MainController.MapboxAccessToken}";
                GetTexture(url, tmp++);
                //LoadTile(Properties.Zoom, x, y);
            }
        }
    }

    /*//TODO CLEANUP
    void LoadTily()
    {
        GetTexture("terrain");
        GetTexture("satellite");

        BuildMesh(terrain);
        object material = satellite
    }*/



    async void GetTexture(string url, int tmp)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            UnityWebRequestAsyncOperation operation = uwr.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                RenderPlane(texture, tmp);
                GenerateTerrainMesh(texture, tmp);
            }
            else
            {
                Logger.Log(uwr.error);
            }
        }
    }

    private void RenderPlane(Texture2D texture, int tmp)
    {
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        int step = Properties.TileViewDistance * 2 + 1;
        gameObject.transform.position = new Vector3(10 * (tmp / step), 1, (-10 * tmp) - (-10 * step * (tmp / step)));
        gameObject.transform.eulerAngles = new Vector3(0, -180, 0);
        gameObject.GetComponent<Renderer>().material.mainTexture = texture;
    }

    private void GenerateTerrainMesh(Texture2D texture, int tmp)
    {
        int step = Properties.TileViewDistance * 2 + 1;
        int width = 64;
        int height = 64;
        int squareSize = 1;
        Vector2D originOffset = new Vector2D(width * (tmp / step), (-height * tmp) - (-height * step * (tmp / step)));

        // Setup the gameobject
        GameObject terrainTileGameObject = new GameObject($"{tmp}");
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
                int squareXOrigin = squareX * squareSize;
                int squareYOrigin = squareY * squareSize;

                // Setup vertices
                vertices[(currentSquare * 4) + 0] = new Vector3(squareXOrigin, 0, squareYOrigin);
                vertices[(currentSquare * 4) + 1] = new Vector3(squareXOrigin + squareSize, 0, squareYOrigin);
                vertices[(currentSquare * 4) + 2] = new Vector3(squareXOrigin + squareSize, 0, squareYOrigin + squareSize);
                vertices[(currentSquare * 4) + 3] = new Vector3(squareXOrigin, 0, squareYOrigin + squareSize);

                // Setup triangles
                triangles[(currentSquare * 6) + 0] = (currentSquare * 4) + 0;
                triangles[(currentSquare * 6) + 1] = (currentSquare * 4) + 2;
                triangles[(currentSquare * 6) + 2] = (currentSquare * 4) + 1;
                triangles[(currentSquare * 6) + 3] = (currentSquare * 4) + 0;
                triangles[(currentSquare * 6) + 4] = (currentSquare * 4) + 3;
                triangles[(currentSquare * 6) + 5] = (currentSquare * 4) + 2;

                // Setup uvs
                uvs[(currentSquare * 4) + 0] = new Vector2((squareXOrigin) / (float)width, (squareYOrigin) / (float)height);
                uvs[(currentSquare * 4) + 1] = new Vector2((squareXOrigin + squareSize) / (float)width, (squareYOrigin) / (float)height);
                uvs[(currentSquare * 4) + 2] = new Vector2((squareXOrigin + squareSize) / (float)width, (squareYOrigin + squareSize) / (float)height);
                uvs[(currentSquare * 4) + 3] = new Vector2((squareXOrigin) / (float)width, (squareYOrigin + squareSize) / (float)height);
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