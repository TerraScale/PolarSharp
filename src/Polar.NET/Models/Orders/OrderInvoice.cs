using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.Orders;

/// <summary>
/// Represents an invoice for an order in the Polar system.
/// </summary>
public record OrderInvoice
{
    /// <summary>
    /// The unique identifier of the invoice.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The order ID associated with the invoice.
    /// </summary>
    [Required]
    public string OrderId { get; init; } = string.Empty;

    /// <summary>
    /// The invoice number.
    /// </summary>
    [Required]
    public string InvoiceNumber { get; init; } = string.Empty;

    /// <summary>
    /// The URL to download the invoice PDF.
    /// </summary>
    [Required]
    public string InvoiceUrl { get; init; } = string.Empty;

    /// <summary>
    /// The status of the invoice.
    /// </summary>
    [Required]
    public InvoiceStatus Status { get; init; }

    /// <summary>
    /// The amount of the invoice in the smallest currency unit (e.g., cents).
    /// </summary>
    [Required]
    public long Amount { get; init; }

    /// <summary>
    /// The currency of the invoice (ISO 4217 format).
    /// </summary>
    [Required]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The tax amount in the smallest currency unit.
    /// </summary>
    public long? TaxAmount { get; init; }

    /// <summary>
    /// The creation date of the invoice.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the invoice.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The due date of the invoice.
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>
    /// The customer information for the invoice.
    /// </summary>
    public InvoiceCustomer? Customer { get; init; }

    /// <summary>
    /// The line items of the invoice.
    /// </summary>
    public List<InvoiceLineItem>? LineItems { get; init; }
}

/// <summary>
/// Represents the status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// The invoice is a draft.
    /// </summary>
    Draft,

    /// <summary>
    /// The invoice is open and awaiting payment.
    /// </summary>
    Open,

    /// <summary>
    /// The invoice has been paid.
    /// </summary>
    Paid,

    /// <summary>
    /// The invoice is void.
    /// </summary>
    Void,

    /// <summary>
    /// The invoice is uncollectible.
    /// </summary>
    Uncollectible
}

/// <summary>
/// Represents customer information on an invoice.
/// </summary>
public record InvoiceCustomer
{
    /// <summary>
    /// The customer ID.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The customer's email address.
    /// </summary>
    [Required]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The customer's name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The customer's billing address.
    /// </summary>
    public InvoiceAddress? Address { get; init; }
}

/// <summary>
/// Represents an address on an invoice.
/// </summary>
public record InvoiceAddress
{
    /// <summary>
    /// The address line 1.
    /// </summary>
    public string? Line1 { get; init; }

    /// <summary>
    /// The address line 2.
    /// </summary>
    public string? Line2 { get; init; }

    /// <summary>
    /// The city.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// The state or province.
    /// </summary>
    public string? State { get; init; }

    /// <summary>
    /// The postal code.
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    /// The country.
    /// </summary>
    public string? Country { get; init; }
}

/// <summary>
/// Represents a line item on an invoice.
/// </summary>
public record InvoiceLineItem
{
    /// <summary>
    /// The description of the line item.
    /// </summary>
    [Required]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// The quantity of the line item.
    /// </summary>
    [Required]
    public int Quantity { get; init; }

    /// <summary>
    /// The unit price in the smallest currency unit.
    /// </summary>
    [Required]
    public long UnitAmount { get; init; }

    /// <summary>
    /// The total amount in the smallest currency unit.
    /// </summary>
    [Required]
    public long Amount { get; init; }

    /// <summary>
    /// The tax amount in the smallest currency unit.
    /// </summary>
    public long? TaxAmount { get; init; }

    /// <summary>
    /// The product ID associated with the line item.
    /// </summary>
    public string? ProductId { get; init; }

    /// <summary>
    /// The product name.
    /// </summary>
    public string? ProductName { get; init; }
}