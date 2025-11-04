using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.LicenseKeys;

/// <summary>
/// Represents a license key in the Polar system.
/// </summary>
public record LicenseKey
{
    /// <summary>
    /// The unique identifier of the license key.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The license key string.
    /// </summary>
    [Required]
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// The status of the license key.
    /// </summary>
    [Required]
    public LicenseKeyStatus Status { get; init; }

    /// <summary>
    /// The customer ID associated with the license key.
    /// </summary>
    [Required]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The benefit ID associated with the license key.
    /// </summary>
    [Required]
    public string BenefitId { get; init; } = string.Empty;

    /// <summary>
    /// The order ID associated with the license key.
    /// </summary>
    public string? OrderId { get; init; }

    /// <summary>
    /// The subscription ID associated with the license key.
    /// </summary>
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// The metadata associated with the license key.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the license key.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the license key.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The expiration date of the license key.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The activation date of the license key.
    /// </summary>
    public DateTime? ActivatedAt { get; init; }

    /// <summary>
    /// The last used date of the license key.
    /// </summary>
    public DateTime? LastUsedAt { get; init; }

    /// <summary>
    /// The usage limit of the license key.
    /// </summary>
    public int? UsageLimit { get; init; }

    /// <summary>
    /// The usage count of the license key.
    /// </summary>
    public int UsageCount { get; init; }

    /// <summary>
    /// The activation information.
    /// </summary>
    public LicenseKeyActivation? Activation { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    public Models.Customers.Customer? Customer { get; init; }

    /// <summary>
    /// The benefit information.
    /// </summary>
    public Models.Benefits.Benefit? Benefit { get; init; }

    /// <summary>
    /// The order information.
    /// </summary>
    public Models.Orders.Order? Order { get; init; }

    /// <summary>
    /// The subscription information.
    /// </summary>
    public Models.Subscriptions.Subscription? Subscription { get; init; }
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
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The activation date.
    /// </summary>
    [Required]
    public DateTime ActivatedAt { get; init; }

    /// <summary>
    /// The activation metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The device information.
    /// </summary>
    public string? Device { get; init; }

    /// <summary>
    /// The IP address of the activation.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The user agent of the activation.
    /// </summary>
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
    public bool Valid { get; init; }

    /// <summary>
    /// The license key information.
    /// </summary>
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The validation error message.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// The validation error code.
    /// </summary>
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
    public bool Success { get; init; }

    /// <summary>
    /// The license key information.
    /// </summary>
    public LicenseKey? LicenseKey { get; init; }

    /// <summary>
    /// The activation information.
    /// </summary>
    public LicenseKeyActivation? Activation { get; init; }

    /// <summary>
    /// The activation error message.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// The activation error code.
    /// </summary>
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