using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Benefits;

/// <summary>
/// Represents a benefit in the Polar system.
/// </summary>
public record Benefit
{
    /// <summary>
    /// The unique identifier of the benefit.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the benefit.
    /// </summary>
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the benefit.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The type of the benefit.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public BenefitType Type { get; init; }

    /// <summary>
    /// The metadata associated with the benefit.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the benefit is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; init; }

    /// <summary>
    /// The creation date of the benefit.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the benefit.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The selectable property for the benefit.
    /// </summary>
    [JsonPropertyName("selectable")]
    public bool Selectable { get; init; }

    /// <summary>
    /// The time period for time-based benefits.
    /// </summary>
    [JsonPropertyName("time_period")]
    public BenefitTimePeriod? TimePeriod { get; init; }

    /// <summary>
    /// The usage limit for usage-based benefits.
    /// </summary>
    [JsonPropertyName("usage_limit")]
    public int? UsageLimit { get; init; }

    /// <summary>
    /// The properties for the benefit.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; init; }
}

/// <summary>
/// Represents the type of a benefit.
/// </summary>
public enum BenefitType
{
    /// <summary>
    /// Downloadable file benefit.
    /// </summary>
    [JsonPropertyName("downloadables")]
    Downloadable,

    /// <summary>
    /// License key benefit.
    /// </summary>
    [JsonPropertyName("license_keys")]
    LicenseKeys,

    /// <summary>
    /// Custom benefit.
    /// </summary>
    [JsonPropertyName("custom")]
    Custom,

    /// <summary>
    /// GitHub repository benefit.
    /// </summary>
    [JsonPropertyName("github_repository")]
    GithubRepository,

    /// <summary>
    /// Discord role benefit.
    /// </summary>
    [JsonPropertyName("discord")]
    DiscordRole,

    /// <summary>
    /// Advertisement benefit.
    /// </summary>
    [JsonPropertyName("advertisement")]
    Advertisement,

    /// <summary>
    /// Time-based benefit.
    /// </summary>
    [JsonPropertyName("time")]
    Time,

    /// <summary>
    /// Usage-based benefit.
    /// </summary>
    [JsonPropertyName("usage")]
    Usage,

    /// <summary>
    /// Meter credit benefit.
    /// </summary>
    [JsonPropertyName("meter_credit")]
    MeterCredit
}

/// <summary>
/// Represents the time period for time-based benefits.
/// </summary>
public record BenefitTimePeriod
{
    /// <summary>
    /// The start date of the time period.
    /// </summary>
    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// The end date of the time period.
    /// </summary>
    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// The duration in days.
    /// </summary>
    [JsonPropertyName("duration_days")]
    public int? DurationDays { get; init; }
}

/// <summary>
/// Represents a benefit grant.
/// </summary>
public record BenefitGrant
{
    /// <summary>
    /// The unique identifier of the benefit grant.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The benefit ID associated with the grant.
    /// </summary>
    [Required]
    [JsonPropertyName("benefit_id")]
    public string BenefitId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID associated with the grant.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The order ID associated with the grant.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }

    /// <summary>
    /// The subscription ID associated with the grant.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// The status of the benefit grant.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public BenefitGrantStatus Status { get; init; }

    /// <summary>
    /// The metadata associated with the benefit grant.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the benefit grant.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the benefit grant.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The expiration date of the benefit grant.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The last used date of the benefit grant.
    /// </summary>
    [JsonPropertyName("last_used_at")]
    public DateTime? LastUsedAt { get; init; }

    /// <summary>
    /// The usage count of the benefit grant.
    /// </summary>
    [JsonPropertyName("usage_count")]
    public int UsageCount { get; init; }

    /// <summary>
    /// The benefit information.
    /// </summary>
    [JsonPropertyName("benefit")]
    public Benefit? Benefit { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Models.Customers.Customer? Customer { get; init; }

    /// <summary>
    /// The order information.
    /// </summary>
    [JsonPropertyName("order")]
    public Models.Orders.Order? Order { get; init; }

    /// <summary>
    /// The subscription information.
    /// </summary>
    [JsonPropertyName("subscription")]
    public Models.Subscriptions.Subscription? Subscription { get; init; }
}

/// <summary>
/// Represents the status of a benefit grant.
/// </summary>
public enum BenefitGrantStatus
{
    /// <summary>
    /// The benefit grant is active.
    /// </summary>
    Active,

    /// <summary>
    /// The benefit grant is inactive.
    /// </summary>
    Inactive,

    /// <summary>
    /// The benefit grant is expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The benefit grant is revoked.
    /// </summary>
    Revoked
}

/// <summary>
/// Request to create a new benefit.
/// </summary>
public record BenefitCreateRequest
{
    /// <summary>
    /// The name of the benefit.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Benefit name must be between 1 and 100 characters.")]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the benefit.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Benefit description cannot exceed 1000 characters.")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The type of the benefit.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public BenefitType Type { get; init; }

    /// <summary>
    /// The metadata to associate with the benefit.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the benefit is selectable.
    /// </summary>
    [JsonPropertyName("selectable")]
    public bool Selectable { get; init; }

    /// <summary>
    /// The time period for time-based benefits.
    /// </summary>
    [JsonPropertyName("time_period")]
    public BenefitTimePeriod? TimePeriod { get; init; }

    /// <summary>
    /// The usage limit for usage-based benefits.
    /// </summary>
    [JsonPropertyName("usage_limit")]
    public int? UsageLimit { get; init; }

    /// <summary>
    /// The properties for the benefit.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; init; }
}


/// <summary>
/// Request to grant a benefit to a customer.
/// </summary>
public record BenefitGrantRequest
{
    /// <summary>
    /// The customer ID to grant benefit to.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The order ID associated with the grant.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }

    /// <summary>
    /// The subscription ID associated with the grant.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string? SubscriptionId { get; init; }

    /// <summary>
    /// The expiration date of the grant.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The metadata to associate with the grant.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Response for benefit export.
/// </summary>
public record BenefitExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    [JsonPropertyName("export_url")]
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export file size in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The number of records in export.
    /// </summary>
    [JsonPropertyName("record_count")]
    public int RecordCount { get; init; }
}

/// <summary>
/// Response for benefit grant export.
/// </summary>
public record BenefitGrantExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    [JsonPropertyName("export_url")]
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export file size in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The number of records in export.
    /// </summary>
    [JsonPropertyName("record_count")]
    public int RecordCount { get; init; }
}
