using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Polar.NET.Models.Common;

namespace Polar.NET.Models.Products;

/// <summary>
/// Represents a product in Polar system.
/// </summary>
public record Product
{
    /// <summary>
    /// The unique identifier of product.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

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
    /// Whether product is currently archived.
    /// </summary>
    [JsonPropertyName("is_archived")]
    public bool IsArchived { get; init; }

    /// <summary>
    /// Whether product is a subscription.
    /// </summary>
    [JsonPropertyName("is_subscription")]
    public bool IsSubscription { get; init; }

    /// <summary>
    /// The organization ID that owns product.
    /// </summary>
    [Required]
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The creation date of product.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of product.
    /// </summary>
    [Required]
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The prices associated with this product.
    /// </summary>
    [JsonPropertyName("prices")]
    public IReadOnlyList<ProductPrice> Prices { get; init; } = new List<ProductPrice>();

    /// <summary>
    /// The type of product.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("type")]
    public ProductType? Type { get; init; }
}
/// <summary>
/// Response for product export.
/// </summary>
public record ProductExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    [JsonPropertyName("export_url")]
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export file size in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The number of records in export.
    /// </summary>
    [JsonPropertyName("record_count")]
    public int RecordCount { get; init; }
}

/// <summary>
/// Response for product price export.
/// </summary>
public record ProductPriceExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    [JsonPropertyName("export_url")]
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export file size in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The number of records in export.
    /// </summary>
    [JsonPropertyName("record_count")]
    public int RecordCount { get; init; }
}
