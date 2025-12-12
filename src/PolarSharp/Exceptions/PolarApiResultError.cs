using System.Net;
using System.Text.Json;
using FluentResults;

namespace PolarSharp.Exceptions;

/// <summary>
/// A FluentResults error that contains Polar API error details.
/// </summary>
public class PolarApiResultError : Error
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; }

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
    /// Retry-After duration if provided by the server.
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of the PolarApiResultError from a PolarApiError.
    /// </summary>
    /// <param name="error">The API error details.</param>
    public PolarApiResultError(PolarApiError error)
        : base(error.Message)
    {
        StatusCode = (HttpStatusCode)error.StatusCode;
        ErrorType = error.Type;
        ResponseBody = error.ResponseBody;
        Details = error.Details;
        
        WithMetadata(nameof(StatusCode), (int)StatusCode);
        if (ErrorType != null)
            WithMetadata(nameof(ErrorType), ErrorType);
    }

    /// <summary>
    /// Initializes a new instance of the PolarApiResultError.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="errorType">The error type.</param>
    /// <param name="responseBody">The response body.</param>
    /// <param name="details">Additional details.</param>
    /// <param name="retryAfter">Retry-After duration.</param>
    public PolarApiResultError(
        string message,
        HttpStatusCode statusCode,
        string? errorType = null,
        string? responseBody = null,
        JsonElement? details = null,
        TimeSpan? retryAfter = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorType = errorType;
        ResponseBody = responseBody;
        Details = details;
        RetryAfter = retryAfter;
        
        WithMetadata(nameof(StatusCode), (int)statusCode);
        if (errorType != null)
            WithMetadata(nameof(ErrorType), errorType);
        if (retryAfter.HasValue)
            WithMetadata(nameof(RetryAfter), retryAfter.Value.TotalSeconds);
    }

    /// <summary>
    /// Returns true if this error is due to rate limiting (HTTP 429).
    /// </summary>
    public bool IsRateLimitError => StatusCode == HttpStatusCode.TooManyRequests;

    /// <summary>
    /// Returns true if this error is due to authentication/authorization issues.
    /// </summary>
    public bool IsAuthError => StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

    /// <summary>
    /// Returns true if this error indicates the resource was not found.
    /// </summary>
    public bool IsNotFoundError => StatusCode == HttpStatusCode.NotFound;

    /// <summary>
    /// Returns true if this error is a server-side error (5xx).
    /// </summary>
    public bool IsServerError => (int)StatusCode >= 500;

    /// <summary>
    /// Returns true if this error is a client-side error (4xx).
    /// </summary>
    public bool IsClientError => (int)StatusCode >= 400 && (int)StatusCode < 500;

    /// <summary>
    /// Converts this error to a PolarApiException for throwing.
    /// </summary>
    /// <returns>A PolarApiException instance.</returns>
    public PolarApiException ToPolarApiException()
    {
        return new PolarApiException(
            Message,
            (int)StatusCode,
            ResponseBody);
    }
}

/// <summary>
/// Extension methods for working with PolarApiResultError in FluentResults.
/// </summary>
public static class PolarResultExtensions
{
    /// <summary>
    /// Gets the first PolarApiResultError from a failed result, if any.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>The PolarApiResultError if found, otherwise null.</returns>
    public static PolarApiResultError? GetPolarError<T>(this Result<T> result)
    {
        return result.Errors.OfType<PolarApiResultError>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the first PolarApiResultError from a failed result, if any.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>The PolarApiResultError if found, otherwise null.</returns>
    public static PolarApiResultError? GetPolarError(this Result result)
    {
        return result.Errors.OfType<PolarApiResultError>().FirstOrDefault();
    }

    /// <summary>
    /// Returns true if the result failed due to rate limiting.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the error is a rate limit error.</returns>
    public static bool IsRateLimitError<T>(this Result<T> result)
    {
        return result.GetPolarError()?.IsRateLimitError ?? false;
    }

    /// <summary>
    /// Returns true if the result failed due to a not found error.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the error is a not found error.</returns>
    public static bool IsNotFoundError<T>(this Result<T> result)
    {
        return result.GetPolarError()?.IsNotFoundError ?? false;
    }

    /// <summary>
    /// Throws a PolarApiException if the result is failed.
    /// Useful for maintaining backwards compatibility with exception-based error handling.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>The result if successful.</returns>
    /// <exception cref="PolarApiException">Thrown if the result is failed.</exception>
    public static Result<T> ThrowOnFailure<T>(this Result<T> result)
    {
        if (result.IsFailed)
        {
            var polarError = result.GetPolarError();
            if (polarError != null)
            {
                throw new PolarApiException(
                    polarError.Message,
                    (int)polarError.StatusCode,
                    polarError.ResponseBody);
            }
            
            throw new PolarApiException(
                string.Join("; ", result.Errors.Select(e => e.Message)),
                500);
        }
        
        return result;
    }

    /// <summary>
    /// Gets the value from a successful result or throws a PolarApiException.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>The value if successful.</returns>
    /// <exception cref="PolarApiException">Thrown if the result is failed.</exception>
    public static T ValueOrThrow<T>(this Result<T> result)
    {
        result.ThrowOnFailure();
        return result.Value;
    }
}
