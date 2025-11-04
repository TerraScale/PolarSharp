using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Products;

/// <summary>
/// Request to update an existing product.
/// </summary>
public record ProductUpdateRequest
{
    /// <summary>
    /// The name of the product.
    /// </summary>
    [JsonPropertyName("name")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Product name must be between 1 and 100 characters.")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the product.
    /// </summary>
    [JsonPropertyName("description")]
    [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// The URL of the product's image.
    /// </summary>
    [JsonPropertyName("image_url")]
    [Url(ErrorMessage = "Image URL must be a valid URL.")]
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Whether the product is archived.
    /// </summary>
    [JsonPropertyName("is_archived")]
    public bool? IsArchived { get; init; }
}