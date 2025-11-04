using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Polar.NET.Models.Common;

namespace Polar.NET.Models.Products;

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
    [JsonPropertyName("amount")]
    public int Amount { get; init; }

    /// <summary>
    /// The currency code (e.g., "USD", "EUR").
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The type of price.
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public ProductPriceType Type { get; init; }

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
}