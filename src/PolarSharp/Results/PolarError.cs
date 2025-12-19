using System.Net;
using System.Text.Json;

namespace PolarSharp.Results;

/// <summary>
/// Represents an error from the Polar API.
/// </summary>
public sealed record PolarError
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The error type/code from the API.
    /// </summary>
    public string? ErrorType { get; init; }

    /// <summary>
    /// The raw response body.
    /// </summary>
    public string? ResponseBody { get; init; }

    /// <summary>
    /// Additional error details as JSON.
    /// </summary>
    public JsonElement? Details { get; init; }

    /// <summary>
    /// Retry-After duration if rate limited.
    /// </summary>
    public TimeSpan? RetryAfter { get; init; }

    /// <summary>
    /// Returns true if this error is due to rate limiting (HTTP 429).
    /// </summary>
    public bool IsRateLimitError => StatusCode == HttpStatusCode.TooManyRequests;

    /// <summary>
    /// Returns true if this error is due to authentication/authorization issues (HTTP 401 or 403).
    /// </summary>
    public bool IsAuthError => StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

    /// <summary>
    /// Returns true if this error indicates the resource was not found (HTTP 404).
    /// </summary>
    public bool IsNotFoundError => StatusCode == HttpStatusCode.NotFound;

    /// <summary>
    /// Returns true if this error is a server-side error (HTTP 5xx).
    /// </summary>
    public bool IsServerError => (int)StatusCode >= 500;

    /// <summary>
    /// Returns true if this error is a client-side error (HTTP 4xx).
    /// </summary>
    public bool IsClientError => (int)StatusCode >= 400 && (int)StatusCode < 500;

    /// <summary>
    /// Returns true if this error is a validation error (HTTP 400 or 422).
    /// </summary>
    public bool IsValidationError => StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity;

    /// <summary>
    /// Returns true if this error is a conflict error (HTTP 409).
    /// </summary>
    public bool IsConflictError => StatusCode == HttpStatusCode.Conflict;

    /// <summary>
    /// Returns true if this error is a method not allowed error (HTTP 405).
    /// </summary>
    public bool IsMethodNotAllowedError => StatusCode == HttpStatusCode.MethodNotAllowed;

    /// <summary>
    /// Creates a PolarError from status code and message.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>A new PolarError instance.</returns>
    public static PolarError FromStatusCode(HttpStatusCode statusCode, string message) =>
        new() { StatusCode = statusCode, Message = message };

    /// <summary>
    /// Creates a PolarError for a not found scenario.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new PolarError instance with 404 status.</returns>
    public static PolarError NotFound(string message = "Resource not found") =>
        new() { StatusCode = HttpStatusCode.NotFound, Message = message };

    /// <summary>
    /// Creates a PolarError for a validation error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new PolarError instance with 400 status.</returns>
    public static PolarError ValidationError(string message) =>
        new() { StatusCode = HttpStatusCode.BadRequest, Message = message, ErrorType = "ValidationError" };

    /// <summary>
    /// Creates a PolarError for an internal error.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>A new PolarError instance with 500 status.</returns>
    public static PolarError InternalError(string message) =>
        new() { StatusCode = HttpStatusCode.InternalServerError, Message = message };
}
