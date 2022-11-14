using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Controls the input
/// </summary>
public class InputController
{
    public InputMode Mode { get; set; }

    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map map;

    /// <summary>
    /// The UI controller
    /// </summary>
    private UIController ui;

    /// <summary>
    /// The touch finger ID at the start of the interaction
    /// </summary>
    private int? startTouchFingerId;

    /// <summary>
    /// The mouse position at the start of the interaction
    /// </summary>
    private Vector2? startMousePosition;

    /// <summary>
    /// The current selected ruler node
    /// </summary>
    private int? selectedRulerNode;

    /// <summary>
    /// The action to perform on the ruler node when the mouse is released
    /// </summary>
    private RulerNodeAction rulerNodeAction;

    /// <summary>
    /// Constructs a new Input Controller
    /// </summary>
    /// <param name="map">Reference to the map</param>
    /// <param name="ui">The UI controller</param>
    public InputController(Map map, UIController ui)
    {
        this.map = map;
        this.ui = ui;
    }

    /// <summary>
    /// Processes the input every frame
    /// </summary>
    public void Update()
    {
        if (Input.touchCount > 0)
        {
            ProcessTouchInput();
        }
        else
        {
            ProcessMouseInput();
        }
    }

    /// <summary>
    /// Processes the touch input
    /// </summary>
    private void ProcessTouchInput()
    {
        // Single touch
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Check if the touch is over a UI element
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        return;
                    }
                    startTouchFingerId = touch.fingerId;
                    OnPositionInputBegan(touch.position);
                    break;
                case TouchPhase.Moved:
                    if (startTouchFingerId == touch.fingerId)
                    {
                        OnPositionInputMoved(touch.position);
                    }
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    if (startTouchFingerId == touch.fingerId)
                    {
                        OnPositionInputEnded(touch.position);
                        startTouchFingerId = null;
                    }
                    break;
            }
        }
        // Two touches
        else if (Input.touchCount == 2)
        {
            Debug.LogWarning($"InputController.Update: 2 touches not supported"); // TODO
        }
        // More than two touches
        else
        {
            Debug.LogWarning($"InputController.Update: {Input.touchCount} touches not supported");
        }
    }

    /// <summary>
    /// Processes the mouse input
    /// </summary>
    private void ProcessMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Check if the mouse is over a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            startMousePosition = Input.mousePosition;
            OnPositionInputBegan(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            // Check if the mouse has moved more than 5 pixels
            if (startMousePosition.HasValue && Vector2.Distance(Input.mousePosition, startMousePosition.Value) > 5)
            {
                OnPositionInputMoved(Input.mousePosition);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (startMousePosition.HasValue)
            {
                OnPositionInputEnded(Input.mousePosition);
                startMousePosition = null;
            }
        }
    }

    /// <summary>
    /// Called when a touch begins or a mouse button is pressed
    /// </summary>
    /// <param name="position">The position of the touch or mouse</param>
    private void OnPositionInputBegan(Vector2 position)
    {
        switch (Mode)
        {
            case InputMode.Normal:
                Debug.Log($"[NORMAL] Position input began"); // TODO: Remove this
                break;
            case InputMode.Ruler:
                // Layer masks
                int rulerNodeMask = 1 << 6; // Ruler Node layer is 6
                int rulerEdgeMask = 1 << 7; // Ruler Edge layer is 7
                int notRulerMask = ~rulerNodeMask & ~rulerEdgeMask;

                // Create a ray at the screen point
                Ray ray = Camera.main.ScreenPointToRay(position);

                // Check if the ray hits a ruler node
                if (Physics.Raycast(ray, out RaycastHit rulerNodeHit, Mathf.Infinity, rulerNodeMask))
                {
                    selectedRulerNode = int.Parse(rulerNodeHit.transform.gameObject.name);
                    rulerNodeAction = RulerNodeAction.Remove; // Assume node removal until there's movement
                }
                // Check if the ray hits a ruler edge
                else if (Physics.Raycast(ray, out RaycastHit rulerEdgeHit, Mathf.Infinity, rulerEdgeMask))
                {
                    // Create a new node at the hit point before the edge's node
                    selectedRulerNode = map.Ruler.AddNodeBefore(int.Parse(rulerEdgeHit.transform.gameObject.name), map.WorldToLatLon(rulerEdgeHit.point));
                    ui.Panels.UpdateRulerText();
                    rulerNodeAction = RulerNodeAction.Move; // Allow the new node to be moved
                }
                // Check if the ray hits the map
                else if (Physics.Raycast(ray, out RaycastHit addHit, Mathf.Infinity, notRulerMask))
                {
                    // Create a new node at the hit point
                    selectedRulerNode = map.Ruler.AddNode(map.WorldToLatLon(addHit.point));
                    ui.Panels.UpdateRulerText();
                    rulerNodeAction = RulerNodeAction.Add; // Assume just adding a node until there's movement
                }
                else
                {
                    // Didn't hit anything
                    Debug.LogWarning("Nothing was hit by the Ruler raycast");
                    rulerNodeAction = RulerNodeAction.None;
                }
                break;
        }
    }

    /// <summary>
    /// Called when a touch moves or a mouse button is held and moved
    /// </summary>
    /// <param name="position">The position of the touch or mouse</param>
    private void OnPositionInputMoved(Vector2 position)
    {
        switch (Mode)
        {
            case InputMode.Normal:
                Debug.Log($"[NORMAL] Position input moved"); // TODO: Remove this
                break;
            case InputMode.Ruler:
                switch (rulerNodeAction)
                {
                    case RulerNodeAction.Add:
                        // Since there's movement, the node will also be moved
                        rulerNodeAction = RulerNodeAction.Move;
                        break;
                    case RulerNodeAction.Remove:
                        // Since there's movement, the node will not be removed
                        rulerNodeAction = RulerNodeAction.Move;
                        break;
                    case RulerNodeAction.Move:
                        // Create a ray at the screen point
                        Ray ray = Camera.main.ScreenPointToRay(position);
                        if (Physics.Raycast(ray, out RaycastHit moveHit))
                        {
                            map.Ruler.MoveNode(selectedRulerNode.Value, map.WorldToLatLon(moveHit.point));
                            ui.Panels.UpdateRulerText();
                        }
                        break;
                }
                break;
        }
    }

    /// <summary>
    /// Called when a touch ends or a mouse button is released
    /// </summary>
    /// <param name="position">The position of the touch or mouse</param>
    private void OnPositionInputEnded(Vector2 position)
    {
        switch (Mode)
        {
            case InputMode.Normal:
                Debug.Log($"[NORMAL] Position input ended"); // TODO: Remove this
                break;
            case InputMode.Query:
                ui.Modals.ShowMapQuery(position);
                break;
            case InputMode.Ruler:
                // Create a ray at the screen point
                switch (rulerNodeAction)
                {
                    case RulerNodeAction.Move:
                        Ray ray = Camera.main.ScreenPointToRay(position);
                        if (Physics.Raycast(ray, out RaycastHit moveHit))
                        {
                            map.Ruler.MoveNode(selectedRulerNode.Value, map.WorldToLatLon(moveHit.point));
                            ui.Panels.UpdateRulerText();
                        }
                        break;
                    case RulerNodeAction.Remove:
                        map.Ruler.RemoveNode(selectedRulerNode.Value);
                        ui.Panels.UpdateRulerText();
                        break;
                }
                selectedRulerNode = null;
                rulerNodeAction = RulerNodeAction.None;
                break;
        }
    }

    /// <summary>
    /// The input mode
    /// </summary>
    public enum InputMode
    {
        /// <summary>
        /// The default mode
        /// </summary>
        Normal,
        /// <summary>
        /// Query mode
        /// </summary>
        Query,
        /// <summary>
        /// Ruler mode
        /// </summary>
        Ruler
    }

    /// <summary>
    /// Types of interactions with the ruler nodes
    /// </summary>
    private enum RulerNodeAction
    {
        /// <summary>
        /// No action
        /// </summary>
        None,
        /// <summary>
        /// Adds a node
        /// </summary>
        Add,
        /// <summary>
        /// Moves a node
        /// </summary>
        Move,
        /// <summary>
        /// Removes a node
        /// </summary>
        Remove
    }
}
