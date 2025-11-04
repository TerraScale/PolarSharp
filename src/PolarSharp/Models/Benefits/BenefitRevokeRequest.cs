using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Benefits;

/// <summary>
/// Request to revoke a benefit from a customer.
/// </summary>
public record BenefitRevokeRequest
{
    /// <summary>
    /// The reason for revoking the benefit.
    /// </summary>
    [JsonPropertyName("reason")]
    [StringLength(500, ErrorMessage = "Revocation reason cannot exceed 500 characters.")]
    public string? Reason { get; init; }

    /// <summary>
    /// The metadata associated with the benefit revocation.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}