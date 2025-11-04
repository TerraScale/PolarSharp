using System.Text.Json.Serialization;

namespace Polar.NET.Models.Webhooks;

/// <summary>
/// Represents a webhook delivery attempt.
/// </summary>
public record WebhookDelivery
{
    /// <summary>
    /// The unique identifier for webhook delivery.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The webhook endpoint ID this delivery belongs to.
    /// </summary>
    [JsonPropertyName("webhook_endpoint_id")]
    public string WebhookEndpointId { get; init; } = string.Empty;

    /// <summary>
    /// The event type that was delivered.
    /// </summary>
    [JsonPropertyName("event_type")]
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// The event data payload.
    /// </summary>
    [JsonPropertyName("data")]
    public object Data { get; init; } = new();

    /// <summary>
    /// The HTTP status code returned by webhook URL.
    /// </summary>
    [JsonPropertyName("http_status")]
    public int? HttpStatus { get; init; }

    /// <summary>
    /// The response body from webhook URL.
    /// </summary>
    [JsonPropertyName("response_body")]
    public string? ResponseBody { get; init; }

    /// <summary>
    /// The error message if delivery failed.
    /// </summary>
    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// The number of delivery attempts.
    /// </summary>
    [JsonPropertyName("attempt_count")]
    public int AttemptCount { get; init; }

    /// <summary>
    /// Whether delivery was successful.
    /// </summary>
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The date and time when delivery was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when delivery was last attempted.
    /// </summary>
    [JsonPropertyName("last_attempt_at")]
    public DateTime? LastAttemptAt { get; init; }

    /// <summary>
    /// The date and time when delivery will be retried next.
    /// </summary>
    [JsonPropertyName("next_retry_at")]
    public DateTime? NextRetryAt { get; init; }
}