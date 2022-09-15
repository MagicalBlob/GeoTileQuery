using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents an assert message
/// </summary>
public class AssertMessage : ILogMessage
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
    /// Creates a new AssertMessage
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display</param>
    /// <param name="context">Object to which the message applies</param>
    public AssertMessage(object message, UnityEngine.Object context)
    {
        this.timestamp = DateTime.Now;
        this.content = message;
        this.context = context;
    }

    public void Render(GameObject parent)
    {
        GameObject message = new GameObject("AssertMessage");

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
        return context == null ? $"[{timestamp.ToString("HH:mm:ss")}] [ASSERT] {content.ToString()}" : $"[{timestamp.ToString("HH:mm:ss")}] [ASSERT] {content.ToString()} | ({context.ToString()})";
    }
}