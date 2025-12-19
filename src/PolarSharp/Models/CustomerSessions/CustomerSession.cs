using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.CustomerSessions;

/// <summary>
/// Represents a customer session in the Polar system.
/// A customer session that can be used to authenticate as a customer.
/// </summary>
public record CustomerSession
{
    /// <summary>
    /// Creation timestamp of the object.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modification timestamp of the object.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// The ID of the object.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The session token.
    /// </summary>
    [Required]
    [JsonPropertyName("token")]
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// The expiration date of the session.
    /// </summary>
    [Required]
    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// The return URL for the customer portal back button.
    /// </summary>
    [JsonPropertyName("return_url")]
    public string? ReturnUrl { get; init; }

    /// <summary>
    /// The URL to the customer portal.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_portal_url")]
    public string CustomerPortalUrl { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the customer.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }
}

/// <summary>
/// Request to create a new customer session.
/// You can use either CustomerId or ExternalCustomerId to identify the customer.
/// </summary>
public record CustomerSessionCreateRequest
{
    /// <summary>
    /// The customer ID to create a session for.
    /// Use this when you have the Polar customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The external customer ID to create a session for.
    /// Use this when you identify customers by your own external ID.
    /// </summary>
    [JsonPropertyName("external_customer_id")]
    public string? ExternalCustomerId { get; init; }

    /// <summary>
    /// When set, a back button will be shown in the customer portal to return to this URL.
    /// Must be a valid URI between 1 and 2083 characters.
    /// </summary>
    [JsonPropertyName("return_url")]
    [StringLength(2083, MinimumLength = 1, ErrorMessage = "Return URL must be between 1 and 2083 characters.")]
    public string? ReturnUrl { get; init; }
}



/// <summary>
/// Response for customer session introspection.
/// </summary>
public record CustomerSessionIntrospectResponse
{
    /// <summary>
    /// Whether the customer access token is valid.
    /// </summary>
    [Required]
    public bool Valid { get; init; }

    /// <summary>
    /// The customer ID associated with the token.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// The expires at date of the customer access token.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    public Customers.Customer? Customer { get; init; }
}