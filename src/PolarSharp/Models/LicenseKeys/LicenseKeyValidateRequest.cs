using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.LicenseKeys;

/// <summary>
/// Request to validate a license key.
/// </summary>
public record LicenseKeyValidateRequest
{
    /// <summary>
    /// The license key to validate.
    /// </summary>
    [Required(ErrorMessage = "License key is required for validation.")]
    [StringLength(255, ErrorMessage = "License key cannot exceed 255 characters.")]
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// The user identifier for validation.
    /// </summary>
    [StringLength(100, ErrorMessage = "User identifier cannot exceed 100 characters.")]
    public string? UserIdentifier { get; init; }

    /// <summary>
    /// The machine fingerprint for validation.
    /// </summary>
    [StringLength(255, ErrorMessage = "Machine fingerprint cannot exceed 255 characters.")]
    public string? MachineFingerprint { get; init; }

    /// <summary>
    /// The metadata for validation.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}