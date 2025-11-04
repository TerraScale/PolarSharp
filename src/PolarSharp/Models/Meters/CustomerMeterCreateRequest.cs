using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Meters;

/// <summary>
/// Request to create a customer meter entry.
/// </summary>
public record CustomerMeterCreateRequest
{
    /// <summary>
    /// The customer ID.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The meter ID.
    /// </summary>
    [Required]
    [JsonPropertyName("meter_id")]
    public string MeterId { get; init; } = string.Empty;

    /// <summary>
    /// The amount to add to the meter.
    /// </summary>
    [Required]
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    /// <summary>
    /// The timestamp of the meter entry (optional, defaults to current time).
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// The metadata associated with the meter entry.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}