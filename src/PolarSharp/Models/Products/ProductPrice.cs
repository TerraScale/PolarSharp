using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Common;

namespace PolarSharp.Models.Products;

/// <summary>
/// Represents a product price in Polar system.
/// </summary>
public record ProductPrice
{
    /// <summary>
    /// The unique identifier of price.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The amount in cents.
    /// </summary>
    [Required]
    [Range(0, int.MaxValue)]
    [JsonPropertyName("price_amount")]
    public int Amount { get; init; }

    /// <summary>
    /// The currency code (e.g., "usd", "eur").
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [JsonPropertyName("price_currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The amount type of price.
    /// </summary>
    [Required]
    [JsonPropertyName("amount_type")]
    public ProductPriceType AmountType { get; init; }

    /// <summary>
    /// The type of price (recurring, one_time).
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public PriceType Type { get; init; }

    /// <summary>
    /// The recurring interval for subscription prices.
    /// </summary>
    [JsonPropertyName("recurring_interval")]
    public RecurringInterval? RecurringInterval { get; init; }

    /// <summary>
    /// The creation date of the price.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the price.
    /// </summary>
    [Required]
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The product ID this price belongs to.
    /// </summary>
    [Required]
    [JsonPropertyName("product_id")]
    public string ProductId { get; init; } = string.Empty;
    
    /// <summary>
    /// The source of the price.
    /// </summary>
    [JsonPropertyName("source")]
    public PriceSource Source { get; init; }
    
    /// <summary>
    /// Indicates whether the price is archived.
    /// </summary>
    [JsonPropertyName("is_archived")]
    public bool IsArchived { get; init; }

}

/// <summary>
///  The source of the price.
/// </summary>
public enum PriceSource
{
    /// <summary>
    ///  Catalog price.
    /// </summary>
    [JsonPropertyName("catalog")]
    Catalog = 0,
    
    
    /// <summary>
    ///  Ad-hoc price.
    /// </summary>
    [JsonPropertyName("ad_hoc")]
    AdHoc = 1
}