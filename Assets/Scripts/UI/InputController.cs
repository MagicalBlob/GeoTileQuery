using System;
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
    /// The single touch finger at the start of the interaction
    /// </summary>
    private Touch? startSingleTouchFinger;

    /// <summary>
    /// The 2-finger touch first finger at the start of the interaction
    /// </summary>
    private Touch? startTwoTouchFirstFinger;

    /// <summary>
    /// The 2-finger touch second finger at the start of the interaction
    /// </summary>
    private Touch? startTwoTouchSecondFinger;

    /// <summary>
    /// The mouse position at the start of the interaction
    /// </summary>
    private Vector2? startMousePosition;

    /// <summary>
    /// The mouse position at the previous frame
    /// </summary>
    private Vector2? previousMousePosition;

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
        if (!ui.Modals.IsOpen) // Don't process map input if a modal is open
        {
            if (Application.platform == RuntimePlatform.Android && Input.touchCount > 0)
            {
                ProcessTouchInput();
            }
            else
            {
                ProcessKeyboardInput();
                if (Input.touchSupported && Input.touchCount > 0)
                {
                    ProcessTouchInput();
                }
                else
                {
                    ProcessMouseInput();
                }
            }
        }
    }

    /// <summary>
    /// Processes keyboard input
    /// </summary>
    private void ProcessKeyboardInput()
    {
        // Close button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ui.Modals.IsOpen)
            {
                // Close the modal
                ui.Modals.Close();
            }
            else
            {
                // Close the app
                Application.Quit();
            }
        }

        // Navigation buttons
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            map.MoveCenter((4194304 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction + 90)); // 4194304 was chosen because it moved about 50m at zoom level 16 which felt about right
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            map.MoveCenter((4194304 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction - 90)); // 4194304 was chosen because it moved about 50m at zoom level 16 which felt about right
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            map.MoveCenter((4194304 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction + 180)); // 4194304 was chosen because it moved about 50m at zoom level 16 which felt about right
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            map.MoveCenter((4194304 >> map.ZoomLevel) * Vector2D.FromAngle(-map.Direction)); // 4194304 was chosen because it moved about 50m at zoom level 16 which felt about right
        }

        // Zoom buttons
        if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.Plus))
        {
            // Zoom in
            map.Zoom(1);
        }
        else if (Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.Minus))
        {
            // Zoom out
            map.Zoom(-1);
        }

        // Rotate buttons
        if (Input.GetKey(KeyCode.Q))
        {
            // Rotate left
            map.ChangeDirection(1);
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            // Reset rotation
            map.ResetDirection();
        }
        else if (Input.GetKey(KeyCode.E))
        {
            // Rotate right
            map.ChangeDirection(-1);
        }

        // Tilt buttons
        if (Input.GetKey(KeyCode.F))
        {
            // Tilt up
            map.ChangePitch(1);
        }
        else if (Input.GetKeyDown(KeyCode.H))
        {
            // Reset tilt
            map.ResetPitch();
        }
        else if (Input.GetKey(KeyCode.R))
        {
            // Tilt down
            map.ChangePitch(-1);
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
                    startSingleTouchFinger = touch;
                    OnPositionInputBegan(touch.position);
                    break;
                case TouchPhase.Moved:
                    if (startSingleTouchFinger.HasValue && startSingleTouchFinger.Value.fingerId == touch.fingerId)
                    {
                        OnPositionInputMoved(touch.position, touch.deltaPosition);
                    }
                    break;
                case TouchPhase.Canceled:
                case TouchPhase.Ended:
                    if (startSingleTouchFinger.HasValue && startSingleTouchFinger.Value.fingerId == touch.fingerId)
                    {
                        OnPositionInputEnded(touch.position);
                        startSingleTouchFinger = null;
                    }
                    break;
            }
        }
        // Two touches
        else if (Input.touchCount == 2)
        {
            Touch firstTouch = Input.GetTouch(0); // Get the first touch
            Touch secondTouch = Input.GetTouch(1); // Get the second touch

            if (firstTouch.phase == TouchPhase.Began || secondTouch.phase == TouchPhase.Began)
            {
                // Check if the touch is over a UI element
                if (EventSystem.current.IsPointerOverGameObject(firstTouch.fingerId) || EventSystem.current.IsPointerOverGameObject(secondTouch.fingerId))
                {
                    return;
                }
                // End the single touch interaction, if there is one
                if (startSingleTouchFinger.HasValue && startSingleTouchFinger.Value.fingerId == firstTouch.fingerId)
                {
                    OnPositionInputEnded(firstTouch.position);
                    startSingleTouchFinger = null;
                }

                // Start the two touch interaction
                startTwoTouchFirstFinger = firstTouch;
                startTwoTouchSecondFinger = secondTouch;
                OnDoublePositionInputBegan(firstTouch.position, firstTouch.deltaPosition, secondTouch.position, secondTouch.deltaPosition);
            }
            else if (firstTouch.phase == TouchPhase.Canceled || firstTouch.phase == TouchPhase.Ended || secondTouch.phase == TouchPhase.Canceled || secondTouch.phase == TouchPhase.Ended)
            {
                if (startTwoTouchFirstFinger.HasValue && startTwoTouchFirstFinger.Value.fingerId == firstTouch.fingerId && startTwoTouchSecondFinger.HasValue && startTwoTouchSecondFinger.Value.fingerId == secondTouch.fingerId)
                {
                    OnDoublePositionInputEnded(firstTouch.position, firstTouch.deltaPosition, secondTouch.position, secondTouch.deltaPosition);
                    startTwoTouchFirstFinger = null;
                    startTwoTouchSecondFinger = null;
                }
            }
            else if (firstTouch.phase == TouchPhase.Moved || secondTouch.phase == TouchPhase.Moved)
            {
                if (startTwoTouchFirstFinger.HasValue && startTwoTouchFirstFinger.Value.fingerId == firstTouch.fingerId && startTwoTouchSecondFinger.HasValue && startTwoTouchSecondFinger.Value.fingerId == secondTouch.fingerId)
                {
                    OnDoublePositionInputMoved(firstTouch.position, firstTouch.deltaPosition, secondTouch.position, secondTouch.deltaPosition);
                }
            }
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
            previousMousePosition = Input.mousePosition;
            OnPositionInputBegan(Input.mousePosition);
        }
        if (Input.GetMouseButton(0))
        {
            // Check if the mouse has moved more than 5 pixels
            if (startMousePosition.HasValue && Vector2.Distance(Input.mousePosition, previousMousePosition.Value) > 5)
            {
                OnPositionInputMoved(Input.mousePosition, new Vector2(Input.mousePosition.x - previousMousePosition.Value.x, Input.mousePosition.y - previousMousePosition.Value.y));
                previousMousePosition = Input.mousePosition;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (startMousePosition.HasValue)
            {
                OnPositionInputEnded(Input.mousePosition);
                startMousePosition = null;
                previousMousePosition = null;
            }
        }
        if (Input.mouseScrollDelta.y > 0)
        {
            // Scroll up
            map.Zoom(1);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            // Scroll down
            map.Zoom(-1);
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
    /// <param name="deltaPosition">The change in position since the last update</param>
    private void OnPositionInputMoved(Vector2 position, Vector2 deltaPosition)
    {
        switch (Mode)
        {
            case InputMode.Normal:
                Vector2 previousPosition = position - deltaPosition;
                Ray previousPositionRay = Camera.main.ScreenPointToRay(previousPosition);
                Ray positionRay = Camera.main.ScreenPointToRay(position);
                if (Physics.Raycast(previousPositionRay, out RaycastHit previousHit) && Physics.Raycast(positionRay, out RaycastHit hit))
                {
                    Vector2D delta = new Vector2D(hit.point.x - previousHit.point.x, hit.point.z - previousHit.point.z);
                    map.MoveCenter(-delta);
                }
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
    /// Called when a two-finger touch begins
    /// </summary>
    /// <param name="firstPosition">The position of the first touch</param>
    /// <param name="firstDeltaPosition">The delta position of the first touch</param>
    /// <param name="secondPosition">The position of the second touch</param>
    /// <param name="secondDeltaPosition">The delta position of the second touch</param>
    private void OnDoublePositionInputBegan(Vector2 firstPosition, Vector2 firstDeltaPosition, Vector2 secondPosition, Vector2 secondDeltaPosition)
    {
        // * currently unused *
    }

    /// <summary>
    /// Called when a two-finger touch moves
    /// </summary>
    /// <param name="firstPosition">The position of the first touch</param>
    /// <param name="firstDeltaPosition">The delta position of the first touch</param>
    /// <param name="secondPosition">The position of the second touch</param>
    /// <param name="secondDeltaPosition">The delta position of the second touch</param>
    private void OnDoublePositionInputMoved(Vector2 firstPosition, Vector2 firstDeltaPosition, Vector2 secondPosition, Vector2 secondDeltaPosition)
    {
        switch (Mode)
        {
            case InputMode.Normal:
                if (ui.ARMode)
                {
                    return; // Rotation and tilt are already available in AR mode by moving the device around
                }

                // Determine the sign of the delta positions Y values
                int firstYSign = Math.Sign(firstDeltaPosition.y);
                int secondYSign = Math.Sign(secondDeltaPosition.y);

                // Check if the two touches are moving in the same vertical direction
                const int distanceThreshold = 20;
                if (firstYSign != 0 && firstYSign == secondYSign && Math.Abs(firstDeltaPosition.y) > distanceThreshold && Math.Abs(secondDeltaPosition.y) > distanceThreshold)
                {
                    if (firstYSign == 1)
                    {
                        // The two touches are moving up, tilt up
                        map.ChangePitch(5);
                    }
                    else
                    {
                        // The two touches are moving down, tilt down
                        map.ChangePitch(-5);
                    }
                }
                else
                {
                    // Determine the sign of the delta positions X values
                    int firstXSign = Math.Sign(firstDeltaPosition.x);
                    int secondXSign = Math.Sign(secondDeltaPosition.x);

                    // Determine the relative positions of the two touches
                    bool firstLeftOfSecond = firstPosition.x < secondPosition.x;
                    bool firstRightOfSecond = firstPosition.x > secondPosition.x;
                    bool firstAboveSecond = firstPosition.y > secondPosition.y;
                    bool firstBelowSecond = firstPosition.y < secondPosition.y;

                    // Check if the two touches are rotating clockwise
                    if ((firstXSign == 0 && firstYSign == 1 && firstLeftOfSecond && secondXSign == 0 && secondYSign == -1)
                    || (firstXSign == 1 && firstYSign == 1 && firstLeftOfSecond && secondXSign == -1 && secondYSign == -1)
                    || (firstXSign == 1 && firstYSign == 0 && firstAboveSecond && secondXSign == -1 && secondYSign == 0)
                    || (firstXSign == 1 && firstYSign == -1 && firstAboveSecond && secondXSign == -1 && secondYSign == 1)
                    || (firstXSign == 0 && firstYSign == -1 && firstRightOfSecond && secondXSign == 0 && secondYSign == 1)
                    || (firstXSign == -1 && firstYSign == -1 && firstRightOfSecond && secondXSign == 1 && secondYSign == 1)
                    || (firstXSign == -1 && firstYSign == 0 && firstBelowSecond && secondXSign == 1 && secondYSign == 0)
                    || (firstXSign == -1 && firstYSign == 1 && firstBelowSecond && secondXSign == 1 && secondYSign == -1))
                    {
                        // The two touches are rotating clockwise
                        map.ChangeDirection(-5);
                    }
                    // Check if the two touches are rotating counter-clockwise
                    else if ((firstXSign == 0 && firstYSign == -1 && firstLeftOfSecond && secondXSign == 0 && secondYSign == 1)
                    || (firstXSign == 1 && firstYSign == -1 && firstLeftOfSecond && secondXSign == -1 && secondYSign == 1)
                    || (firstXSign == 1 && firstYSign == 0 && firstBelowSecond && secondXSign == -1 && secondYSign == 0)
                    || (firstXSign == 1 && firstYSign == 1 && firstBelowSecond && secondXSign == -1 && secondYSign == -1)
                    || (firstXSign == 0 && firstYSign == 1 && firstRightOfSecond && secondXSign == 0 && secondYSign == -1)
                    || (firstXSign == -1 && firstYSign == 1 && firstRightOfSecond && secondXSign == 1 && secondYSign == -1)
                    || (firstXSign == -1 && firstYSign == 0 && firstAboveSecond && secondXSign == 1 && secondYSign == 0)
                    || (firstXSign == -1 && firstYSign == -1 && firstAboveSecond && secondXSign == 1 && secondYSign == 1))
                    {
                        // The two touches are rotating counter-clockwise
                        map.ChangeDirection(5);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Called when a two-finger touch ends
    /// </summary>
    /// <param name="firstPosition">The position of the first touch</param>
    /// <param name="firstDeltaPosition">The delta position of the first touch</param>
    /// <param name="secondPosition">The position of the second touch</param>
    /// <param name="secondDeltaPosition">The delta position of the second touch</param>
    private void OnDoublePositionInputEnded(Vector2 firstPosition, Vector2 firstDeltaPosition, Vector2 secondPosition, Vector2 secondDeltaPosition)
    {
        switch (Mode)
        {
            case InputMode.Normal:
                // Determine the delta positions of the two touches since the start of the touch
                Vector2 firstMovement = firstPosition - startTwoTouchFirstFinger.Value.position;
                Vector2 secondMovement = secondPosition - startTwoTouchSecondFinger.Value.position;

                // Determine the sign of the delta positions XY values
                int firstXSign = Math.Sign(firstMovement.x);
                int firstYSign = Math.Sign(firstMovement.y);
                int secondXSign = Math.Sign(secondMovement.x);
                int secondYSign = Math.Sign(secondMovement.y);

                // Check if the two touches are moving in opposite directions
                if ((firstXSign == 0 && secondXSign == 0 && firstYSign != secondYSign)
                || (firstYSign == 0 && secondYSign == 0 && firstXSign != secondXSign)
                || (firstXSign != 0 && firstYSign != 0 && secondXSign != 0 && secondYSign != 0 && firstXSign != secondXSign && firstYSign != secondYSign))
                {
                    // Calculate the previous and current distances between the two touches
                    float startDistance = Vector2.Distance(startTwoTouchFirstFinger.Value.position, startTwoTouchSecondFinger.Value.position);
                    float distance = Vector2.Distance(firstPosition, secondPosition);

                    // Check if the two touches are moving apart
                    const int distanceThreshold = 500;
                    if (distance > startDistance + distanceThreshold)
                    {
                        // The two touches are moving apart, zoom in
                        map.Zoom(1);
                    }
                    else if (distance < startDistance - distanceThreshold)
                    {
                        // The two touches are moving together, zoom out
                        map.Zoom(-1);
                    }
                }
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
