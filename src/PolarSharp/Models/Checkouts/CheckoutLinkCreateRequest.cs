using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.Checkouts;

/// <summary>
/// Request to create a new checkout link.
/// </summary>
public record CheckoutLinkCreateRequest
{
    /// <summary>
    /// The product ID to create a checkout link for.
    /// </summary>
    [Required]
    public string ProductId { get; init; } = string.Empty;

    /// <summary>
    /// The product price ID to create a checkout link for.
    /// </summary>
    [Required]
    public string ProductPriceId { get; init; } = string.Empty;

    /// <summary>
    /// The label for the checkout link.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// The description for the checkout link.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether the checkout link should be enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// The metadata to associate with the checkout link.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to update an existing checkout link.
/// </summary>
public record CheckoutLinkUpdateRequest
{
    /// <summary>
    /// The label for the checkout link.
    /// </summary>
    public string? Label { get; init; }

    /// <summary>
    /// The description for the checkout link.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether the checkout link should be enabled.
    /// </summary>
    public bool? Enabled { get; init; }

    /// <summary>
    /// The metadata to associate with the checkout link.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}