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
    /// The message context
    /// </summary>
    private UnityEngine.Object context;

    /// <summary>
    /// Creates a new ErrorMessage
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display</param>
    /// <param name="context">Object to which the message applies</param>
    public ErrorMessage(object message, UnityEngine.Object context)
    {
        this.timestamp = DateTime.Now;
        this.content = message;
        this.context = context;
    }

    public void Render(GameObject parent)
    {
        GameObject message = new GameObject("ErrorMessage");

        Text text = message.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.red;
        text.text = ToString();

        (message.GetComponent<RectTransform>()).sizeDelta = new Vector2(Logger.MessageWidth, 0);
        message.transform.SetParent(parent.transform, false);
    }

    public override string ToString()
    {
        return context == null ? $"[{timestamp.ToString("HH:mm:ss")}] [ERROR] {content.ToString()}" : $"[{timestamp.ToString("HH:mm:ss")}] [ERROR] {content.ToString()} | ({context.ToString()})";
    }
}