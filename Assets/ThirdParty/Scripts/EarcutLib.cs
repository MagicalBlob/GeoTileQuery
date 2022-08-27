using System;
using System.Collections.Generic;

/// <summary>
/// C# Port of earcut.js v2.2.3 ( https://github.com/mapbox/earcut/releases/tag/v2.2.3 )
/// </summary>
public class EarcutLib
{
    /// <summary>
    /// Triangulates the given polygon
    /// </summary>
    /// <param name="vertices">A flat list of vertex coordinates like [x0,y0, x1,y1, x2,y2, ...]</param>
    /// <param name="holes">A list of hole indices if any</param>
    /// <param name="dimensions">The number of coordinates per vertex in the input list (minimum is 2).</param>
    /// <returns>A flat list of vertex indices, with each group of three forming a triangle</returns>
    public static List<int> Earcut(IList<double> vertices, IList<int> holes, int dimensions)
    {
        dimensions = Math.Max(dimensions, 2);

        bool hasHoles = holes != null && holes.Count > 0;
        int outerLen = hasHoles ? holes[0] * dimensions : vertices.Count;
        Node outerNode = LinkedList(vertices, 0, outerLen, dimensions, true);
        List<int> triangles = new List<int>();

        if (outerNode == null || outerNode.next == outerNode.prev) { return triangles; }

        double invSize = 0;
        double minX = 0;
        double minY = 0;
        double maxX, maxY, x, y;

        if (hasHoles) { outerNode = EliminateHoles(vertices, holes, outerNode, dimensions); }

        // if the shape is not too simple, we'll use z-order curve hash later; calculate polygon bbox
        if (vertices.Count > 80 * dimensions)
        {
            minX = maxX = vertices[0];
            minY = maxY = vertices[1];

            for (int i = dimensions; i < outerLen; i += dimensions)
            {
                x = vertices[i];
                y = vertices[i + 1];
                if (x < minX) { minX = x; }
                if (y < minY) { minY = y; }
                if (x > maxX) { maxX = x; }
                if (y > maxY) { maxY = y; }
            }

            // minX, minY and invSize are later used to transform coords into integers for z-order calculation
            invSize = Math.Max(maxX - minX, maxY - minY);
            invSize = invSize != 0 ? 1 / invSize : 0;
        }

        EarcutLinked(outerNode, triangles, dimensions, minX, minY, invSize);

        return triangles;
    }

    // create a circular doubly linked list from polygon points in the specified winding order
    private static Node LinkedList(IList<double> data, int start, int end, int dimensions, bool clockwise)
    {
        int i = 0;
        Node last = null;

        if (clockwise == (SignedArea(data, start, end, dimensions) > 0))
        {
            for (i = start; i < end; i += dimensions)
            {
                last = InsertNode(i, data[i], data[i + 1], last);
            }
        }
        else
        {
            for (i = end - dimensions; i >= start; i -= dimensions)
            {
                last = InsertNode(i, data[i], data[i + 1], last);
            }
        }

        if (last != null && Equals(last, last.next))
        {
            RemoveNode(last);
            last = last.next;
        }

        return last;
    }

    // eliminate colinear or duplicate points
    private static Node FilterPoints(Node start, Node end = null)
    {
        if (start == null) { return start; }
        if (end == null) { end = start; }

        Node p = start;
        bool again;
        do
        {
            again = false;

            if (!p.steiner && (Equals(p, p.next) || Area(p.prev, p, p.next) == 0))
            {
                RemoveNode(p);
                p = end = p.prev;
                if (p == p.next) { break; }
                again = true;
            }
            else
            {
                p = p.next;
            }
        } while (again || p != end);

        return end;
    }

