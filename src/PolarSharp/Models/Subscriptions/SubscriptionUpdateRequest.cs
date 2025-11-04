using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Common;

namespace PolarSharp.Models.Subscriptions;

/// <summary>
/// Request to update an existing subscription.
/// </summary>
public record SubscriptionUpdateRequest
{
    /// <summary>
    /// The metadata of the subscription.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the subscription is canceled.
    /// </summary>
    [JsonPropertyName("canceled")]
    public bool? Canceled { get; init; }

    /// <summary>
    /// The cancellation reason for the subscription.
    /// </summary>
    [JsonPropertyName("cancellation_reason")]
    [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
    public string? CancellationReason { get; init; }

    /// <summary>
    /// Whether the subscription is paused.
    /// </summary>
    [JsonPropertyName("paused")]
    public bool? Paused { get; init; }

    /// <summary>
    /// The pause reason for the subscription.
    /// </summary>
    [JsonPropertyName("pause_reason")]
    [StringLength(500, ErrorMessage = "Pause reason cannot exceed 500 characters.")]
    public string? PauseReason { get; init; }

    /// <summary>
    /// The product price ID for the subscription.
    /// </summary>
    [JsonPropertyName("product_price_id")]
    public string? ProductPriceId { get; init; }
}