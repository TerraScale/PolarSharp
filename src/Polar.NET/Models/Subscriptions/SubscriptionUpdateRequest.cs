using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Polar.NET.Models.Common;

namespace Polar.NET.Models.Subscriptions;

/// <summary>
/// Request to update an existing subscription.
/// </summary>
public record SubscriptionUpdateRequest
{
    /// <summary>
    /// The metadata of the subscription.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the subscription is canceled.
    /// </summary>
    public bool? Canceled { get; init; }

    /// <summary>
    /// The cancellation reason for the subscription.
    /// </summary>
    [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
    public string? CancellationReason { get; init; }

    /// <summary>
    /// Whether the subscription is paused.
    /// </summary>
    public bool? Paused { get; init; }

    /// <summary>
    /// The pause reason for the subscription.
    /// </summary>
    [StringLength(500, ErrorMessage = "Pause reason cannot exceed 500 characters.")]
    public string? PauseReason { get; init; }

    /// <summary>
    /// The product price ID for the subscription.
    /// </summary>
    public string? ProductPriceId { get; init; }
}