    // main ear slicing loop which triangulates a polygon (given as a linked list)
    private static void EarcutLinked(Node ear, IList<int> triangles, int dimensions, double minX, double minY, double invSize, int pass = 0)
    {
        if (ear == null) { return; }

        // interlink polygon nodes in z-order
        if (pass == 0 && invSize != 0)
        {
            IndexCurve(ear, minX, minY, invSize);
        }

        Node stop = ear;
        Node prev, next;

        // iterate through ears, slicing them one by one
        while (ear.prev != ear.next)
        {
            prev = ear.prev;
            next = ear.next;

            if (invSize != 0 ? IsEarHashed(ear, minX, minY, invSize) : IsEar(ear))
            {
                // cut off the triangle
                triangles.Add(prev.i / dimensions);
                triangles.Add(ear.i / dimensions);
                triangles.Add(next.i / dimensions);

                RemoveNode(ear);

                // skipping the next vertex leads to less sliver triangles
                ear = next.next;
                stop = next.next;

                continue;
            }

            ear = next;

            // if we looped through the whole remaining polygon and can't find any more ears
            if (ear == stop)
            {
                // try filtering points and slicing again
                if (pass == 0)
                {
                    EarcutLinked(FilterPoints(ear), triangles, dimensions, minX, minY, invSize, 1);

                    // if this didn't work, try curing all small self-intersections locally
                }
                else if (pass == 1)
                {
                    ear = CureLocalIntersections(FilterPoints(ear), triangles, dimensions);
                    EarcutLinked(ear, triangles, dimensions, minX, minY, invSize, 2);

                    // as a last resort, try splitting the remaining polygon into two
                }
                else if (pass == 2)
                {
                    SplitEarcut(ear, triangles, dimensions, minX, minY, invSize);
                }

                break;
            }
        }
    }

    // check whether a polygon node forms a valid ear with adjacent nodes
    private static bool IsEar(Node ear)
    {
        Node a = ear.prev;
        Node b = ear;
        Node c = ear.next;

        if (Area(a, b, c) >= 0) { return false; } // reflex, can't be an ear

        // now make sure we don't have other points inside the potential ear
        Node p = ear.next.next;

        while (p != ear.prev)
        {
            if (PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                Area(p.prev, p, p.next) >= 0)
            {
                return false;
            }
            p = p.next;
        }

        return true;
    }

    private static bool IsEarHashed(Node ear, double minX, double minY, double invSize)
    {
        Node a = ear.prev;
        Node b = ear;
        Node c = ear.next;

        if (Area(a, b, c) >= 0) { return false; } // reflex, can't be an ear

        // triangle bbox; min & max are calculated like this for speed
        double minTX = a.x < b.x ? (a.x < c.x ? a.x : c.x) : (b.x < c.x ? b.x : c.x);
        double minTY = a.y < b.y ? (a.y < c.y ? a.y : c.y) : (b.y < c.y ? b.y : c.y);
        double maxTX = a.x > b.x ? (a.x > c.x ? a.x : c.x) : (b.x > c.x ? b.x : c.x);
        double maxTY = a.y > b.y ? (a.y > c.y ? a.y : c.y) : (b.y > c.y ? b.y : c.y);

        // z-order range for the current triangle bbox;
        int minZ = ZOrder(minTX, minTY, minX, minY, invSize);
        int maxZ = ZOrder(maxTX, maxTY, minX, minY, invSize);

        Node p = ear.prevZ;
        Node n = ear.nextZ;

        // look for points inside the triangle in both directions
        while (p != null && p.z >= minZ && n != null && n.z <= maxZ)
        {
            if (p != ear.prev && p != ear.next &&
                PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                Area(p.prev, p, p.next) >= 0)
            {
                return false;
            }
            p = p.prevZ;

            if (n != ear.prev && n != ear.next &&
                PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, n.x, n.y) &&
                Area(n.prev, n, n.next) >= 0)
            {
                return false;
            }
            n = n.nextZ;
        }

