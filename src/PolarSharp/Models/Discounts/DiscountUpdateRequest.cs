using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Discounts;

/// <summary>
/// Request to update an existing discount.
/// </summary>
public record DiscountUpdateRequest
{
    /// <summary>
    /// The name of the discount.
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Discount name must be between 1 and 100 characters.")]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the discount.
    /// </summary>
    [StringLength(500, ErrorMessage = "Discount description cannot exceed 500 characters.")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the discount should be active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

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
    /// The metadata to update on the discount.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}