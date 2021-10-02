using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an exception message
/// </summary>
public class ExceptionMessage : ILogMessage
{
    /// <summary>
    /// The message's timestamp
    /// </summary>
    private DateTime timestamp;

    /// <summary>
    /// The message content
    /// </summary>
    private Exception content;

    /// <summary>
    /// Creates a new ExceptionMessage
    /// </summary>
    /// <param name="exception">The message content</param>
    public ExceptionMessage(Exception exception)
    {
        this.timestamp = DateTime.Now;
        this.content = exception;
    }

    public void Render(GameObject parent)
    {
        GameObject message = new GameObject("ExceptionMessage");

        Text text = message.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.magenta;
        text.text = ToString();

        (message.GetComponent<RectTransform>()).sizeDelta = new Vector2(Logger.messageWidth, 0);
        message.transform.SetParent(parent.transform, false);
    }

    public override string ToString()
    {
        return $"[{timestamp.ToString("HH:mm:ss")}] [EXCEPTION] {content.ToString()}";
    }
}