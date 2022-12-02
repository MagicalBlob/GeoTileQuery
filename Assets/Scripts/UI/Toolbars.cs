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
        mainToolbar.Find("About").GetComponent<Button>().onClick.AddListener(() => { ui.Modals.Show("About", 900, 1000); });
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
        toolbar.Find("Ruler").GetComponent<Button>().interactable = ui.Input.Mode == InputController.InputMode.Query;
        Color color = ui.Input.Mode == InputController.InputMode.Query ? Color.white : new Color(1, 1, 1, 0.5f);
        toolbar.Find("Ruler/Image").GetComponent<Image>().color = color;
        toolbar.Find("Ruler/Text").GetComponent<Text>().color = color;
        toolbar.Find("Query/Text").GetComponent<Text>().text = ui.Input.Mode == InputController.InputMode.Query ? "Query: Off" : "Query: On";
        ui.Input.Mode = ui.Input.Mode == InputController.InputMode.Query ? InputController.InputMode.Normal : InputController.InputMode.Query;
    }

    /// <summary>
    /// Toggles ruler mode
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    private void ToggleRuler(Transform toolbar)
    {
        toolbar.Find("Query").GetComponent<Button>().interactable = ui.Input.Mode == InputController.InputMode.Ruler;
        Color color = ui.Input.Mode == InputController.InputMode.Ruler ? Color.white : new Color(1, 1, 1, 0.5f);
        toolbar.Find("Query/Image").GetComponent<Image>().color = color;
        toolbar.Find("Query/Text").GetComponent<Text>().color = color;
        toolbar.Find("Ruler/Text").GetComponent<Text>().text = ui.Input.Mode == InputController.InputMode.Ruler ? "Ruler: Off" : "Ruler: On";
        if (ui.Input.Mode == InputController.InputMode.Ruler)
        {
            ui.Panels.Close();
            map.Ruler.Clear();
            ui.Input.Mode = InputController.InputMode.Normal;
        }
        else
        {
            ui.Panels.UpdateRulerText();
            ui.Panels.Show("Ruler");
            ui.Input.Mode = InputController.InputMode.Ruler;
        }
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
            ToggleRotateAndTiltButtons(toolbar, true);
        }
        else
        {
            ToggleRotateAndTiltButtons(toolbar, false);
            map.SwitchToARMode();
        }
        toolbar.Find("AR/Text").GetComponent<Text>().text = ui.ARMode ? "AR: Off" : "AR: On";
        ui.ARMode = !ui.ARMode;
    }

    /// <summary>
    /// Enables AR button
    /// </summary>
    internal void EnableARButton()
    {
        Transform arButton = toolbars.Find("Main/AR");
        Text text = arButton.Find("Text").GetComponent<Text>();
        arButton.GetComponent<Button>().interactable = true;
        text.text = ui.ARMode ? "AR: On" : "AR: Off";
        text.color = Color.white;
        arButton.Find("Image").GetComponent<Image>().color = Color.white;
    }

    /// <summary>
    /// Toggle rotate and tilt buttons
    /// </summary>
    /// <param name="toolbar">The toolbar</param>
    /// <param name="enabled">Whether the buttons should be enabled</param>
    private void ToggleRotateAndTiltButtons(Transform toolbar, bool enabled)
    {
        Color color = enabled ? Color.white : new Color(1, 1, 1, 0.5f);

        toolbar.Find("Rotate/Left").GetComponent<Button>().interactable = enabled;
        toolbar.Find("Rotate/Left/Image").GetComponent<Image>().color = color;
        toolbar.Find("Rotate/Reset").GetComponent<Button>().interactable = enabled;
        toolbar.Find("Rotate/Reset/Image").GetComponent<Image>().color = color;
        toolbar.Find("Rotate/Right").GetComponent<Button>().interactable = enabled;
        toolbar.Find("Rotate/Right/Image").GetComponent<Image>().color = color;
        toolbar.Find("Tilt/Down").GetComponent<Button>().interactable = enabled;
        toolbar.Find("Tilt/Down/Image").GetComponent<Image>().color = color;
        toolbar.Find("Tilt/Reset").GetComponent<Button>().interactable = enabled;
        toolbar.Find("Tilt/Reset/Image").GetComponent<Image>().color = color;
        toolbar.Find("Tilt/Up").GetComponent<Button>().interactable = enabled;
        toolbar.Find("Tilt/Up/Image").GetComponent<Image>().color = color;
    }
}
