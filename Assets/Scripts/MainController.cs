using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net.Http;
using System.Threading;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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
    public Button testButton;

    public ARTrackedImageManager arTrackedImageManager;
    public GameObject testPrefab;

    public static readonly HttpClient client = new HttpClient();
    public static readonly SemaphoreSlim networkSemaphore = new SemaphoreSlim(6, 6);

    public static string MapboxAccessToken { get; private set; }

    private UIController ui;

    private void Awake()
    {
        MapboxAccessToken = Resources.Load<TextAsset>("Config/secrets").text;
        ui = new UIController();
    }

    private void Start()
    {
        map = new Map();

        SwitchTo2DMode();

        loadButton.onClick.AddListener(LoadButtonClicked);
        arButton.onClick.AddListener(ARButtonClicked);
        testButton.onClick.AddListener(TestButtonClicked);
    }

    private void Update()
    {
        ui.Render();
    }

    private void SwitchToARMode()
    {
        ModeRoot2D.SetActive(false);
        ModeRootAR.SetActive(true);

        arTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged; // Start listening to the changed tracked images event so we can place the map
    }

    private void SwitchTo2DMode()
    {
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged; // Stop listening to the changed tracked images event since we aren't using AR

        ModeRootAR.SetActive(false);
        ModeRoot2D.SetActive(true);

        map.GameObject.transform.parent = ModeRoot2D.transform; // Set the map as a child of the 2D Mode root
        map.GameObject.transform.SetPositionAndRotation(ModeRoot2D.transform.position, ModeRoot2D.transform.rotation); // Match position and rotation with parent
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            OnImageChanged(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            OnImageChanged(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.removed)
        {
            Logger.LogWarning($"Image `{trackedImage.referenceImage.name}` removed!");
        }
    }

    private void OnImageChanged(ARTrackedImage trackedImage)
    {
        // Check if it's the correct image
        if (trackedImage.referenceImage.name == "lisboa")
        {
            // Check if the image is being tracked
            if (trackedImage.trackingState != TrackingState.None)
            {
                // Check if the tracked image still isn't the map parent and if the image is being actively tracked
                if (map.GameObject.transform.parent != trackedImage.transform && trackedImage.trackingState == TrackingState.Tracking)
                {
                    // Set the map as a child of the tracked image
                    map.GameObject.transform.parent = trackedImage.transform;

                    //TODO remove debug
                    Logger.Log($"Setting tracked image `{trackedImage.referenceImage.name}` as the map parent. Tracking state: {trackedImage.trackingState}");
                    Transform t = map.GameObject.transform;
                    while (t != null)
                    {
                        Logger.Log($"{t.name} | {t.gameObject.activeInHierarchy} | {t.gameObject.activeSelf} | {t.gameObject.transform.position} | {t.gameObject.transform.localPosition}  | {t.gameObject.transform.rotation} | {t.gameObject.transform.localRotation} | {t.gameObject.transform.localScale}");
                        t = t.parent;
                    }
                }
                // Match position and rotation with parent
                map.GameObject.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
            }
        }
        else
        {
            Logger.LogWarning($"Detected `{trackedImage.referenceImage.name}` image instead!");
        }
    }

    private void LoadButtonClicked()
    {
        Logger.Log("Loading data!");
        try
        {
            map.Load();
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

    int currentCameraAngle = 0;
    private void TestButtonClicked()
    {
        Logger.Log("Test button clicked!");
        switch (currentCameraAngle)
        {
            case 0:
                ModeRoot2D.transform.GetChild(0).position = new Vector3(350, 20, 235);
                ModeRoot2D.transform.GetChild(0).eulerAngles = new Vector3(20, 325, 0);
                currentCameraAngle = 1;
                break;
            case 1:
                ModeRoot2D.transform.GetChild(0).position = new Vector3(420, 120, 125);
                ModeRoot2D.transform.GetChild(0).eulerAngles = new Vector3(30, 310, 0);
                currentCameraAngle = 2;
                break;
            case 2:
                ModeRoot2D.transform.GetChild(0).position = new Vector3(600, 270, -60);
                ModeRoot2D.transform.GetChild(0).eulerAngles = new Vector3(40, 310, 0);
                currentCameraAngle = 3;
                break;
            case 3:
                ModeRoot2D.transform.GetChild(0).position = new Vector3(350, 760, -980);
                ModeRoot2D.transform.GetChild(0).eulerAngles = new Vector3(35, 330, 0);
                currentCameraAngle = 0;
                break;
        }
    }
}