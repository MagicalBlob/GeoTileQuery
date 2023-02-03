using System;
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
    /// The Points of Interest in the Map
    /// </summary>
    public List<PointOfInterest> POIs { get; private set; }

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
    /// The direction the map is facing relative to north
    /// </summary>
    public int Direction { get; private set; }

    /// <summary>
    /// The map's pitch
    /// </summary>
    public int Pitch { get; private set; }

    private bool _filtered = false;
    /// <summary>
    /// Whether the map is filtered
    /// </summary>
    public bool Filtered
    {
        get
        {
            return _filtered;
        }
        set
        {
            // Set the backing field
            _filtered = value;

            // Make the layers filter settings match the current map state
            foreach (ILayer layer in Layers.Values)
            {
                if (layer is IFilterableLayer)
                {
                    IFilterableLayer filterableLayer = layer as IFilterableLayer;
                    filterableLayer.Filtered = Filtered;
                }
            }

            // Apply the filters
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
    }

    private bool _elevatedTerrain = false;
    /// <summary>
    /// Whether the map uses the elevation data for the terrain or not (is flat)
    /// </summary>
    public bool ElevatedTerrain
    {
        get
        {
            return _elevatedTerrain;
        }
        set
        {
            // Set the backing field
            _elevatedTerrain = value;

            // Update the terrain elevation for all the tiles already in the map
            int count = 0;
            foreach (Tile tile in Tiles.Values)
            {
                foreach (ITileLayer tileLayer in tile.Layers.Values)
                {
                    _ = tileLayer.ApplyTerrainAsync(count);
                }
                count++;
            }
        }
    }

    /// <summary>
    /// The minimum elevation of the map in meters
    /// </summary>
    public double MinElevation { get; private set; }

    /// <summary>
    /// The maximum elevation of the map (in meters)
    /// </summary>
    public double MaxElevation { get; private set; }

    /// <summary>
    /// The minimum zoom level of the map
    /// </summary>
    public int MinZoomLevel { get; private set; }

    /// <summary>
    /// The maximum zoom level of the map
    /// </summary>
    public int MaxZoomLevel { get; private set; }

    /// <summary>
    /// The map's ruler
    /// </summary>
    public Ruler Ruler { get; }

    /// <summary>
    /// Size of a tile in pixels
    /// </summary>
    public const int TileSize = GlobalMercator.TileSize;

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
    /// The camera for 2D mode
    /// </summary>
    private Camera Camera2D { get; }

    /// <summary>
    /// The height of the camera in 2D mode
    /// </summary>
    public double Camera2DHeight { get; private set; }

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
        GameObject = new GameObject("Map");
        Layers = new Dictionary<string, ILayer>();
        POIs = new List<PointOfInterest>();
        Tiles = new Dictionary<string, Tile>();
        Ruler = new Ruler(this);
        Transform worldTransform = GameObject.Find("/World").transform;
        Map2D = worldTransform.Find("2D").gameObject;
        Camera2D = Map2D.transform.Find("Camera").GetComponent<Camera>();
        MapAR = worldTransform.Find("AR").gameObject;
        ARTrackedImageManager = MapAR.transform.Find("AR Session Origin").GetComponent<ARTrackedImageManager>();

        // Add the data layers        
        IRasterRenderer defaultRasterRenderer = new DefaultRasterRenderer();
        IGeoJsonRenderer defaultGeoJsonRenderer = new DefaultGeoJsonRenderer();
        bool useLocalhost = true;
        string geoJsonBaseUrl = useLocalhost ? "http://localhost:8123" : "http://192.168.68.114:8123";// : "https://tese.flamino.eu/api/tiles/";
        Layers.Add("OSMStandard", new RasterLayer(this, "OSMStandard", "OpenStreetMap", "OpenStreetMap standard style", "OpenStreetMap", DateTime.MinValue, false, defaultRasterRenderer, "https://tile.openstreetmap.org/{0}.png"));
        Layers.Add("MapboxSatellite", new RasterLayer(this, "MapboxSatellite", "Satellite", "Satellite and Aerial imagery", "Mapbox", DateTime.MinValue, false, defaultRasterRenderer, $"https://api.mapbox.com/v4/mapbox.satellite/{{0}}.jpg?access_token={MainController.MapboxAccessToken}"));
        Layers.Add("StamenWatercolor", new RasterLayer(this, "StamenWatercolor", "Watercolor", "A watercolor styled design, reminiscent of hand drawn maps", "Stamen Design", DateTime.MinValue, true, defaultRasterRenderer, "https://stamen-tiles-b.a.ssl.fastly.net/watercolor/{0}.jpg"));
        Layers.Add("StamenToner", new RasterLayer(this, "StamenToner", "Toner", "High-contrast B&W maps for data mashups and exploring river meanders and coastal zones", "Stamen Design", DateTime.MinValue, false, defaultRasterRenderer, "https://stamen-tiles-c.a.ssl.fastly.net/toner/{0}.png"));
        Layers.Add("StamenTerrain", new RasterLayer(this, "StamenTerrain", "Terrain", "Terrain maps featuring hill shading and natural vegetation color", "Stamen Design", DateTime.MinValue, false, defaultRasterRenderer, "https://stamen-tiles-c.a.ssl.fastly.net/terrain/{0}.png"));
        Layers.Add("Bikepaths", new GeoJsonLayer(this, "Bikepaths", "Bikepaths", "Location of the cycle routes in Lisbon", "Câmara Municipal de Lisboa", new DateTime(2022, 11, 18), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/Bikepaths/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{0}"), new StringFeatureProperty("COD_VIA", "Código Via", "{0}"), new StringFeatureProperty("DESIGNACAO", "Designação", "{0}"), new CategoryFeatureProperty("HIERARQUIA", "Hierarquia", "{0}", new string[] { "Local", "Principal", "Complementar" }), new CategoryFeatureProperty("EIXO", "Eixo", "{0}", new string[] { "Circular Exterior", "Central", "Marginal", "Olivais - Beato", "Baixa - Aeroporto", "Panorâmico", "Alcântara - Pontinha" }), new CategoryFeatureProperty("TIPOLOGIA", "Tipologia", "{0}", new string[] { "30+Bici", "Pista Ciclável Bidirecional", "Percurso Ciclopedonal", "Pista Ciclável Unidirecional", "Trilho", "Contrassentido", "Faixa Ciclável", "Zona de Coexistência" }), new CategoryFeatureProperty("SITUACAO", "Situação", "{0}", new string[] { "Executado" }), new CategoryFeatureProperty("ENTIDADE_R", "Entidade Responsável", "{0}", new string[] { "CML", "EMEL", "CML/DMEVAE", "CML/DMMT", "CML/DMU/DEP", "CML/DMM", "CML/DMM; EMEL", "CML/DMM; CML/DMMC/DIOA", "ISA; CML/DMAEVCE", "CML/DMAEVCE; EMEL", "CML/DMAEVCE", "SRU", "CML/DMM; UCT", "CML/DMAEVCE; SRU", "CML/UCT/UITCH", "CML; ATL" }), new NumberFeatureProperty("COMPRIMENT", "Comprimento", "{0}m"), new CategoryFeatureProperty("IDTIPO", "ID Tipo", "{0}", new string[] { "9196" }), new CategoryFeatureProperty("USR_ADD", "Adicionado por", "{0}", new string[] { "ricardo.baptista" }), new DateFeatureProperty("DTM_ADD", "Data de adição", "{0}"), new CategoryFeatureProperty("USR_UPD", "Atualizado por", "{0}", new string[] { "ricardo.baptista" }), new DateFeatureProperty("DTM_UPD", "Data de atualização", "{0}"), new CategoryFeatureProperty("FREGUESIA", "Freguesia", "{0}", new string[] { "Ajuda", "Alcântara", "Alvalade", "Areeiro", "Arroios", "Avenidas Novas", "Beato", "Belém", "Benfica", "Campo de Ourique", "Campolide", "Carnide", "Estrela", "Lumiar", "Marvila", "Misericórdia", "Olivais", "Parque das Nações", "Penha de França", "Santa Clara", "Santa Maria Maior", "Santo António", "São Domingos de Benfica", "São Vicente" }), new StringFeatureProperty("ZONAMENTO", "Zonamento", "{0}"), new StringFeatureProperty("NOME_PROJE", "Nome do Projeto", "{0}"), new CategoryFeatureProperty("ANO", "Ano", "{0}", new string[] { "Sem dados", "2022", "2021", "2020", "2019", "2018", "2017", "2016", "2015", "2014", "2013", "2012", "2011", "2010", "2009", "2001" }), new StringFeatureProperty("SGPI", "SGPI", "{0}"), new NumberFeatureProperty("COMP_KM", "Comprimento", "{0}km"), new StringFeatureProperty("COD_CICLOV", "Código Ciclovia", "{0}"), new CategoryFeatureProperty("NIVEL_SEGR", "Nível Segregação", "{0}", new string[] { "Via banalizada (coexistência)", "Pista ciclável (separação física)", "Faixa ciclável (separação visual)" }), new CategoryFeatureProperty("TIPO_INTER", "Tipo Intervenção", "{0}", new string[] { "Definitiva", "Provisória" }), new StringFeatureProperty("GlobalID", "Global ID", "{0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{0}m") }));
        Layers.Add("Buildings", new GeoJsonLayer(this, "Buildings", "Buildings", "Buildings of Lisbon with low detail (extruded from floor plans)", "Câmara Municipal de Lisboa", new DateTime(2020, 07, 28), true, new BuildingRenderer(), geoJsonBaseUrl + "/Buildings/{0}.geojson", "name", new IFeatureProperty[] { new StringFeatureProperty("ruleFile", "Rule File", "{0}"), new StringFeatureProperty("startRule", "Start Rule", "{0}"), new StringFeatureProperty("name", "Name", "{0}"), new StringFeatureProperty("OBJECTID", "Object ID", "{0}"), new NumberFeatureProperty("Altiude", "Altitude", "{0}m"), new BooleanFeatureProperty("isHole", "Is Hole?", "{0}"), new StringFeatureProperty("SHAPE__ID", "Shape ID", "{0}"), new NumberFeatureProperty("EXTRUDE", "Extrude", "{0}m"), new NumberFeatureProperty("Z_Min", "Z Min", "{0}"), new NumberFeatureProperty("Z_Max", "Z Max", "{0}") }));
        Layers.Add("BuildingsLOD3", new GeoJsonLayer(this, "BuildingsLOD3", "Detailed Buildings", "Buildings of Lisbon with higher detailed geometry", "Câmara Municipal de Lisboa", new DateTime(2020, 07, 28), true, new PrefabRenderer(), geoJsonBaseUrl + "/BuildingsLOD3/{0}.geojson", "id", new IFeatureProperty[] { new StringFeatureProperty("id", "ID", "{0}"), new StringFeatureProperty("model", "Model", "{0}") }));
        Layers.Add("Electrical", new GeoJsonLayer(this, "Electrical", "Electrical Grid", "The electricity distribution network cables, substations, along with other infrastructures", "EDP Distribuição", new DateTime(2020, 07, 07), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/Electrical/{0}.geojson", null, new IFeatureProperty[] { new CategoryFeatureProperty("type", "Tipo de Infraestrutura", "{0}", new string[] { "IP Especial", "PS", "PTC", "PTD", "Subestação", "Troço AT-MT" }) }));
        Layers.Add("Environment", new GeoJsonLayer(this, "Environment", "Environment", "Environment sensor data (Air quality, Noise, Weather and Traffic)", "Câmara Municipal de Lisboa", new DateTime(2022, 11, 27), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/Environment/{0}.geojson", "id", new IFeatureProperty[] { new StringFeatureProperty("id", "ID", "{0}"), new StringFeatureProperty("avg", "Average", "{0}"), new DateFeatureProperty("date", "Date", "{0}"), new NumberFeatureProperty("value", "Value", "{0}"), new CategoryFeatureProperty("unit", "Unit", "{0}", new string[] { "", "%", "dB(A)", "km/h", "mbar", "mg/m3", "mm", "Veículos", "µg/m3", "º", "ºC" }), new StringFeatureProperty("address", "Address", "{0}"), new StringFeatureProperty("from_to_direction", "From-To Direction", "{0}"), new NumberFeatureProperty("from_to_P", "From-To P", "{0}"), new NumberFeatureProperty("from_to_M", "From-To M", "{0}"), new NumberFeatureProperty("from_to_L", "From-To L", "{0}"), new NumberFeatureProperty("from_to_total", "From-To Total", "{0}"), new StringFeatureProperty("to_from_direction", "To-From Direction", "{0}"), new NumberFeatureProperty("to_from_P", "To-From P", "{0}"), new NumberFeatureProperty("to_from_M", "To-From M", "{0}"), new NumberFeatureProperty("to_from_L", "To-From L", "{0}"), new NumberFeatureProperty("to_from_total", "To-From Total", "{0}") }));
        Layers.Add("Interventions", new GeoJsonLayer(this, "Interventions", "Interventions", "Various kinds of interventions carried out in public spaces", "Câmara Municipal de Lisboa", new DateTime(2022, 11, 10), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/Interventions/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{0}"), new CategoryFeatureProperty("ESTADO", "Estado", "{0}", new string[] { "Em curso", "Previsto", "Realizado", "Proposto", "Suspenso", "Em lançamento", "Pendente" }), new DateFeatureProperty("INICIO_OBRA", "Início da Obra", "{0}"), new DateFeatureProperty("CONCLUSAO_OBRA", "Conclusão da Obra", "{0}"), new CategoryFeatureProperty("TIPO_INTERVENCAO", "Tipo de Intervenção", "{0}", new string[] { "Acessibilidades", "Campo desportivo", "Demolições", "Equipamento Rodoviário", "Equipamentos", "Espaço Público (Manutenção)", "Espaço Público (manutenção)", "Espaço Público (projetos)", "Espaço Público (Requalificação)", "Espaços verdes", "Mercados e feiras", "Mobiliário urbano", "Passeios", "Pavimentar", "Pavimentos rodoviários", "Sinalização rodoviária", "Áreas expectantes" }), new StringFeatureProperty("COD_SIG", "Código SIG", "{0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{0}"), new StringFeatureProperty("FREGUESIA", "Freguesia", "{0}"), new NumberFeatureProperty("AREA_M2", "Área", "{0}m²"), new StringFeatureProperty("NOME_ARRUAMENTO", "Nome do Arruamento", "{0}"), new StringFeatureProperty("GlobalID", "Global ID", "{0}"), new NumberFeatureProperty("Shape__Area", "Shape Area", "{0}m²"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{0}m") }));
        Layers.Add("Lamps", new GeoJsonLayer(this, "Lamps", "Lamps", "Public illumination equipment", "Câmara Municipal de Lisboa", new DateTime(2022, 10, 28), true, new PrefabRenderer("Lamp"), geoJsonBaseUrl + "/Lamps/{0}.geojson", "OBJECTID_1", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID_1", "Object ID", "{0}"), new StringFeatureProperty("FREGUESIA", "Freguesia", "{0}"), new CategoryFeatureProperty("TIPOLOGIA", "Tipo de Candeeiro", "{0}", new string[] { "Coluna", "Comando", "Composto", "Consola", "Equipamento", "Fuste", "Poste", "Quadro" }), new StringFeatureProperty("COD_SIG", "Código SIG", "{0}"), new StringFeatureProperty("MORADA", "Morada", "{0}"), new StringFeatureProperty("GlobalID", "Global ID", "{0}") }));
        Layers.Add("Rails", new GeoJsonLayer(this, "Rails", "Rails", "Rail network", "Câmara Municipal de Lisboa", new DateTime(2022, 11, 02), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/Rails/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{0}"), new CategoryFeatureProperty("DESIG_ZONA", "Designação - Zona", "{0}", new string[] { "Rede Principal", "Rede Secundária" }), new StringFeatureProperty("DESIG_SEGM", "Designação - Segmento", "{0}"), new StringFeatureProperty("DESIG_LINH", "Designação - Linha", "{0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{0}"), new StringFeatureProperty("LEGISLACAO", "Legislação", "{0}"), new CategoryFeatureProperty("ACTIVO", "Activo", "{0}", new string[] { "N", "S" }), new CategoryFeatureProperty("INFOPDM", "Informação PDM", "{0}", new string[] { "Rede Ferroviária" }), new CategoryFeatureProperty("ENTIDADE", "Entidade", "{0}", new string[] { "Infraestruturas de Portugal, S.A. (IP, S.A.)" }), new CategoryFeatureProperty("CONDICIONA", "Condicionante", "{0}", new string[] { "Rede Ferroviária Nacional" }), new StringFeatureProperty("NOME", "Nome", "{0}"), new StringFeatureProperty("GlobalID", "Global ID", "{0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{0}m") }));
        Layers.Add("Roads", new GeoJsonLayer(this, "Roads", "Roads", "Road network", "Câmara Municipal de Lisboa", new DateTime(2022, 06, 09), true, new RoadRenderer(), geoJsonBaseUrl + "/Roads/{0}.geojson", "OBJECTID_1", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID_1", "Object ID 1", "{0}"), new StringFeatureProperty("OBJECTID", "Object ID", "{0}"), new StringFeatureProperty("COD_SIG", "Código SIG", "{0}"), new StringFeatureProperty("IDTIPO", "ID Tipo", "{0}"), new StringFeatureProperty("COD_VIA", "Código Via", "{0}"), new StringFeatureProperty("DESIGNACAO", "Designação", "{0}"), new CategoryFeatureProperty("ESTADO_VIA", "Estado da Via", "{0}", new string[] { "Existente", " " }), new DateFeatureProperty("DTM_ADD", "Adicionado", "{0}"), new DateFeatureProperty("DTM_UPD", "Atualizado", "{0}"), new NumberFeatureProperty("COMPRIMENT", "Comprimento", "{0}m"), new NumberFeatureProperty("Max_Z", "Max Z", "{0}m"), new NumberFeatureProperty("Min_Z", "Min Z", "{0}m"), new NumberFeatureProperty("Len_Up", "Length Up", "{0}m"), new NumberFeatureProperty("Len_Down", "Length Down", "{0}m"), new NumberFeatureProperty("H_Up", "H Up", "{0}"), new NumberFeatureProperty("H_Down", "H Down", "{0}"), new NumberFeatureProperty("Max_S_Up", "Max S Up", "{0}"), new NumberFeatureProperty("Max_S_Down", "Max S Down", "{0}"), new NumberFeatureProperty("Av_S_Up", "Av S Up", "{0}"), new NumberFeatureProperty("Av_S_Down", "Av S Down", "{0}"), new NumberFeatureProperty("slope", "Slope", "{0}"), new StringFeatureProperty("GlobalID", "Global ID", "{0}"), new NumberFeatureProperty("Shape__Length", "Shape Length", "{0}m") }));
        Layers.Add("Sidewalks", new GeoJsonLayer(this, "Sidewalks", "Sidewalks", "Partial pedestrian network for distinct Lisbon neighbourhoods", "U-Shift - Instituto Superior Técnico", new DateTime(2019, 10, 03), true, new SidewalkRenderer(), geoJsonBaseUrl + "/Sidewalks/{0}.geojson", null, new IFeatureProperty[] { new StringFeatureProperty("Id", "ID", "{0}"), new StringFeatureProperty("Tipologia", "Tipologia", "{0}"), new NumberFeatureProperty("Length", "Length", "{0}m"), new CategoryFeatureProperty("AEstudo", "AEstudo", "{0}", new string[] { "Alvalade", "Arroios", "AvNovas", "Belem", "Benfica", "Expo" }), new StringFeatureProperty("Tipo", "Tipo", "{0}"), new StringFeatureProperty("REF_ID_3", "Ref ID 3", "{0}"), new NumberFeatureProperty("METERS", "Meters", "{0}m"), new StringFeatureProperty("REF_ID_3_N", "Ref ID 3 N", "{0}"), new NumberFeatureProperty("Lenght", "Length", "{0}m"), new CategoryFeatureProperty("layer", "Layer", "{0}", new string[] { "PedNet_Alvalade", "PedNet_Arroios", "PedNet_AvNovas", "PedNet_Belem", "PedNet_Benfica", "PedNet_ParqueNacoes" }) }));
        Layers.Add("Signs", new GeoJsonLayer(this, "Signs", "Street Signs", "Vertical signage managed by EMEL", "EMEL", new DateTime(2020, 09, 10), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/Signs/{0}.geojson", "IdSV_Posic", new IFeatureProperty[] { new StringFeatureProperty("IdSV_Posic", "ID SV Posic", "{0}"), new StringFeatureProperty("CodViaCML", "Código Via CML", "{0}"), new StringFeatureProperty("Arruamento", "Arruamento", "{0}"), new StringFeatureProperty("NumeroPoli", "Número Policia", "{0}"), new StringFeatureProperty("NPCompleme", "NP Complementar", "{0}"), new StringFeatureProperty("Local", "Local", "{0}"), new StringFeatureProperty("Sinal", "Sinal", "{0}"), new StringFeatureProperty("Tipologia", "Tipologia", "{0}"), new NumberFeatureProperty("NumeroChap", "Número de Chapas", "{0}"), new CategoryFeatureProperty("Suporte", "Suporte", "{0}", new string[] { "Candeeiro", "Outro", "Parede", "Poste", "Poste de electricidade", "Poste duplo", "Poste em bandeira", "Pórtico", "Semáforo" }), new StringFeatureProperty("Chapa1", "Chapa 1", "{0}"), new StringFeatureProperty("TextoChapa", "Texto Chapa", "{0}"), new StringFeatureProperty("NSChapa1", "NS Chapa 1", "{0}"), new StringFeatureProperty("Chapa2", "Chapa 2", "{0}"), new StringFeatureProperty("TextoCha_1", "Texto Chapa 1", "{0}"), new StringFeatureProperty("NSChapa2", "NS Chapa 2", "{0}"), new StringFeatureProperty("Chapa3", "Chapa 3", "{0}"), new StringFeatureProperty("TextoCha_2", "Texto Chapa 2", "{0}"), new StringFeatureProperty("NSChapa3", "NS Chapa 3", "{0}"), new StringFeatureProperty("Chapa4", "Chapa 4", "{0}"), new StringFeatureProperty("TextoCha_3", "Texto Chapa 3", "{0}"), new StringFeatureProperty("NSChapa4", "NS Chapa 4", "{0}"), new StringFeatureProperty("Chapa5", "Chapa 5", "{0}"), new StringFeatureProperty("TextoCha_4", "Texto Chapa 4", "{0}"), new StringFeatureProperty("NSChapa5", "NS Chapa 5", "{0}"), new DateFeatureProperty("DataExecuc", "Data Execução", "{0}") }));
        Layers.Add("TrafficRestrictions", new GeoJsonLayer(this, "TrafficRestrictions", "Traffic Restrictions", "Current and planned traffic restrictions", "Câmara Municipal de Lisboa", new DateTime(2022, 11, 26), true, defaultGeoJsonRenderer, geoJsonBaseUrl + "/TrafficRestrictions/{0}.geojson", "id", new IFeatureProperty[] { new StringFeatureProperty("id", "ID", "{0}"), new StringFeatureProperty("morada", "Morada", "{0}"), new StringFeatureProperty("pedido", "Pedido", "{0}"), new StringFeatureProperty("freguesias", "Freguesias", "{0}"), new CategoryFeatureProperty("restricao_circulacao", "Restrição de circulação", "{0}", new string[] { "Corte num sentido", "Corte total", "Cortes temporários", "Estacionamento", "Estreitamento de via", "Mantém perfil de via", "Passeio" }), new CategoryFeatureProperty("impacto", "Impacto", "{0}", new string[] { "Pouco relevante", "Relevante" }), new CategoryFeatureProperty("motivo", "Motivo", "{0}", new string[] { "ACESSO DE VEÍCULOS À OBRA", "AUTOGRUA", "BETONAGENS", "BETONAGENS/CARGAS DESCARGAS", "CARGAS E DESCARGAS", "CARGAS E DESCARGAS/OBRAS", "CONCENTRAÇÃO", "DESMONTAGEM DE GRUA", "EVENTO", "FILMAGENS", "ILUMINAÇÃO PÚBLICA", "LIGAÇÃO DE RAMAL - FAIXA DE RODAGEM", "LIGAÇÃO DE RAMAL - PASSEIO E ESTACION.", "MANIFESTAÇÃO", "MONTAGEM DE GRUA", "MUDANÇAS", "OBRA - FAIXA DE RODAGEM", "OBRA - PASSEIO E ESTACION.", "OBRAS NO SUBSOLO - FAIXA DE RODAGEM", "OBRAS NO SUBSOLO - PASSEIO E ESTACION.", "PINTURAS", "PLANTAÇÃO / PODA DE ÁRVORES", "PROCISSÃO", "REP. COLECTOR/RAMAL - PASSEIO E ESTAC.", "REPAVIMENTAÇÕES", "RESERVA DE ESTACIONAMENTO", "SONDAGENS - PASSEIO E ESTACION.", "SUBSTITUIÇÃO DE CAIXA MULTIBANCO" }), new StringFeatureProperty("panfleto", "Panfleto", "{0}"), new DateFeatureProperty("dtm_min", "Início", "{0}"), new DateFeatureProperty("dtm_max", "Fim", "{0}") }));
        Layers.Add("Trees", new GeoJsonLayer(this, "Trees", "Trees", "The various kinds of trees in Lisbon", "Câmara Municipal de Lisboa", new DateTime(2022, 11, 02), true, new PrefabRenderer("Tree"), geoJsonBaseUrl + "/Trees/{0}.geojson", "OBJECTID", new IFeatureProperty[] { new StringFeatureProperty("OBJECTID", "Object ID", "{0}"), new StringFeatureProperty("COD_SIG_NEW", "Código SIG Novo", "{0}"), new StringFeatureProperty("MORADA", "Morada", "{0}"), new StringFeatureProperty("ESPECIE_VA", "Espécie", "{0}"), new StringFeatureProperty("PAP", "PAP", "{0}"), new CategoryFeatureProperty("MANUTENCAO", "Manutenção", "{0}", new string[] { "Administração Central", "APL", "CML", "Condomínio", "DJ", "DMREV", "EDÍFICIO PORTUGAL TELECOM", "EGEAC", "Em atualização", "Estado", "Grupo SONAE", "JF", "JF Benfica", "JF Parque das Nações", "Junta de Freguesia", "Junta de freguesia do Lum", "Junta de freguesia dos Ol", "Lidl", "Liquidambar styraciflua", "Ministério da Educação", "Não identificada", "Perenifolia", "Privado", "SGAL" }), new CategoryFeatureProperty("OCUPACAO", "Ocupação", "{0}", new string[] { "Árvore", "Árvore seca", "Cepo", "Eliminada", "Em atualização", "Não identificada", "Pavimentada", "Vazia" }), new CategoryFeatureProperty("LOCAL", "Local", "{0}", new string[] { "Cemitério", "Em atualização", "Escola", "Espaço verde", "Mata", "Não identificado", "Privado", "Via pública", "Via Pública", "Viveiro" }), new CategoryFeatureProperty("TIPOLOGIA", "Tipologia", "{0}", new string[] { "Caldeira", "Canteiro", "Em atualização", "Espaço verde", "Floreira", "Floreira Amovível", "Não identificada", "Outra", "Sem caldeira", "Talude", "Verde de enquadramento" }), new StringFeatureProperty("FREG_2012", "Freguesia", "{0}"), new StringFeatureProperty("NOME_VULGA", "Nome Vulgar", "{0}"), new StringFeatureProperty("GlobalID", "Global ID", "{0}") }));

        // Add the points of interest
        POIs.Add(new PointOfInterest("Alta de Lisboa", new Vector2D(38.773310, -9.153689)));
        POIs.Add(new PointOfInterest("Campolide", new Vector2D(38.733744, -9.160745)));
        POIs.Add(new PointOfInterest("Entrecampos", new Vector2D(38.744169, -9.149994)));
        POIs.Add(new PointOfInterest("Largo do Carmo", new Vector2D(38.711992, -9.140663)));
        POIs.Add(new PointOfInterest("Marquês de Pombal", new Vector2D(38.725249, -9.149994)));
        POIs.Add(new PointOfInterest("Parque das Nações", new Vector2D(38.765514, -9.093839)));
        POIs.Add(new PointOfInterest("Praça do Comércio", new Vector2D(38.706808, -9.136164)));

        // Set the map's initial zoom level and center, as well as the tile load distance
        ZoomLevel = 17;
        Center = GlobalMercator.LatLonToMeters(38.706808, -9.136164);
        Direction = 0;
        Pitch = 0;
        TileLoadDistance = new Vector2Int(4, 4);
        MinElevation = 0;
        MaxElevation = 5000;
        MinZoomLevel = 15;
        MaxZoomLevel = 19;
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
                    _ = existingTile.CheckNeighboursAsync(); // Check neighbours for elevation data in case they were only loaded now
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

        // Move the ruler
        Ruler.Move(-delta);

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
        if (ZoomLevel < MinZoomLevel)
        {
            Debug.LogWarning($"Zoom level cannot be less than {MinZoomLevel}");
            ZoomLevel = MinZoomLevel;
        }
        if (ZoomLevel > MaxZoomLevel)
        {
            Debug.LogWarning($"Zoom level is too high, setting it to {MaxZoomLevel}");
            ZoomLevel = MaxZoomLevel;
        }

        // Calculate the new 2D camera height
        Calculate2DCameraHeight();

        // Update the 2D camera
        Update2DCamera();

        // Zoom the ruler
        Ruler.Zoom();

        // Load the new tiles
        Load();

        // Unload all the old tiles
        Unload();
    }

    /// <summary>
    /// Update the 2D camera height according to the current zoom level
    /// </summary>
    private void Calculate2DCameraHeight()
    {
        // Calculate the bounds of the origin tile
        Vector2Int originTile = GlobalMercator.MetersToGoogleTile(Center, ZoomLevel);
        Bounds bounds = GlobalMercator.GoogleTileBounds(originTile.x, originTile.y, ZoomLevel);

        // Calculate what is the screen height in meters if we keep the tile size on screen constant
        double meterHeight = (Camera2D.pixelHeight * bounds.Height) / TileSize;

        // Calculate the distance from the camera to the center of the tile at sea level, such that the tile size on screen is constant
        Camera2DHeight = (meterHeight / 2) / System.Math.Tan((Camera2D.fieldOfView * System.Math.PI) / 360);

        // Update the clip planes to make sure the map is always visible
        Camera2D.nearClipPlane = (float)(Camera2DHeight / 100);
        Camera2D.farClipPlane = (float)(Camera2DHeight * 2);

        // Match the clip planes for the ruler cameras
        Camera2D.transform.Find("Ruler Node").GetComponent<Camera>().nearClipPlane = Camera2D.nearClipPlane;
        Camera2D.transform.Find("Ruler Node").GetComponent<Camera>().farClipPlane = Camera2D.farClipPlane;
        Camera2D.transform.Find("Ruler Edge").GetComponent<Camera>().nearClipPlane = Camera2D.nearClipPlane;
        Camera2D.transform.Find("Ruler Edge").GetComponent<Camera>().farClipPlane = Camera2D.farClipPlane;
    }

    /// <summary>
    /// Change the map's direction by the given amount
    /// </summary>
    /// <param name="amount">The amount to rotate by</param>
    public void ChangeDirection(int amount)
    {
        // Update the direction
        Direction += amount;

        // Clamp the direction
        if (Direction > 180)
        {
            Direction -= 360;
        }
        else if (Direction <= -180)
        {
            Direction += 360;
        }

        // Update the 2D camera
        Update2DCamera();
    }

    /// <summary>
    /// Reset the map's direction
    /// </summary>
    public void ResetDirection()
    {
        Direction = 0;
        Update2DCamera();
    }

    /// <summary>
    /// Change the map's pitch by the given amount
    /// </summary>
    /// <param name="amount">The amount to tilt by</param>
    public void ChangePitch(int amount)
    {
        // Update the pitch
        Pitch += amount;

        // Clamp the pitch
        if (Pitch > 60)
        {
            Pitch = 60;
        }
        else if (Pitch < 0)
        {
            Pitch = 0;
        }

        // Update the 2D camera
        Update2DCamera();
    }

    /// <summary>
    /// Reset the map's pitch
    /// </summary>
    public void ResetPitch()
    {
        Pitch = 0;
        Update2DCamera();
    }

    /// <summary>
    /// Update the 2D camera's position and rotation
    /// </summary>
    private void Update2DCamera()
    {
        // Apply the camera height
        Camera2D.transform.position = new Vector3(0, (float)Camera2DHeight, 0);

        // Apply the camera direction rotation around the Y axis
        Camera2D.transform.rotation = Quaternion.Euler(90, Direction, 0);

        // Apply the camera pitch rotation around the X axis relative to the camera's direction
        Camera2D.transform.RotateAround(Vector3.zero, Camera2D.transform.right, -Pitch);
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
    /// Get the coordinates relative to the map's center
    /// </summary>
    /// <param name="meters">The coordinates in meters (WGS84)</param>
    /// <returns>The coordinates in map local space</returns>
    public Vector2D GetRelativePosition(Vector2D meters)
    {
        return meters - Center;
    }

    /// <summary>
    /// Get the coordinates in meters at given world position
    /// </summary>
    /// <param name="worldPosition">The world position to get the coordinates at (Unity XYZ)</param>
    /// <returns>The coordinates in meters (WGS84)</returns>
    public Vector2D WorldToMeters(Vector3 worldPosition)
    {
        // Convert the world position to local map position
        Vector3 mapPoint = GameObject.transform.InverseTransformPoint(worldPosition);

        // Remove the map's center offset
        return Center + new Vector2D(mapPoint.x, mapPoint.z);
    }

    /// <summary>
    /// Get the coordinates (lat/lon) of the given point on the world
    /// </summary>
    /// <param name="point">The point on the world (Unity space)</param>
    /// <returns>The coordinates of the given point (lat/lon)</returns>
    public Vector2D WorldToLatLon(Vector3 point)
    {
        return GlobalMercator.MetersToLatLon(WorldToMeters(point));
    }

    /// <summary>
    /// Get the height of the terrain at the given point on the world
    /// </summary>
    /// <param name="point">The point on the world (Unity space)</param>
    /// <returns>The height of the terrain at the given coordinates (meters)</returns>
    /// <remarks>
    /// Will only return a value if the tile containing the given coordinates is loaded, otherwise returns 0
    /// </remarks>
    public double GetHeight(Vector3 point)
    {
        // Convert the given point to meters
        Vector2D metersPoint = WorldToMeters(point);

        // Get the tile containing the given coordinates
        Vector2Int tileXY = GlobalMercator.MetersToGoogleTile(metersPoint, ZoomLevel);
        if (Tiles.TryGetValue($"{ZoomLevel}/{tileXY.x}/{tileXY.y}", out Tile tile))
        {
            // Get the height of the terrain at the given coordinates
            return tile.GetHeight(metersPoint);
        }

        // The tile is not loaded, return 0
        return 0;
    }

    /// <summary>
    /// Decode pixel values to height values. The height will be returned in meters
    /// </summary>
    /// <param name="color">The queried location's pixel</param>
    /// <returns>Height at location (meters)</returns>
    public static double MapboxHeightFromColor(Color color)
    {
        // Convert from 0..1 to 0..255
        float R = color.r * 255;
        float G = color.g * 255;
        float B = color.b * 255;

        return -10000 + ((R * 256 * 256 + G * 256 + B) * 0.1);
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

        // Calculate the new 2D camera height
        Calculate2DCameraHeight();

        // Update the 2D camera
        Update2DCamera();
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
