using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Orders;

/// <summary>
/// Request to create a new order.
/// </summary>
public record OrderCreateRequest
{
    /// <summary>
    /// The product ID to create an order for.
    /// </summary>
    [Required]
    [JsonPropertyName("product_id")]
    public string ProductId { get; init; } = string.Empty;

    /// <summary>
    /// The product price ID to create an order for.
    /// </summary>
    [Required]
    [JsonPropertyName("product_price_id")]
    public string ProductPriceId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID to create an order for.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The discount ID to apply to the order.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The metadata to associate with the order.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the order.
    /// </summary>
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The customer email to create an order for.
    /// </summary>
    [EmailAddress]
    [JsonPropertyName("customer_email")]
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// The customer name to create an order for.
    /// </summary>
    [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
    [JsonPropertyName("customer_name")]
    public string? CustomerName { get; init; }

    /// <summary>
    /// Whether to charge the order immediately.
    /// </summary>
    [JsonPropertyName("charge_immediately")]
    public bool? ChargeImmediately { get; init; }
}

