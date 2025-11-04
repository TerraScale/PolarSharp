using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Payments;

/// <summary>
/// Request to create a payment method.
/// </summary>
public record PaymentMethodCreateRequest
{
    /// <summary>
    /// The type of payment method.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public PaymentMethodType Type { get; init; }

    /// <summary>
    /// The payment method details.
    /// </summary>
    [Required]
    [JsonPropertyName("details")]
    public Dictionary<string, object> Details { get; init; } = new();

    /// <summary>
    /// Whether this should be set as the default payment method.
    /// </summary>
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; init; }

    /// <summary>
    /// The customer ID to associate with this payment method.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// Metadata associated with the payment method.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}