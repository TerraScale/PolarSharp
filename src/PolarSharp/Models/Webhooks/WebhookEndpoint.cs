using System.Text.Json.Serialization;

namespace PolarSharp.Models.Webhooks;

/// <summary>
/// Represents a webhook endpoint configuration.
/// </summary>
public record WebhookEndpoint
{
    /// <summary>
    /// The unique identifier for the webhook endpoint.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The URL where webhook events will be sent.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// The description of the webhook endpoint.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The events that this webhook endpoint subscribes to.
    /// </summary>
    [JsonPropertyName("events")]
    public string[] Events { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The secret used to verify webhook signatures.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; init; }

    /// <summary>
    /// Whether the webhook endpoint is active.
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// The HTTP method used for webhook delivery.
    /// </summary>
    [JsonPropertyName("http_method")]
    public string HttpMethod { get; init; } = "POST";

    /// <summary>
    /// Additional headers to include in webhook requests.
    /// </summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>
    /// The date and time when the webhook endpoint was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The date and time when the webhook endpoint was last modified.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// Alias for ModifiedAt for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt => ModifiedAt;

    /// <summary>
    /// The organization ID this webhook endpoint belongs to.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;
}