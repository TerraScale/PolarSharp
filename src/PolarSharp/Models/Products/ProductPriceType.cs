using System.Text.Json.Serialization;

namespace PolarSharp.Models.Products;

/// <summary>
/// The type of product price amount.
/// </summary>
public enum ProductPriceType
{
    /// <summary>
    /// Fixed price amount.
    /// </summary>
    [JsonPropertyName("fixed")]
    Fixed,
    
    /// <summary>
    /// Custom price amount.
    /// </summary>
    [JsonPropertyName("custom")]
    Custom,
    
    /// <summary>
    /// Free price amount.
    /// </summary>
    [JsonPropertyName("free")]
    Free,
    
    /// <summary>
    /// Seat-based price amount.
    /// </summary>
    [JsonPropertyName("seat_based")]
    SeatBased,
    
    /// <summary>
    /// Metered unit price amount.
    /// </summary>
    [JsonPropertyName("metered_unit")]
    MeteredUnit
}