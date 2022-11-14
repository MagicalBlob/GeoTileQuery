using UnityEngine;

/// <summary>
/// Represents the ruler
/// </summary>
public class Ruler
{
    /// <summary>
    /// The ruler's currently measured distance (meters)
    /// </summary>
    public double Length { get; private set; }

    /// <summary>
    /// Reference to the map
    /// </summary>
    private Map map;

    /// <summary>
    /// The prefab to use for the ruler nodes
    /// </summary>
    private GameObject nodePrefab;

    /// <summary>
    /// The ruler's GameObject representation
    /// </summary>
    private GameObject gameObject;

    /// <summary>
    /// The number of nodes in the ruler
    /// </summary>
    private int count = 0;

    /// <summary>
    /// The ruler's start point
    /// </summary>
    private Node head;

    /// <summary>
    /// The ruler's end point
    /// </summary>
    private Node tail;

    /// <summary>
    /// The id for the next new node in the ruler
    /// </summary>
    private int newNodeId;

    /// <summary>
    /// Constructs a new ruler
    /// </summary>
    /// <param name="map">Reference to the map</param>
    public Ruler(Map map)
    {
        this.map = map;
        this.nodePrefab = Resources.Load<GameObject>($"UI/Ruler Node"); // TODO use Addressables instead?

        this.gameObject = new GameObject("Ruler");
        this.gameObject.transform.parent = map.GameObject.transform; // Set it as a child of the map gameobject
        this.gameObject.transform.rotation = map.GameObject.transform.rotation; // Match ruler rotation with the map
    }

    /// <summary>
    /// Adds a node to the ruler
    /// </summary>
    /// <param name="coordinates">The coordinates of the node (lat/lon)</param>
    /// <returns>The node ID</returns>
    public int AddNode(Vector2D coordinates)
    {
        // Create a new node
        Node node = new Node(this, coordinates);

        // Add it to the ruler
        if (count == 0)
        {
            // First node
            head = node;
            tail = node;
            count++;
        }
        else
        {
            // Add the node to the end of the ruler
            tail.Next = node;
            node.Previous = tail;
            tail = node;
            count++;
        }

        return node.Id;
    }

    /// <summary>
    /// Adds a node to the ruler before the given node
    /// </summary>
    /// <param name="nextId">The next node's ID</param>
    /// <param name="coordinates">The coordinates of the node (lat/lon)</param>
    /// <returns>The node ID</returns>
    public int AddNodeBefore(int nextId, Vector2D coordinates)
    {
        // Find the next node
        Node next = FindNode(nextId);

        if (next == null)
        {
            Debug.LogWarning($"Ruler: Could not find node with id {nextId}. Adding node at the end of the ruler instead");
            return AddNode(coordinates);
        }

        // Create a new node
        Node node = new Node(this, coordinates);

        // Add it to the ruler
        if (next.Previous == null)
        {
            // The next node is the head
            head = node;
            node.Next = next;
            next.Previous = node;
            count++;
        }
        else
        {
            // The next node is not the head
            node.Previous = next.Previous;
            node.Next = next;
            node.Previous.Next = node;
            next.Previous = node;
            count++;
        }

        return node.Id;
    }

    /// <summary>
    /// Finds a node in the ruler
    /// </summary>
    /// <param name="id">The node's ID</param>
    /// <returns>The node or null if not found</returns>
    private Node FindNode(int id)
    {
        Node node = head;

        while (node != null)
        {
            if (node.Id == id)
            {
                return node;
            }

            node = node.Next;
        }

        return null;
    }

    /// <summary>
    /// Moves the node to the given coordinates
    /// </summary>
    /// <param name="id">The node's ID</param>
    /// <param name="coordinates">The coordinates of the node (meters)</param>
    public void MoveNode(int id, Vector2D coordinates)
    {
        // Find the node
        Node node = FindNode(id);

        if (node == null)
        {
            Debug.LogWarning($"Ruler: Could not find node with id {id}. Nothing to move");
            return;
        }

        // Move the node
        node.Coordinates = coordinates;
    }

