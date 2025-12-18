using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Products;

namespace PolarSharp.Models.Subscriptions;

/// <summary>
/// Represents a subscription in the Polar system.
/// </summary>
public record Subscription
{
    /// <summary>
    /// The creation date of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// The unique identifier of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The amount of the subscription in cents.
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; init; }

    /// <summary>
    /// The currency of the subscription.
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The recurring interval of the subscription (day, month, year).
    /// </summary>
    [JsonPropertyName("recurring_interval")]
    public RecurringInterval RecurringInterval { get; init; }

    /// <summary>
    /// The recurring interval count of the subscription.
    /// </summary>
    [JsonPropertyName("recurring_interval_count")]
    public int RecurringIntervalCount { get; init; }

    /// <summary>
    /// The status of the subscription.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public SubscriptionStatus Status { get; init; }

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
    public DateTime? CurrentPeriodEnd { get; init; }

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
    /// Whether the subscription will be canceled at the end of the period.
    /// </summary>
    [JsonPropertyName("cancel_at_period_end")]
    public bool CancelAtPeriodEnd { get; init; }

    /// <summary>
    /// The canceled at date.
    /// </summary>
    [JsonPropertyName("canceled_at")]
    public DateTime? CanceledAt { get; init; }

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
    /// The ended at date.
    /// </summary>
    [JsonPropertyName("ended_at")]
    public DateTime? EndedAt { get; init; }

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
    /// The discount ID associated with the subscription.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The checkout ID associated with the subscription.
    /// </summary>
    [JsonPropertyName("checkout_id")]
    public string? CheckoutId { get; init; }

    /// <summary>
    /// The customer cancellation reason.
    /// </summary>
    [JsonPropertyName("customer_cancellation_reason")]
    public CustomerCancellationReason? CustomerCancellationReason { get; init; }

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
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer Customer { get; init; } = new();

    /// <summary>
    /// The product information.
    /// </summary>
    [JsonPropertyName("product")]
    public Product Product { get; init; } = new();

    /// <summary>
    /// The discount information.
    /// </summary>
    [JsonPropertyName("discount")]
    public Discounts.Discount? Discount { get; init; }

    /// <summary>
    /// The prices associated with the subscription.
    /// </summary>
    [JsonPropertyName("prices")]
    public List<ProductPrice>? Prices { get; init; }

    /// <summary>
    /// The meters associated with the subscription.
    /// </summary>
    [JsonPropertyName("meters")]
    public List<SubscriptionMeterInfo>? Meters { get; init; }

    /// <summary>
    /// The number of seats associated with the subscription.
    /// </summary>
    [JsonPropertyName("seats")]
    public int? Seats { get; init; }

    /// <summary>
    /// The custom field data associated with the subscription.
    /// </summary>
    [JsonPropertyName("custom_field_data")]
    public Dictionary<string, object>? CustomFieldData { get; init; }
}

/// <summary>
/// Represents meter information associated with a subscription.
/// </summary>
public record SubscriptionMeterInfo
{
    /// <summary>
    /// The creation date of the meter info.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the meter info.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The unique identifier of the subscription meter info.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The number of consumed units.
    /// </summary>
    [JsonPropertyName("consumed_units")]
    public int ConsumedUnits { get; init; }

    /// <summary>
    /// The number of credited units.
    /// </summary>
    [JsonPropertyName("credited_units")]
    public int CreditedUnits { get; init; }

    /// <summary>
    /// The amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; init; }

    /// <summary>
    /// The meter ID.
    /// </summary>
    [JsonPropertyName("meter_id")]
    public string MeterId { get; init; } = string.Empty;

    /// <summary>
    /// The meter details.
    /// </summary>
    [JsonPropertyName("meter")]
    public SubscriptionMeterDetails Meter { get; init; } = new();
}

