using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using PolarSharp.Extensions;
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
    /// Whether the order has been paid.
    /// </summary>
    [Required]
    [JsonPropertyName("paid")]
    public bool Paid { get; init; }

    /// <summary>
    /// The subtotal amount before discounts and taxes in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("subtotal_amount")]
    public long SubtotalAmount { get; init; }

    /// <summary>
    /// The discount amount in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("discount_amount")]
    public long DiscountAmount { get; init; }

    /// <summary>
    /// The net amount after discounts in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("net_amount")]
    public long NetAmount { get; init; }

    /// <summary>
    /// The tax amount in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("tax_amount")]
    public long TaxAmount { get; init; }

    /// <summary>
    /// The total amount including taxes in the smallest currency unit.
    /// </summary>
    [Required]
    [JsonPropertyName("total_amount")]
    public long TotalAmount { get; init; }

    /// <summary>
    /// The amount applied from customer balance in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("applied_balance_amount")]
    public long AppliedBalanceAmount { get; init; }

    /// <summary>
    /// The amount due after balance applied in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("due_amount")]
    public long DueAmount { get; init; }

    /// <summary>
    /// The amount refunded in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("refunded_amount")]
    public long RefundedAmount { get; init; }

    /// <summary>
    /// The tax amount refunded in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("refunded_tax_amount")]
    public long RefundedTaxAmount { get; init; }

    /// <summary>
    /// The currency of the order (ISO 4217 format).
    /// </summary>
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The reason for the billing.
    /// </summary>
    [JsonPropertyName("billing_reason")]
    public OrderBillingReason? BillingReason { get; init; }

    /// <summary>
    /// The billing name for the order.
    /// </summary>
    [JsonPropertyName("billing_name")]
    public string? BillingName { get; init; }

    /// <summary>
    /// The billing address for the order.
    /// </summary>
    [JsonPropertyName("billing_address")]
    public Address? BillingAddress { get; init; }

    /// <summary>
    /// The invoice number for the order.
    /// </summary>
    [JsonPropertyName("invoice_number")]
    public string? InvoiceNumber { get; init; }

    /// <summary>
    /// Whether an invoice has been generated.
    /// </summary>
    [JsonPropertyName("is_invoice_generated")]
    public bool IsInvoiceGenerated { get; init; }

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
    /// The discount ID applied to the order.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// The subscription ID associated with the order.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// The checkout ID associated with the order.
    /// </summary>
    [JsonPropertyName("checkout_id")]
    public string? CheckoutId { get; init; }

    /// <summary>
    /// The metadata associated with the order.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The platform fee amount in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("platform_fee_amount")]
    public long? PlatformFeeAmount { get; init; }

    /// <summary>
    /// The order line items.
    /// </summary>
    [JsonPropertyName("items")]
    public List<OrderItem>? Items { get; init; }

    /// <summary>
    /// The description of the order.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The number of seats for the order.
    /// </summary>
    [JsonPropertyName("seats")]
    public int? Seats { get; init; }

    /// <summary>
    /// Custom field data associated with the order.
    /// </summary>
    [JsonPropertyName("custom_field_data")]
    public Dictionary<string, object>? CustomFieldData { get; init; }

    /// <summary>
    /// The creation date of the order.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the order.
    /// </summary>
    [Required]
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// Alias for ModifiedAt for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt => ModifiedAt;

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
    /// The discount information.
    /// </summary>
    [JsonPropertyName("discount")]
    public Discounts.Discount? Discount { get; init; }

    /// <summary>
    /// The subscription information.
    /// </summary>
    [JsonPropertyName("subscription")]
    public Subscriptions.Subscription? Subscription { get; init; }

    /// <summary>
    /// Alias for TotalAmount for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public long Amount => TotalAmount;

    /// <summary>
    /// The product price ID (for backward compatibility, use Items[0].ProductPriceId instead).
    /// </summary>
    [JsonIgnore]
    public string? ProductPriceId => Items?.FirstOrDefault()?.ProductPriceId;
}

/// <summary>
/// Represents an order line item.
/// </summary>
public record OrderItem
{
    /// <summary>
    /// The unique identifier of the order item.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The label/name of the item.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// The amount in the smallest currency unit.
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    /// <summary>
    /// The quantity of the item.
    /// </summary>
    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    /// <summary>
    /// The product price ID for this item.
    /// </summary>
    [JsonPropertyName("product_price_id")]
    public string? ProductPriceId { get; init; }

    /// <summary>
    /// The tax amount for this item (for backward compatibility).
    /// </summary>
    [JsonPropertyName("tax_amount")]
    public long? TaxAmount { get; init; }

    /// <summary>
    /// The total amount for this item (for backward compatibility).
    /// </summary>
    [JsonPropertyName("total_amount")]
    public long? TotalAmount { get; init; }

    /// <summary>
    /// The price amount for this item (for backward compatibility).
    /// </summary>
    [JsonPropertyName("price_amount")]
    public long? PriceAmount { get; init; }

    /// <summary>
    /// The currency for this item (for backward compatibility).
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }
}

/// <summary>
/// Represents a billing address.
/// </summary>
public record Address
{
    /// <summary>
    /// The first line of the address.
    /// </summary>
    [JsonPropertyName("line1")]
    public string? Line1 { get; init; }

    /// <summary>
    /// The second line of the address.
    /// </summary>
    [JsonPropertyName("line2")]
    public string? Line2 { get; init; }

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
    /// The country code (ISO 3166-1 alpha-2).
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }
}

/// <summary>
/// Represents the status of an order.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverterWithAttributeNames))]
public enum OrderStatus
{
    /// <summary>
    /// The order is pending payment.
    /// </summary>
    [JsonPropertyName("pending")]
    Pending,

    /// <summary>
    /// The order has been paid.
    /// </summary>
    [JsonPropertyName("paid")]
    Paid,

    /// <summary>
    /// The order has been refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    Refunded,

    /// <summary>
    /// The order has been partially refunded.
    /// </summary>
    [JsonPropertyName("partially_refunded")]
    PartiallyRefunded,

    /// <summary>
    /// The order is disputed.
    /// </summary>
    [JsonPropertyName("disputed")]
    Disputed,

    /// <summary>
    /// The order is complete (for backward compatibility).
    /// </summary>
    [JsonPropertyName("complete")]
    Complete = Paid,

    /// <summary>
    /// The order is processing (for backward compatibility).
    /// </summary>
    [JsonPropertyName("processing")]
    Processing = Pending
}

/// <summary>
/// Represents the billing reason for an order.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverterWithAttributeNames))]
public enum OrderBillingReason
{
    /// <summary>
    /// One-time purchase.
    /// </summary>
    [JsonPropertyName("purchase")]
    Purchase,

    /// <summary>
    /// New subscription created.
    /// </summary>
    [JsonPropertyName("subscription_create")]
    SubscriptionCreate,

    /// <summary>
    /// Subscription renewal cycle.
    /// </summary>
    [JsonPropertyName("subscription_cycle")]
    SubscriptionCycle,

    /// <summary>
    /// Subscription update (e.g., plan change).
    /// </summary>
    [JsonPropertyName("subscription_update")]
    SubscriptionUpdate
}
