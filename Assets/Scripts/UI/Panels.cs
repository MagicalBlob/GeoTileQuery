using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the panels
/// </summary>
public class Panels
{
    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map map;

    /// <summary>
    /// The UI controller
    /// </summary>
    private UIController ui;

    /// <summary>
    /// The panels root transform
    /// </summary>
    private Transform panels;

    /// <summary>
    /// The current panel
    /// </summary>
    private GameObject current;

    /// <summary>
    /// The route's From point input field
    /// </summary>
    private InputField routeFromInput;

    /// <summary>
    /// The route's To point input field
    /// </summary>
    private InputField routeToInput;

    /// <summary>
    /// The route's text
    /// </summary>
    private Text routeText;

    /// <summary>
    /// Ruler text
    /// </summary>
    private Text rulerText;

    /// <summary>
    /// Constructs the panels
    /// </summary>
    /// <param name="map">Reference to the map</param>
    /// <param name="ui">The UI controller</param>
    /// <param name="panels">The panels root transform</param>
    public Panels(Map map, UIController ui, Transform panels)
    {
        this.map = map;
        this.ui = ui;
        this.panels = panels;

        // Set up the route panel
        SetupRoutePanel();

        // Set up the ruler panel
        SetupRulerPanel();
    }

    /// <summary>
    /// Shows the given panel
    /// </summary>
    /// <param name="panel">The panel name</param>
    public void Show(string panel)
    {
        current = panels.Find(panel).gameObject;
        current.SetActive(true);
        panels.gameObject.SetActive(true);
    }

    /// <summary>
    /// Closes the current panel
    /// </summary>
    public void Close()
    {
        panels.gameObject.SetActive(false);
        current.SetActive(false);
    }

    /// <summary>
    /// Sets up the route panel
    /// </summary>
    private void SetupRoutePanel()
    {
        Transform routePanel = panels.Find("Route");
        routeFromInput = routePanel.Find("Positions/From").GetComponent<InputField>();
        routeToInput = routePanel.Find("Positions/To").GetComponent<InputField>();
        routeText = routePanel.Find("Text").GetComponent<Text>();
        routePanel.Find("Find").GetComponent<Button>().onClick.AddListener(() =>
        {
            map.Route.Clear();
            OSMServices.GeocodingQueryResult? from = OSMServices.GetCoordinates(routeFromInput.text);
            OSMServices.GeocodingQueryResult? to = OSMServices.GetCoordinates(routeToInput.text);
            if (!from.HasValue && !to.HasValue)
            {
                routeText.text = $"Can't find '{routeFromInput.text}' and '{routeToInput.text}'";
            }
            else if (!from.HasValue)
            {
                routeText.text = $"Can't find '{routeFromInput.text}'";
            }
            else if (!to.HasValue)
            {
                routeText.text = $"Can't find '{routeToInput.text}'";
            }
            else
            {
                routeFromInput.text = from.Value.DisplayName;
                routeToInput.text = to.Value.DisplayName;
                map.MoveCenter(from.Value.Coordinates.X, from.Value.Coordinates.Y);
                if (OSMServices.GetRoute(map.Route, from.Value.Coordinates, to.Value.Coordinates))
                {
                    routeText.text = $"Mode: Driving\t\t\t\tDistance: {PrettyPrint.HumanDistance(map.Route.Distance)}\t\t\t\tDuration: {PrettyPrint.HumanTime(map.Route.Duration)}";
                }
                else
                {
                    routeText.text = "Unable to find a route! :(";
                }
            }
        });
    }

    /// <summary>
    /// Clears the route text
    /// </summary>
    public void ClearRouteText()
    {
        routeText.text = "Fill the From and To positions to find the route";
    }

    /// <summary>
    /// Sets up the ruler panel
    /// </summary>
    private void SetupRulerPanel()
    {
        Transform rulerPanel = panels.Find("Ruler");
        rulerText = rulerPanel.Find("Text").GetComponent<Text>();
        UpdateRulerText();
        rulerPanel.Find("Clear").GetComponent<Button>().onClick.AddListener(() =>
        {
            map.Ruler.Clear();
            UpdateRulerText();
        });
    }

    /// <summary>
    /// Updates the ruler panel text
    /// </summary>
    public void UpdateRulerText()
    {
        rulerText.text = $"Measured Distance: {map.Ruler.Length:0.##}m";
    }
}
