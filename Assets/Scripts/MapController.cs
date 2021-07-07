using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    private string world = "lisbon"; // Loaded world //TODO maybe points to a json manifest thingy that tells which layers and where is the data to load? do we really need this?
    private Vector2 center = new Vector2();
    private float scale = 0;
    private int time = 0;
    private List<ILayer> layers = new List<ILayer>(); // Layers in the map

    // LatLon coords for the center of the map
    public Vector2 Center { get => center; set => center = value; }

    // Scale of the map
    public float Scale { get => scale; set => scale = value; }

    // Current time // TODO: do we really need this?
    public int Time { get => time; set => time = value; }
}
