using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;

public static class Logger
{
    private static StringBuilder log = new StringBuilder();

    public static UnityEvent OnNewMessage = new UnityEvent();

    // Get log contents
    public static string Print()
    {
        return log.ToString();
    }

    // Add message to log
    public static void Log(object message)
    {
        Debug.Log(message); //TODO: remove?
        log.Append(DateTime.Now.ToString("[HH:mm:ss] "));
        log.AppendLine(message.ToString());
        OnNewMessage.Invoke();
    }

    // Add error to log
    public static void LogError(object message)
    {
        Debug.LogError(message); //TODO: remove?
        log.Append(DateTime.Now.ToString("[HH:mm:ss] "));
        log.AppendLine(message.ToString());
        OnNewMessage.Invoke();
    }

    // Add exception to log
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception); //TODO: remove?
        log.Append(DateTime.Now.ToString("[HH:mm:ss] "));
        log.AppendLine(exception.ToString());
        OnNewMessage.Invoke();
    }
}