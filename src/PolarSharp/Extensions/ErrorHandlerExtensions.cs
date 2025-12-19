using System.Net;
using System.Text.Json;
using PolarSharp.Results;

namespace PolarSharp.Extensions;

/// <summary>
/// Extension methods for enhanced error handling in HTTP responses.
/// </summary>
internal static class ErrorHandlerExtensions
{
    /// <summary>
    /// Handles HTTP response errors and returns a PolarResult indicating success or failure.
    /// This method does not throw exceptions - errors are returned as failed Results.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A PolarResult indicating success or containing error details.</returns>
    public static async Task<PolarResult> ToPolarResultAsync(
        this HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
            return PolarResult.Success();

        var error = await ParseErrorAsync(response, jsonOptions, cancellationToken);
        return PolarResult.Failure(error);
    }

    /// <summary>
    /// Handles HTTP response errors and returns a PolarResult with value on success or error on failure.
    /// This method does not throw exceptions - errors are returned as failed Results.
    /// </summary>
    /// <typeparam name="T">The type of value expected on success.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A PolarResult containing the deserialized value or error details.</returns>
    public static async Task<PolarResult<T>> ToPolarResultAsync<T>(
        this HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Handle empty response (e.g., 204 No Content)
            if (string.IsNullOrWhiteSpace(content))
            {
                return PolarResult<T>.Failure(PolarError.InternalError("Response body was empty."));
            }

            var value = JsonSerializer.Deserialize<T>(content, jsonOptions);

            if (value == null)
                return PolarResult<T>.Failure(PolarError.InternalError("Failed to deserialize response."));

            return PolarResult<T>.Success(value);
        }

