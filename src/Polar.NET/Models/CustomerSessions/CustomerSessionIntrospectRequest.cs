using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.CustomerSessions;

/// <summary>
/// Request to introspect a customer session.
/// </summary>
public record CustomerSessionIntrospectRequest
{
    /// <summary>
    /// The customer access token to introspect.
    /// </summary>
    [Required(ErrorMessage = "Customer access token is required for introspection.")]
    [StringLength(1000, ErrorMessage = "Customer access token cannot exceed 1000 characters.")]
    public string CustomerAccessToken { get; init; } = string.Empty;
}