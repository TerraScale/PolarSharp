using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Common;

namespace PolarSharp.Models.Customers;

/// <summary>
/// Request to update an existing customer.
/// </summary>
public record CustomerUpdateRequest
{
    /// <summary>
    /// The email address of customer.
    /// </summary>
    [EmailAddress(ErrorMessage = "Email address must be a valid email.")]
    [JsonPropertyName("email")]
    public string? Email { get; init; }
 
    /// <summary>
    /// The name of customer.
    /// </summary>
    [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
    [JsonPropertyName("name")]
    public string? Name { get; init; }
 
    /// <summary>
    /// The external ID of customer.
    /// </summary>
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }
 
    /// <summary>
    /// The metadata of customer.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
 
    /// <summary>
    /// The avatar URL of customer.
    /// </summary>
    [Url(ErrorMessage = "Avatar URL must be a valid URL.")]
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }
 
    /// <summary>
    /// The billing address of customer.
    /// </summary>
    [JsonPropertyName("billing_address")]
    public Address? BillingAddress { get; init; }
 
    /// <summary>
    /// The shipping address of customer.
    /// </summary>
    [JsonPropertyName("shipping_address")]
    public Address? ShippingAddress { get; init; }
}