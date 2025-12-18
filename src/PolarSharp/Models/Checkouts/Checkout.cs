using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Checkouts;

/// <summary>
/// Represents a checkout session in the Polar system.
/// </summary>
public record Checkout
{
    /// <summary>
    /// The unique identifier of the checkout session.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The status of the checkout session.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public CheckoutStatus Status { get; init; }

    /// <summary>
    /// The customer ID associated with the checkout session.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = null!;

    /// <summary>
    /// The customer email for the checkout session.
    /// </summary>
    [EmailAddress]
    [JsonPropertyName("customer_email")]
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// The product ID associated with the checkout session.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; init; }

    /// <summary>
    /// The product price ID associated with the checkout session.
    /// </summary>
    [JsonPropertyName("product_price_id")]
    public string? ProductPriceId { get; init; }

    /// <summary>
    /// The discount ID applied to the checkout session.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The amount of the checkout session in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("amount")]
    public long? Amount { get; init; }

    /// <summary>
    /// The currency of the checkout session.
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>
    /// The success URL for the checkout session.
    /// </summary>
    [JsonPropertyName("success_url")]
    public string? SuccessUrl { get; init; }

    /// <summary>
    /// The cancel URL for the checkout session.
    /// </summary>
    [JsonPropertyName("cancel_url")]
    public string? CancelUrl { get; init; }

    /// <summary>
    /// The URL for the checkout session.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; init; } = null!;

    /// <summary>
    /// The client secret for client-side operations.
    /// This is used with the /v1/checkouts/client/{client_secret} endpoints.
    /// </summary>
    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; init; }

    /// <summary>
    /// The metadata associated with the checkout session.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the checkout session.
    /// </summary>
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// Whether the checkout session is for a subscription.
    /// </summary>
    [JsonPropertyName("is_subscription")]
    public bool? IsSubscription { get; init; }

    /// <summary>
    /// The trial period days for the subscription.
    /// </summary>
    [JsonPropertyName("trial_period_days")]
    public int? TrialPeriodDays { get; init; }

    /// <summary>
    /// The creation date of the checkout session.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the checkout session.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The expiration date of the checkout session.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }
    
    /// <summary>
    /// The product information.
    /// </summary>
    [JsonPropertyName("product")]
    public required Products.Product Product { get; init; }

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
    /// The order information if checkout is completed.
    /// </summary>
    [JsonPropertyName("order")]
    public Orders.Order? Order { get; init; }

    /// <summary>
    /// The subscription id if checkout is for a subscription.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string? SubscriptionId { get; init; }
    
    /// <summary>
    /// Seats information for the checkout session.
    /// </summary>
    [JsonPropertyName("seats")]
    public int? Seats { get; init; }
}

/// <summary>
/// Represents the status of a checkout session.
/// </summary>
public enum CheckoutStatus
{
    /// <summary>
    /// The checkout session is open.
    /// </summary>
    [JsonPropertyName("open")]
    Open,

    /// <summary>
    /// The checkout session is completed.
    /// </summary>
    [JsonPropertyName("completed")]
    Completed,

    /// <summary>
    /// The checkout session is expired.
    /// </summary>
    [JsonPropertyName("expired")]
    Expired,

    /// <summary>
    /// The checkout session is canceled.
    /// </summary>
    [JsonPropertyName("canceled")]
    Canceled
}

/// <summary>
/// Request to create a new checkout session.
/// </summary>
public record CheckoutCreateRequest
{
    /// <summary>
    /// List of product IDs available to select at that checkout.
    /// The first one will be selected by default.
    /// </summary>
    [Required]
    [JsonPropertyName("products")]
    public List<string> Products { get; init; } = new();

    /// <summary>
    /// The product ID to create a checkout session for.
    /// Deprecated: Use Products instead.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; init; }

    /// <summary>
    /// The product price ID to create a checkout session for.
    /// Deprecated: Use Products instead.
    /// </summary>
    [JsonPropertyName("product_price_id")]
    public string? ProductPriceId { get; init; }

    /// <summary>
    /// The customer ID to create a checkout session for.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The customer email to create a checkout session for.
    /// </summary>
    [EmailAddress]
    [JsonPropertyName("customer_email")]
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// The discount ID to apply to the checkout session.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The success URL for checkout session.
    /// </summary>
    [JsonPropertyName("success_url")]
    public string? SuccessUrl { get; init; }

    /// <summary>
    /// The cancel URL for checkout session.
    /// </summary>
    [JsonPropertyName("cancel_url")]
    public string? CancelUrl { get; init; }

    /// <summary>
    /// URL where the customer can return to from the checkout.
    /// </summary>
    [JsonPropertyName("return_url")]
    public string? ReturnUrl { get; init; }

    /// <summary>
    /// The metadata to associate with the checkout session.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external customer ID for checkout session.
    /// </summary>
    [JsonPropertyName("external_customer_id")]
    public string? ExternalCustomerId { get; init; }

    /// <summary>
    /// Whether checkout session is for a subscription.
    /// </summary>
    [JsonPropertyName("is_subscription")]
    public bool? IsSubscription { get; init; }

    /// <summary>
    /// The trial period days for the subscription.
    /// </summary>
    [JsonPropertyName("trial_period_days")]
    public int? TrialPeriodDays { get; init; }
}

/// <summary>
/// Response for checkout export.
/// </summary>
public record CheckoutExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    [JsonPropertyName("export_url")]
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export file size in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The number of records in export.
    /// </summary>
    [JsonPropertyName("record_count")]
    public int RecordCount { get; init; }
}

