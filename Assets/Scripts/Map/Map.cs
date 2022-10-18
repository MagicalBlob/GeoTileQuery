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
    private GameObject Map2D { get; }

    /// <summary>
    /// The root GameObject for AR mode
    /// </summary>
    private GameObject MapAR { get; }

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
        Transform mapTransform = GameObject.Find("/Map").transform;
        Map2D = mapTransform.Find("2D").gameObject;
        MapAR = mapTransform.Find("AR").gameObject;
        ARTrackedImageManager = MapAR.transform.Find("AR Session Origin").GetComponent<ARTrackedImageManager>();

        // Add the data layers        
        IRasterRenderer defaultRasterRenderer = new DefaultRasterRenderer();
        IGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        // https://tese.flamino.eu/api/tiles/
        // http://192.168.68.114:8123/
        Layers.Add("StamenWatercolor", new RasterLayer("StamenWatercolor", "Stamen - Watercolor", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultRasterRenderer, "https://stamen-tiles-b.a.ssl.fastly.net/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new RasterLayer("StamenToner", "Stamen - Toner", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, false, defaultRasterRenderer, "https://stamen-tiles-c.a.ssl.fastly.net/toner/{0}.png"));
        Layers.Add("StamenTerrain", new RasterLayer("StamenTerrain", "Stamen - Terrain", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, false, defaultRasterRenderer, "https://stamen-tiles-c.a.ssl.fastly.net/terrain/{0}.png"));
        Layers.Add("OSMStandard", new RasterLayer("OSMStandard", "OpenStreetMap", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, false, defaultRasterRenderer, "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new RasterLayer("MapboxSatellite", "Mapbox Satellite", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, false, defaultRasterRenderer, $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        Layers.Add("Bikepaths", new GeoJsonLayer("Bikepaths", "Bikepaths", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Bikepaths/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{1}: {0}"), new StringFeatureProperty("COD_VIA", "Código Via", "{1}: {0}"), new StringFeatureProperty("DESIGNACAO", "Designação", "{1}: {0}"), new StringFeatureProperty("HIERARQUIA", "Hierarquia", "{1}: {0}"), new StringFeatureProperty("EIXO", "Eixo", "{1}: {0}"), new StringFeatureProperty("TIPOLOGIA", "Tipologia", "{1}: {0}"), new StringFeatureProperty("SITUACAO", "Situação", "{1}: {0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{1}: {0}"), new DateFeatureProperty("DTM_UPD", "DateTime Updated", "{1}: {0}"), new StringFeatureProperty("FREGUESIA", "Freguesia", "{1}: {0}"), new StringFeatureProperty("NOME_PROJETO", "Nome Projeto", "{1}: {0}"), new StringFeatureProperty("ANO", "Ano", "{1}: {0}"), new StringFeatureProperty("COD_CICLOVIA", "Código Ciclovia", "{1}: {0}"), new StringFeatureProperty("NIVEL_SEGREGACAO", "Nível Segregação", "{1}: {0}"), new StringFeatureProperty("TIPO_INTERVENCAO", "Tipo Intervenção", "{1}: {0}"), new StringFeatureProperty("SE_ANNO_CAD_DATA", "SE Anno Cad Data", "{1}: {0}"), new StringFeatureProperty("GlobalID", "Global ID", "{1}: {0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{1}: {0}") }));
        Layers.Add("Buildings", new GeoJsonLayer("Buildings", "Buildings", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, new BuildingRenderer(), "http://192.168.68.114:8123/Buildings/{0}.geojson", "name", new IFeatureProperty[] { new StringFeatureProperty("ruleFile", "Rule File", "{1}: {0}"), new StringFeatureProperty("startRule", "Start Rule", "{1}: {0}"), new StringFeatureProperty("name", "Name", "{1}: {0}"), new StringFeatureProperty("OBJECTID", "Object ID", "{1}: {0}"), new NumberFeatureProperty("Altiude", "Altiude", "{1}: {0}"), new BooleanFeatureProperty("isHole", "Is Hole?", "{1}: {0}"), new StringFeatureProperty("SHAPE__ID", "Shape ID", "{1}: {0}"), new NumberFeatureProperty("EXTRUDE", "Extrude", "{1}: {0}m"), new NumberFeatureProperty("Z_Min", "Z Min", "{1}: {0}"), new NumberFeatureProperty("Z_Max", "Z Max", "{1}: {0}") }));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer("BuildingsLOD3", "Buildings LOD3", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, new PrefabRenderer(), "http://192.168.68.114:8123/BuildingsLOD3/{0}.geojson", "id", new IFeatureProperty[] { new StringFeatureProperty("id", "ID", "{1}: {0}"), new StringFeatureProperty("model", "Model", "{1}: {0}") }));
        Layers.Add("Closures", new GeoJsonLayer("Closures", "Closures", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Closures/{0}.geojson", "id", new IFeatureProperty[] { new StringFeatureProperty("id", "ID", "{1}: {0}"), new StringFeatureProperty("periodos_condicionamentos", "Periodos Condicionamentos", "{1}: {0}"), new StringFeatureProperty("morada", "Morada", "{1}: {0}"), new StringFeatureProperty("pedido", "Pedido", "{1}: {0}"), new StringFeatureProperty("freguesias", "Freguesias", "{1}: {0}"), new StringFeatureProperty("restricao_circulacao", "Restrição Circulação", "{1}: {0}"), new StringFeatureProperty("impacto", "Impacto", "{1}: {0}"), new StringFeatureProperty("motivo", "Motivo", "{1}: {0}"), new StringFeatureProperty("panfleto", "Panfleto", "{1}: {0}") }));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer("Electrical_IP_especial", "Electrical Iluminação Pública Especial", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Electrical_IP_especial/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("IP_ESPECIA", "Iluminação Pública Especial", "{1}: {0}") }));
        Layers.Add("Electrical_PS", new GeoJsonLayer("Electrical_PS", "Electrical PS", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Electrical_PS/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("PS", "Posto de Seccionamento", "{1}: {0}") }));
        Layers.Add("Electrical_PTC", new GeoJsonLayer("Electrical_PTC", "Electrical PTC", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Electrical_PTC/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("PTC", "Posto de Transformação de Cliente", "{1}: {0}") }));
        Layers.Add("Electrical_PTD", new GeoJsonLayer("Electrical_PTD", "Electrical PTD", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Electrical_PTD/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("PTD", "Posto de Transformação de Distribuição", "{1}: {0}") }));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer("Electrical_Subestacao", "Electrical Subestação", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Electrical_Subestacao/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("SUBESTAÇÃO", "Subestação", "{1}: {0}") }));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer("Electrical_Troco_MT-AT", "Electrical Troço MT-AT", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Electrical_Troco_MT-AT/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("TROÇO_AT_M", "Troço Alta Tensão - Média Tensão", "{1}: {0}") }));
        Layers.Add("Environment", new GeoJsonLayer("Environment", "Environment", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Environment/{0}.geojson", "id", new IFeatureProperty[] { new StringFeatureProperty("id", "ID", "{1}: {0}"), new StringFeatureProperty("avg", "Average", "{1}: {0}"), new DateFeatureProperty("date", "Date", "{1}: {0}"), new StringFeatureProperty("dateStandard", "Date Standard", "{1}: {0}"), new NumberFeatureProperty("value", "Value", "{1}: {0}"), new StringFeatureProperty("unit", "Unit", "{1}: {0}"), new StringFeatureProperty("directions", "Directions", "{1}: {0}"), new StringFeatureProperty("address", "Address", "{1}: {0}") }));
        Layers.Add("Interventions", new GeoJsonLayer("Interventions", "Interventions", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Interventions/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{1}: {0}"), new CategoryFeatureProperty("ESTADO", "Estado", "{1}: {0}", new string[] { "Em curso", "Previsto", "Realizado", "Proposto", "Suspenso", "Em lançamento", "Pendente" }), new DateFeatureProperty("INICIO_OBRA", "Início de Obra", "{1}: {0}"), new DateFeatureProperty("CONCLUSAO_OBRA", "Conclusão de Obra", "{1}: {0}"), new StringFeatureProperty("TIPO_INTERVENCAO", "Tipo de Intervenção", "{1}: {0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{1}: {0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{1}: {0}"), new StringFeatureProperty("FREGUESIA", "Freguesia", "{1}: {0}"), new NumberFeatureProperty("AREA_M2", "Área M²", "{1}: {0}"), new StringFeatureProperty("NOME_ARRUAMENTO", "Nome do Arruamento", "{1}: {0}"), new StringFeatureProperty("GlobalID", "Global ID", "{1}: {0}"), new NumberFeatureProperty("Shape__Area", "Shape Area", "{1}: {0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{1}: {0}") }));
        Layers.Add("Lamps", new GeoJsonLayer("Lamps", "Lamps", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, new PrefabRenderer("Lamp"), "http://192.168.68.114:8123/Lamps/{0}.geojson", "OBJECTID_1", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID_1", "Object ID", "{1}: {0}"), new StringFeatureProperty("FREGUESIA", "Freguesia", "{1}: {0}"), new StringFeatureProperty("TIPOLOGIA", "Tipologia", "{1}: {0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{1}: {0}"), new StringFeatureProperty("MORADA", "Morada", "{1}: {0}"), new StringFeatureProperty("GlobalID", "Global ID", "{1}: {0}") }));
        Layers.Add("Rails", new GeoJsonLayer("Rails", "Rails", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Rails/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{1}: {0}"), new StringFeatureProperty("DESIG_ZONA", "Designação da Zona", "{1}: {0}"), new StringFeatureProperty("DESIG_SEGM", "Designação do Segmento", "{1}: {0}"), new StringFeatureProperty("DESIG_LINH", "Designação da Linha", "{1}: {0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{1}: {0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{1}: {0}"), new StringFeatureProperty("LEGISLACAO", "Legislação", "{1}: {0}"), new StringFeatureProperty("ACTIVO", "Activo", "{1}: {0}"), new StringFeatureProperty("INFOPDM", "Info PDM", "{1}: {0}"), new StringFeatureProperty("ENTIDADE", "Entidade", "{1}: {0}"), new StringFeatureProperty("CONDICIONA", "Condiciona", "{1}: {0}"), new StringFeatureProperty("NOME", "Nome", "{1}: {0}"), new StringFeatureProperty("GlobalID", "Global ID", "{1}: {0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{1}: {0}") }));
        Layers.Add("Roads", new GeoJsonLayer("Roads", "Roads", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, new RoadRenderer(), "http://192.168.68.114:8123/Roads/{0}.geojson", "OBJECTID_1", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID_1", "Object ID 1", "{1}: {0}"), new StringFeatureProperty("OBJECTID", "Object ID", "{1}: {0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{1}: {0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{1}: {0}"), new StringFeatureProperty("COD_VIA", "Código Via", "{1}: {0}"), new StringFeatureProperty("DESIGNACAO", "Designação", "{1}: {0}"), new StringFeatureProperty("ESTADO_VIA", "Estado da Via", "{1}: {0}"), new DateFeatureProperty("DTM_ADD", "DateTime Added", "{1}: {0}"), new DateFeatureProperty("DTM_UPD", "DateTime Updated", "{1}: {0}"), new NumberFeatureProperty("COMPRIMENT", "Comprimento", "{1}: {0}"), new NumberFeatureProperty("Max_Z", "Max Z", "{1}: {0}"), new NumberFeatureProperty("Min_Z", "Min Z", "{1}: {0}"), new NumberFeatureProperty("Len_Up", "Length Up", "{1}: {0}"), new NumberFeatureProperty("Len_Down", "Length Down", "{1}: {0}"), new NumberFeatureProperty("H_Up", "H Up", "{1}: {0}"), new NumberFeatureProperty("H_Down", "H Down", "{1}: {0}"), new NumberFeatureProperty("Max_S_Up", "Max S Up", "{1}: {0}"), new NumberFeatureProperty("Max_S_Down", "Max S Down", "{1}: {0}"), new NumberFeatureProperty("Av_S_Up", "Av S Up", "{1}: {0}"), new NumberFeatureProperty("Av_S_Down", "Av S Down", "{1}: {0}"), new NumberFeatureProperty("slope", "Slope", "{1}: {0}"), new StringFeatureProperty("GlobalID", "Global ID", "{1}: {0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{1}: {0}") }));
        Layers.Add("Sidewalks", new GeoJsonLayer("Sidewalks", "Sidewalks", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, new SidewalkRenderer(), "http://192.168.68.114:8123/Sidewalks/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("Id", "ID", "{1}: {0}"), new StringFeatureProperty("Tipologia", "Tipologia", "{1}: {0}"), new NumberFeatureProperty("Length", "Length", "{1}: {0}"), new StringFeatureProperty("AEstudo", "AEstudo", "{1}: {0}"), new StringFeatureProperty("Tipo", "Tipo", "{1}: {0}"), new StringFeatureProperty("REF_ID_3", "Ref ID 3", "{1}: {0}"), new NumberFeatureProperty("METERS", "Meters", "{1}: {0}"), new StringFeatureProperty("REF_ID_3_N", "Ref ID 3 N", "{1}: {0}"), new NumberFeatureProperty("Lenght", "Length", "{1}: {0}"), new StringFeatureProperty("COD_RUA", "Código Rua", "{1}: {0}"), new StringFeatureProperty("COD_SEGM", "Código Segmento", "{1}: {0}"), new StringFeatureProperty("COD_LADO", "Código Lado", "{1}: {0}"), new StringFeatureProperty("NREf", "N Ref", "{1}: {0}"), new StringFeatureProperty("layer", "Layer", "{1}: {0}"), new StringFeatureProperty("path", "Path", "{1}: {0}") }));
        Layers.Add("Signs", new GeoJsonLayer("Signs", "Signs", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, defaultGeoJsonRenderer, "http://192.168.68.114:8123/Signs/{0}.geojson", "IdSV_Posic", new IFeatureProperty[] { new StringFeatureProperty("IdSV_Posic", "ID SV Posic", "{1}: {0}"), new StringFeatureProperty("CodViaCML", "Código Via CML", "{1}: {0}"), new StringFeatureProperty("Arruamento", "Arruamento", "{1}: {0}"), new StringFeatureProperty("NumeroPoli", "Número Policia", "{1}: {0}"), new StringFeatureProperty("NPCompleme", "NP Compleme", "{1}: {0}"), new StringFeatureProperty("Local", "Local", "{1}: {0}"), new StringFeatureProperty("Sinal", "Sinal", "{1}: {0}"), new StringFeatureProperty("Tipologia", "Tipologia", "{1}: {0}"), new StringFeatureProperty("NumeroChap", "Número de Chapas", "{1}: {0}"), new StringFeatureProperty("Suporte", "Suporte", "{1}: {0}"), new StringFeatureProperty("Chapa1", "Chapa 1", "{1}: {0}"), new StringFeatureProperty("TextoChapa", "Texto Chapa", "{1}: {0}"), new StringFeatureProperty("NSChapa1", "NS Chapa 1", "{1}: {0}"), new StringFeatureProperty("Chapa2", "Chapa 2", "{1}: {0}"), new StringFeatureProperty("TextoCha_1", "Texto Chapa 1", "{1}: {0}"), new StringFeatureProperty("NSChapa2", "NS Chapa 2", "{1}: {0}"), new StringFeatureProperty("Chapa3", "Chapa 3", "{1}: {0}"), new StringFeatureProperty("TextoCha_2", "Texto Chapa 2", "{1}: {0}"), new StringFeatureProperty("NSChapa3", "NS Chapa 3", "{1}: {0}"), new StringFeatureProperty("Chapa4", "Chapa 4", "{1}: {0}"), new StringFeatureProperty("TextoCha_3", "Texto Chapa 3", "{1}: {0}"), new StringFeatureProperty("NSChapa4", "NS Chapa 4", "{1}: {0}"), new StringFeatureProperty("Chapa5", "Chapa 5", "{1}: {0}"), new StringFeatureProperty("TextoCha_4", "Texto Chapa 4", "{1}: {0}"), new StringFeatureProperty("NSChapa5", "NS Chapa 5", "{1}: {0}"), new DateFeatureProperty("DataExecuc", "Data Execução", "{1}: {0}") }));
        Layers.Add("Trees", new GeoJsonLayer("Trees", "Trees", "DESCRIPTION", "<a href=\"https://example.com\">DATA_SOURCE</a>", System.DateTime.UnixEpoch, true, new PrefabRenderer("Tree"), "http://192.168.68.114:8123/Trees/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{1}: {0}"), new StringFeatureProperty("COD_SIG_NEW", "Código SIG New", "{1}: {0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{1}: {0}"), new StringFeatureProperty("MORADA", "Morada", "{1}: {0}"), new StringFeatureProperty("ESPECIE_VA", "Espécie VA", "{1}: {0}"), new StringFeatureProperty("PAP", "PAP", "{1}: {0}"), new StringFeatureProperty("MANUTENCAO", "Manutenção", "{1}: {0}"), new StringFeatureProperty("OCUPACAO", "Ocupação", "{1}: {0}"), new StringFeatureProperty("LOCAL", "Local", "{1}: {0}"), new StringFeatureProperty("TIPOLOGIA", "Tipologia", "{1}: {0}"), new StringFeatureProperty("FREG_2012", "Freguesia 2012", "{1}: {0}"), new StringFeatureProperty("NOME_VULGA", "Nome Vulgar", "{1}: {0}"), new StringFeatureProperty("GlobalID", "Global ID", "{1}: {0}") }));

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
        Load();
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        // * crickets *
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
        if (ZoomLevel > 20)
        {
            Debug.LogWarning("Zoom level is too high, setting it to 20");
            ZoomLevel = 20;
        }

        // Move the 2D camera to the new zoom level
        Update2DCameraHeight();

        // Load the new tiles
        Load();

        // Unload all the old tiles
        Unload();
    }

    /// <summary>
    /// Applies the layers' filters to the map
    /// </summary>
    public void ApplyFilters()
    {
        foreach (Tile tile in Tiles.Values)
        {
            foreach (ITileLayer tileLayer in tile.Layers.Values)
            {
                if (tileLayer is IFilterableTileLayer)
                {
                    ((IFilterableTileLayer)tileLayer).ApplyFilters();
                }
            }
        }
    }

    /// <summary>
    /// Applies a given layer's filters to the map
    /// </summary>
    /// <param name="layerId">The ID of the layer to apply the filters to</param>
    public void ApplyFilters(string layerId)
    {
        foreach (Tile tile in Tiles.Values)
        {
            if (tile.Layers.TryGetValue(layerId, out ITileLayer tileLayer))
            {
                if (tileLayer is IFilterableTileLayer)
                {
                    ((IFilterableTileLayer)tileLayer).ApplyFilters();
                }
            }
        }
    }

    /// <summary>
    /// Update the 2D camera height according to the current zoom level
    /// </summary>
    private void Update2DCameraHeight()
    {
        // Grab the camera
        Transform cameraTransform = Map2D.transform.GetChild(0);
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
    /// Get the coordinates (lat/lon) of the given point on the map
    /// </summary>
    /// <param name="point">The point on the map</param>
    /// <returns>The coordinates of the given point (lat/lon)</returns>
    public Vector2D WorldToLatLon(Vector3 point)
    {
        return GlobalMercator.MetersToLatLon(Center + new Vector2D(point.x, point.z));
    }

    /// <summary>
    /// Move the 2D camera to the given position and rotation
    /// </summary>
    /// <param name="position">The position to move the camera to</param>
    /// <param name="eulerAngles">The rotation to move the camera to</param>
    public void Test2DCamera(Vector3 position, Vector3 eulerAngles)
    {
        Map2D.transform.GetChild(0).position = position;
        Map2D.transform.GetChild(0).eulerAngles = eulerAngles;
    }

    /// <summary>
    /// Switch the map to 2D mode
    /// </summary>
    public void SwitchTo2DMode()
    {
        // Stop listening to the changed tracked images event since we aren't using AR
        ARTrackedImageManager.trackedImagesChanged -= OnARTrackedImagesChanged;

        // Update the currently active root
        MapAR.SetActive(false);
        Map2D.SetActive(true);

        // Set the tiles as a child of the 2D root and match their scale and position with it
        GameObject.transform.parent = Map2D.transform;
        GameObject.transform.SetPositionAndRotation(Map2D.transform.position, Map2D.transform.rotation);

        // Update the 2D camera
        Update2DCameraHeight();
    }

    /// <summary>
    /// Switch the map to AR mode
    /// </summary>
    public void SwitchToARMode()
    {
        // Update the currently active root
        Map2D.SetActive(false);
        MapAR.SetActive(true);

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
