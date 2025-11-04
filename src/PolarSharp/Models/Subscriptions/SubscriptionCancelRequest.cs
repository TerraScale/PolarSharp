using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Subscriptions;

/// <summary>
/// Request to cancel a subscription.
/// </summary>
public record SubscriptionCancelRequest
{
    /// <summary>
    /// The cancellation reason.
    /// </summary>
    [JsonPropertyName("cancellation_reason")]
    [StringLength(500, ErrorMessage = "Cancellation reason cannot exceed 500 characters.")]
    public string? CancellationReason { get; init; }

    /// <summary>
    /// Whether to cancel immediately (true) or at period end (false).
    /// </summary>
    [JsonPropertyName("cancel_immediately")]
    public bool? CancelImmediately { get; init; }

    /// <summary>
    /// Whether to issue a refund for the unused portion.
    /// </summary>
    [JsonPropertyName("issue_refund")]
    public bool? IssueRefund { get; init; }
}