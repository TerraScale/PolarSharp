using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.LicenseKeys;

/// <summary>
/// Request to activate a license key.
/// </summary>
public record LicenseKeyActivateRequest
{
    /// <summary>
    /// The user identifier for activation.
    /// </summary>
    [Required(ErrorMessage = "User identifier is required for activation.")]
    [StringLength(100, ErrorMessage = "User identifier cannot exceed 100 characters.")]
    public string? UserIdentifier { get; init; }

    /// <summary>
    /// The machine fingerprint for activation.
    /// </summary>
    [StringLength(255, ErrorMessage = "Machine fingerprint cannot exceed 255 characters.")]
    public string? MachineFingerprint { get; init; }

    /// <summary>
    /// The metadata for activation.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The display name for the activation.
    /// </summary>
    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
    public string? DisplayName { get; init; }
}