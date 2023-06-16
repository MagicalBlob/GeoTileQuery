using UnityEngine;

/// <summary>
/// Represents a route
/// </summary>
public class Route
{
    /// <summary>
    /// The route's distance (meters)
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// The route's duration (seconds)
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map map;

    /// <summary>
    /// The prefab to use for the route nodes
    /// </summary>
    private GameObject nodePrefab;

    /// <summary>
    /// The prefab to use for the route start node
    /// </summary>
    private GameObject startNodePrefab;

    /// <summary>
    /// The prefab to use for the route end node
    /// </summary>
    private GameObject endNodePrefab;

    /// <summary>
    /// The route's GameObject representation
    /// </summary>
    private GameObject gameObject;

    /// <summary>
    /// The number of nodes in the route
    /// </summary>
    private int count;

    /// <summary>
    /// The route's start point
    /// </summary>
    private Node start;

    /// <summary>
    /// The route's end point
    /// </summary>
    private Node end;

    /// <summary>
    /// The route's path start point (for intermediate nodes)
    /// </summary>
    private Node head;

    /// <summary>
    /// The route's path end point (for intermediate nodes)
    /// </summary>
    private Node tail;

    /// <summary>
    /// Constructs a new route
    /// </summary>
    /// <param name="map">Reference to the map</param>
    public Route(Map map)
    {
        this.map = map;
        this.nodePrefab = Resources.Load<GameObject>($"UI/Route Node"); // TODO use Addressables instead?
        this.startNodePrefab = Resources.Load<GameObject>($"UI/Route Start Node"); // TODO use Addressables instead?
        this.endNodePrefab = Resources.Load<GameObject>($"UI/Route End Node"); // TODO use Addressables instead?

        this.gameObject = new GameObject("Route");
        this.gameObject.transform.parent = map.GameObject.transform; // Set it as a child of the map gameobject
        this.gameObject.transform.rotation = map.GameObject.transform.rotation; // Match route rotation with the map

        this.count = 0;
        this.Distance = 0;
        this.Duration = 0;
    }

    /// <summary>
    /// Adds a node to the route
    /// </summary>
    /// <param name="coordinates">The coordinates of the node (lat/lon)</param>
    /// <returns>The node ID</returns>
    public int AddNode(Vector2D coordinates)
    {
        // Create a new node
        Node node = new Node(this, coordinates, nodePrefab);

        // Add it to the route
        if (head == null)
        {
            // First node
            head = node;
            tail = node;
            count++;
        }
        else
        {
            // Add the node to the end of the route
            tail.Next = node;
            node.Previous = tail;
            tail = node;
            count++;
        }

        return node.Id;
    }

    /// <summary>
    /// Set the route's start node
    /// </summary>
    /// <param name="coordinates">The coordinates of the node (lat/lon)</param>
    public void SetStart(Vector2D coordinates)
    {
        start = new Node(this, coordinates, startNodePrefab);
    }

    /// <summary>
    /// Set the route's end node
    /// </summary>
    /// <param name="coordinates">The coordinates of the node (lat/lon)</param>
    public void SetEnd(Vector2D coordinates)
    {
        end = new Node(this, coordinates, endNodePrefab);
    }

    /// <summary>
    /// Clears the route
    /// </summary>
    /// <remarks>
    /// This method is an O(n) operation, where n is the number of nodes in the route
    /// </remarks>
    public void Clear()
    {
        // Destroy all nodes
        Node node = head;
        while (node != null)
        {
            Node next = node.Next;
            node.Previous = null;
            node.Next = null;
            node.Destroy();
            node = next;
        }

        // Reset the route
        if (start != null) { start.Destroy(); }
        if (end != null) { end.Destroy(); }
        start = null;
        end = null;
        head = null;
        tail = null;
        count = 0;
        Distance = 0;
        Duration = 0;
    }

    /// <summary>
    /// Move the route according to the given vector
    /// </summary>
    /// <param name="delta">The vector to move the route by</param>
    internal void Move(Vector2D delta)
    {
        if (start != null) { start.Move(delta); }
        if (end != null) { end.Move(delta); }
        Node node = head;
        while (node != null)
        {
            node.Move(delta);
            node = node.Next;
        }
    }

    /// <summary>
    /// Zoom the route according to the map's zoom level
    /// </summary>
    internal void Zoom()
    {
        if (start != null) { start.Zoom(); }
        if (end != null) { end.Zoom(); }
        Node node = head;
        while (node != null)
        {
            node.Zoom();
            node = node.Next;
        }
    }

    /// <summary>
    /// Represents a node in the route
    /// </summary>
    private class Node
    {
        /// <summary>
        /// The node's width at zoom level 0
        /// </summary>
        /// <remarks>
        /// This value is used to calculate the node's width at other zoom levels.
        /// This was chosen because it's 50m-ish at zoom level 16 which felt about right.
        /// Yes, we're going with vibes here.
        /// </remarks>
        private const int NodeWidthZ0 = 4194304;

        /// <summary>
        /// Reference to the route
        /// </summary>
        private Route Route { get; }

        /// <summary>
        /// The node's ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The next node in the route
        /// </summary>
        public Node Next { get; internal set; }