        // look for remaining points in decreasing z-order
        while (p != null && p.z >= minZ)
        {
            if (p != ear.prev && p != ear.next &&
                PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) &&
                Area(p.prev, p, p.next) >= 0)
            {
                return false;
            }
            p = p.prevZ;
        }

        // look for remaining points in increasing z-order
        while (n != null && n.z <= maxZ)
        {
            if (n != ear.prev && n != ear.next &&
                PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, n.x, n.y) &&
                Area(n.prev, n, n.next) >= 0)
            {
                return false;
            }
            n = n.nextZ;
        }

        return true;
    }

    // go through all polygon nodes and cure small local self-intersections
    private static Node CureLocalIntersections(Node start, IList<int> triangles, int dimensions)
    {
        Node p = start;
        do
        {
            Node a = p.prev;
            Node b = p.next.next;

            if (!Equals(a, b) && Intersects(a, p, p.next, b) && LocallyInside(a, b) && LocallyInside(b, a))
            {

                triangles.Add(a.i / dimensions);
                triangles.Add(p.i / dimensions);
                triangles.Add(b.i / dimensions);

                // remove two nodes involved
                RemoveNode(p);
                RemoveNode(p.next);

                p = start = b;
            }
            p = p.next;
        } while (p != start);

        return FilterPoints(p);
    }

    // try splitting polygon into two and triangulate them independently
    private static void SplitEarcut(Node start, IList<int> triangles, int dimensions, double minX, double minY, double invSize)
    {
        // look for a valid diagonal that divides the polygon into two
        Node a = start;
        do
        {
            Node b = a.next.next;
            while (b != a.prev)
            {
                if (a.i != b.i && IsValidDiagonal(a, b))
                {
                    // split the polygon in two by the diagonal
                    Node c = SplitPolygon(a, b);

                    // filter colinear points around the cuts
                    a = FilterPoints(a, a.next);
                    c = FilterPoints(c, c.next);

                    // run earcut on each half
                    EarcutLinked(a, triangles, dimensions, minX, minY, invSize);
                    EarcutLinked(c, triangles, dimensions, minX, minY, invSize);
                    return;
                }
                b = b.next;
            }
            a = a.next;
        } while (a != start);
    }

    // link every hole into the outer loop, producing a single-ring polygon without holes
    private static Node EliminateHoles(IList<double> data, IList<int> holeIndices, Node outerNode, int dimensions)
    {
        int len = holeIndices.Count;
        List<Node> queue = new List<Node>(len);

        for (int i = 0; i < len; i++)
        {
            int start = holeIndices[i] * dimensions;
            int end = i < len - 1 ? holeIndices[i + 1] * dimensions : data.Count;
            Node list = LinkedList(data, start, end, dimensions, false);
            if (list == list.next) list.steiner = true;
            queue.Add(GetLeftmost(list));
        }

        queue.Sort(CompareX);

        // process holes from left to right
        for (int i = 0; i < queue.Count; i++)
        {
            outerNode = EliminateHole(queue[i], outerNode);
            outerNode = FilterPoints(outerNode, outerNode.next);
        }

        return outerNode;
    }

    private static int CompareX(Node a, Node b)
    {
        return Math.Sign(a.x - b.x);
    }

    // find a bridge between vertices that connects hole with an outer ring and and link it
    private static Node EliminateHole(Node hole, Node outerNode)
    {
        Node bridge = FindHoleBridge(hole, outerNode);
        if (bridge == null)
        {
            return outerNode;
        }

        Node bridgeReverse = SplitPolygon(bridge, hole);

        // filter collinear points around the cuts
        Node filteredBridge = FilterPoints(bridge, bridge.next);
        FilterPoints(bridgeReverse, bridgeReverse.next);

        // Check if input node was removed by the filtering
        return outerNode == bridge ? filteredBridge : outerNode;
    }

    // David Eberly's algorithm for finding a bridge between hole and outer polygon
    private static Node FindHoleBridge(Node hole, Node outerNode)
    {
        Node p = outerNode;
        double hx = hole.x;
        double hy = hole.y;
        double qx = double.NegativeInfinity;
        Node m = null;

        // find a segment intersected by a ray from the hole's leftmost point to the left;
        // segment's endpoint with lesser x will be potential connection point
        do
        {
            if (hy <= p.y && hy >= p.next.y && p.next.y != p.y)
            {
                double x = p.x + (hy - p.y) * (p.next.x - p.x) / (p.next.y - p.y);
                if (x <= hx && x > qx)
                {
                    qx = x;
                    if (x == hx)
                    {
                        if (hy == p.y) { return p; }
                        if (hy == p.next.y) { return p.next; }
                    }
                    m = p.x < p.next.x ? p : p.next;
                }
            }
            p = p.next;
        } while (p != outerNode);

        if (m == null)
        {
            return null;
        }

        if (hx == qx) return m; // hole touches outer segment; pick leftmost endpoint

        // look for points inside the triangle of hole point, segment intersection and endpoint;
        // if there are no points found, we have a valid connection;
        // otherwise choose the point of the minimum angle with the ray as connection point

        Node stop = m;
        double mx = m.x;
        double my = m.y;
        double tanMin = double.PositiveInfinity;
        double tan;

        p = m;

        do
        {
            if (hx >= p.x && p.x >= mx && hx != p.x &&
                PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y))
            {

                tan = Math.Abs(hy - p.y) / (hx - p.x); // tangential

                if (LocallyInside(p, hole) &&
                    (tan < tanMin || (tan == tanMin && (p.x > m.x || (p.x == m.x && SectorContainsSector(m, p))))))
                {
                    m = p;
                    tanMin = tan;
                }
            }

            p = p.next;
        } while (p != stop);

        return m;
    }

    // whether sector in vertex m contains sector in vertex p in the same coordinates
    private static bool SectorContainsSector(Node m, Node p)
    {
        return Area(m.prev, m, p.prev) < 0 && Area(p.next, m, m.next) < 0;
    }

    // interlink polygon nodes in z-order
    private static void IndexCurve(Node start, double minX, double minY, double invSize)
    {
        Node p = start;
        do
        {
            if (p.z == null)
            {
                p.z = ZOrder(p.x, p.y, minX, minY, invSize);
            }
            p.prevZ = p.prev;
            p.nextZ = p.next;
            p = p.next;
        } while (p != start);

        p.prevZ.nextZ = null;
        p.prevZ = null;

        SortLinked(p);
    }

    // Simon Tatham's linked list merge sort algorithm
    // http://www.chiark.greenend.org.uk/~sgtatham/algorithms/listsort.html
    private static Node SortLinked(Node list)
    {
        Node p, q, e, tail;
        int numMerges;
        int pSize, qSize;
        int inSize = 1;

        do
        {
            p = list;
            list = null;
            tail = null;
            numMerges = 0;

            while (p != null)
            {
                numMerges++;
                q = p;
                pSize = 0;
                for (int i = 0; i < inSize; i++)
                {
                    pSize++;
                    q = q.nextZ;
                    if (q == null) break;
                }
                qSize = inSize;

                while (pSize > 0 || (qSize > 0 && q != null))
                {

                    if (pSize != 0 && (qSize == 0 || q == null || p.z <= q.z))
                    {
                        e = p;
                        p = p.nextZ;
                        pSize--;
                    }
                    else
                    {
                        e = q;
                        q = q.nextZ;
                        qSize--;
                    }

                    if (tail != null)
                    {
                        tail.nextZ = e;
                    }
                    else
                    {
                        list = e;
                    }

                    e.prevZ = tail;
                    tail = e;
                }

                p = q;
            }

            tail.nextZ = null;
            inSize *= 2;

        } while (numMerges > 1);

        return list;
    }

    // z-order of a point given coords and inverse of the longer side of data bbox
    private static int ZOrder(double x, double y, double minX, double minY, double invSize)
    {
        // coords are transformed into non-negative 15-bit integer range
        int xI = (int)(32767 * (x - minX) * invSize);
        int yI = (int)(32767 * (y - minY) * invSize);

        xI = (xI | (xI << 8)) & 0x00FF00FF;
        xI = (xI | (xI << 4)) & 0x0F0F0F0F;
        xI = (xI | (xI << 2)) & 0x33333333;
        xI = (xI | (xI << 1)) & 0x55555555;

        yI = (yI | (yI << 8)) & 0x00FF00FF;
        yI = (yI | (yI << 4)) & 0x0F0F0F0F;
        yI = (yI | (yI << 2)) & 0x33333333;
        yI = (yI | (yI << 1)) & 0x55555555;

        return xI | (yI << 1);
    }

    // find the leftmost node of a polygon ring
    private static Node GetLeftmost(Node start)
    {
        Node p = start;
        Node leftmost = start;
        do
        {
            if (p.x < leftmost.x || (p.x == leftmost.x && p.y < leftmost.y))
            {
                leftmost = p;
            }
            p = p.next;
        } while (p != start);

        return leftmost;
    }

    // check if a point lies within a convex triangle
    private static bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
    {
        return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
               (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
               (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
    }

    // check if a diagonal between two polygon nodes is valid (lies in polygon interior)
    private static bool IsValidDiagonal(Node a, Node b)
    {
        return a.next.i != b.i && a.prev.i != b.i && !IntersectsPolygon(a, b) && // dones't intersect other edges
            (LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b) && // locally visible
            (Area(a.prev, a, b.prev) != 0 || Area(a, b.prev, b) != 0) || // does not create opposite-facing sectors
            Equals(a, b) && Area(a.prev, a, a.next) > 0 && Area(b.prev, b, b.next) > 0); // special zero-length case
    }

    // signed area of a triangle
    private static double Area(Node p, Node q, Node r)
    {
        return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
    }

    // check if two points are equal
    private static bool Equals(Node p1, Node p2)
    {
        return p1.x == p2.x && p1.y == p2.y;
    }

    // check if two segments intersect
    private static bool Intersects(Node p1, Node q1, Node p2, Node q2)
    {
        int o1 = Sign(Area(p1, q1, p2));
        int o2 = Sign(Area(p1, q1, q2));
        int o3 = Sign(Area(p2, q2, p1));
        int o4 = Sign(Area(p2, q2, q1));

        if (o1 != o2 && o3 != o4) { return true; }// general case

        if (o1 == 0 && OnSegment(p1, p2, q1)) { return true; } // p1, q1 and p2 are collinear and p2 lies on p1q1
        if (o2 == 0 && OnSegment(p1, q2, q1)) { return true; } // p1, q1 and q2 are collinear and q2 lies on p1q1
        if (o3 == 0 && OnSegment(p2, p1, q2)) { return true; } // p2, q2 and p1 are collinear and p1 lies on p2q2
        if (o4 == 0 && OnSegment(p2, q1, q2)) { return true; } // p2, q2 and q1 are collinear and q1 lies on p2q2

        return false;
    }

    // for collinear points p, q, r, check if point q lies on segment pr
    private static bool OnSegment(Node p, Node q, Node r)
    {
        return q.x <= Math.Max(p.x, r.x) && q.x >= Math.Min(p.x, r.x) && q.y <= Math.Max(p.y, r.y) && q.y >= Math.Min(p.y, r.y);
    }

    private static int Sign(double num)
    {
        return num > 0 ? 1 : num < 0 ? -1 : 0;
    }

    // check if a polygon diagonal intersects any polygon segments
    private static bool IntersectsPolygon(Node a, Node b)
    {
        Node p = a;
        do
        {
            if (p.i != a.i && p.next.i != a.i && p.i != b.i && p.next.i != b.i && Intersects(p, p.next, a, b))
            {
                return true;
            }

            p = p.next;
        } while (p != a);

        return false;
    }

    // check if a polygon diagonal is locally inside the polygon
    private static bool LocallyInside(Node a, Node b)
    {
        return Area(a.prev, a, a.next) < 0 ?
            Area(a, b, a.next) >= 0 && Area(a, a.prev, b) >= 0 :
            Area(a, b, a.prev) < 0 || Area(a, a.next, b) < 0;
    }

    // check if the middle point of a polygon diagonal is inside the polygon
    private static bool MiddleInside(Node a, Node b)
    {
        Node p = a;
        bool inside = false;
        double px = (a.x + b.x) / 2;
        double py = (a.y + b.y) / 2;
        do
        {
            if (((p.y > py) != (p.next.y > py)) && p.next.y != p.y &&
                (px < (p.next.x - p.x) * (py - p.y) / (p.next.y - p.y) + p.x))
            {
                inside = !inside;
            }
            p = p.next;
        } while (p != a);

        return inside;
    }

    // link two polygon vertices with a bridge; if the vertices belong to the same ring, it splits polygon into two;
    // if one belongs to the outer ring and another to a hole, it merges it into a single ring
    private static Node SplitPolygon(Node a, Node b)
    {
        Node a2 = new Node(a.i, a.x, a.y);
        Node b2 = new Node(b.i, b.x, b.y);
        Node an = a.next;
        Node bp = b.prev;

        a.next = b;
        b.prev = a;

        a2.next = an;
        an.prev = a2;

        b2.next = a2;
        a2.prev = b2;

        bp.next = b2;
        b2.prev = bp;

        return b2;
    }

    // create a node and optionally link it with previous one (in a circular doubly linked list)
    private static Node InsertNode(int i, double x, double y, Node last)
    {
        Node p = new Node(i, x, y);

        if (last == null)
        {
            p.prev = p;
            p.next = p;

        }
        else
        {
            p.next = last.next;
            p.prev = last;
            last.next.prev = p;
            last.next = p;
        }

        return p;
    }

    private static void RemoveNode(Node p)
    {
        p.next.prev = p.prev;
        p.prev.next = p.next;

        if (p.prevZ != null)
        {
            p.prevZ.nextZ = p.nextZ;
        }

        if (p.nextZ != null)
        {
            p.nextZ.prevZ = p.prevZ;
        }
    }

    private class Node
    {
        public int i;
        public double x;
        public double y;
        public Node prev;
        public Node next;
        public int? z;
        public Node prevZ;
        public Node nextZ;
        public bool steiner;

        public Node(int i, double x, double y)
        {
            // vertex index in coordinates array
            this.i = i;

            // vertex coordinates
            this.x = x;
            this.y = y;

            // previous and next vertex nodes in a polygon ring
            this.prev = null;
            this.next = null;

            // z-order curve value
            this.z = null;

            // previous and next nodes in z-order
            this.prevZ = null;
            this.nextZ = null;

            // indicates whether this is a steiner point
            this.steiner = false;
        }
    }

    /// <summary>
    /// Used to verify correctness of triangulation
    /// </summary>
    /// <param name="vertices">A flat list of vertex coordinates like [x0,y0, x1,y1, x2,y2, ...]</param>
    /// <param name="holes">A list of hole indices if any</param>
    /// <param name="dimensions">The number of coordinates per vertex in the input list (minimum is 2).</param>
    /// <param name="triangles">A flat list of vertex indices, with each group of three forming a triangle</param>
    /// <returns>A percentage difference between the polygon area and its triangulation area, 0 means the triangulation is fully correct.</returns>
    public static double Deviation(IList<double> vertices, IList<int> holes, int dimensions, IList<int> triangles)
    {
        bool hasHoles = holes != null && holes.Count > 0;
        int outerLen = hasHoles ? holes[0] * dimensions : vertices.Count;

        double polygonArea = Math.Abs(SignedArea(vertices, 0, outerLen, dimensions));
        if (hasHoles)
        {
            int len = holes.Count;

            for (int i = 0; i < len; i++)
            {
                int start = holes[i] * dimensions;
                int end = i < len - 1 ? holes[i + 1] * dimensions : vertices.Count;
                polygonArea -= Math.Abs(SignedArea(vertices, start, end, dimensions));
            }
        }

        double trianglesArea = 0;
        for (int i = 0; i < triangles.Count; i += 3)
        {
            int a = triangles[i] * dimensions;
            int b = triangles[i + 1] * dimensions;
            int c = triangles[i + 2] * dimensions;
            trianglesArea += Math.Abs(
                (vertices[a] - vertices[c]) * (vertices[b + 1] - vertices[a + 1]) -
                (vertices[a] - vertices[b]) * (vertices[c + 1] - vertices[a + 1]));
        }

        return polygonArea == 0 && trianglesArea == 0 ? 0 :
            Math.Abs((trianglesArea - polygonArea) / polygonArea);
    }

    private static double SignedArea(IList<double> data, int start, int end, int dimensions)
    {
        double sum = 0;
        for (int i = start, j = end - dimensions; i < end; i += dimensions)
        {
            sum += (data[j] - data[i]) * (data[i + 1] + data[j + 1]);
            j = i;
        }
        return sum;
    }

    /// <summary>
    /// Turn a polygon in a multi-dimensional array form (e.g. as in GeoJSON) into a form Earcut accepts
    /// </summary>
    /// <param name="coordinates">The coordinates for each point of each linear ring of the GeoJSON Polygon</param>
    /// <param name="tileLayer">The feature's tile layer</param>
    /// <returns>A wrapper object containing the polygon vertices, holes and dimensions to use with Earcut</returns>
    public static Data Flatten(Position[][] coordinates, GeoJsonTileLayer tileLayer)
    {
        int dimensions = coordinates[0][0].Dimensions;

        List<int> holes = new List<int>(coordinates.Length - 1);

        int holeIndex = 0;
        int numVertices = 0;
        for (int ring = 0; ring < coordinates.Length; ring++)
        {
            numVertices += coordinates[ring].Length;

            if (ring > 0)
            {
                holeIndex += coordinates[ring - 1].Length;
                holes.Add(holeIndex);
            }
        }

        List<double> vertices = new List<double>(numVertices * dimensions);
        for (int ring = 0; ring < coordinates.Length; ring++)
        {
            for (int position = 0; position < coordinates[ring].Length; position++)
            {
                vertices.Add(coordinates[ring][position].GetRelativeX(tileLayer.Tile.Bounds.Min.X));
                vertices.Add(coordinates[ring][position].GetRelativeY(tileLayer.Tile.Bounds.Min.Y));
                vertices.Add(coordinates[ring][position].GetRelativeZ());
            }

        }

        return new Data(vertices, holes, dimensions);
    }

    /// <summary>
    /// A wrapper object containing the polygon vertices, holes and dimensions to use with Earcut
    /// </summary>
    public class Data
    {
        /// <summary>
        /// A flat list of vertex coordinates like [x0,y0, x1,y1, x2,y2, ...]
        /// </summary>
        public IList<double> Vertices { get; }
        /// <summary>
        /// A list of hole indices if any
        /// </summary>
        public IList<int> Holes { get; }
        /// <summary>
        /// The number of coordinates per vertex in the input list
        /// </summary>
        public int Dimensions { get; }

        public Data(IList<double> vertices, IList<int> holes, int dimensions)
        {
            this.Vertices = vertices;
            this.Holes = holes;
            this.Dimensions = dimensions;
        }
    }
}