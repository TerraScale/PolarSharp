using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Products; // Add this using directive

namespace PolarSharp.Models.Subscriptions;

/// <summary>
/// Represents a subscription in the Polar system.
/// </summary>
public record Subscription
{
    /// <summary>
    /// The unique identifier of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The status of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public SubscriptionStatus Status { get; init; }

    /// <summary>
    /// The customer ID associated with the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The product ID associated with the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("product_id")]
    public string ProductId { get; init; } = string.Empty;

    /// <summary>
    /// The product price ID associated with the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("product_price_id")]
    public string ProductPriceId { get; init; } = string.Empty;

    /// <summary>
    /// The amount of the subscription.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    /// <summary>
    /// The currency of the subscription.
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>
    /// The recurring interval of the subscription.
    /// </summary>
    [JsonPropertyName("recurring_interval")]
    public RecurringInterval? RecurringInterval { get; init; }

    /// <summary>
    /// The recurring interval count of the subscription.
    /// </summary>
    [JsonPropertyName("recurring_interval_count")]
    public int? RecurringIntervalCount { get; init; }

    /// <summary>
    /// Whether the subscription will be canceled at the end of the period.
    /// </summary>
    [JsonPropertyName("cancel_at_period_end")]
    public bool? CancelAtPeriodEnd { get; init; }

    /// <summary>
    /// The current period start date.
    /// </summary>
    [Required]
    [JsonPropertyName("current_period_start")]
    public DateTime CurrentPeriodStart { get; init; }

    /// <summary>
    /// The current period end date.
    /// </summary>
    [Required]
    [JsonPropertyName("current_period_end")]
    public DateTime CurrentPeriodEnd { get; init; }

    /// <summary>
    /// The trial period start date.
    /// </summary>
    [JsonPropertyName("trial_start")]
    public DateTime? TrialStart { get; init; }

    /// <summary>
    /// The trial period end date.
    /// </summary>
    [JsonPropertyName("trial_end")]
    public DateTime? TrialEnd { get; init; }

    /// <summary>
    /// The started at date.
    /// </summary>
    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; init; }

    /// <summary>
    /// The ends at date.
    /// </summary>
    [JsonPropertyName("ends_at")]
    public DateTime? EndsAt { get; init; }

    /// <summary>
    /// The canceled at date.
    /// </summary>
    [JsonPropertyName("canceled_at")]
    public DateTime? CanceledAt { get; init; }

    /// <summary>
    /// The ended at date.
    /// </summary>
    [JsonPropertyName("ended_at")]
    public DateTime? EndedAt { get; init; }

    /// <summary>
    /// The customer cancellation reason.
    /// </summary>
    [JsonPropertyName("customer_cancellation_reason")]
    public string? CustomerCancellationReason { get; init; }

    /// <summary>
    /// The customer cancellation comment.
    /// </summary>
    [JsonPropertyName("customer_cancellation_comment")]
    public string? CustomerCancellationComment { get; init; }

    /// <summary>
    /// The metadata associated with the subscription.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the subscription.
    /// </summary>
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The creation date of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }

    /// <summary>
    /// The product information.
    /// </summary>
    [JsonPropertyName("product")]
    public Products.Product? Product { get; init; }

    /// <summary>
    /// The product price information.
    /// </summary>
    [JsonPropertyName("product_price")]
    public Products.ProductPrice? ProductPrice { get; init; }

    /// <summary>
    /// The discount information.
    /// </summary>
    [JsonPropertyName("discount")]
    public Discounts.Discount? Discount { get; init; }

    /// <summary>
    /// The prices associated with the subscription.
    /// </summary>
    [JsonPropertyName("prices")]
    public List<Products.ProductPrice>? Prices { get; init; }

    /// <summary>
    /// The meters associated with the subscription.
    /// </summary>
    [JsonPropertyName("meters")]
    public List<Meters.Meter>? Meters { get; init; }

    /// <summary>
    /// The custom field data associated with the subscription.
    /// </summary>
    [JsonPropertyName("custom_field_data")]
    public Dictionary<string, object>? CustomFieldData { get; init; }

    /// <summary>
    /// The seats associated with the subscription.
    /// </summary>
    [JsonPropertyName("seats")]
    public List<Seats.Seat>? Seats { get; init; }
}

/// <summary>
/// Represents the status of a subscription.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// The subscription is active.
    /// </summary>
    Active,

    /// <summary>
    /// The subscription is trialing.
    /// </summary>
    Trialing,

    /// <summary>
    /// The subscription is past due.
    /// </summary>
    PastDue,

    /// <summary>
    /// The subscription is canceled.
    /// </summary>
    Canceled,

    /// <summary>
    /// The subscription is incomplete.
    /// </summary>
    Incomplete,

    /// <summary>
    /// The subscription is incomplete expired.
    /// </summary>
    IncompleteExpired,

    /// <summary>
    /// The subscription is unpaid.
    /// </summary>
    Unpaid,

    /// <summary>
    /// The subscription is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The subscription is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The subscription is incomplete pending.
    /// </summary>
    IncompletePending
}

/// <summary>
/// Request to create a new subscription.
/// </summary>
public record SubscriptionCreateRequest
{
    /// <summary>
    /// The customer ID to create a subscription for.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The product price ID to create a subscription for.
    /// </summary>
    [Required]
    [JsonPropertyName("product_price_id")]
    public string ProductPriceId { get; init; } = string.Empty;

    /// <summary>
    /// The discount ID to apply to the subscription.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The metadata to associate with the subscription.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the subscription.
    /// </summary>
    [JsonPropertyName("external_id")]
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The trial period days.
    /// </summary>
    [JsonPropertyName("trial_period_days")]
    [Range(0, 365, ErrorMessage = "Trial period days must be between 0 and 365 days.")]
    public int? TrialPeriodDays { get; init; }

    /// <summary>
    /// Whether to start the subscription immediately.
    /// </summary>
    [JsonPropertyName("start_immediately")]
    public bool? StartImmediately { get; init; }

    /// <summary>
    /// The recurring interval of the subscription.
    /// </summary>
    [JsonPropertyName("recurring_interval")]
    public RecurringInterval? RecurringInterval { get; init; }

    /// <summary>
    /// The recurring interval count of the subscription.
    /// </summary>
    [JsonPropertyName("recurring_interval_count")]
    public int? RecurringIntervalCount { get; init; }

    /// <summary>
    /// Whether the subscription should be canceled at the end of the period.
    /// </summary>
    [JsonPropertyName("cancel_at_period_end")]
    public bool? CancelAtPeriodEnd { get; init; }

/// <summary>
/// The seats to assign to the subscription.
/// </summary>
[JsonPropertyName("seats")]
public List<Seats.CustomerSeatAssignRequest>? Seats { get; init; }

    /// <summary>
    /// The custom field data to associate with the subscription.
    /// </summary>
    [JsonPropertyName("custom_field_data")]
    public Dictionary<string, object>? CustomFieldData { get; init; }
}