        /// <summary>
        /// The previous node in the route
        /// </summary>
        public Node Previous
        {
            get { return previous; }
            internal set
            {
                // Update the previous node
                previous = value;

                // Update the edge
                if (previous == null)
                {
                    // The node is the head and there's no previous node, so hide the edge
                    edgeTransform.gameObject.SetActive(false);
                }
                else
                {
                    // Calculate the vector between the nodes
                    Vector2D edgeVector = CoordinatesMeters - previous.CoordinatesMeters;

                    // Calculate the magnitude and scale it to take the node's width into account
                    double magnitude = edgeVector.Magnitude;
                    magnitude /= NodeWidthZ0 >> Route.map.ZoomLevel;

                    // Calculate the angle
                    double angle = Vector2D.SignedAngle(Vector2D.Right, edgeVector);

                    // Update the edge's position
                    edgeTransform.localPosition = new Vector3((float)(magnitude / 2), 0, 0);

                    // Update the edge's rotation
                    edgeTransform.localRotation = Quaternion.identity;
                    edgeTransform.RotateAround(nodeTransform.position, Vector3.down, (float)angle);

                    // Update the edge's scale
                    edgeTransform.localScale = new Vector3((float)magnitude, 0.5f, 1);

                    // Make sure the edge is visible
                    edgeTransform.gameObject.SetActive(true);
                }
            }
        }
        private Node previous;

        /// <summary>
        /// The node's coordinates (lat/lon)
        /// </summary>
        public Vector2D Coordinates
        {
            get
            {
                return coordinates;
            }
            internal set
            {
                coordinates = value;
                this.CoordinatesMeters = GlobalMercator.LatLonToMeters(Coordinates.X, Coordinates.Y);
                // HACK: This is a hack to make sure the node is updated when the coordinates are set
                Vector2D relativePos = Route.map.GetRelativePosition(CoordinatesMeters);
                nodeTransform.transform.localPosition = new UnityEngine.Vector3((float)relativePos.X, 0, (float)relativePos.Y);
                if (Previous != null)
                {
                    Previous = Previous;
                }
                if (Next != null)
                {
                    Next.Previous = this;
                }
            }
        }
        private Vector2D coordinates;

        /// <summary>
        /// The node's coordinates (meters)
        /// </summary>
        public Vector2D CoordinatesMeters { get; private set; }

        /// <summary>
        /// The node's transform
        /// </summary>
        private Transform nodeTransform;

        /// <summary>
        /// The edge's transform
        /// </summary>
        private Transform edgeTransform;

        /// <summary>
        /// Constructs a new node
        /// </summary>
        /// <param name="route">Reference to the route</param>
        /// <param name="coordinates">The node's coordinates (lat/lon)</param>
        public Node(Route route, Vector2D coordinates, GameObject prefab)
        {
            this.Route = route;
            this.Id = Route.count++;

            // Setup the node GameObject
            nodeTransform = GameObject.Instantiate(prefab).transform;
            nodeTransform.name = $"{Id}";
            nodeTransform.transform.parent = Route.gameObject.transform; // Set it as a child of the route gameobject
            this.Coordinates = coordinates;
            nodeTransform.transform.localScale = new Vector3(NodeWidthZ0 >> Route.map.ZoomLevel, NodeWidthZ0 >> Route.map.ZoomLevel, 1);
            // Setup the edge GameObject
            edgeTransform = nodeTransform.Find("Edge");
            edgeTransform.name = $"{Id}";
        }

        /// <summary>
        /// Destroys the node
        /// </summary>
        public void Destroy()
        {
            // Destroy the node's GameObject
            GameObject.Destroy(nodeTransform.gameObject);
        }

        /// <summary>
        /// Moves the node according to the given vector
        /// </summary>
        /// <param name="delta">The vector to move the node by</param>
        internal void Move(Vector2D delta)
        {
            nodeTransform.transform.localPosition += new Vector3((float)delta.X, 0, (float)delta.Y);
        }

        /// <summary>
        /// Zooms the node according to the map's zoom level
        /// </summary>
        internal void Zoom()
        {
            // Update the node's scale
            nodeTransform.transform.localScale = new Vector3(NodeWidthZ0 >> Route.map.ZoomLevel, NodeWidthZ0 >> Route.map.ZoomLevel, 1);

            // Update the edge if there's a previous node
            if (previous != null)
            {
                // Calculate the vector between the nodes
                Vector2D edgeVector = CoordinatesMeters - previous.CoordinatesMeters;

                // Calculate the magnitude and scale it to take the node's width into account
                double magnitude = edgeVector.Magnitude;
                magnitude /= NodeWidthZ0 >> Route.map.ZoomLevel;

                // Calculate the angle
                double angle = Vector2D.SignedAngle(Vector2D.Right, edgeVector);

                // Update the edge's position
                edgeTransform.localPosition = new Vector3((float)(magnitude / 2), 0, 0);

                // Update the edge's rotation
                edgeTransform.localRotation = Quaternion.identity;
                edgeTransform.RotateAround(nodeTransform.position, Vector3.down, (float)angle);

                // Update the edge's scale
                edgeTransform.localScale = new Vector3((float)magnitude, 0.5f, 1);
            }
        }
    }
}
