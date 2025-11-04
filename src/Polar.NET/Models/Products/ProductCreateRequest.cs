using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Products;

/// <summary>
/// Request to create a new product.
/// </summary>
public record ProductCreateRequest
{
    /// <summary>
    /// The name of product.
    /// </summary>
    [Required]
    [StringLength(255)]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of product.
    /// </summary>
    [StringLength(2000)]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The URL of product's image.
    /// </summary>
    [Url]
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    /// <summary>
    /// The type of product.
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public ProductType Type { get; init; }

    /// <summary>
    /// Whether the product is a subscription.
    /// </summary>
    [JsonPropertyName("is_subscription")]
    public bool? IsSubscription { get; init; }
}