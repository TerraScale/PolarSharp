using System.Text.Json.Serialization;

namespace PolarSharp.Models.Products;

/// <summary>
/// The type of price (recurring vs one-time).
/// </summary>
public enum PriceType
{
    /// <summary>
    /// One-time payment price.
    /// </summary>
    [JsonPropertyName("one_time")]
    OneTime,
    
    /// <summary>
    /// Recurring payment price.
    /// </summary>
    [JsonPropertyName("recurring")]
    Recurring
}