using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.Payments;

/// <summary>
/// Request to create a payment method.
/// </summary>
public record PaymentMethodCreateRequest
{
    /// <summary>
    /// The type of the payment method.
    /// </summary>
    [Required]
    public PaymentMethodType Type { get; init; }

    /// <summary>
    /// The payment method details.
    /// </summary>
    [Required]
    public Dictionary<string, object> Details { get; init; } = new();

    /// <summary>
    /// Whether this should be set as the default payment method.
    /// </summary>
    public bool IsDefault { get; init; }

    /// <summary>
    /// The customer ID to associate with this payment method.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// Metadata associated with the payment method.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}