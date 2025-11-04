using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Polar.NET.Models.Common;

namespace Polar.NET.Models.Benefits;

/// <summary>
/// Request to update an existing benefit.
/// </summary>
public record BenefitUpdateRequest
{
    /// <summary>
    /// The name of benefit.
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Benefit name must be between 1 and 100 characters.")]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of benefit.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Benefit description cannot exceed 1000 characters.")]
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether benefit is active.
    /// </summary>
    [JsonPropertyName("active")]
    public bool? Active { get; init; }

    /// <summary>
    /// Whether benefit is selectable.
    /// </summary>
    [JsonPropertyName("selectable")]
    public bool? Selectable { get; init; }

    /// <summary>
    /// The metadata of benefit.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The time period for benefit.
    /// </summary>
    [JsonPropertyName("time_period")]
    public BenefitTimePeriod? TimePeriod { get; init; }

    /// <summary>
    /// The usage limit for benefit.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be a positive number.")]
    [JsonPropertyName("usage_limit")]
    public int? UsageLimit { get; init; }

    /// <summary>
    /// The properties of benefit.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; init; }
}