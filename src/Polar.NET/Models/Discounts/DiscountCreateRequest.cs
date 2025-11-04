using System.ComponentModel.DataAnnotations;

namespace Polar.NET.Models.Discounts;

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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the discount.
    /// </summary>
    [StringLength(500, ErrorMessage = "Discount description cannot exceed 500 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// The type of the discount.
    /// </summary>
    [Required(ErrorMessage = "Discount type is required.")]
    public DiscountType Type { get; init; }

    /// <summary>
    /// The amount of the discount (in cents for fixed amount).
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "Discount amount must be greater than 0.")]
    public long? Amount { get; init; }

    /// <summary>
    /// The percentage of the discount.
    /// </summary>
    [Range(0.01, 100, ErrorMessage = "Discount percentage must be between 0.01 and 100.")]
    public decimal? Percentage { get; init; }

    /// <summary>
    /// The currency of the discount (for fixed amount discounts).
    /// </summary>
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a valid 3-letter ISO 4217 code.")]
    public string? Currency { get; init; }

    /// <summary>
    /// Whether the discount should be active.
    /// </summary>
    public bool Active { get; init; } = true;

    /// <summary>
    /// The start date of the discount.
    /// </summary>
    public DateTime? StartsAt { get; init; }

    /// <summary>
    /// The end date of the discount.
    /// </summary>
    public DateTime? EndsAt { get; init; }

    /// <summary>
    /// The maximum number of times the discount can be used.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Maximum redemptions must be greater than 0.")]
    public int? MaxRedemptions { get; init; }

    /// <summary>
    /// The duration of the discount.
    /// </summary>
    public DiscountDuration? Duration { get; init; }

    /// <summary>
    /// The duration in months (for repeating discounts).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Duration in months must be greater than 0.")]
    public int? DurationInMonths { get; init; }

    /// <summary>
    /// The metadata to associate with the discount.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to update an existing discount.
/// </summary>
public record DiscountUpdateRequest
{
    /// <summary>
    /// The name of the discount.
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Discount name must be between 1 and 100 characters.")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the discount.
    /// </summary>
    [StringLength(500, ErrorMessage = "Discount description cannot exceed 500 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the discount should be active.
    /// </summary>
    public bool? Active { get; init; }

    /// <summary>
    /// The start date of the discount.
    /// </summary>
    public DateTime? StartsAt { get; init; }

    /// <summary>
    /// The end date of the discount.
    /// </summary>
    public DateTime? EndsAt { get; init; }

    /// <summary>
    /// The maximum number of times the discount can be used.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Maximum redemptions must be greater than 0.")]
    public int? MaxRedemptions { get; init; }

    /// <summary>
    /// The metadata to update on the discount.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}