using System.Text.Json;

namespace PolarSharp.Exceptions;

/// <summary>
/// Represents an error response from the Polar API.
/// </summary>
public record PolarApiError
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public int StatusCode { get; init; }

    /// <summary>
    /// The error message from the API.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The error type/code.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// The raw response body.
    /// </summary>
    public string? ResponseBody { get; init; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    public JsonElement? Details { get; init; }
}

/// <summary>
/// Exception thrown when the Polar API returns an error response.
/// </summary>
public class PolarApiException : Exception
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The error type/code from the API.
    /// </summary>
    public string? ErrorType { get; }

    /// <summary>
    /// The raw response body.
    /// </summary>
    public string? ResponseBody { get; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    public JsonElement? Details { get; }

    /// <summary>
    /// Initializes a new instance of the PolarApiException with a PolarApiError.
    /// </summary>
    /// <param name="error">The API error details.</param>
    public PolarApiException(PolarApiError error) : base(error.Message)
    {
        StatusCode = error.StatusCode;
        ErrorType = error.Type;
        ResponseBody = error.ResponseBody;
        Details = error.Details;
    }

    /// <summary>
    /// Initializes a new instance of the PolarApiException with a message and status code.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="responseBody">The optional response body.</param>
    public PolarApiException(string message, int statusCode, string? responseBody = null) : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    /// <summary>
    /// Initializes a new instance of the PolarApiException with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public PolarApiException(string message, Exception innerException) : base(message, innerException)
    {
    }
}