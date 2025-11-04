using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Payments;

/// <summary>
/// Represents a payment in the Polar system.
/// </summary>
public record Payment
{
    /// <summary>
    /// The unique identifier of the payment.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The amount of the payment in the smallest currency unit (e.g., cents).
    /// </summary>
    [Required]
    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    /// <summary>
    /// The currency of the payment (ISO 4217 format).
    /// </summary>
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The status of the payment.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public PaymentStatus Status { get; init; }

    /// <summary>
    /// The type of the payment.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public PaymentType Type { get; init; }

    /// <summary>
    /// The payment method ID used for the payment.
    /// </summary>
    [JsonPropertyName("payment_method_id")]
    public string? PaymentMethodId { get; init; }

    /// <summary>
    /// The customer ID associated with the payment.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The order ID associated with the payment.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }

    /// <summary>
    /// The subscription ID associated with the payment.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// The checkout ID associated with the payment.
    /// </summary>
    [JsonPropertyName("checkout_id")]
    public string? CheckoutId { get; init; }

    /// <summary>
    /// The failure reason if the payment failed.
    /// </summary>
    [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; init; }

    /// <summary>
    /// The failure code if the payment failed.
    /// </summary>
    [JsonPropertyName("failure_code")]
    public string? FailureCode { get; init; }

    /// <summary>
    /// The metadata associated with the payment.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the payment.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the payment.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The payment method details.
    /// </summary>
    [JsonPropertyName("payment_method")]
    public PaymentMethod? PaymentMethod { get; init; }

    /// <summary>
    /// The refund information.
    /// </summary>
    [JsonPropertyName("refunds")]
    public List<Models.Refunds.Refund>? Refunds { get; init; }
}

/// <summary>
/// Represents the status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// The payment is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The payment succeeded.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The payment failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The payment was canceled.
    /// </summary>
    Canceled,

    /// <summary>
    /// The payment requires action.
    /// </summary>
    RequiresAction,

    /// <summary>
    /// The payment requires confirmation.
    /// </summary>
    RequiresConfirmation,

    /// <summary>
    /// The payment requires payment method.
    /// </summary>
    RequiresPaymentMethod
}

/// <summary>
/// Represents the type of a payment.
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// One-time payment.
    /// </summary>
    OneTime,

    /// <summary>
    /// Subscription payment.
    /// </summary>
    Subscription,

    /// <summary>
    /// Installment payment.
    /// </summary>
    Installment
}

/// <summary>
/// Represents a payment method.
/// </summary>
public record PaymentMethod
{
    /// <summary>
    /// The unique identifier of the payment method.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The type of the payment method.
    /// </summary>
    [Required]
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