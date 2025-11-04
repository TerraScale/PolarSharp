using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.LicenseKeys;

/// <summary>
/// Request to deactivate a license key.
/// </summary>
public record LicenseKeyDeactivateRequest
{
    /// <summary>
    /// The user identifier for deactivation.
    /// </summary>
    [StringLength(100, ErrorMessage = "User identifier cannot exceed 100 characters.")]
    public string? UserIdentifier { get; init; }

    /// <summary>
    /// The machine fingerprint for deactivation.
    /// </summary>
    [StringLength(255, ErrorMessage = "Machine fingerprint cannot exceed 255 characters.")]
    public string? MachineFingerprint { get; init; }

    /// <summary>
    /// The metadata for deactivation.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The reason for deactivation.
    /// </summary>
    [StringLength(500, ErrorMessage = "Deactivation reason cannot exceed 500 characters.")]
    public string? Reason { get; init; }
}