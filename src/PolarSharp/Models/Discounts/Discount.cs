using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Discounts;

/// <summary>
/// Represents a discount in Polar system.
/// </summary>
public record Discount
{
    /// <summary>
    /// The unique identifier of discount.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of discount.
    /// </summary>
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of discount.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The type of discount.
    /// </summary>
    [Required]
    [JsonPropertyName("type")]
    public DiscountType Type { get; init; }

    /// <summary>
    /// The amount of discount (in cents for fixed amount).
    /// </summary>
    [JsonPropertyName("amount")]
    public int? Amount { get; init; }

    /// <summary>
    /// The percentage of discount.
    /// </summary>
    [JsonPropertyName("percentage")]
    public decimal? Percentage { get; init; }

    /// <summary>
    /// The currency of discount (for fixed amount discounts).
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>
    /// Whether discount is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool Active { get; init; }

    /// <summary>
    /// The start date of discount.
    /// </summary>
    [JsonPropertyName("starts_at")]
    public DateTime? StartsAt { get; init; }

    /// <summary>
    /// The end date of discount.
    /// </summary>
    [JsonPropertyName("ends_at")]
    public DateTime? EndsAt { get; init; }

    /// <summary>
    /// The maximum number of times discount can be used.
    /// </summary>
    [JsonPropertyName("max_redemptions")]
    public int? MaxRedemptions { get; init; }

    /// <summary>
    /// The number of times discount has been used.
    /// </summary>
    [JsonPropertyName("times_redeemed")]
    public int TimesRedeemed { get; init; }

    /// <summary>
    /// The duration of discount.
    /// </summary>
    [JsonPropertyName("duration")]
    public DiscountDuration? Duration { get; init; }

    /// <summary>
    /// The duration in months (for repeating discounts).
    /// </summary>
    [JsonPropertyName("duration_in_months")]
    public int? DurationInMonths { get; init; }

    /// <summary>
    /// The metadata associated with discount.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of discount.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of discount.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
    
    /// <summary>
    /// The organization ID associated with the discount.
    /// </summary>
    [Required]
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;
    
    /// <summary>
    /// The code used to redeem the discount.
    /// </summary>
    [Required]
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Represents the type of a discount.
/// </summary>
public enum DiscountType
{
    /// <summary>
    /// Fixed amount discount.
    /// </summary>
    FixedAmount,

    /// <summary>
    /// Percentage discount.
    /// </summary>
    Percentage
}

/// <summary>
/// Represents the duration of a discount.
/// </summary>
public enum DiscountDuration
{
    /// <summary>
    /// One-time discount.
    /// </summary>
    Once,

    /// <summary>
    /// Forever discount.
    /// </summary>
    Forever,

    /// <summary>
    /// Repeating discount.
    /// </summary>
    Repeating
}

/// <summary>
/// Response for discount export.
/// </summary>
public record DiscountExportResponse
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

