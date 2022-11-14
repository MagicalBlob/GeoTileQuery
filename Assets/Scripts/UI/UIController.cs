using UnityEngine;

/// <summary>
/// Controls the user interface
/// </summary>
public class UIController
{
    /// <summary>
    /// The toolbars
    /// </summary>
    public Toolbars Toolbars { get; }

    /// <summary>
    /// The panels
    /// </summary>
    public Panels Panels { get; }

    /// <summary>
    /// The modals
    /// </summary>
    public Modals Modals { get; }

    /// <summary>
    /// The input controller
    /// </summary>
    public InputController Input { get; }

    /// <summary>
    /// Whether AR mode is enabled
    /// </summary>
    public bool ARMode { get; set; }

    /// <summary>
    /// Time since last fixed update
    /// </summary>
    private float lastFixedUpdate = 0.0f;

    /// <summary>
    /// Constructs a new UI Controller
    /// <param name="map">The map</param>
    /// </summary>
    public UIController(Map map)
    {
        Transform ui = GameObject.Find("UI").transform;

        this.Toolbars = new Toolbars(map, this, ui.Find("Toolbars").transform);
        this.Panels = new Panels(map, this, ui.Find("Panels").transform);
        this.Modals = new Modals(map, this, ui.Find("Modals").transform);
        this.Input = new InputController(map, this);
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public void Update()
    {
        Modals.Update();

        // Fixed update about once per second
        lastFixedUpdate += Time.unscaledDeltaTime;
        if (lastFixedUpdate > 1.0f)
        {
            lastFixedUpdate = 0.0f;
            Modals.FixedUpdate();
        }

        // Process input
        Input.Update();
    }
}