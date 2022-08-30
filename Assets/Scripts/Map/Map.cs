using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Represents the Map
/// </summary>
public class Map
{
    /// <summary>
    /// The Map's GameObject representation
    /// </summary>
    public GameObject GameObject { get; }

    /// <summary>
    /// The data layers
    /// </summary>
    public Dictionary<string, ILayer> Layers { get; private set; }

    /// <summary>
    /// The tiles in the map
    /// </summary>
    public Dictionary<string, Tile> Tiles { get; private set; }

    /// <summary>
    /// The map's origin in Unity space
    /// </summary>
    public Vector2D Origin { get; private set; }

    /// <summary>
    /// The root GameObject for 2D mode
    /// </summary>
    private GameObject Root2D { get; }

    /// <summary>
    /// The root GameObject for AR mode
    /// </summary>
    private GameObject RootAR { get; }

    /// <summary>
    /// AR Manager for 2D images tracking
    /// </summary>
    private ARTrackedImageManager ARTrackedImageManager { get; }

    /// <summary>
    /// Constructs a new Map
    /// </summary>
    public Map()
    {
        GameObject = new GameObject("Tiles");
        Layers = new Dictionary<string, ILayer>();
        Tiles = new Dictionary<string, Tile>();
        Root2D = GameObject.Find("/Map/Root 2D");
        RootAR = GameObject.Find("/Map/Root AR");
        ARTrackedImageManager = GameObject.Find("/Map/Root AR/AR Session Origin").GetComponent<ARTrackedImageManager>();

        SwitchTo2DMode();

        // Add the data layers        
        IRasterRenderer defaultRasterRenderer = new DefaultRasterRenderer();
        IGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        Layers.Add("StamenWatercolor", new RasterLayer("StamenWatercolor", true, defaultRasterRenderer, "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new RasterLayer("StamenToner", false, defaultRasterRenderer, "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        Layers.Add("StamenTerrain", new RasterLayer("StamenTerrain", false, defaultRasterRenderer, "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        Layers.Add("OSMStandard", new RasterLayer("OSMStandard", false, defaultRasterRenderer, "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new RasterLayer("MapboxSatellite", false, defaultRasterRenderer, $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        Layers.Add("Bikepaths", new GeoJsonLayer("Bikepaths", true, defaultGeoJsonRenderer, "OBJECTID"));
        Layers.Add("Buildings", new GeoJsonLayer("Buildings", true, new BuildingRenderer(), "name"));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer("BuildingsLOD3", true, new PrefabRenderer(null), "id"));
        Layers.Add("Closures", new GeoJsonLayer("Closures", true, defaultGeoJsonRenderer, "id"));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer("Electrical_IP_especial", true, defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_PS", new GeoJsonLayer("Electrical_PS", true, defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_PTC", new GeoJsonLayer("Electrical_PTC", true, defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_PTD", new GeoJsonLayer("Electrical_PTD", true, defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer("Electrical_Subestacao", true, defaultGeoJsonRenderer, null));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer("Electrical_Troco_MT-AT", true, defaultGeoJsonRenderer, null));
        Layers.Add("Environment", new GeoJsonLayer("Environment", true, defaultGeoJsonRenderer, "id"));
        Layers.Add("Interventions", new GeoJsonLayer("Interventions", true, defaultGeoJsonRenderer, "OBJECTID"));
        Layers.Add("Lamps", new GeoJsonLayer("Lamps", true, new PrefabRenderer("Lamp"), "OBJECTID_1"));
        Layers.Add("Rails", new GeoJsonLayer("Rails", true, defaultGeoJsonRenderer, "OBJECTID"));
        Layers.Add("Roads", new GeoJsonLayer("Roads", true, new RoadRenderer(), "OBJECTID_1"));
        Layers.Add("Sidewalks", new GeoJsonLayer("Sidewalks", true, new SidewalkRenderer(), null));
        Layers.Add("Signs", new GeoJsonLayer("Signs", true, defaultGeoJsonRenderer, "IdSV_Posic"));
        Layers.Add("Trees", new GeoJsonLayer("Trees", true, new PrefabRenderer("Tree"), "OBJECTID"));

        // Set the origin
        Origin = GlobalMercator.LatLonToMeters(38.706808, -9.136164);
    }

    /// <summary>
    /// Load the map  
    /// </summary>
    public void Load()
    {
        int zoom = 16;

        Vector2Int originTile = GlobalMercator.MetersToGoogleTile(Origin, zoom);
        int tileLoadDistance = 1;
        for (int y = originTile.y - tileLoadDistance; y <= originTile.y + tileLoadDistance; y++)
        {
            for (int x = originTile.x - tileLoadDistance; x <= originTile.x + tileLoadDistance; x++)
            {
                // Only load tiles that haven't been loaded already
                if (!Tiles.ContainsKey($"{zoom}/{x}/{y}"))
                {
                    Tile tile = new Tile(this, zoom, x, y);
                    Tiles.Add(tile.Id, tile);
                    tile.Load();
                }
            }
        }
    }

    /// <summary>
    /// Move the map origin
    /// </summary>
    /// <param name="latitude">The new origin's latitude</param>
    /// <param name="longitude">The new origin's longitude</param>
    public void MoveOrigin(double latitude, double longitude)
    {
        // Update the origin
        Origin = GlobalMercator.LatLonToMeters(latitude, longitude);

        // Move the currently loaded tiles
        // TODO this

        // Load any new tiles around the origin
        Load();
    }

    /// <summary>
    /// Switch the map to 2D mode
    /// </summary>
    public void SwitchTo2DMode()
    {
        // Stop listening to the changed tracked images event since we aren't using AR
        ARTrackedImageManager.trackedImagesChanged -= OnARTrackedImagesChanged;

        // Update the currently active root
        RootAR.SetActive(false);
        Root2D.SetActive(true);

        // Set the tiles as a child of the 2D root and match their scale and position with it
        GameObject.transform.parent = Root2D.transform;
        GameObject.transform.SetPositionAndRotation(Root2D.transform.position, Root2D.transform.rotation);
    }

    /// <summary>
    /// Move the 2D camera to the given position and rotation
    /// </summary>
    /// <param name="position">The position to move the camera to</param>
    /// <param name="eulerAngles">The rotation to move the camera to</param>
    public void Move2DCamera(Vector3 position, Vector3 eulerAngles)
    {
        Root2D.transform.GetChild(0).position = position;
        Root2D.transform.GetChild(0).eulerAngles = eulerAngles;
    }

    /// <summary>
    /// Switch the map to AR mode
    /// </summary>
    public void SwitchToARMode()
    {
        // Update the currently active root
        Root2D.SetActive(false);
        RootAR.SetActive(true);

        // Start listening to the changed tracked images event so we can place the tiles
        ARTrackedImageManager.trackedImagesChanged += OnARTrackedImagesChanged;
    }

    /// <summary>
    /// React to the changed AR tracked images
    /// </summary>
    /// <param name="args">The changed AR tracked images event arguments</param>
    private void OnARTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (ARTrackedImage trackedImage in args.added)
        {
            OnARImageChanged(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.updated)
        {
            OnARImageChanged(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in args.removed)
        {
            Logger.LogWarning($"Image `{trackedImage.referenceImage.name}` removed!");
        }
    }

    /// <summary>
    /// React to a changed AR tracked image
    /// </summary>
    /// <param name="trackedImage">The AR tracked image that changed</param>
    private void OnARImageChanged(ARTrackedImage trackedImage)
    {
        // Check if it's the correct image
        if (trackedImage.referenceImage.name == "lisboa")
        {
            // Check if the image is being tracked
            if (trackedImage.trackingState != TrackingState.None)
            {
                // Check if the tracked image still isn't the map parent and if the image is being actively tracked
                if (GameObject.transform.parent != trackedImage.transform && trackedImage.trackingState == TrackingState.Tracking)
                {
                    // Set the tiles as a child of the tracked image
                    GameObject.transform.parent = trackedImage.transform;

                    //TODO remove debug
                    Logger.Log($"Setting tracked image `{trackedImage.referenceImage.name}` as the map parent. Tracking state: {trackedImage.trackingState}");
                    Transform t = GameObject.transform;
                    while (t != null)
                    {
                        Logger.Log($"{t.name} | {t.gameObject.activeInHierarchy} | {t.gameObject.activeSelf} | {t.gameObject.transform.position} | {t.gameObject.transform.localPosition}  | {t.gameObject.transform.rotation} | {t.gameObject.transform.localRotation} | {t.gameObject.transform.localScale}");
                        t = t.parent;
                    }
                }

                // Match position and rotation with parent
                GameObject.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
            }
        }
        else
        {
            Logger.LogWarning($"Detected `{trackedImage.referenceImage.name}` image instead!");
        }
    }
}
