using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Meters;

/// <summary>
/// Request to update a customer meter entry.
/// </summary>
public record CustomerMeterUpdateRequest
{
    /// <summary>
    /// The amount to set for the meter.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    /// <summary>
    /// The timestamp of the meter entry.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }

    /// <summary>
    /// The metadata associated with the meter entry.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}