    /// <summary>
    /// Removes the node
    /// </summary>
    /// <param name="id">The node's ID</param>
    public void RemoveNode(int id)
    {
        // Find the node
        Node node = FindNode(id);

        if (node == null)
        {
            Debug.LogWarning($"Ruler: Could not find node with id {id}. Nothing to remove");
            return;
        }

        // Remove it from the ruler
        if (node.Previous == null)
        {
            // The node is the head
            head = node.Next;
            if (head != null)
            {
                head.Previous = null;
            }
            count--;
        }
        else if (node.Next == null)
        {
            // The node is the tail
            tail = node.Previous;
            if (tail != null)
            {
                tail.Next = null;
            }
            count--;
        }
        else
        {
            // The node is not the head nor the tail
            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;
            count--;
        }

        // Destroy the node
        node.Destroy();
    }

    /// <summary>
    /// Clears the ruler
    /// </summary>
    /// <remarks>
    /// This method is an O(n) operation, where n is the number of nodes in the ruler
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

        // Reset the ruler
        head = null;
        tail = null;
        count = 0;
        Length = 0;
        newNodeId = 0;
    }

    /// <summary>
    /// Move the ruler according to the given vector
    /// </summary>
    /// <param name="delta">The vector to move the ruler by</param>
    internal void Move(Vector2D delta)
    {
        Node node = head;
        while (node != null)
        {
            node.Move(delta);
            node = node.Next;
        }
    }

    /// <summary>
    /// Zoom the ruler according to the map's zoom level
    /// </summary>
    internal void Zoom()
    {
        Node node = head;
        while (node != null)
        {
            node.Zoom();
            node = node.Next;
        }
    }

    /// <summary>
    /// Represents a node in the ruler
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
        /// Reference to the ruler
        /// </summary>
        private Ruler Ruler { get; }

        /// <summary>
        /// The node's ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The next node in the ruler
        /// </summary>
        public Node Next { get; internal set; }

        /// <summary>
        /// The previous node in the ruler
        /// </summary>
        public Node Previous
        {
            get { return previous; }
            internal set
            {
                // Subtract the old length from the ruler's total length
                Ruler.Length -= Length;

                // Update the previous node
                previous = value;

                // Update the length
                Length = (previous == null) ? 0 : GlobalMercator.Distance(Coordinates, previous.Coordinates);

                // Add the new length to the ruler's total length
                Ruler.Length += Length;

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
                    magnitude /= NodeWidthZ0 >> Ruler.map.ZoomLevel;

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
                Vector2D relativePos = Ruler.map.GetRelativePosition(CoordinatesMeters);
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
        /// The length of line between this node and the previous (meters)
        /// </summary>
        /// <remarks>
        /// 0 if previous is null
        /// </remarks>
        public double Length { get; private set; }

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
        /// <param name="ruler">Reference to the ruler</param>
        /// <param name="coordinates">The node's coordinates (lat/lon)</param>
        public Node(Ruler ruler, Vector2D coordinates)
        {
            this.Ruler = ruler;
            this.Id = Ruler.newNodeId++;

            // Setup the node GameObject
            nodeTransform = GameObject.Instantiate(ruler.nodePrefab).transform;
            nodeTransform.name = $"{Id}";
            nodeTransform.transform.parent = Ruler.gameObject.transform; // Set it as a child of the ruler gameobject
            this.Coordinates = coordinates;
            nodeTransform.transform.localScale = new Vector3(NodeWidthZ0 >> Ruler.map.ZoomLevel, NodeWidthZ0 >> Ruler.map.ZoomLevel, 1);
            // Setup the edge GameObject
            edgeTransform = nodeTransform.Find("Edge");
            edgeTransform.name = $"{Id}";
        }

        /// <summary>
        /// Destroys the node
        /// </summary>
        public void Destroy()
        {
            // Subtract the node's length from the ruler's total length
            Ruler.Length -= Length;

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
            nodeTransform.transform.localScale = new Vector3(NodeWidthZ0 >> Ruler.map.ZoomLevel, NodeWidthZ0 >> Ruler.map.ZoomLevel, 1);

            // Update the edge if there's a previous node
            if (previous != null)
            {
                // Calculate the vector between the nodes
                Vector2D edgeVector = CoordinatesMeters - previous.CoordinatesMeters;

                // Calculate the magnitude and scale it to take the node's width into account
                double magnitude = edgeVector.Magnitude;
                magnitude /= NodeWidthZ0 >> Ruler.map.ZoomLevel;

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
