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
    /// The map's zoom level
    /// </summary>
    public int ZoomLevel { get; private set; }

    /// <summary>
    /// The map's origin in Unity space
    /// </summary>
    public Vector2D Center { get; private set; }

    /// <summary>
    /// Number of tiles to be loaded in each direction relative to the origin (0 loads only the origin tile)
    /// </summary>
    /// <remarks>
    /// The total number of tiles loaded will be (2 * X + 1) * (2 * Y + 1)
    /// </remarks>
    private Vector2Int TileLoadDistance { get; set; }

    /// <summary>
    /// The current tile generation
    /// </summary>
    private uint CurrentTileGeneration { get; set; }

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

        // Add the data layers        
        IRasterRenderer defaultRasterRenderer = new DefaultRasterRenderer();
        IGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        Layers.Add("StamenWatercolor", new RasterLayer("StamenWatercolor", false, defaultRasterRenderer, "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new RasterLayer("StamenToner", true, defaultRasterRenderer, "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        Layers.Add("StamenTerrain", new RasterLayer("StamenTerrain", false, defaultRasterRenderer, "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        Layers.Add("OSMStandard", new RasterLayer("OSMStandard", false, defaultRasterRenderer, "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new RasterLayer("MapboxSatellite", false, defaultRasterRenderer, $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        Layers.Add("Bikepaths", new GeoJsonLayer("Bikepaths", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Bikepaths/{0}.geojson", "OBJECTID"));
        Layers.Add("Buildings", new GeoJsonLayer("Buildings", true, new BuildingRenderer(), "https://tese.flamino.eu/api/tiles/Buildings/{0}.geojson", "name"));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer("BuildingsLOD3", true, new PrefabRenderer(), "https://tese.flamino.eu/api/tiles/BuildingsLOD3/{0}.geojson", "id"));
        Layers.Add("Closures", new GeoJsonLayer("Closures", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Closures/{0}.geojson", "id"));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer("Electrical_IP_especial", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_IP_especial/{0}.geojson", null));
        Layers.Add("Electrical_PS", new GeoJsonLayer("Electrical_PS", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_PS/{0}.geojson", null));
        Layers.Add("Electrical_PTC", new GeoJsonLayer("Electrical_PTC", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_PTC/{0}.geojson", null));
        Layers.Add("Electrical_PTD", new GeoJsonLayer("Electrical_PTD", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_PTD/{0}.geojson", null));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer("Electrical_Subestacao", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_Subestacao/{0}.geojson", null));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer("Electrical_Troco_MT-AT", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_Troco_MT-AT/{0}.geojson", null));
        Layers.Add("Environment", new GeoJsonLayer("Environment", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Environment/{0}.geojson", "id"));
        Layers.Add("Interventions", new GeoJsonLayer("Interventions", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Interventions/{0}.geojson", "OBJECTID"));
        Layers.Add("Lamps", new GeoJsonLayer("Lamps", true, new PrefabRenderer("Lamp"), "https://tese.flamino.eu/api/tiles/Lamps/{0}.geojson", "OBJECTID_1"));
        Layers.Add("Rails", new GeoJsonLayer("Rails", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Rails/{0}.geojson", "OBJECTID"));
        Layers.Add("Roads", new GeoJsonLayer("Roads", true, new RoadRenderer(), "https://tese.flamino.eu/api/tiles/Roads/{0}.geojson", "OBJECTID_1"));
        Layers.Add("Sidewalks", new GeoJsonLayer("Sidewalks", true, new SidewalkRenderer(), "https://tese.flamino.eu/api/tiles/Sidewalks/{0}.geojson", null));
        Layers.Add("Signs", new GeoJsonLayer("Signs", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Signs/{0}.geojson", "IdSV_Posic"));
        Layers.Add("Trees", new GeoJsonLayer("Trees", true, new PrefabRenderer("Tree"), "https://tese.flamino.eu/api/tiles/Trees/{0}.geojson", "OBJECTID"));

        // Set the map's initial zoom level and center, as well as the tile load distance
        ZoomLevel = 17;
        Center = GlobalMercator.LatLonToMeters(38.704802, -9.137878);
        TileLoadDistance = new Vector2Int(4, 3);
    }

    /// <summary>
    /// Starts the map
    /// </summary>
    public void Start()
    {
        SwitchTo2DMode();
        Update2DCameraHeight();
        Load();
    }

    private float tileCleanup = 0.0f;
    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        tileCleanup += Time.unscaledDeltaTime;
        if (tileCleanup > 60.0f)
        {
            tileCleanup = 0.0f;
            int oldCount = Tiles.Count;
            Unload();
            Debug.Log($"Auto unloading old tiles. {oldCount - Tiles.Count} tiles unloaded.");
        }
    }

    /// <summary>
    /// Load the map tiles
    /// </summary>
    private void Load()
    {
        CurrentTileGeneration += 1;
        Vector2Int originTile = GlobalMercator.MetersToGoogleTile(Center, ZoomLevel);
        for (int tileY = originTile.y - TileLoadDistance.y; tileY <= originTile.y + TileLoadDistance.y; tileY++)
        {
            for (int tileX = originTile.x - TileLoadDistance.x; tileX <= originTile.x + TileLoadDistance.x; tileX++)
            {
                Tile existingTile;
                if (!Tiles.TryGetValue($"{ZoomLevel}/{tileX}/{tileY}", out existingTile))
                {
                    // Only load tiles that haven't been loaded already
                    Tile tile = new Tile(this, ZoomLevel, tileX, tileY, CurrentTileGeneration);
                    Tiles.Add(tile.Id, tile);
                    _ = tile.LoadAsync();
                }
                else
                {
                    // The tile was loaded already
                    existingTile.Generation = CurrentTileGeneration; // Update its generation
                }
            }
        }
    }

    /// <summary>
    /// Unload the old map tiles
    /// </summary>
    private void Unload()
    {
        List<string> tilesToRemove = new List<string>();

        // Iterate over all tiles and unload the old ones
        foreach (Tile tile in Tiles.Values)
        {
            if (tile.Generation < CurrentTileGeneration)
            {
                // Hide the tile
                tile.GameObject.SetActive(false);

                // Unload the tile
                tile.Unload();
                tilesToRemove.Add(tile.Id);
            }
        }

        // Remove the unloaded tiles from the dictionary
        foreach (string tileId in tilesToRemove)
        {
            Tiles.Remove(tileId);
        }
    }

    /// <summary>
    /// Move the map center according to the given delta
    /// </summary>
    /// <param name="delta">The delta vector to move the center by (in meters)</param>
    public void MoveCenter(Vector2D delta)
    {
        // Update the center
        Center += delta;

        // Move the currently loaded tiles
        foreach (Tile tile in Tiles.Values)
        {
            tile.Move(-delta);
        }

        // Load any new tiles around the center and update the generation of the existing ones
        Load();

        // Unload all the old tiles
        Unload();
    }

    /// <summary>
    /// Move the map center to the given position (lat/lon)
    /// </summary>
    /// <param name="latitude">The new center's latitude</param>
    /// <param name="longitude">The new center's longitude</param>
    public void MoveCenter(double latitude, double longitude)
    {
        // Convert the given coordinates to meters, calculate the difference and move the center
        MoveCenter(GlobalMercator.LatLonToMeters(latitude, longitude) - Center);
    }

    /// <summary>
    /// Zoom the map by the given amount
    /// </summary>
    /// <param name="amount">The amount to zoom by</param>
    public void Zoom(int amount)
    {
        // Update the zoom level
        ZoomLevel += amount;
        if (ZoomLevel < 0)
        {
            Debug.LogWarning("Zoom level cannot be less than 0");
            ZoomLevel = 0;
        }
        if (ZoomLevel > 17)
        {
            Debug.LogWarning("Zoom level is too high, setting it to 17");
            ZoomLevel = 17;
        }

        // Move the 2D camera to the new zoom level
        Update2DCameraHeight();

        // Load the new tiles
        Load();

        // Unload all the old tiles
        Unload();
    }

    /// <summary>
    /// Update the 2D camera height according to the current zoom level
    /// </summary>
    private void Update2DCameraHeight()
    {
        // Grab the camera
        Transform cameraTransform = Root2D.transform.GetChild(0);
        Camera camera = cameraTransform.GetComponent<Camera>();
        // Calculate the bounds of the origin tile
        Vector2Int originTile = GlobalMercator.MetersToGoogleTile(Center, ZoomLevel);
        Bounds bounds = GlobalMercator.GoogleTileBounds(originTile.x, originTile.y, ZoomLevel);
        // Calculate what is the screen height in meters if we keep the tile size on screen constant
        double meterHeight = (camera.pixelHeight * bounds.Height) / GlobalMercator.TileSize;
        // Calculate the distance from the camera to the center of the tile at sea level, such that the tile size on screen is constant
        double cameraHeight = (meterHeight / 2) / System.Math.Tan((camera.fieldOfView * System.Math.PI) / 360);
        // Move the camera to the new position
        cameraTransform.position = new Vector3(cameraTransform.position.x, (float)cameraHeight, cameraTransform.position.z);
        cameraTransform.eulerAngles = new Vector3(90, 0, 0);
        // Update the clip planes to make sure the map is always visible
        camera.nearClipPlane = (float)(cameraHeight / 100);
        camera.farClipPlane = (float)(cameraHeight * 2);
    }

    /// <summary>
    /// Move the 2D camera to the given position and rotation
    /// </summary>
    /// <param name="position">The position to move the camera to</param>
    /// <param name="eulerAngles">The rotation to move the camera to</param>
    public void Test2DCamera(Vector3 position, Vector3 eulerAngles)
    {
        Root2D.transform.GetChild(0).position = position;
        Root2D.transform.GetChild(0).eulerAngles = eulerAngles;
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
            Debug.LogWarning($"Image `{trackedImage.referenceImage.name}` removed!");
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

                    Debug.Log($"Setting tracked image `{trackedImage.referenceImage.name}` as the map parent. Tracking state: {trackedImage.trackingState}");
                    Transform t = GameObject.transform;
                    while (t != null)
                    {
                        Debug.Log($"{t.name} | {t.gameObject.activeInHierarchy} | {t.gameObject.activeSelf} | {t.gameObject.transform.position} | {t.gameObject.transform.localPosition}  | {t.gameObject.transform.rotation} | {t.gameObject.transform.localRotation} | {t.gameObject.transform.localScale}");
                        t = t.parent;
                    }
                }

                // Match position and rotation with parent
                GameObject.transform.SetPositionAndRotation(trackedImage.transform.position, trackedImage.transform.rotation);
            }
        }
        else
        {
            Debug.LogWarning($"Detected `{trackedImage.referenceImage.name}` image instead!");
        }
    }
}
