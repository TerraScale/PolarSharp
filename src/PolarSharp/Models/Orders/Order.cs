using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Common;

namespace PolarSharp.Models.Orders;

/// <summary>
/// Represents an order in the Polar system.
/// </summary>
public record Order
{
    /// <summary>
    /// The unique identifier of the order.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The status of the order.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public OrderStatus Status { get; init; }
    
    /// <summary>
    /// If the order is paid.
    /// </summary>
    [Required]
    [JsonPropertyName("is_paid")]
    public bool IsPaid { get; init; }

    /// <summary>
    /// The total amount after discounts in the smallest currency unit.
    /// </summary>
    [Required]
    [JsonPropertyName("total_amount")]
    public int Amount { get; init; }

    /// <summary>
    /// The currency of the order (ISO 4217 format).
    /// </summary>
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID associated with the order.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The product ID associated with the order.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; init; }

    /// <summary>
    /// The product price ID associated with the order.
    /// </summary>
    [JsonPropertyName("product_price_id")]
    public string? ProductPriceId { get; init; }

    /// <summary>
    /// The discount ID applied to the order.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The metadata associated with the order.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID for the order.
    /// </summary>
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The URL for the order receipt.
    /// </summary>
    [JsonPropertyName("receipt_url")]
    public string? ReceiptUrl { get; init; }

    /// <summary>
    /// The URL for the order invoice.
    /// </summary>
    [JsonPropertyName("invoice_url")]
    public string? InvoiceUrl { get; init; }

    /// <summary>
    /// The refund status of the order.
    /// </summary>
    [JsonPropertyName("refund_status")]
    public OrderRefundStatus? RefundStatus { get; init; }

    /// <summary>
    /// The amount refunded in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("refund_amount")]
    public long? RefundAmount { get; init; }

    /// <summary>
    /// The creation date of the order.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the order.
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
    /// The payment information.
    /// </summary>
    [JsonPropertyName("payment")]
    public Payments.Payment? Payment { get; init; }
}

/// <summary>
/// Represents the status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order is pending payment.
    /// </summary>
    Pending,

    /// <summary>
    /// The order has been paid.
    /// </summary>
    Paid,

    /// <summary>
    /// The order has been refunded.
    /// </summary>
    Refunded,

    /// <summary>
    /// The order has been partially refunded.
    /// </summary>
    PartiallyRefunded,

    /// <summary>
    /// The order has been canceled.
    /// </summary>
    Canceled,

    /// <summary>
    /// The order has failed.
    /// </summary>
    Failed
}

/// <summary>
/// Represents the refund status of an order.
/// </summary>
public enum OrderRefundStatus
{
    /// <summary>
    /// The order has not been refunded.
    /// </summary>
    None,

    /// <summary>
    /// The order has been fully refunded.
    /// </summary>
    Full,

    /// <summary>
    /// The order has been partially refunded.
    /// </summary>
    Partial
}