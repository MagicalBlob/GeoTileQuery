public class InvalidGeoJsonException : System.Exception
{
    public InvalidGeoJsonException() { }
    public InvalidGeoJsonException(string message) : base(message) { }
    public InvalidGeoJsonException(string message, System.Exception inner) : base(message, inner) { }
}