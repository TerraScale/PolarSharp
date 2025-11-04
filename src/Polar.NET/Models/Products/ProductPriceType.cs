using System.Text.Json.Serialization;

namespace Polar.NET.Models.Products;

/// <summary>
/// The type of product price.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProductPriceType
{
    /// <summary>
    /// One-time payment.
    /// </summary>
    [JsonPropertyName("one_time")]
    OneTime,
    
    /// <summary>
    /// Recurring payment.
    /// </summary>
    [JsonPropertyName("recurring")]
    Recurring
}