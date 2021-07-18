/// <summary>
/// Represents errors when parsing GeoJSON data
/// </summary>
public class InvalidGeoJsonException : System.Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidGeoJsonException class
    /// </summary>
    public InvalidGeoJsonException() { }

    /// <summary>
    /// Initializes a new instance of the InvalidGeoJsonException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public InvalidGeoJsonException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the InvalidGeoJsonException class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception
    /// </summary>
    /// <param name="message">The error message string</param>
    /// <param name="inner">The inner exception reference</param>
    public InvalidGeoJsonException(string message, System.Exception inner) : base(message, inner) { }
}