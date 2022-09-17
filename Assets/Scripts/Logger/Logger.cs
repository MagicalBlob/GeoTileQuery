using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// The application log
/// </summary>
public class Logger : ILogHandler
{
    /// <summary>
    /// Message width
    /// </summary>
    public static int MessageWidth { get { return 978; } }

    /// <summary>
    /// All the messages in the log
    /// </summary>
    private List<ILogMessage> messages;

    /// <summary>
    /// Messages that are still to be printed
    /// </summary>
    private List<ILogMessage> toPrint;

    /// <summary>
    /// The instance of the log
    /// </summary>
    private static readonly Logger instance = new Logger();

    /// <summary>
    /// An event that is triggered every time a new message is added to the log
    /// </summary>
    private event EventHandler onNewMessage;

    /// <summary>
    /// Constructs a new Logger
    /// </summary>
    public Logger()
    {
        this.messages = new List<ILogMessage>();
        this.toPrint = new List<ILogMessage>();
        // Set the Unity log handler to our custom log handler
        Debug.unityLogger.logHandler = this;
    }

    /// <summary>
    /// Subscribes to the new log message event
    /// </summary>
    /// <param name="callback">The callback method</param>
    public static void Subscribe(EventHandler callback)
    {
        instance.onNewMessage += callback;
    }

    /// <summary>
    /// Render the messages that are still to be printed
    /// </summary>
    /// <param name="parent">The parent GameObject</param>
    public static void Render(GameObject parent)
    {
        if (parent == null)
        {
            // If there is no parent, then we can't render anything
            return;
        }

        foreach (ILogMessage message in instance.toPrint)
        {
            message.Render(parent);
        }
        instance.toPrint.Clear();
    }

    /// <summary>
    /// Logs a formatted message
    /// </summary>
    /// <remarks>
    /// For formatting details, see the MSDN documentation on Composite Formatting. Rich text markup can be used to add emphasis. See the manual page about <see href="https://docs.unity3d.com/Manual/StyledText.html">rich text</see> for details of the different markup tags available.
    /// </remarks>
    /// <param name="logType">The type of the log message</param>
    /// <param name="context">Object to which the message applies</param>
    /// <param name="format">A composite format string</param>
    /// <param name="args">Format arguments</param>
    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        ILogMessage logMessage;
        switch (logType)
        {
            case LogType.Error:
                logMessage = new ErrorMessage(string.Format(format, args), context);
                break;
            case LogType.Assert:
                logMessage = new AssertMessage(string.Format(format, args), context);
                break;
            case LogType.Warning:
                logMessage = new WarningMessage(string.Format(format, args), context);
                break;
            case LogType.Log:
                logMessage = new InfoMessage(string.Format(format, args), context);
                break;
            case LogType.Exception:
                logMessage = new ExceptionMessage(new Exception(string.Format(format, args)), context);
                break;
            default:
                logMessage = new InfoMessage(string.Format(format, args), context);
                break;
        }
        messages.Add(logMessage);
        toPrint.Add(logMessage);
        onNewMessage.Invoke(instance, new EventArgs());
    }

    /// <summary>
    /// A variant of Log that logs an exception message
    /// </summary>
    /// <param name="exception">Runtime Exception</param>
    /// <param name="context">Object to which the message applies</param>
    public void LogException(Exception exception, UnityEngine.Object context)
    {
        LogFormat(LogType.Exception, context, "{0}", exception.ToString());
    }
}