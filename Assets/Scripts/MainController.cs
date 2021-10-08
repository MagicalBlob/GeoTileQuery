using UnityEngine;
using UnityEngine.UI;
using System;
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
    public Map map;

    public Button testButton;

    public static readonly HttpClient client = new HttpClient();
    public static readonly SemaphoreSlim networkSemaphore = new SemaphoreSlim(6, 6);

    public static string MapboxAccessToken { get; private set; }

    private void Start()
    {
        MapboxAccessToken = Resources.Load<TextAsset>("Config/secrets").text;

        map = new Map();

        testButton.onClick.AddListener(ButtonClicked);
    }

    private void ButtonClicked()
    {
        Logger.Log("Button pressed!");
        try
        {
            map.Render();
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }
}