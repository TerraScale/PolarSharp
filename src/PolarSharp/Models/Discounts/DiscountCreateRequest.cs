using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Discounts;

/// <summary>
/// Request to create a new discount.
/// </summary>
public record DiscountCreateRequest
{
    /// <summary>
    /// The name of the discount.
    /// </summary>
    [Required(ErrorMessage = "Discount name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Discount name must be between 1 and 100 characters.")]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the discount (not supported by API, kept for backwards compatibility).
    /// </summary>
    [StringLength(500, ErrorMessage = "Discount description cannot exceed 500 characters.")]
    [JsonIgnore]
    public string? Description { get; init; }

    /// <summary>
    /// The type of the discount.
    /// </summary>
    [Required(ErrorMessage = "Discount type is required.")]
    [JsonPropertyName("type")]
    public DiscountType Type { get; init; }

    /// <summary>
    /// The amount of the discount (in cents for fixed amount).
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "Discount amount must be greater than 0.")]
    [JsonPropertyName("amount")]
    public long? Amount { get; init; }

    /// <summary>
    /// The percentage of the discount (for display purposes, converted to basis_points).
    /// </summary>
    [Range(0.01, 100, ErrorMessage = "Discount percentage must be between 0.01 and 100.")]
    [JsonIgnore]
    public decimal? Percentage { get; init; }

    /// <summary>
    /// The basis points for percentage discounts (100 = 1%).
    /// </summary>
    [JsonPropertyName("basis_points")]
    public int? BasisPoints
    {
        get => Percentage.HasValue ? (int)(Percentage.Value * 100) : null;
        init => Percentage = value.HasValue ? value.Value / 100m : null;
    }

    /// <summary>
    /// The currency of the discount (for fixed amount discounts).
    /// </summary>
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a valid 3-letter ISO 4217 code.")]
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>
    /// Whether the discount should be active (not supported by API, kept for backwards compatibility).
    /// </summary>
    [JsonIgnore]
    public bool Active { get; init; } = true;

    /// <summary>
    /// The start date of the discount.
    /// </summary>
    [JsonPropertyName("starts_at")]
    public DateTime? StartsAt { get; init; }

    /// <summary>
    /// The end date of the discount.
    /// </summary>
    [JsonPropertyName("ends_at")]
    public DateTime? EndsAt { get; init; }

    /// <summary>
    /// The maximum number of times the discount can be used.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Maximum redemptions must be greater than 0.")]
    [JsonPropertyName("max_redemptions")]
    public int? MaxRedemptions { get; init; }

    /// <summary>
    /// The duration of the discount.
    /// </summary>
    [JsonPropertyName("duration")]
    public DiscountDuration? Duration { get; init; }

    /// <summary>
    /// The duration in months (for repeating discounts).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Duration in months must be greater than 0.")]
    [JsonPropertyName("duration_in_months")]
    public int? DurationInMonths { get; init; }

    /// <summary>
    /// The metadata to associate with the discount.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

