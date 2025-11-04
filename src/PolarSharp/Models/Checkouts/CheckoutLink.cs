using System.ComponentModel.DataAnnotations;
using PolarSharp.Models.Common;

namespace PolarSharp.Models.Checkouts;

/// <summary>
/// Represents a checkout link in the Polar system.
/// </summary>
public record CheckoutLink
{
    /// <summary>
    /// The unique identifier of the checkout link.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The URL of the checkout link.
    /// </summary>
    [Required]
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// The label of the checkout link.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// The description of the checkout link.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The product ID associated with the checkout link.
    /// </summary>
    public string? ProductId { get; init; }

    /// <summary>
    /// The product price ID associated with the checkout link.
    /// </summary>
    public string? ProductPriceId { get; init; }

    /// <summary>
    /// Whether the checkout link is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Whether the checkout link is archived.
    /// </summary>
    public bool Archived { get; init; }

    /// <summary>
    /// The metadata associated with the checkout link.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the checkout link.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the checkout link.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The product information.
    /// </summary>
    public Products.Product? Product { get; init; }

    /// <summary>
    /// The product price information.
    /// </summary>
    public Products.ProductPrice? ProductPrice { get; init; }
}