using UnityEngine;

/// <summary>
/// Represents a log message
/// </summary>
public interface ILogMessage
{
    /// <summary>
    /// Render the message as a child of given parent GameObject
    /// </summary>
    /// <param name="parent">The parent GameObject</param>
    void Render(GameObject parent);
}