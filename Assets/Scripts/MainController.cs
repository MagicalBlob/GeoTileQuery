using UnityEngine;
using System.Net.Http;
using System.Threading;

/// <summary>
/// The main app controller
/// </summary>
public class MainController : MonoBehaviour
{
    /// <summary>
    /// The map
    /// </summary>
    private Map Map { get; set; }

    /// <summary>
    /// The User Interface controller
    /// </summary>
    private UIController UI { get; set; }

    /// <summary>
    /// The Mapbox API access token
    /// </summary>
    public static string MapboxAccessToken { get; private set; }

    /// <summary>
    /// An HTTP client for making tile requests
    /// </summary>
    public static readonly HttpClient client = new HttpClient();

    /// <summary>
    /// A semaphore for limiting the number of concurrent tile requests
    /// </summary>
    public static readonly SemaphoreSlim networkSemaphore = new SemaphoreSlim(6, 6);

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // Grab the mapbox access token
        MapboxAccessToken = Resources.Load<TextAsset>("Config/secrets").text;

        // Create the map
        Map = new Map();

        // Create the UI
        UI = new UIController(Map);
    }

    /// <summary>
    /// Called by Unity on the frame when a script is enabled just before any of the Update methods are called the first time
    /// </summary>
    private void Start()
    {
        Map.Start();
    }

    /// <summary>
    /// Called by Unity every frame, if the MonoBehaviour is enabled
    /// </summary>
    private void Update()
    {
        // Update the map
        Map.Update();
        // Update the UI
        UI.Update();
    }
}