/// <summary>
/// Represents meter details within a subscription.
/// </summary>
public record SubscriptionMeterDetails
{
    /// <summary>
    /// The metadata associated with the meter.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the meter.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the meter.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The unique identifier of the meter.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the meter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The filter configuration for the meter.
    /// </summary>
    [JsonPropertyName("filter")]
    public MeterFilter Filter { get; init; } = new();

    /// <summary>
    /// The aggregation configuration for the meter.
    /// </summary>
    [JsonPropertyName("aggregation")]
    public MeterAggregation Aggregation { get; init; } = new();

    /// <summary>
    /// The organization ID that owns the meter.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The archived date of the meter, if archived.
    /// </summary>
    [JsonPropertyName("archived_at")]
    public DateTime? ArchivedAt { get; init; }
}

/// <summary>
/// Represents the filter configuration for a meter.
/// </summary>
public record MeterFilter
{
    /// <summary>
    /// The conjunction type (and/or).
    /// </summary>
    [JsonPropertyName("conjunction")]
    public string Conjunction { get; init; } = "and";

    /// <summary>
    /// The filter clauses.
    /// </summary>
    [JsonPropertyName("clauses")]
    public List<MeterFilterClause> Clauses { get; init; } = new();
}

/// <summary>
/// Represents a filter clause for a meter.
/// </summary>
public record MeterFilterClause
{
    /// <summary>
    /// The property to filter on.
    /// </summary>
    [JsonPropertyName("property")]
    public string Property { get; init; } = string.Empty;

    /// <summary>
    /// The operator for the filter (eq, ne, gt, lt, etc.).
    /// </summary>
    [JsonPropertyName("operator")]
    public string Operator { get; init; } = "eq";

    /// <summary>
    /// The value to compare against.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;
}

/// <summary>
/// Represents the aggregation configuration for a meter.
/// </summary>
public record MeterAggregation
{
    /// <summary>
    /// The aggregation function (count, sum, avg, etc.).
    /// </summary>
    [JsonPropertyName("func")]
    public string Func { get; init; } = "count";
}

/// <summary>
/// Represents the status of a subscription.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionStatus
{
    /// <summary>
    /// The subscription is incomplete.
    /// </summary>
    [JsonPropertyName("incomplete")]
    Incomplete,

    /// <summary>
    /// The subscription is incomplete expired.
    /// </summary>
    [JsonPropertyName("incomplete_expired")]
    IncompleteExpired,

    /// <summary>
    /// The subscription is trialing.
    /// </summary>
    [JsonPropertyName("trialing")]
    Trialing,

    /// <summary>
    /// The subscription is active.
    /// </summary>
    [JsonPropertyName("active")]
    Active,

    /// <summary>
    /// The subscription is past due.
    /// </summary>
    [JsonPropertyName("past_due")]
    PastDue,

    /// <summary>
    /// The subscription is canceled.
    /// </summary>
    [JsonPropertyName("canceled")]
    Canceled,

    /// <summary>
    /// The subscription is unpaid.
    /// </summary>
    [JsonPropertyName("unpaid")]
    Unpaid
}

/// <summary>
/// Represents the customer cancellation reason for a subscription.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CustomerCancellationReason
{
    /// <summary>
    /// Customer service related cancellation.
    /// </summary>
    [JsonPropertyName("customer_service")]
    CustomerService,

    /// <summary>
    /// Too expensive.
    /// </summary>
    [JsonPropertyName("too_expensive")]
    TooExpensive,

    /// <summary>
    /// Missing features.
    /// </summary>
    [JsonPropertyName("missing_features")]
    MissingFeatures,

    /// <summary>
    /// Switched to competitor.
    /// </summary>
    [JsonPropertyName("switched_service")]
    SwitchedService,

    /// <summary>
    /// Not using the service.
    /// </summary>
    [JsonPropertyName("unused")]
    Unused,

    /// <summary>
    /// Technical issues.
    /// </summary>
    [JsonPropertyName("too_complex")]
    TooComplex,

    /// <summary>
    /// Other reason.
    /// </summary>
    [JsonPropertyName("other")]
    Other
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

