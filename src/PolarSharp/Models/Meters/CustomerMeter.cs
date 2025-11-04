using System.Text.Json.Serialization;

namespace PolarSharp.Models.Meters;

/// <summary>
/// Represents a customer meter in the Polar system.
/// </summary>
public record CustomerMeter
{
    /// <summary>
    /// The unique identifier of the customer meter.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The meter ID.
    /// </summary>
    [JsonPropertyName("meter_id")]
    public string MeterId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The current quantity value.
    /// </summary>
    [JsonPropertyName("current_quantity")]
    public decimal CurrentQuantity { get; init; }

    /// <summary>
    /// The period start date for current quantity.
    /// </summary>
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; init; }

    /// <summary>
    /// The period end date for current quantity.
    /// </summary>
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; init; }

    /// <summary>
    /// The creation date of the customer meter.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the customer meter.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The meter information.
    /// </summary>
    [JsonPropertyName("meter")]
    public Meter? Meter { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }
}