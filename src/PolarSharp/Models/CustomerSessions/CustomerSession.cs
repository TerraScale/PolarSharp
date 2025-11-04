using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.CustomerSessions;

/// <summary>
/// Represents a customer session in the Polar system.
/// </summary>
public record CustomerSession
{
    /// <summary>
    /// The unique identifier of the customer session.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID associated with the session.
    /// </summary>
    [Required]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The customer access token.
    /// </summary>
    [Required]
    public string CustomerAccessToken { get; init; } = string.Empty;

    /// <summary>
    /// The expires at date of the customer access token.
    /// </summary>
    [Required]
    public DateTime CustomerAccessTokenExpiresAt { get; init; }

    /// <summary>
    /// The metadata associated with the customer session.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the customer session.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the customer session.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    public Customers.Customer? Customer { get; init; }
}

/// <summary>
/// Request to create a new customer session.
/// </summary>
public record CustomerSessionCreateRequest
{
    /// <summary>
    /// The customer ID to create a session for.
    /// </summary>
    [Required]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The metadata to associate with the customer session.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The expires at date for the customer access token.
    /// </summary>
    public DateTime? CustomerAccessTokenExpiresAt { get; init; }
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