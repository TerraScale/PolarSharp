using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Polar.NET.Models.Common;

namespace Polar.NET.Models.LicenseKeys;

/// <summary>
/// Request to update an existing license key.
/// </summary>
public record LicenseKeyUpdateRequest
{
    /// <summary>
    /// The display name of the license key.
    /// </summary>
    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
    public string? DisplayName { get; init; }

    /// <summary>
    /// The metadata of the license key.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the license key is enabled.
    /// </summary>
    public bool? Enabled { get; init; }

    /// <summary>
    /// The expiration date of the license key.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The maximum number of activations.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Maximum activations must be a positive number.")]
    public int? MaxActivations { get; init; }
}