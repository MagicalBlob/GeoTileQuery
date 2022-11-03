using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the toolbars
/// </summary>
public class Toolbars
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
    /// The toolbars root transform
    /// </summary>
    private Transform toolbars;

    /// <summary>
    /// Constructs the toolbars
    /// </summary>
    /// <param name="map">Reference to the map</param>
    /// <param name="ui">The UI controller</param>
    /// <param name="toolbars">The toolbars root transform</param>
    public Toolbars(Map map, UIController ui, Transform toolbars)
    {
        this.map = map;
        this.ui = ui;
        this.toolbars = toolbars;

        // Set up the main toolbar
        SetupMainToolbar();
    }

    /// <summary>
    /// Sets up the main toolbar
    /// </summary>
    private void SetupMainToolbar()
    {
        Transform mainToolbar = toolbars.Find("Main").transform;

        // Rotate buttons
        mainToolbar.Find("Rotate/Left").GetComponent<Button>().onClick.AddListener(() => map.ChangeDirection(15));
        mainToolbar.Find("Rotate/Reset").GetComponent<Button>().onClick.AddListener(() => map.ResetDirection());
        mainToolbar.Find("Rotate/Right").GetComponent<Button>().onClick.AddListener(() => map.ChangeDirection(-15));

        // Tilt buttons
        mainToolbar.Find("Tilt/Down").GetComponent<Button>().onClick.AddListener(() => map.ChangePitch(-10));
        mainToolbar.Find("Tilt/Reset").GetComponent<Button>().onClick.AddListener(() => map.ResetPitch());
        mainToolbar.Find("Tilt/Up").GetComponent<Button>().onClick.AddListener(() => map.ChangePitch(10));

        // Navigation buttons
        mainToolbar.Find("Navigation/Up").GetComponent<Button>().onClick.AddListener(() => map.MoveCenter((8388608 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction + 90))); // 8388608 was chosen because it moved about 100m at zoom level 16 which felt about right
        mainToolbar.Find("Navigation/Down").GetComponent<Button>().onClick.AddListener(() => map.MoveCenter((8388608 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction - 90))); // 8388608 was chosen because it moved about 100m at zoom level 16 which felt about right
        mainToolbar.Find("Navigation/Left").GetComponent<Button>().onClick.AddListener(() => map.MoveCenter((8388608 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction + 180))); // 8388608 was chosen because it moved about 100m at zoom level 16 which felt about right
        mainToolbar.Find("Navigation/Right").GetComponent<Button>().onClick.AddListener(() => map.MoveCenter((8388608 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction))); // 8388608 was chosen because it moved about 100m at zoom level 16 which felt about right

        // Zoom buttons
        mainToolbar.Find("Zoom/In").GetComponent<Button>().onClick.AddListener(() => map.Zoom(1));
        mainToolbar.Find("Zoom/Out").GetComponent<Button>().onClick.AddListener(() => map.Zoom(-1));

        // Other buttons
        mainToolbar.Find("Location").GetComponent<Button>().onClick.AddListener(() => { ui.Modals.Show("Location", 500, 205); });
        mainToolbar.Find("Layers").GetComponent<Button>().onClick.AddListener(() => { ui.Modals.Show("Layers"); });
        mainToolbar.Find("Filter").GetComponent<Button>().onClick.AddListener(() => { ToggleFilters(mainToolbar); });
        mainToolbar.Find("Query").GetComponent<Button>().onClick.AddListener(() => { ToggleQuery(mainToolbar); });
        mainToolbar.Find("Ruler").GetComponent<Button>().onClick.AddListener(() => { ToggleRuler(mainToolbar); });
        mainToolbar.Find("Terrain").GetComponent<Button>().onClick.AddListener(() => { ToggleTerrain(mainToolbar); });
        mainToolbar.Find("AR").GetComponent<Button>().onClick.AddListener(() => { ToggleAR(mainToolbar); });
        mainToolbar.Find("Debug").GetComponent<Button>().onClick.AddListener(() => { ui.Modals.Show("Debug"); });
        mainToolbar.Find("About").GetComponent<Button>().onClick.AddListener(() => { ui.Modals.Show("About", 500, 500); });
    }

    /// <summary>
    /// Toggle filters
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    private void ToggleFilters(Transform toolbar)
    {
        map.Filtered = !map.Filtered;
        toolbar.Find("Filter/Text").GetComponent<Text>().text = $"Filters: {(map.Filtered ? "On" : "Off")}";
    }

    /// <summary>
    /// Toggles query mode
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    private void ToggleQuery(Transform toolbar)
    {
        toolbar.Find("Ruler").GetComponent<Button>().interactable = ui.QueryMode;
        Color color = ui.QueryMode ? Color.white : new Color(1, 1, 1, 0.5f);
        toolbar.Find("Ruler/Image").GetComponent<Image>().color = color;
        toolbar.Find("Ruler/Text").GetComponent<Text>().color = color;
        toolbar.Find("Query/Text").GetComponent<Text>().text = ui.QueryMode ? "Query: Off" : "Query: On";
        ui.QueryMode = !ui.QueryMode;
    }

    /// <summary>
    /// Toggles ruler mode
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    private void ToggleRuler(Transform toolbar)
    {
        toolbar.Find("Query").GetComponent<Button>().interactable = ui.RulerMode;
        Color color = ui.RulerMode ? Color.white : new Color(1, 1, 1, 0.5f);
        toolbar.Find("Query/Image").GetComponent<Image>().color = color;
        toolbar.Find("Query/Text").GetComponent<Text>().color = color;
        toolbar.Find("Ruler/Text").GetComponent<Text>().text = ui.RulerMode ? "Ruler: Off" : "Ruler: On";
        ui.RulerMode = !ui.RulerMode;
    }

    /// <summary>
    /// Toggle the terrain
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    private void ToggleTerrain(Transform toolbar)
    {
        map.ElevatedTerrain = !map.ElevatedTerrain;
        toolbar.Find("Terrain/Text").GetComponent<Text>().text = $"Terrain: {(map.ElevatedTerrain ? "On" : "Off")}";
    }

    /// <summary>
    /// Toggles AR mode
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    private void ToggleAR(Transform toolbar)
    {
        if (ui.ARMode)
        {
            map.SwitchTo2DMode();
        }
        else
        {
            map.SwitchToARMode();
        }
        toolbar.Find("AR/Text").GetComponent<Text>().text = ui.ARMode ? "AR: Off" : "AR: On";
        ui.ARMode = !ui.ARMode;
    }
}
