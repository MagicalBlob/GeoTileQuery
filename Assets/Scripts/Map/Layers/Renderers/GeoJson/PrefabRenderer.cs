using UnityEngine;

/// <summary>
/// Prefab GeoJSON renderer
/// </summary>
public class PrefabRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Whether to use the feature's properties to determine the prefab to use
    /// </summary>
    private bool checkGeoJson;

    /// <summary>
    /// Name of the model to load
    /// </summary>
    private string model;

    /// <summary>
    /// Constructs a new PrefabRenderer
    /// </summary>
    /// <remarks>
    /// Since no model is specified, it will lookup the model name in the GeoJSON properties under the key "model"
    /// </remarks>
    public PrefabRenderer()
    {
        this.checkGeoJson = true;
        this.model = null;
    }

    /// <summary>
    /// Constructs a new PrefabRenderer
    /// </summary>
    /// <param name="model">Name of the model to load</param>
    public PrefabRenderer(string model)
    {
        this.checkGeoJson = false;
        this.model = model;
    }

    public void RenderNode(GeoJsonTileLayer tileLayer, Feature feature, Position coordinates)
    {
        double terrainHeightOffset = 0;
        if (tileLayer.Tile.Map.ElevatedTerrain)
        {
            // If we're using the elevation data, get the height at the node's position to offset it (assumes node's at sea level in dataset)
            terrainHeightOffset = tileLayer.Tile.GetHeight(GlobalMercator.LatLonToMeters(coordinates.y, coordinates.x));
        }

        // Render Node with an existing model instead
        double x = coordinates.GetRelativeX(tileLayer.Tile.Bounds.Min.X);
        double y = terrainHeightOffset + coordinates.GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
        double z = coordinates.GetRelativeY(tileLayer.Tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

        // Get model name
        if (checkGeoJson)
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
            node.transform.rotation = feature.GameObject.transform.rotation;
        }
        else
        {
            Debug.LogWarning($"[PrefabRenderer] {tileLayer.FullId}: Unable to find resource 'Prefabs/{model}'");
        }
    }

    public void RenderEdge(GeoJsonTileLayer tileLayer, Feature feature, Position[] coordinates)
    {
        Debug.LogWarning($"[PrefabRenderer] {tileLayer.FullId}: Tried to render an Edge!");
    }

    public void RenderArea(GeoJsonTileLayer tileLayer, Feature feature, Position[][] coordinates)
    {
        Debug.LogWarning($"[PrefabRenderer] {tileLayer.FullId}: Tried to render an Area!");
    }
}
