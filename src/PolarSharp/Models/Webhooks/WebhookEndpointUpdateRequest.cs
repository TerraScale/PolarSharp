using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.Webhooks;

/// <summary>
/// Request model for updating an existing webhook endpoint.
/// </summary>
public record WebhookEndpointUpdateRequest
{
    /// <summary>
    /// The URL where webhook events will be sent.
    /// </summary>
    [Url]
    public string? Url { get; init; }

    /// <summary>
    /// The description of webhook endpoint.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// The events that this webhook endpoint subscribes to.
    /// </summary>
    public string[]? Events { get; init; }

    /// <summary>
    /// Whether webhook endpoint is active.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// The HTTP method used for webhook delivery.
    /// </summary>
    [RegularExpression("^(GET|POST|PUT|PATCH)$", ErrorMessage = "HTTP method must be GET, POST, PUT, or PATCH.")]
    public string? HttpMethod { get; init; }

    /// <summary>
    /// Additional headers to include in webhook requests.
    /// </summary>
    public Dictionary<string, string>? Headers { get; init; }
}