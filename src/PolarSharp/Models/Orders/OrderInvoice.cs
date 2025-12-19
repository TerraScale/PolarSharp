using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Orders;

/// <summary>
/// Represents an order's invoice data from the Polar API.
/// Contains the URL to access the invoice.
/// </summary>
public record OrderInvoice
{
    /// <summary>
    /// The URL to the invoice.
    /// </summary>
    [JsonPropertyName("url")]
    [Required]
    public string Url { get; init; } = string.Empty;
}