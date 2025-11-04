using System.Net.Http.Headers;

namespace PolarSharp.Extensions;

/// <summary>
/// Helper class for advanced rate limiting and retry logic.
/// </summary>
public static class RateLimitHelper
{
    /// <summary>
    /// Extracts retry delay from Retry-After header.
    /// </summary>
    /// <param name="retryAfterHeader">The Retry-After header value.</param>
    /// <param name="maxDelayMs">Maximum allowed delay in milliseconds.</param>
    /// <returns>The retry delay, or null if no valid delay found.</returns>
    public static TimeSpan? ExtractRetryDelay(RetryConditionHeaderValue? retryAfterHeader, int maxDelayMs)
    {
        if (retryAfterHeader?.Delta.HasValue == true)
        {
            // Retry-After is a duration
            var delay = retryAfterHeader.Delta.Value;
            return TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, maxDelayMs));
        }
        else if (retryAfterHeader?.Date.HasValue == true)
        {
            // Retry-After is a date
            var delay = retryAfterHeader.Date.Value - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                return TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds, maxDelayMs));
            }
        }

        return null;
    }

    /// <summary>
    /// Calculates exponential backoff delay with jitter.
    /// </summary>
    /// <param name="retryAttempt">The current retry attempt (1-based).</param>
    /// <param name="initialDelayMs">Initial delay in milliseconds.</param>
    /// <param name="maxDelayMs">Maximum delay in milliseconds.</param>
    /// <param name="jitterFactor">Jitter factor between 0.0 and 1.0.</param>
    /// <returns>The calculated delay.</returns>
    public static TimeSpan CalculateExponentialBackoffWithJitter(
        int retryAttempt, 
        int initialDelayMs, 
        int maxDelayMs, 
        double jitterFactor)
    {
        // Calculate base exponential delay
        var baseDelayMs = initialDelayMs * Math.Pow(2, retryAttempt - 1);
        
        // Cap at maximum delay
        var cappedDelayMs = Math.Min(baseDelayMs, maxDelayMs);
        
        // Apply jitter
        var jitterRangeMs = cappedDelayMs * jitterFactor;
        var jitterMs = Random.Shared.NextDouble() * jitterRangeMs;
        
        var finalDelayMs = cappedDelayMs + jitterMs;
        return TimeSpan.FromMilliseconds(finalDelayMs);
    }

    /// <summary>
    /// Determines if a response should be retried based on status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>True if the request should be retried.</returns>
    public static bool ShouldRetry(System.Net.HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.TooManyRequests => true,
            System.Net.HttpStatusCode.InternalServerError => true,
            System.Net.HttpStatusCode.BadGateway => true,
            System.Net.HttpStatusCode.ServiceUnavailable => true,
            System.Net.HttpStatusCode.GatewayTimeout => true,
            System.Net.HttpStatusCode.RequestTimeout => true,
            System.Net.HttpStatusCode.Conflict => true,
            System.Net.HttpStatusCode.PreconditionFailed => true,
            System.Net.HttpStatusCode.Locked => true,
            System.Net.HttpStatusCode.FailedDependency => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets a human-readable description for retry reason.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A description of why the request is being retried.</returns>
    public static string GetRetryReason(System.Net.HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded",
            System.Net.HttpStatusCode.InternalServerError => "Internal server error",
            System.Net.HttpStatusCode.BadGateway => "Bad gateway",
            System.Net.HttpStatusCode.ServiceUnavailable => "Service unavailable",
            System.Net.HttpStatusCode.GatewayTimeout => "Gateway timeout",
            System.Net.HttpStatusCode.RequestTimeout => "Request timeout",
            System.Net.HttpStatusCode.Conflict => "Resource conflict",
            System.Net.HttpStatusCode.PreconditionFailed => "Precondition failed",
            System.Net.HttpStatusCode.Locked => "Resource locked",
            System.Net.HttpStatusCode.FailedDependency => "Failed dependency",
            _ => $"HTTP {(int)statusCode} error"
        };
    }
}