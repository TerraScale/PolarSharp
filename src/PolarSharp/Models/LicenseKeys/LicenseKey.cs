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
    /// The license key string.
    /// </summary>
    [Required]
    [JsonPropertyName("key")]
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// The status of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public LicenseKeyStatus Status { get; init; }

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
    /// The order ID associated with the license key.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }

    /// <summary>
    /// The subscription ID associated with the license key.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// The metadata associated with the license key.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the license key.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The expiration date of the license key.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The activation date of the license key.
    /// </summary>
    [JsonPropertyName("activated_at")]
    public DateTime? ActivatedAt { get; init; }

    /// <summary>
    /// The last used date of the license key.
    /// </summary>
    [JsonPropertyName("last_used_at")]
    public DateTime? LastUsedAt { get; init; }

    /// <summary>
    /// The usage limit of the license key.
    /// </summary>
    [JsonPropertyName("usage_limit")]
    public int? UsageLimit { get; init; }

    /// <summary>
    /// The usage count of the license key.
    /// </summary>
    [JsonPropertyName("usage_count")]
    public int UsageCount { get; init; }

    /// <summary>
    /// The activation information.
    /// </summary>
    [JsonPropertyName("activation")]
    public LicenseKeyActivation? Activation { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }

    /// <summary>
    /// The benefit information.
    /// </summary>
    [JsonPropertyName("benefit")]
    public Benefits.Benefit? Benefit { get; init; }

    /// <summary>
    /// The order information.
    /// </summary>
    [JsonPropertyName("order")]
    public Orders.Order? Order { get; init; }

    /// <summary>
    /// The subscription information.
    /// </summary>
    [JsonPropertyName("subscription")]
    public Subscriptions.Subscription? Subscription { get; init; }
}

/// <summary>
/// Represents the status of a license key.
/// </summary>
public enum LicenseKeyStatus
{
    /// <summary>
    /// The license key is active.
    /// </summary>
    Active,

    /// <summary>
    /// The license key is inactive.
    /// </summary>
    Inactive,

    /// <summary>
    /// The license key is expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The license key is revoked.
    /// </summary>
    Revoked,

    /// <summary>
    /// The license key is used.
    /// </summary>
    Used
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
    /// The activation date.
    /// </summary>
    [Required]
    [JsonPropertyName("activated_at")]
    public DateTime ActivatedAt { get; init; }

    /// <summary>
    /// The activation metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The device information.
    /// </summary>
    [JsonPropertyName("device")]
    public string? Device { get; init; }

    /// <summary>
    /// The IP address of the activation.
    /// </summary>
    [JsonPropertyName("ip_address")]
    public string? IpAddress { get; init; }

    /// <summary>
    /// The user agent of the activation.
    /// </summary>
    [JsonPropertyName("user_agent")]
    public string? UserAgent { get; init; }
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
    /// The validation error message.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The validation error code.
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
    /// Whether the license key was activated successfully.
    /// </summary>
    [Required]
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>
    /// The license key information.
    /// </summary>
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The activation information.
    /// </summary>
    [JsonPropertyName("activation")]
    public LicenseKeyActivation? Activation { get; init; }

    /// <summary>
    /// The activation error message.
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    /// <summary>
    /// The activation error code.
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
    /// Whether the license key was deactivated successfully.
    /// </summary>
    [Required]
    public bool Success { get; init; }

    /// <summary>
    /// The license key information.
    /// </summary>
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The deactivation error message.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// The deactivation error code.
    /// </summary>
    public string? ErrorCode { get; init; }
}