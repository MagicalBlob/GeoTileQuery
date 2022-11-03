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
    /// Adds a node to the ruler
    /// </summary>
    /// <param name="coordinates">The coordinates of the node (lat/lon)</param>
    public void AddNode(Vector2D coordinates)
    {
        throw new System.NotImplementedException();
    }

    public void AddNodeBefore(int nextId, Vector2D coordinates)
    {
        throw new System.NotImplementedException();
    }

    public void MoveNode(int id, Vector2D coordinates)
    {
        throw new System.NotImplementedException();
    }

    public void RemoveNode(int id)
    {
        throw new System.NotImplementedException();
    }

    public void Clear()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Move the ruler according to the given vector
    /// </summary>
    /// <param name="delta">The vector to move the ruler by</param>
    internal void Move(Vector2D delta)
    {
        throw new System.NotImplementedException();
    }
}
