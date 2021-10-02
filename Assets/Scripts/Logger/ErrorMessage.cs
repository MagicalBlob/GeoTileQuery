using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an error message
/// </summary>
public class ErrorMessage : ILogMessage
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
    /// Creates a new ErrorMessage
    /// </summary>
    /// <param name="message">The message content</param>
    public ErrorMessage(object message)
    {
        this.timestamp = DateTime.Now;
        this.content = message;
    }

    public void Render(GameObject parent)
    {
        GameObject message = new GameObject("ErrorMessage");

        Text text = message.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.red;
        text.text = ToString();

        (message.GetComponent<RectTransform>()).sizeDelta = new Vector2(Logger.messageWidth, 0);
        message.transform.SetParent(parent.transform, false);
    }

    public override string ToString()
    {
        return $"[{timestamp.ToString("HH:mm:ss")}] [ERROR] {content.ToString()}";
    }
}