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
        Vector2Int tileCoords = GlobalMercator.MetersToTile(Properties.CenterX, Properties.CenterY, Properties.Zoom);
        Vector2Int googleTileCoords = GlobalMercator.GoogleTile(tileCoords.x, tileCoords.y, Properties.Zoom);
        for (int x = googleTileCoords.x - Properties.TileViewDistance; x <= googleTileCoords.x + Properties.TileViewDistance; x++)
        {
            for (int y = googleTileCoords.y - Properties.TileViewDistance; y <= googleTileCoords.y + Properties.TileViewDistance; y++)
            {
                string url = $"https://api.mapbox.com/v4/{Id}/{Properties.Zoom}/{x}/{y}.jpg70?access_token={MainController.MapboxAccessToken}";
                GetTexture(url);
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



    async void GetTexture(string url)
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
                Logger.Log(texture);
                GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                gameObject.transform.position = new Vector3(20, 1, 0);
                gameObject.transform.localScale = new Vector3(65, 1, 65);
                gameObject.transform.eulerAngles = new Vector3(0, -180, 0);
                gameObject.GetComponent<Renderer>().material.mainTexture = texture;

                Color color = texture.GetPixel(0, 0);
                // Convert from 0..1 to 0.255
                float R = color.r * 255;
                float G = color.g * 255;
                float B = color.b * 255;
                double height = -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1);
                Logger.Log(height);
            }
            else
            {
                Logger.Log(uwr.error);
            }
        }
    }
}