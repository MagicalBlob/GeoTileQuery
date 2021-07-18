using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the Map
/// </summary>
public class MapController : MonoBehaviour
{
    private string world = "lisbon"; // Loaded world //TODO maybe points to a json manifest thingy that tells which layers and where is the data to load? do we really need this?
    private Vector2 center = new Vector2();
    private float scale = 0;
    private int time = 0;
    private List<ILayer> layers = new List<ILayer>(); // Layers in the map

    /// <summary>
    /// LatLon coords for the center of the map
    /// </summary>
    public Vector2 Center { get => center; set => center = value; }

    /// <summary>
    /// Scale of the map
    /// </summary>
    public float Scale { get => scale; set => scale = value; }

    /// <summary>
    /// Current time // TODO: do we really need this?
    /// </summary>
    public int Time { get => time; set => time = value; }
}
