using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.Webhooks;

/// <summary>
/// Request model for creating a new webhook endpoint.
/// </summary>
public record WebhookEndpointCreateRequest
{
    /// <summary>
    /// The URL where webhook events will be sent.
    /// </summary>
    [Required]
    [Url]
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// The description of webhook endpoint.
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// The events that this webhook endpoint subscribes to.
    /// </summary>
    [Required]
    public string[] Events { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Whether webhook endpoint is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// The HTTP method used for webhook delivery.
    /// </summary>
    [RegularExpression("^(GET|POST|PUT|PATCH)$", ErrorMessage = "HTTP method must be GET, POST, PUT, or PATCH.")]
    public string HttpMethod { get; init; } = "POST";

    /// <summary>
    /// Additional headers to include in webhook requests.
    /// </summary>
    public Dictionary<string, string>? Headers { get; init; }
}