using System.Text.Json.Serialization;

namespace Polar.NET.Models.Products;

/// <summary>
/// The type of product.
/// </summary>

public enum ProductType
{
    /// <summary>
    /// One-time purchase product.
    /// </summary>
    [JsonPropertyName("one_time")]
    OneTime,
    
    /// <summary>
    /// Subscription product.
    /// </summary>
    [JsonPropertyName("subscription")]
    Subscription
}