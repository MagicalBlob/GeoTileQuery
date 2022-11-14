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

        // Set up the ruler panel
        SetupRulerPanel();
    }

    /// <summary>
    /// Shows the given panel
    /// </summary>
    /// <param name="modal">The panel name</param>
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
