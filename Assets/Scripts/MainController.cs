using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Http;
using System.Threading;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// The main app controller
/// </summary>
public class MainController : MonoBehaviour
{
    /// <summary>
    /// The map
    /// </summary>
    public Map map;

    public GameObject ModeRoot2D;
    public GameObject ModeRootAR;

    private bool arMode = false;

    public Button loadButton;
    public Button arButton;

    public ARTrackedImageManager arTrackedImageManager;
    public GameObject testPrefab;

    public static readonly HttpClient client = new HttpClient();
    public static readonly SemaphoreSlim networkSemaphore = new SemaphoreSlim(6, 6);

    public static string MapboxAccessToken { get; private set; }

    private void Awake()
    {
        MapboxAccessToken = Resources.Load<TextAsset>("Config/secrets").text;
    }

    private void Start()
    {
        map = new Map();

        SwitchTo2DMode();

        loadButton.onClick.AddListener(LoadButtonClicked);
        arButton.onClick.AddListener(ARButtonClicked);
    }

    private void SwitchToARMode()
    {
        ModeRoot2D.SetActive(false);
        ModeRootAR.SetActive(true);
        arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        map.GameObject.transform.parent = ModeRootAR.transform; // TODO the tracked image should be the parent instead of the AR root
    }

    private void SwitchTo2DMode()
    {
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        ModeRootAR.SetActive(false);
        ModeRoot2D.SetActive(true);

        map.GameObject.transform.parent = ModeRoot2D.transform;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            Logger.Log($"Image `{trackedImage.referenceImage.name}` added");
            GameObject testObject = GameObject.Instantiate(testPrefab);
            testObject.transform.parent = trackedImage.transform;
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            Logger.Log($"Image `{trackedImage.referenceImage.name}` updated");
        }

        foreach (ARTrackedImage trackedImage in args.removed)
        {
            Logger.Log($"Image `{trackedImage.referenceImage.name}` removed");
        }
    }

    private void LoadButtonClicked()
    {
        Logger.Log("Loading data!");
        try
        {
            map.Render();
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }

    private void ARButtonClicked()
    {
        try
        {
            if (arMode)
            {
                SwitchTo2DMode();
                arMode = false;
                Logger.Log("Switched to 2D mode!");
            }
            else
            {
                SwitchToARMode();
                arMode = true;
                Logger.Log("Switched to AR mode!");
            }
        }
        catch (Exception e)
        {
            Logger.LogException(e); // TODO make it so all Unity console output goes through our logger (https://docs.unity3d.com/ScriptReference/ILogger.html)
        }
    }
}