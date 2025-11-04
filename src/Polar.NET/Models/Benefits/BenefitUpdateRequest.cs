using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Polar.NET.Models.Common;

namespace Polar.NET.Models.Benefits;

/// <summary>
/// Request to update an existing benefit.
/// </summary>
public record BenefitUpdateRequest
{
    /// <summary>
    /// The name of the benefit.
    /// </summary>
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Benefit name must be between 1 and 100 characters.")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the benefit.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Benefit description cannot exceed 1000 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the benefit is active.
    /// </summary>
    public bool? Active { get; init; }

    /// <summary>
    /// Whether the benefit is selectable.
    /// </summary>
    public bool? Selectable { get; init; }

    /// <summary>
    /// The metadata of the benefit.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The time period for the benefit.
    /// </summary>
    public BenefitTimePeriod? TimePeriod { get; init; }

    /// <summary>
    /// The usage limit for the benefit.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be a positive number.")]
    public int? UsageLimit { get; init; }

    /// <summary>
    /// The properties of the benefit.
    /// </summary>
    public Dictionary<string, object>? Properties { get; init; }
}