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

        // Set up the ruler panel
        SetupRulerPanel();
    }

    /// <summary>
    /// Sets up the ruler panel
    /// </summary>
    private void SetupRulerPanel()
    {
        Transform rulerPanel = panels.Find("Ruler");
        rulerPanel.Find("Clear").GetComponent<Button>().onClick.AddListener(() => { map.Ruler.Clear(); });
    }
}
