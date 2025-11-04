using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Customers;

/// <summary>
/// Request to create a new customer.
/// </summary>
public record CustomerCreateRequest
{
    /// <summary>
    /// The email address of customer.
    /// </summary>
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The name of customer.
    /// </summary>
    [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The external ID for the customer.
    /// </summary>
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The metadata to associate with customer.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The avatar URL of customer.
    /// </summary>
    [Url(ErrorMessage = "Avatar URL must be a valid URL.")]
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The billing address of customer.
    /// </summary>
    [JsonPropertyName("billing_address")]
    public Address? BillingAddress { get; init; }

    /// <summary>
    /// The shipping address of customer.
    /// </summary>
    [JsonPropertyName("shipping_address")]
    public Address? ShippingAddress { get; init; }
}

/// <summary>
/// Request to update an existing customer.
/// </summary>


/// <summary>
/// Represents the state of a customer.
/// </summary>
public record CustomerState
{
    /// <summary>
    /// The customer ID.
    /// </summary>
    [Required]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// Whether the customer has active subscriptions.
    /// </summary>
    public bool HasActiveSubscriptions { get; init; }

    /// <summary>
    /// Whether the customer has active benefits.
    /// </summary>
    public bool HasActiveBenefits { get; init; }

    /// <summary>
    /// Whether the customer has active license keys.
    /// </summary>
    public bool HasActiveLicenseKeys { get; init; }

    /// <summary>
    /// The number of active subscriptions.
    /// </summary>
    public int ActiveSubscriptionsCount { get; init; }

    /// <summary>
    /// The number of active benefits.
    /// </summary>
    public int ActiveBenefitsCount { get; init; }

    /// <summary>
    /// The number of active license keys.
    /// </summary>
    public int ActiveLicenseKeysCount { get; init; }

    /// <summary>
    /// The total amount spent by the customer.
    /// </summary>
    public long TotalAmountSpent { get; init; }

    /// <summary>
    /// The currency of the total amount spent.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The last order date of the customer.
    /// </summary>
    public DateTime? LastOrderAt { get; init; }

    /// <summary>
    /// The last subscription date of the customer.
    /// </summary>
    public DateTime? LastSubscriptionAt { get; init; }

    /// <summary>
    /// The creation date of the customer state.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the customer state.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Represents the balance of a customer.
/// </summary>
public record CustomerBalance
{
    /// <summary>
    /// The customer ID.
    /// </summary>
    [Required]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The current balance of the customer.
    /// </summary>
    public long Balance { get; init; }

    /// <summary>
    /// The currency of the balance.
    /// </summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The creation date of the customer balance.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the customer balance.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }
}