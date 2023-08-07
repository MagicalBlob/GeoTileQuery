/*

    This place is a message... and part of a system of messages... pay attention to it!

    Sending this message was important to us. We considered ourselves to be a powerful culture.

    This place is not a place of honor... no highly esteemed deed is commemorated here... nothing valued is here.

    What is here was dangerous and repulsive to us. This message is a warning about danger.

    The danger is in a particular location... it increases towards a center... the center of danger is here... of a particular size and shape, and below us.

    The danger is still present, in your time, as it was in ours.

    The danger is to the body, and it can kill.

    The form of the danger is an emanation of energy.

    The danger is unleashed only if you substantially disturb this place physically. This place is best shunned and left uninhabited.

*/

using UnityEngine;
using System.Net.Http;
using System.Threading;
using System.Collections;
using UnityEngine.XR.ARFoundation;

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
    /// An HTTP client for making the requests (using a static client allows us to reuse the same connection)
    /// </summary>
    private static readonly HttpClientHandler handler = new HttpClientHandler()
    {
        Proxy = null,
        UseProxy = false,
    };
    public static readonly HttpClient client = new HttpClient(handler);

    /// <summary>
    /// A semaphore for limiting the number of concurrent tile requests
    /// </summary>
    public static SemaphoreSlim networkSemaphore;

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // Grab secrets
        MapboxAccessToken = Resources.Load<TextAsset>("Config/Secrets/Mapbox").text;

        // Set the HTTP client user agent
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"{Application.productName}/{Application.version} ({Application.identifier})");

        // Create the map
        Map = new Map();

        // Create the UI
        UI = new UIController(Map);
    }

    /// <summary>
    /// Called by Unity on the frame when a script is enabled just before any of the Update methods are called the first time
    /// </summary>
    private IEnumerator Start()
    {
        // Check which platform we're running on
        if (Application.platform != RuntimePlatform.Android)
        {
            // We're not running on Android
            networkSemaphore = new SemaphoreSlim(16, 16);
            Debug.Log("Not running on Android, AR will not be available");
        }
        else
        {
            // We're running on Android
            networkSemaphore = new SemaphoreSlim(6, 6);
            // Check if the device supports AR
            while ((ARSession.state == ARSessionState.None) || (ARSession.state == ARSessionState.CheckingAvailability))
            {
                Debug.Log("Checking Android device for AR support...");
                yield return ARSession.CheckAvailability();
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                // The device does not support AR
                Debug.Log("Android device does not support AR, AR will not be available");
            }
            else
            {
                // Device supports AR, check if it's installed
                while (ARSession.state == ARSessionState.NeedsInstall || ARSession.state == ARSessionState.Installing)
                {
                    // The device supports AR, but requires installation of the AR services
                    Debug.Log("Android device supports AR, but requires services to be installed, installing...");
                    yield return ARSession.Install();
                }

                // Check if the installation failed
                if (ARSession.state == ARSessionState.Unsupported)
                {
                    // The device supports AR, but the installation failed
                    Debug.Log("Android device supports AR, but required services failed to install, AR will not be available");
                }
                else
                {
                    // The device supports AR and the services are installed
                    Debug.Log($"Android device supports AR (State: {ARSession.state})");
                    GameObject.Find("/World").transform.Find("AR/AR Session").GetComponent<ARSession>().enabled = true; // Enable the AR session
                    UI.Toolbars.EnableARButton(); // Enable the AR button
                }
            }
        }

        // Start the map
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