        var error = await ParseErrorAsync(response, jsonOptions, cancellationToken);
        return PolarResult<T>.Failure(error);
    }

    /// <summary>
    /// Handles HTTP response for nullable return types.
    /// Returns Failure for 404/422, Success(value) for success, or Failure for other errors.
    /// </summary>
    /// <typeparam name="T">The type of value expected on success.</typeparam>
    /// <param name="response">The HTTP response.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A PolarResult containing the value or error details.</returns>
    public static async Task<PolarResult<T?>> ToPolarResultWithNullableAsync<T>(
        this HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken = default) where T : class
    {
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Handle empty response (e.g., 204 No Content)
            if (string.IsNullOrWhiteSpace(content))
                return PolarResult<T?>.Success(null);

            var value = JsonSerializer.Deserialize<T>(content, jsonOptions);
            return PolarResult<T?>.Success(value);
        }

        var error = await ParseErrorAsync(response, jsonOptions, cancellationToken);
        return PolarResult<T?>.Failure(error);
    }

    /// <summary>
    /// Parses error details from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A PolarError with the error details.</returns>
    private static async Task<PolarError> ParseErrorAsync(
        HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken)
    {
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var retryAfter = response.Headers.RetryAfter;
        return TryParseError(responseBody, response.StatusCode, jsonOptions, retryAfter);
    }

    /// <summary>
    /// Tries to parse the error response from the API.
    /// </summary>
    /// <param name="responseBody">The response body.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="retryAfter">Optional retry-after information from response headers.</param>
    /// <returns>A PolarError instance.</returns>
    private static PolarError TryParseError(
        string responseBody,
        HttpStatusCode statusCode,
        JsonSerializerOptions jsonOptions,
        System.Net.Http.Headers.RetryConditionHeaderValue? retryAfter = null)
    {
        TimeSpan? retryAfterDuration = null;
        if (retryAfter?.Delta.HasValue == true)
        {
            retryAfterDuration = retryAfter.Delta.Value;
        }
        else if (retryAfter?.Date.HasValue == true)
        {
            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                retryAfterDuration = delay;
            }
        }

        try
        {
            // Try to parse as JSON error response
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(responseBody, jsonOptions);

            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                // Look for common error fields
                var message = GetErrorMessage(jsonElement);
                var type = GetErrorType(jsonElement);
                var details = GetErrorDetails(jsonElement);

                // Add rate limit context if applicable
                if (statusCode == HttpStatusCode.TooManyRequests)
                {
                    message = AddRateLimitContext(message, retryAfter);
                }

                return new PolarError
                {
                    StatusCode = statusCode,
                    Message = message,
                    ErrorType = type,
                    ResponseBody = responseBody,
                    Details = details,
                    RetryAfter = retryAfterDuration
                };
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, fall back to plain text
        }

        // Fallback to status code based error messages
        var defaultMessage = GetDefaultErrorMessage(statusCode, responseBody);

        // Add rate limit context if applicable
        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            defaultMessage = AddRateLimitContext(defaultMessage, retryAfter);
        }

        return new PolarError
        {
            StatusCode = statusCode,
            Message = defaultMessage,
            ErrorType = statusCode.ToString(),
            ResponseBody = responseBody,
            RetryAfter = retryAfterDuration
        };
    }

    /// <summary>
    /// Extracts the error message from a JSON element.
    /// </summary>
    /// <param name="jsonElement">The JSON element containing error information.</param>
    /// <returns>The error message.</returns>
    private static string GetErrorMessage(JsonElement jsonElement)
    {
        // Try common error message field names
        var messageFields = new[] { "message", "error", "detail", "description" };

        foreach (var field in messageFields)
        {
            if (jsonElement.TryGetProperty(field, out var messageElement))
            {
                return messageElement.ValueKind switch
                {
                    JsonValueKind.String => messageElement.GetString() ?? string.Empty,
                    JsonValueKind.Number => messageElement.ToString(),
                    _ => messageElement.GetRawText()
                };
            }
        }

        // If no message field found, check for errors array
        if (jsonElement.TryGetProperty("errors", out var errorsElement) &&
            errorsElement.ValueKind == JsonValueKind.Array)
        {
            var errors = new List<string>();
            foreach (var error in errorsElement.EnumerateArray())
            {
                if (error.TryGetProperty("message", out var errorElement) &&
                    errorElement.ValueKind == JsonValueKind.String)
                {
                    errors.Add(errorElement.GetString() ?? string.Empty);
                }
            }

            if (errors.Count > 0)
                return string.Join("; ", errors);
        }

        return "An error occurred while processing the request.";
    }

    /// <summary>
    /// Extracts the error type from a JSON element.
    /// </summary>
    /// <param name="jsonElement">The JSON element containing error information.</param>
    /// <returns>The error type.</returns>
    private static string? GetErrorType(JsonElement jsonElement)
    {
        var typeFields = new[] { "type", "code", "error_code", "error_type" };

        foreach (var field in typeFields)
        {
            if (jsonElement.TryGetProperty(field, out var typeElement) &&
                typeElement.ValueKind == JsonValueKind.String)
            {
                return typeElement.GetString();
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts error details from a JSON element.
    /// </summary>
    /// <param name="jsonElement">The JSON element containing error information.</param>
    /// <returns>The error details.</returns>
    private static JsonElement? GetErrorDetails(JsonElement jsonElement)
    {
        var detailFields = new[] { "details", "data", "context", "validation_errors" };

        foreach (var field in detailFields)
        {
            if (jsonElement.TryGetProperty(field, out var detailElement))
            {
                return detailElement;
            }
        }

        return null;
    }

    /// <summary>
    /// Adds rate limit context to error messages.
    /// </summary>
    /// <param name="message">The original error message.</param>
    /// <param name="retryAfter">The retry-after header value.</param>
    /// <returns>The enhanced error message.</returns>
    private static string AddRateLimitContext(string message, System.Net.Http.Headers.RetryConditionHeaderValue? retryAfter)
    {
        if (retryAfter?.Delta.HasValue == true)
        {
            return $"{message} Retry after {retryAfter.Delta.Value.TotalSeconds:F0} seconds.";
        }
        else if (retryAfter?.Date.HasValue == true)
        {
            var delay = retryAfter.Date.Value - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return $"{message} Retry after {delay.TotalSeconds:F0} seconds.";
            }
        }

        return $"{message} Consider implementing exponential backoff and reducing request frequency.";
    }

    /// <summary>
    /// Gets a default error message based on HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="responseBody">The response body.</param>
    /// <returns>A default error message.</returns>
    private static string GetDefaultErrorMessage(HttpStatusCode statusCode, string responseBody)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "The request was invalid or malformed.",
            HttpStatusCode.Unauthorized => "Authentication failed or was not provided.",
            HttpStatusCode.Forbidden => "Access to the requested resource is forbidden.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.MethodNotAllowed => "The HTTP method is not allowed for this endpoint.",
            HttpStatusCode.Conflict => "The request conflicts with the current state of the resource.",
            HttpStatusCode.TooManyRequests => "Rate limit exceeded. Please try again later.",
            HttpStatusCode.InternalServerError => "An internal server error occurred.",
            HttpStatusCode.BadGateway => "The server received an invalid response.",
            HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable.",
            HttpStatusCode.GatewayTimeout => "The gateway timed out.",
            _ => $"HTTP {(int)statusCode}: {responseBody}"
        };
    }
}
