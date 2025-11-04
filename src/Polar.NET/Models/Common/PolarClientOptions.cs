using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.Common;

/// <summary>
/// Configuration options for the Polar client.
/// </summary>
public record PolarClientOptions
{
    /// <summary>
    /// The API access token for authentication.
    /// </summary>
    [Required]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// The base URL for the Polar API. If not specified, uses the default production URL.
    /// </summary>
    public Uri? BaseUrl { get; init; }

    /// <summary>
    /// The API environment to use.
    /// </summary>
    public PolarEnvironment Environment { get; init; } = PolarEnvironment.Production;

    /// <summary>
    /// The timeout for HTTP requests in seconds. Default is 30 seconds.
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// The maximum number of retry attempts for failed requests. Default is 3.
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; init; } = 3;

    /// <summary>
    /// The initial delay for retry attempts in milliseconds. Default is 1000ms.
    /// </summary>
    [Range(100, 60000)]
    public int InitialRetryDelayMs { get; init; } = 1000;

    /// <summary>
    /// The maximum number of requests per minute. Default is 300.
    /// </summary>
    [Range(1, 10000)]
    public int RequestsPerMinute { get; init; } = 300;

    /// <summary>
    /// Custom user agent string to identify the client.
    /// </summary>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Custom JSON serializer options.
    /// </summary>
    public System.Text.Json.JsonSerializerOptions? JsonSerializerOptions { get; init; }
}