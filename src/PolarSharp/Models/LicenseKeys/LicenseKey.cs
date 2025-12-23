using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.LicenseKeys;

/// <summary>
/// Represents a license key in the Polar system.
/// </summary>
public record LicenseKey
{
    /// <summary>
    /// The unique identifier of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The organization ID.
    /// </summary>
    [Required]
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID associated with the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The benefit ID associated with the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("benefit_id")]
    public string BenefitId { get; init; } = string.Empty;

    /// <summary>
    /// The full license key string.
    /// </summary>
    [Required]
    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// The display version of the license key (partially masked).
    /// </summary>
    [JsonPropertyName("display_key")]
    public string? DisplayKey { get; init; }

    /// <summary>
    /// The status of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public LicenseKeyStatus Status { get; init; }

    /// <summary>
    /// The maximum number of activations allowed.
    /// </summary>
    [JsonPropertyName("limit_activations")]
    public int? LimitActivations { get; init; }

    /// <summary>
    /// The current number of activations (usage).
    /// </summary>
    [JsonPropertyName("usage")]
    public int Usage { get; init; }

    /// <summary>
    /// The maximum usage limit.
    /// </summary>
    [JsonPropertyName("limit_usage")]
    public int? LimitUsage { get; init; }

    /// <summary>
    /// The number of times this license key has been validated.
    /// </summary>
    [JsonPropertyName("validations")]
    public int Validations { get; init; }

    /// <summary>
    /// The last validation timestamp.
    /// </summary>
    [JsonPropertyName("last_validated_at")]
    public DateTime? LastValidatedAt { get; init; }

    /// <summary>
    /// The expiration date of the license key.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The creation date of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// Alias for ModifiedAt for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt => ModifiedAt;

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }

    /// <summary>
    /// List of activations for this license key.
    /// </summary>
    [JsonPropertyName("activations")]
    public List<LicenseKeyActivation>? Activations { get; init; }
}

/// <summary>
/// Represents the status of a license key.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LicenseKeyStatus
{
    /// <summary>
    /// The license key has been granted (active).
    /// </summary>
    [JsonPropertyName("granted")]
    Granted,

    /// <summary>
    /// Alias for Granted - the license key is active.
    /// </summary>
    [JsonPropertyName("active")]
    Active = Granted,

    /// <summary>
    /// The license key is revoked.
    /// </summary>
    [JsonPropertyName("revoked")]
    Revoked,

    /// <summary>
    /// Alias for Revoked - the license key is inactive.
    /// </summary>
    [JsonPropertyName("inactive")]
    Inactive = Revoked,

    /// <summary>
    /// The license key is disabled.
    /// </summary>
    [JsonPropertyName("disabled")]
    Disabled
}

/// <summary>
/// Represents the activation information of a license key.
/// </summary>
public record LicenseKeyActivation
{
    /// <summary>
    /// The activation ID.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The license key ID this activation belongs to.
    /// </summary>
    [Required]
    [JsonPropertyName("license_key_id")]
    public string LicenseKeyId { get; init; } = string.Empty;

    /// <summary>
    /// The label for this activation (e.g., device name).
    /// </summary>
    [Required]
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// The metadata associated with this activation.
    /// </summary>
    [JsonPropertyName("meta")]
    public Dictionary<string, object>? Meta { get; init; }

    /// <summary>
    /// The creation date of the activation.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the activation.
    /// </summary>
    [Required]
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }
}

/// <summary>
/// Response for license key validation.
/// </summary>
public record LicenseKeyValidateResponse
{
    /// <summary>
    /// Whether the license key is valid.
    /// </summary>
    [Required]
    [JsonPropertyName("valid")]
    public bool Valid { get; init; }

    /// <summary>
    /// The license key information.
    /// </summary>
    [JsonPropertyName("license_key")]
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The activation information if provided.
    /// </summary>
    [JsonPropertyName("activation")]
    public LicenseKeyActivation? Activation { get; init; }

    /// <summary>
    /// Error message if validation failed (for backward compatibility).
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// Error code if validation failed (for backward compatibility).
    /// </summary>
    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }
}

/// <summary>
/// Response for license key activation.
/// </summary>
public record LicenseKeyActivateResponse
{
    /// <summary>
    /// The license key information.
    /// </summary>
    [JsonPropertyName("license_key")]
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The activation information.
    /// </summary>
    [JsonPropertyName("activation")]
    public LicenseKeyActivation? Activation { get; init; }

    /// <summary>
    /// Whether the activation was successful (for backward compatibility).
    /// </summary>
    [JsonIgnore]
    public bool Success => LicenseKey != null && Activation != null;

    /// <summary>
    /// Error message if activation failed (for backward compatibility).
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// Error code if activation failed (for backward compatibility).
    /// </summary>
    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }
}

/// <summary>
/// Response for license key deactivation.
/// </summary>
public record LicenseKeyDeactivateResponse
{
    /// <summary>
    /// The license key information.
    /// </summary>
    [JsonPropertyName("license_key")]
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The activation that was deactivated.
    /// </summary>
    [JsonPropertyName("activation")]
    public LicenseKeyActivation? Activation { get; init; }
}
