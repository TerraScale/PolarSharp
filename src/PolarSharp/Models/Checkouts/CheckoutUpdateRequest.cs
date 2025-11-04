using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.Checkouts;

/// <summary>
/// Request to update an existing checkout session.
/// </summary>
public record CheckoutUpdateRequest
{
    /// <summary>
    /// The metadata of the checkout.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID of the checkout.
    /// </summary>
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The success URL of the checkout.
    /// </summary>
    [Url(ErrorMessage = "Success URL must be a valid URL.")]
    public string? SuccessUrl { get; init; }

    /// <summary>
    /// The cancel URL of the checkout.
    /// </summary>
    [Url(ErrorMessage = "Cancel URL must be a valid URL.")]
    public string? CancelUrl { get; init; }
}