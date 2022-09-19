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
        Layers.Add("StamenWatercolor", new RasterLayer("StamenWatercolor", true, defaultRasterRenderer, "https://watercolormaps.collection.cooperhewitt.org/tile/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new RasterLayer("StamenToner", false, defaultRasterRenderer, "https://stamen-tiles-b.a.ssl.fastly.net/toner-background/{0}.png"));
        Layers.Add("StamenTerrain", new RasterLayer("StamenTerrain", false, defaultRasterRenderer, "http://stamen-tiles-c.a.ssl.fastly.net/terrain-background/{0}.png"));
        Layers.Add("OSMStandard", new RasterLayer("OSMStandard", false, defaultRasterRenderer, "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new RasterLayer("MapboxSatellite", false, defaultRasterRenderer, $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        Layers.Add("Bikepaths", new GeoJsonLayer("Bikepaths", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Bikepaths/{0}.geojson", "OBJECTID", new FeatureProperty[] { new FeatureProperty("OBJECTID", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("COD_VIA", FeaturePropertyType.String, "Código Via", "{1}: {0}"), new FeatureProperty("DESIGNACAO", FeaturePropertyType.String, "Designação", "{1}: {0}"), new FeatureProperty("HIERARQUIA", FeaturePropertyType.String, "Hierarquia", "{1}: {0}"), new FeatureProperty("EIXO", FeaturePropertyType.String, "Eixo", "{1}: {0}"), new FeatureProperty("TIPOLOGIA", FeaturePropertyType.String, "Tipologia", "{1}: {0}"), new FeatureProperty("SITUACAO", FeaturePropertyType.String, "Situação", "{1}: {0}"), new FeatureProperty("IDTIPO", FeaturePropertyType.String, "ID Tipo", "{1}: {0}"), new FeatureProperty("DTM_UPD", FeaturePropertyType.Date, "DateTime Updated", "{1}: {0}"), new FeatureProperty("FREGUESIA", FeaturePropertyType.String, "Freguesia", "{1}: {0}"), new FeatureProperty("NOME_PROJETO", FeaturePropertyType.String, "Nome Projeto", "{1}: {0}"), new FeatureProperty("ANO", FeaturePropertyType.String, "Ano", "{1}: {0}"), new FeatureProperty("COD_CICLOVIA", FeaturePropertyType.String, "Código Ciclovia", "{1}: {0}"), new FeatureProperty("NIVEL_SEGREGACAO", FeaturePropertyType.String, "Nível Segregação", "{1}: {0}"), new FeatureProperty("TIPO_INTERVENCAO", FeaturePropertyType.String, "Tipo Intervenção", "{1}: {0}"), new FeatureProperty("SE_ANNO_CAD_DATA", FeaturePropertyType.String, "SE Anno Cad Data", "{1}: {0}"), new FeatureProperty("GlobalID", FeaturePropertyType.String, "Global ID", "{1}: {0}"), new FeatureProperty("Shape__Length", FeaturePropertyType.Double, "Shape Length", "{1}: {0}") }));
        Layers.Add("Buildings", new GeoJsonLayer("Buildings", true, new BuildingRenderer(), "https://tese.flamino.eu/api/tiles/Buildings/{0}.geojson", "name", new FeatureProperty[] { new FeatureProperty("ruleFile", FeaturePropertyType.String, "Rule File", "{1}: {0}"), new FeatureProperty("startRule", FeaturePropertyType.String, "Start Rule", "{1}: {0}"), new FeatureProperty("name", FeaturePropertyType.String, "Name", "{1}: {0}"), new FeatureProperty("OBJECTID", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("Altiude", FeaturePropertyType.Double, "Altiude", "{1}: {0}"), new FeatureProperty("isHole", FeaturePropertyType.Bool, "Is Hole?", "{1}: {0}"), new FeatureProperty("SHAPE__ID", FeaturePropertyType.String, "Shape ID", "{1}: {0}"), new FeatureProperty("EXTRUDE", FeaturePropertyType.Double, "Extrude", "{1}: {0}m"), new FeatureProperty("Z_Min", FeaturePropertyType.Double, "Z Min", "{1}: {0}"), new FeatureProperty("Z_Max", FeaturePropertyType.Double, "Z Max", "{1}: {0}") }));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer("BuildingsLOD3", true, new PrefabRenderer(), "https://tese.flamino.eu/api/tiles/BuildingsLOD3/{0}.geojson", "id", new FeatureProperty[] { new FeatureProperty("id", FeaturePropertyType.String, "ID", "{1}: {0}"), new FeatureProperty("model", FeaturePropertyType.String, "Model", "{1}: {0}") }));
        Layers.Add("Closures", new GeoJsonLayer("Closures", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Closures/{0}.geojson", "id", new FeatureProperty[] { new FeatureProperty("id", FeaturePropertyType.String, "ID", "{1}: {0}"), new FeatureProperty("periodos_condicionamentos", FeaturePropertyType.String, "Periodos Condicionamentos", "{1}: {0}"), new FeatureProperty("morada", FeaturePropertyType.String, "Morada", "{1}: {0}"), new FeatureProperty("pedido", FeaturePropertyType.String, "Pedido", "{1}: {0}"), new FeatureProperty("freguesias", FeaturePropertyType.String, "Freguesias", "{1}: {0}"), new FeatureProperty("restricao_circulacao", FeaturePropertyType.String, "Restrição Circulação", "{1}: {0}"), new FeatureProperty("impacto", FeaturePropertyType.String, "Impacto", "{1}: {0}"), new FeatureProperty("motivo", FeaturePropertyType.String, "Motivo", "{1}: {0}"), new FeatureProperty("panfleto", FeaturePropertyType.String, "Panfleto", "{1}: {0}") }));
        Layers.Add("Electrical_IP_especial", new GeoJsonLayer("Electrical_IP_especial", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_IP_especial/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("IP_ESPECIA", FeaturePropertyType.String, "Iluminação Pública Especial", "{1}: {0}") }));
        Layers.Add("Electrical_PS", new GeoJsonLayer("Electrical_PS", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_PS/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("PS", FeaturePropertyType.String, "Posto de Seccionamento", "{1}: {0}") }));
        Layers.Add("Electrical_PTC", new GeoJsonLayer("Electrical_PTC", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_PTC/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("PTC", FeaturePropertyType.String, "Posto de Transformação de Cliente", "{1}: {0}") }));
        Layers.Add("Electrical_PTD", new GeoJsonLayer("Electrical_PTD", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_PTD/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("PTD", FeaturePropertyType.String, "Posto de Transformação de Distribuição", "{1}: {0}") }));
        Layers.Add("Electrical_Subestacao", new GeoJsonLayer("Electrical_Subestacao", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_Subestacao/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("SUBESTAÇÃO", FeaturePropertyType.String, "Subestação", "{1}: {0}") }));
        Layers.Add("Electrical_Troco_MT", new GeoJsonLayer("Electrical_Troco_MT-AT", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Electrical_Troco_MT-AT/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("TROÇO_AT_M", FeaturePropertyType.String, "Troço Alta Tensão - Média Tensão", "{1}: {0}") }));
        Layers.Add("Environment", new GeoJsonLayer("Environment", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Environment/{0}.geojson", "id", new FeatureProperty[] { new FeatureProperty("id", FeaturePropertyType.String, "ID", "{1}: {0}"), new FeatureProperty("avg", FeaturePropertyType.String, "Average", "{1}: {0}"), new FeatureProperty("date", FeaturePropertyType.Date, "Date", "{1}: {0}"), new FeatureProperty("dateStandard", FeaturePropertyType.String, "Date Standard", "{1}: {0}"), new FeatureProperty("value", FeaturePropertyType.Double, "Value", "{1}: {0}"), new FeatureProperty("unit", FeaturePropertyType.String, "Unit", "{1}: {0}"), new FeatureProperty("directions", FeaturePropertyType.String, "Directions", "{1}: {0}"), new FeatureProperty("address", FeaturePropertyType.String, "Address", "{1}: {0}") }));
        Layers.Add("Interventions", new GeoJsonLayer("Interventions", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Interventions/{0}.geojson", "OBJECTID", new FeatureProperty[] { new FeatureProperty("OBJECTID", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("ESTADO", FeaturePropertyType.String, "Estado", "{1}: {0}"), new FeatureProperty("INICIO_OBRA", FeaturePropertyType.Date, "Início de Obra", "{1}: {0}"), new FeatureProperty("CONCLUSAO_OBRA", FeaturePropertyType.Date, "Conclusão de Obra", "{1}: {0}"), new FeatureProperty("TIPO_INTERVENCAO", FeaturePropertyType.String, "Tipo de Intervenção", "{1}: {0}"), new FeatureProperty("COD_SIG", FeaturePropertyType.String, "Código SIG", "{1}: {0}"), new FeatureProperty("IDTIPO", FeaturePropertyType.String, "ID Tipo", "{1}: {0}"), new FeatureProperty("FREGUESIA", FeaturePropertyType.String, "Freguesia", "{1}: {0}"), new FeatureProperty("AREA_M2", FeaturePropertyType.Double, "Área M²", "{1}: {0}"), new FeatureProperty("NOME_ARRUAMENTO", FeaturePropertyType.String, "Nome do Arruamento", "{1}: {0}"), new FeatureProperty("GlobalID", FeaturePropertyType.String, "Global ID", "{1}: {0}"), new FeatureProperty("Shape__Area", FeaturePropertyType.Double, "Shape Area", "{1}: {0}"), new FeatureProperty("Shape__Length", FeaturePropertyType.Double, "Shape Length", "{1}: {0}") }));
        Layers.Add("Lamps", new GeoJsonLayer("Lamps", true, new PrefabRenderer("Lamp"), "https://tese.flamino.eu/api/tiles/Lamps/{0}.geojson", "OBJECTID_1", new FeatureProperty[] { new FeatureProperty("OBJECTID_1", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("FREGUESIA", FeaturePropertyType.String, "Freguesia", "{1}: {0}"), new FeatureProperty("TIPOLOGIA", FeaturePropertyType.String, "Tipologia", "{1}: {0}"), new FeatureProperty("COD_SIG", FeaturePropertyType.String, "Código SIG", "{1}: {0}"), new FeatureProperty("MORADA", FeaturePropertyType.String, "Morada", "{1}: {0}"), new FeatureProperty("GlobalID", FeaturePropertyType.String, "Global ID", "{1}: {0}") }));
        Layers.Add("Rails", new GeoJsonLayer("Rails", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Rails/{0}.geojson", "OBJECTID", new FeatureProperty[] { new FeatureProperty("OBJECTID", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("DESIG_ZONA", FeaturePropertyType.String, "Designação da Zona", "{1}: {0}"), new FeatureProperty("DESIG_SEGM", FeaturePropertyType.String, "Designação do Segmento", "{1}: {0}"), new FeatureProperty("DESIG_LINH", FeaturePropertyType.String, "Designação da Linha", "{1}: {0}"), new FeatureProperty("IDTIPO", FeaturePropertyType.String, "ID Tipo", "{1}: {0}"), new FeatureProperty("COD_SIG", FeaturePropertyType.String, "Código SIG", "{1}: {0}"), new FeatureProperty("LEGISLACAO", FeaturePropertyType.String, "Legislação", "{1}: {0}"), new FeatureProperty("ACTIVO", FeaturePropertyType.String, "Activo", "{1}: {0}"), new FeatureProperty("INFOPDM", FeaturePropertyType.String, "Info PDM", "{1}: {0}"), new FeatureProperty("ENTIDADE", FeaturePropertyType.String, "Entidade", "{1}: {0}"), new FeatureProperty("CONDICIONA", FeaturePropertyType.String, "Condiciona", "{1}: {0}"), new FeatureProperty("NOME", FeaturePropertyType.String, "Nome", "{1}: {0}"), new FeatureProperty("GlobalID", FeaturePropertyType.String, "Global ID", "{1}: {0}"), new FeatureProperty("Shape__Length", FeaturePropertyType.Double, "Shape Length", "{1}: {0}") }));
        Layers.Add("Roads", new GeoJsonLayer("Roads", true, new RoadRenderer(), "https://tese.flamino.eu/api/tiles/Roads/{0}.geojson", "OBJECTID_1", new FeatureProperty[] { new FeatureProperty("OBJECTID_1", FeaturePropertyType.String, "Object ID 1", "{1}: {0}"), new FeatureProperty("OBJECTID", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("COD_SIG", FeaturePropertyType.String, "Código SIG", "{1}: {0}"), new FeatureProperty("IDTIPO", FeaturePropertyType.String, "ID Tipo", "{1}: {0}"), new FeatureProperty("COD_VIA", FeaturePropertyType.String, "Código Via", "{1}: {0}"), new FeatureProperty("DESIGNACAO", FeaturePropertyType.String, "Designação", "{1}: {0}"), new FeatureProperty("ESTADO_VIA", FeaturePropertyType.String, "Estado da Via", "{1}: {0}"), new FeatureProperty("DTM_ADD", FeaturePropertyType.Date, "DateTime Added", "{1}: {0}"), new FeatureProperty("DTM_UPD", FeaturePropertyType.Date, "DateTime Updated", "{1}: {0}"), new FeatureProperty("COMPRIMENT", FeaturePropertyType.Double, "Comprimento", "{1}: {0}"), new FeatureProperty("Max_Z", FeaturePropertyType.Double, "Max Z", "{1}: {0}"), new FeatureProperty("Min_Z", FeaturePropertyType.Double, "Min Z", "{1}: {0}"), new FeatureProperty("Len_Up", FeaturePropertyType.Double, "Length Up", "{1}: {0}"), new FeatureProperty("Len_Down", FeaturePropertyType.Double, "Length Down", "{1}: {0}"), new FeatureProperty("H_Up", FeaturePropertyType.Double, "H Up", "{1}: {0}"), new FeatureProperty("H_Down", FeaturePropertyType.Double, "H Down", "{1}: {0}"), new FeatureProperty("Max_S_Up", FeaturePropertyType.Double, "Max S Up", "{1}: {0}"), new FeatureProperty("Max_S_Down", FeaturePropertyType.Double, "Max S Down", "{1}: {0}"), new FeatureProperty("Av_S_Up", FeaturePropertyType.Double, "Av S Up", "{1}: {0}"), new FeatureProperty("Av_S_Down", FeaturePropertyType.Double, "Av S Down", "{1}: {0}"), new FeatureProperty("slope", FeaturePropertyType.Double, "Slope", "{1}: {0}"), new FeatureProperty("GlobalID", FeaturePropertyType.String, "Global ID", "{1}: {0}"), new FeatureProperty("Shape__Length", FeaturePropertyType.Double, "Shape Length", "{1}: {0}") }));
        Layers.Add("Sidewalks", new GeoJsonLayer("Sidewalks", true, new SidewalkRenderer(), "https://tese.flamino.eu/api/tiles/Sidewalks/{0}.geojson", null, new FeatureProperty[] { new FeatureProperty("Id", FeaturePropertyType.String, "ID", "{1}: {0}"), new FeatureProperty("Tipologia", FeaturePropertyType.String, "Tipologia", "{1}: {0}"), new FeatureProperty("Length", FeaturePropertyType.Double, "Length", "{1}: {0}"), new FeatureProperty("AEstudo", FeaturePropertyType.String, "AEstudo", "{1}: {0}"), new FeatureProperty("Tipo", FeaturePropertyType.String, "Tipo", "{1}: {0}"), new FeatureProperty("REF_ID_3", FeaturePropertyType.String, "Ref ID 3", "{1}: {0}"), new FeatureProperty("METERS", FeaturePropertyType.Double, "Meters", "{1}: {0}"), new FeatureProperty("REF_ID_3_N", FeaturePropertyType.String, "Ref ID 3 N", "{1}: {0}"), new FeatureProperty("Lenght", FeaturePropertyType.Double, "Length", "{1}: {0}"), new FeatureProperty("COD_RUA", FeaturePropertyType.String, "Código Rua", "{1}: {0}"), new FeatureProperty("COD_SEGM", FeaturePropertyType.String, "Código Segmento", "{1}: {0}"), new FeatureProperty("COD_LADO", FeaturePropertyType.String, "Código Lado", "{1}: {0}"), new FeatureProperty("NREf", FeaturePropertyType.String, "N Ref", "{1}: {0}"), new FeatureProperty("layer", FeaturePropertyType.String, "Layer", "{1}: {0}"), new FeatureProperty("path", FeaturePropertyType.String, "Path", "{1}: {0}") }));
        Layers.Add("Signs", new GeoJsonLayer("Signs", true, defaultGeoJsonRenderer, "https://tese.flamino.eu/api/tiles/Signs/{0}.geojson", "IdSV_Posic", new FeatureProperty[] { new FeatureProperty("IdSV_Posic", FeaturePropertyType.String, "ID SV Posic", "{1}: {0}"), new FeatureProperty("CodViaCML", FeaturePropertyType.String, "Código Via CML", "{1}: {0}"), new FeatureProperty("Arruamento", FeaturePropertyType.String, "Arruamento", "{1}: {0}"), new FeatureProperty("NumeroPoli", FeaturePropertyType.String, "Número Policia", "{1}: {0}"), new FeatureProperty("NPCompleme", FeaturePropertyType.String, "NP Compleme", "{1}: {0}"), new FeatureProperty("Local", FeaturePropertyType.String, "Local", "{1}: {0}"), new FeatureProperty("Sinal", FeaturePropertyType.String, "Sinal", "{1}: {0}"), new FeatureProperty("Tipologia", FeaturePropertyType.String, "Tipologia", "{1}: {0}"), new FeatureProperty("NumeroChap", FeaturePropertyType.String, "Número de Chapas", "{1}: {0}"), new FeatureProperty("Suporte", FeaturePropertyType.String, "Suporte", "{1}: {0}"), new FeatureProperty("Chapa1", FeaturePropertyType.String, "Chapa 1", "{1}: {0}"), new FeatureProperty("TextoChapa", FeaturePropertyType.String, "Texto Chapa", "{1}: {0}"), new FeatureProperty("NSChapa1", FeaturePropertyType.String, "NS Chapa 1", "{1}: {0}"), new FeatureProperty("Chapa2", FeaturePropertyType.String, "Chapa 2", "{1}: {0}"), new FeatureProperty("TextoCha_1", FeaturePropertyType.String, "Texto Chapa 1", "{1}: {0}"), new FeatureProperty("NSChapa2", FeaturePropertyType.String, "NS Chapa 2", "{1}: {0}"), new FeatureProperty("Chapa3", FeaturePropertyType.String, "Chapa 3", "{1}: {0}"), new FeatureProperty("TextoCha_2", FeaturePropertyType.String, "Texto Chapa 2", "{1}: {0}"), new FeatureProperty("NSChapa3", FeaturePropertyType.String, "NS Chapa 3", "{1}: {0}"), new FeatureProperty("Chapa4", FeaturePropertyType.String, "Chapa 4", "{1}: {0}"), new FeatureProperty("TextoCha_3", FeaturePropertyType.String, "Texto Chapa 3", "{1}: {0}"), new FeatureProperty("NSChapa4", FeaturePropertyType.String, "NS Chapa 4", "{1}: {0}"), new FeatureProperty("Chapa5", FeaturePropertyType.String, "Chapa 5", "{1}: {0}"), new FeatureProperty("TextoCha_4", FeaturePropertyType.String, "Texto Chapa 4", "{1}: {0}"), new FeatureProperty("NSChapa5", FeaturePropertyType.String, "NS Chapa 5", "{1}: {0}"), new FeatureProperty("DataExecuc", FeaturePropertyType.Date, "Data Execução", "{1}: {0}") }));
        Layers.Add("Trees", new GeoJsonLayer("Trees", true, new PrefabRenderer("Tree"), "https://tese.flamino.eu/api/tiles/Trees/{0}.geojson", "OBJECTID", new FeatureProperty[] { new FeatureProperty("OBJECTID", FeaturePropertyType.String, "Object ID", "{1}: {0}"), new FeatureProperty("COD_SIG_NEW", FeaturePropertyType.String, "Código SIG New", "{1}: {0}"), new FeatureProperty("COD_SIG", FeaturePropertyType.String, "Código SIG", "{1}: {0}"), new FeatureProperty("MORADA", FeaturePropertyType.String, "Morada", "{1}: {0}"), new FeatureProperty("ESPECIE_VA", FeaturePropertyType.String, "Espécie VA", "{1}: {0}"), new FeatureProperty("PAP", FeaturePropertyType.String, "PAP", "{1}: {0}"), new FeatureProperty("MANUTENCAO", FeaturePropertyType.String, "Manutenção", "{1}: {0}"), new FeatureProperty("OCUPACAO", FeaturePropertyType.String, "Ocupação", "{1}: {0}"), new FeatureProperty("LOCAL", FeaturePropertyType.String, "Local", "{1}: {0}"), new FeatureProperty("TIPOLOGIA", FeaturePropertyType.String, "Tipologia", "{1}: {0}"), new FeatureProperty("FREG_2012", FeaturePropertyType.String, "Freguesia 2012", "{1}: {0}"), new FeatureProperty("NOME_VULGA", FeaturePropertyType.String, "Nome Vulgar", "{1}: {0}"), new FeatureProperty("GlobalID", FeaturePropertyType.String, "Global ID", "{1}: {0}") }));

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
