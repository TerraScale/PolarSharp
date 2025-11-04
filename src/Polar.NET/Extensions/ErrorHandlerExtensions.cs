using System.Net;
using System.Text.Json;
using Polar.NET.Exceptions;

namespace Polar.NET.Extensions;

/// <summary>
/// Extension methods for enhanced error handling in HTTP responses.
/// </summary>
internal static class ErrorHandlerExtensions
{
    /// <summary>
    /// Handles HTTP response errors and throws appropriate exceptions.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    /// <exception cref="PolarApiException">Thrown when the API returns an error response.</exception>
    public static async Task HandleErrorsAsync(
        this HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var error = TryParseError(responseBody, response.StatusCode, jsonOptions);
        
        throw new PolarApiException(error);
    }

    /// <summary>
    /// Tries to parse the error response from the API.
    /// </summary>
    /// <param name="responseBody">The response body.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <returns>A PolarApiError instance.</returns>
    private static PolarApiError TryParseError(
        string responseBody,
        HttpStatusCode statusCode,
        JsonSerializerOptions jsonOptions)
    {
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

                return new PolarApiError
                {
                    StatusCode = (int)statusCode,
                    Message = message,
                    Type = type,
                    ResponseBody = responseBody,
                    Details = details
                };
            }
        }
        catch (JsonException)
        {
            // If JSON parsing fails, fall back to plain text
        }

        // Fallback to status code based error messages
        return new PolarApiError
        {
            StatusCode = (int)statusCode,
            Message = GetDefaultErrorMessage(statusCode, responseBody),
            Type = statusCode.ToString(),
            ResponseBody = responseBody
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