using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Products;

/// <summary>
/// Request to update an existing product price.
/// </summary>
public record ProductPriceUpdateRequest
{
    /// <summary>
    /// The amount of the price.
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    /// <summary>
    /// The currency code of the price.
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>
    /// Whether the price is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>
    /// The metadata associated with the price.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}