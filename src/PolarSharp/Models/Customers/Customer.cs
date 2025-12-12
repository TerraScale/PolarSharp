using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Customers;

/// <summary>
/// Represents a customer in the Polar system.
/// </summary>
public record Customer
{
    /// <summary>
    /// The unique identifier of customer.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

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
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The external ID for the customer.
    /// </summary>
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The metadata associated with the customer.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether customer has a verified email address.
    /// </summary>
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; init; }

    /// <summary>
    /// The avatar URL of customer.
    /// </summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The creation date of customer.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// The deletion date of customer.
    /// </summary>
    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; init; }

    /// <summary>
    /// The last update date of customer.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The customer's billing address.
    /// </summary>
    [JsonPropertyName("billing_address")]
    public Address? BillingAddress { get; init; }

    /// <summary>
    /// The customer's shipping address.
    /// </summary>
    [JsonPropertyName("shipping_address")]
    public Address? ShippingAddress { get; init; }

    /// <summary>
    /// The customer's payment methods.
    /// </summary>
    public List<PaymentMethod>? PaymentMethods { get; init; }

    /// <summary>
    /// The customer's subscriptions.
    /// </summary>
    public List<Subscriptions.Subscription>? Subscriptions { get; init; }

    /// <summary>
    /// The customer's orders.
    /// </summary>
    public List<Orders.Order>? Orders { get; init; }
    
    /// <summary>
    /// The customer's tax ID.
    /// </summary>
    [JsonPropertyName("tax_id")]
    public Dictionary<string, string>? TaxIds { get; init; }
    
    /// <summary>
    /// The organization ID associated with the customer.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;
}

/// <summary>
/// Represents an address.
/// </summary>
public record Address
{
    /// <summary>
    /// The street address.
    /// </summary>
    [JsonPropertyName("street")]
    public string? Street { get; init; }

    /// <summary>
    /// The city.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; init; }

    /// <summary>
    /// The state or province.
    /// </summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>
    /// The postal code.
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; init; }

    /// <summary>
    /// The country.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }
    
    /// <summary>
    /// The phone number.
    /// </summary>
    [JsonPropertyName("line1")]
    public string? Line1 { get; init; }
    
    /// <summary>
    /// The phone number.
    /// </summary>
    [JsonPropertyName("line2")]
    public string? Line2 { get; init; }
}

/// <summary>
/// Represents a payment method.
/// </summary>
public record PaymentMethod
{
    /// <summary>
    /// The unique identifier of payment method.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The type of payment method.
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public PaymentMethodType Type { get; init; }

    /// <summary>
    /// Whether this is the default payment method.
    /// </summary>
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; init; }

    /// <summary>
    /// The last 4 digits of the card (if applicable).
    /// </summary>
    [JsonPropertyName("last4")]
    public string? Last4 { get; init; }

    /// <summary>
    /// The expiration month of the card (if applicable).
    /// </summary>
    [JsonPropertyName("expiration_month")]
    public int? ExpirationMonth { get; init; }

    /// <summary>
    /// The expiration year of the card (if applicable).
    /// </summary>
    [JsonPropertyName("expiration_year")]
    public int? ExpirationYear { get; init; }

    /// <summary>
    /// The brand of the card (if applicable).
    /// </summary>
    [JsonPropertyName("brand")]
    public string? Brand { get; init; }

    /// <summary>
    /// The creation date of the payment method.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the payment method.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Represents the type of a payment method.
/// </summary>
public enum PaymentMethodType
{
    /// <summary>
    /// Credit card payment method.
    /// </summary>
    Card,

    /// <summary>
    /// Bank account payment method.
    /// </summary>
    BankAccount,

    /// <summary>
    /// PayPal payment method.
    /// </summary>
    PayPal,

    /// <summary>
    /// Other payment method.
    /// </summary>
    Other
}