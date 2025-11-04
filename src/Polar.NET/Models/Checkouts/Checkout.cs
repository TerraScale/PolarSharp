using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.Checkouts;

/// <summary>
/// Represents a checkout session in the Polar system.
/// </summary>
public record Checkout
{
    /// <summary>
    /// The unique identifier of the checkout session.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The status of the checkout session.
    /// </summary>
    [Required]
    public CheckoutStatus Status { get; init; }

    /// <summary>
    /// The customer ID associated with the checkout session.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// The customer email for the checkout session.
    /// </summary>
    [EmailAddress]
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// The product ID associated with the checkout session.
    /// </summary>
    public string? ProductId { get; init; }

    /// <summary>
    /// The product price ID associated with the checkout session.
    /// </summary>
    public string? ProductPriceId { get; init; }

    /// <summary>
    /// The discount ID applied to the checkout session.
    /// </summary>
    public string? DiscountId { get; init; }

    /// <summary>
    /// The amount of the checkout session in the smallest currency unit.
    /// </summary>
    public long? Amount { get; init; }

    /// <summary>
    /// The currency of the checkout session.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// The success URL for the checkout session.
    /// </summary>
    public string? SuccessUrl { get; init; }

    /// <summary>
    /// The cancel URL for the checkout session.
    /// </summary>
    public string? CancelUrl { get; init; }

    /// <summary>
    /// The URL for the checkout session.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// The metadata associated with the checkout session.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the checkout session.
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// Whether the checkout session is for a subscription.
    /// </summary>
    public bool? IsSubscription { get; init; }

    /// <summary>
    /// The trial period days for the subscription.
    /// </summary>
    public int? TrialPeriodDays { get; init; }

    /// <summary>
    /// The creation date of the checkout session.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the checkout session.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The expiration date of the checkout session.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    public Models.Customers.Customer? Customer { get; init; }

    /// <summary>
    /// The product information.
    /// </summary>
    public Models.Products.Product? Product { get; init; }

    /// <summary>
    /// The product price information.
    /// </summary>
    public Models.Products.ProductPrice? ProductPrice { get; init; }

    /// <summary>
    /// The discount information.
    /// </summary>
    public Models.Discounts.Discount? Discount { get; init; }

    /// <summary>
    /// The order information if the checkout is completed.
    /// </summary>
    public Models.Orders.Order? Order { get; init; }

    /// <summary>
    /// The subscription information if the checkout is for a subscription.
    /// </summary>
    public Models.Subscriptions.Subscription? Subscription { get; init; }
}

/// <summary>
/// Represents the status of a checkout session.
/// </summary>
public enum CheckoutStatus
{
    /// <summary>
    /// The checkout session is open.
    /// </summary>
    Open,

    /// <summary>
    /// The checkout session is completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The checkout session is expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The checkout session is canceled.
    /// </summary>
    Canceled
}

/// <summary>
/// Request to create a new checkout session.
/// </summary>
public record CheckoutCreateRequest
{
    /// <summary>
    /// The product ID to create a checkout session for.
    /// </summary>
    [Required]
    public string ProductId { get; init; } = string.Empty;

    /// <summary>
    /// The product price ID to create a checkout session for.
    /// </summary>
    [Required]
    public string ProductPriceId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID to create a checkout session for.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// The customer email to create a checkout session for.
    /// </summary>
    [EmailAddress]
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// The discount ID to apply to the checkout session.
    /// </summary>
    public string? DiscountId { get; init; }

    /// <summary>
    /// The success URL for the checkout session.
    /// </summary>
    public string? SuccessUrl { get; init; }

    /// <summary>
    /// The cancel URL for the checkout session.
    /// </summary>
    public string? CancelUrl { get; init; }

    /// <summary>
    /// The metadata to associate with the checkout session.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the checkout session.
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// Whether the checkout session is for a subscription.
    /// </summary>
    public bool? IsSubscription { get; init; }

    /// <summary>
    /// The trial period days for the subscription.
    /// </summary>
    public int? TrialPeriodDays { get; init; }
}

