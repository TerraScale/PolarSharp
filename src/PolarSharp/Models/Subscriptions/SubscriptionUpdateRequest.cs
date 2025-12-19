using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Products;

namespace PolarSharp.Models.Subscriptions;

/// <summary>
/// Request to update an existing subscription.
/// This is a unified request that can be used for different update scenarios.
/// Only set the properties relevant to your update operation.
/// </summary>
public record SubscriptionUpdateRequest
{
    /// <summary>
    /// Update subscription to another product.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; init; }

    /// <summary>
    /// Determine how to handle the proration billing. If not provided, will use the
    /// default organization setting.
    /// </summary>
    [JsonPropertyName("proration_behavior")]
    public SubscriptionProrationBehavior? ProrationBehavior { get; init; }

    /// <summary>
    /// The discount ID to apply to the subscription. Set to null to remove an existing discount.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// Number of trial days to set. Used for trial updates.
    /// </summary>
    [JsonPropertyName("trial_days")]
    [Range(0, 365, ErrorMessage = "Trial days must be between 0 and 365.")]
    public int? TrialDays { get; init; }

    /// <summary>
    /// The number of seats for seat-based subscriptions.
    /// </summary>
    [JsonPropertyName("seats")]
    [Range(1, int.MaxValue, ErrorMessage = "Seats must be at least 1.")]
    public int? Seats { get; init; }

    /// <summary>
    /// The new recurring interval for the subscription.
    /// </summary>
    [JsonPropertyName("recurring_interval")]
    public RecurringInterval? RecurringInterval { get; init; }

    /// <summary>
    /// Whether to cancel the subscription at the end of the current period.
    /// </summary>
    [JsonPropertyName("cancel_at_period_end")]
    public bool? CancelAtPeriodEnd { get; init; }

    /// <summary>
    /// The customer cancellation reason when canceling.
    /// </summary>
    [JsonPropertyName("customer_cancellation_reason")]
    public CustomerCancellationReason? CustomerCancellationReason { get; init; }

    /// <summary>
    /// Additional cancellation comment from the customer.
    /// </summary>
    [JsonPropertyName("customer_cancellation_comment")]
    [StringLength(500, ErrorMessage = "Cancellation comment cannot exceed 500 characters.")]
    public string? CustomerCancellationComment { get; init; }

    /// <summary>
    /// Whether to revoke the subscription immediately.
    /// </summary>
    [JsonPropertyName("revoke")]
    public bool? Revoke { get; init; }
}

/// <summary>
/// Determines how to handle proration billing when updating a subscription.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionProrationBehavior
{
    /// <summary>
    /// Create an invoice immediately for the prorated amount.
    /// </summary>
    [JsonPropertyName("invoice")]
    Invoice,

    /// <summary>
    /// Prorate the amount and add it to the next invoice.
    /// </summary>
    [JsonPropertyName("prorate")]
    Prorate
}