/// <summary>
/// Represents a map layer
/// </summary>
public interface ILayer
{
    /// <summary>
    /// The layer name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The layer rendering properties
    /// </summary>
    public RenderingProperties Properties { get; }

    //TODO description
    public void Load();
}
