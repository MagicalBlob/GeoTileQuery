using UnityEngine;

/// <summary>
/// Prefab GeoJSON renderer
/// </summary>
public class PrefabRenderer : IGeoJsonRenderer
{
    /// <summary>
    /// Name of the model to load
    /// </summary>
    private string model;

    /// <summary>
    /// Constructs a new PrefabRenderer
    /// </summary>
    /// <param name="model">Name of the model to load. If null, it tries to find the value in the GeoJSON</param>
    public PrefabRenderer(string model)
    {
        this.model = model;
    }

    public void RenderNode(GeoJsonTileLayer tileLayer, Feature feature, Position coordinates)
    {
        // Render Node with an existing model instead
        double x = coordinates.GetRelativeX(tileLayer.Tile.Bounds.Min.X);
        double y = coordinates.GetRelativeZ(); // GeoJSON uses z for height, while Unity uses y
        double z = coordinates.GetRelativeY(tileLayer.Tile.Bounds.Min.Y); // GeoJSON uses z for height, while Unity uses y

        // Get model name
        if (model == null)
        {
            model = feature.GetPropertyAsString("model");
            Logger.LogError($"No model name specified for prefab renderer, trying to get it from the GeoJSON properties. Model name: {model}. // TODO This will cause issues if the model name is not the same for all features, but I'm too tired to fix it right now");
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
            Logger.LogWarning($"[PrefabRenderer] {tileLayer.FullId}: Unable to find resource 'Prefabs/{model}'");
        }
    }

    public void RenderEdge(GeoJsonTileLayer tileLayer, Feature feature, Position[] coordinates)
    {
        Logger.LogWarning($"[PrefabRenderer] {tileLayer.FullId}: Tried to render an Edge!");
    }

    public void RenderArea(GeoJsonTileLayer tileLayer, Feature feature, Position[][] coordinates)
    {
        Logger.LogWarning($"[PrefabRenderer] {tileLayer.FullId}: Tried to render an Area!");
    }
}
