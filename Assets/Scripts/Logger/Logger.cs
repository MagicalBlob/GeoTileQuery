using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

/// <summary>
/// The application log
/// </summary>
public static class Logger
{
    /// <summary>
    /// All the messages in the log
    /// </summary>
    private static List<ILogMessage> messages = new List<ILogMessage>();

    /// <summary>
    /// Messages that are still to be printed
    /// </summary>
    private static List<ILogMessage> toPrint = new List<ILogMessage>();

    /// <summary>
    /// An event that is triggered every time a new message is added to the log
    /// </summary>
    public static UnityEvent onNewMessage = new UnityEvent();

    /// <summary>
    /// Message width
    /// </summary>
    public static int messageWidth = 978;

    /// <summary>
    /// Subscribes to the new log message event
    /// </summary>
    /// <param name="callback">The callback method</param>
    public static void Subscribe(UnityAction callback)
    {
        onNewMessage.AddListener(callback);
    }

    /// <summary>
    /// Render the messages that are still to be printed
    /// </summary>
    /// <param name="parent">The parent GameObject</param>
    public static void Render(GameObject parent)
    {
        foreach (ILogMessage message in toPrint)
        {
            message.Render(parent);
        }
        toPrint.Clear();
    }

    /// <summary>
    /// Adds a message to the log
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display</param>
    public static void Log(object message)
    {
        ILogMessage logMessage = new InfoMessage(message);
        messages.Add(logMessage);
        toPrint.Add(logMessage);
        onNewMessage.Invoke();
    }

    /// <summary>
    /// Adds a warning to the log
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display</param>
    public static void LogWarning(object message)
    {
        ILogMessage logMessage = new WarningMessage(message);
        messages.Add(logMessage);
        toPrint.Add(logMessage);
        onNewMessage.Invoke();
    }

    /// <summary>
    /// Adds an error to the log
    /// </summary>
    /// <param name="message">String or object to be converted to string representation for display</param>
    public static void LogError(object message)
    {
        ILogMessage logMessage = new ErrorMessage(message);
        messages.Add(logMessage);
        toPrint.Add(logMessage);
        onNewMessage.Invoke();
    }

    /// <summary>
    /// Adds an exception to the log
    /// </summary>
    /// <param name="exception">Runtime Exception</param>
    public static void LogException(Exception exception)
    {
        ILogMessage logMessage = new ExceptionMessage(exception);
        messages.Add(logMessage);
        toPrint.Add(logMessage);
        onNewMessage.Invoke();
    }
}