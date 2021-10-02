using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a warning message
/// </summary>
public class WarningMessage : ILogMessage
{
    /// <summary>
    /// The message's timestamp
    /// </summary>
    private DateTime timestamp;

    /// <summary>
    /// The message content
    /// </summary>
    private object content;

    /// <summary>
    /// Creates a new WarningMessage
    /// </summary>
    /// <param name="message">The message content</param>
    public WarningMessage(object message)
    {
        this.timestamp = DateTime.Now;
        this.content = message;
    }

    public void Render(GameObject parent)
    {
        GameObject message = new GameObject("WarningMessage");

        Text text = message.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.yellow;
        text.text = ToString();

        (message.GetComponent<RectTransform>()).sizeDelta = new Vector2(Logger.messageWidth, 0);
        message.transform.SetParent(parent.transform, false);
    }

    public override string ToString()
    {
        return $"[{timestamp.ToString("HH:mm:ss")}] [WARNING] {content.ToString()}";
    }
}