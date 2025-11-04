using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Polar.NET.Models.Common;
using Polar.NET.Models.Customers;

namespace Polar.NET.Models.Orders;

/// <summary>
/// Request to update an existing order.
/// </summary>
public record OrderUpdateRequest
{
    /// <summary>
    /// The metadata of the order.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The external ID of the order.
    /// </summary>
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The customer address for the order.
    /// </summary>
    public Address? CustomerAddress { get; init; }

    /// <summary>
    /// The customer name for the order.
    /// </summary>
    [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
    public string? CustomerName { get; init; }

    /// <summary>
    /// The customer email for the order.
    /// </summary>
    [EmailAddress(ErrorMessage = "Customer email must be a valid email address.")]
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// The customer IP address for the order.
    /// </summary>
    [RegularExpression(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", ErrorMessage = "Customer IP address must be a valid IPv4 address.")]
    public string? CustomerIpAddress { get; init; }

    /// <summary>
    /// Whether to refund the order.
    /// </summary>
    public bool? Refund { get; init; }

    /// <summary>
    /// The reason for the refund.
    /// </summary>
    [StringLength(500, ErrorMessage = "Refund reason cannot exceed 500 characters.")]
    public string? RefundReason { get; init; }
}