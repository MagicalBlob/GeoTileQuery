using UnityEngine;
using UnityEngine.EventSystems;

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
    /// Whether query mode is enabled
    /// </summary>
    public bool QueryMode { get; set; }

    /// <summary>
    /// Whether ruler mode is enabled
    /// </summary>
    public bool RulerMode { get; set; }

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
        ProcessInput();
    }

    /// <summary>
    /// Processes input events
    /// </summary>
    public void ProcessInput()
    {
        // Query mode input
        if (QueryMode)
        {
            // User clicked/tapped
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.touchCount > 0)
                {
                    // Touch input
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        Modals.QueryMap(touch.position);
                    }
                }
                else
                {
                    // Mouse input
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        Modals.QueryMap(Input.mousePosition);
                    }
                }
            }
        }
    }
}