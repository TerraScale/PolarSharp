using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Meters;

/// <summary>
/// Represents a meter for usage-based billing in the Polar system.
/// </summary>
public record Meter
{
    /// <summary>
    /// The unique identifier of the meter.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the meter.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the meter.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The aggregation type of the meter.
    /// </summary>
    [JsonPropertyName("aggregation_type")]
    public MeterAggregationType AggregationType { get; init; }

    /// <summary>
    /// The unit of measurement for the meter.
    /// </summary>
    [JsonPropertyName("unit")]
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// Whether the meter is active.
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    /// <summary>
    /// The organization ID that owns the meter.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The creation date of the meter.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the meter.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The metadata associated with the meter.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents the aggregation type for a meter.
/// </summary>
public enum MeterAggregationType
{
    /// <summary>
    /// Sum of all values.
    /// </summary>
    [JsonPropertyName("sum")]
    Sum,

    /// <summary>
    /// Average of all values.
    /// </summary>
    [JsonPropertyName("average")]
    Average,

    /// <summary>
    /// Maximum value.
    /// </summary>
    [JsonPropertyName("max")]
    Max,

    /// <summary>
    /// Minimum value.
    /// </summary>
    [JsonPropertyName("min")]
    Min,

    /// <summary>
    /// Count of values.
    /// </summary>
    [JsonPropertyName("count")]
    Count,

    /// <summary>
    /// Latest value.
    /// </summary>
    [JsonPropertyName("latest")]
    Latest
}

/// <summary>
/// Represents a meter quantity for a specific time period.
/// </summary>
public record MeterQuantity
{
    /// <summary>
    /// The unique identifier of the meter quantity.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The meter ID.
    /// </summary>
    [JsonPropertyName("meter_id")]
    public string MeterId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The quantity value.
    /// </summary>
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; init; }

    /// <summary>
    /// The period start date.
    /// </summary>
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; init; }

    /// <summary>
    /// The period end date.
    /// </summary>
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; init; }

    /// <summary>
    /// The creation date of the meter quantity.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the meter quantity.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }
}

/// <summary>
/// Request to create a new meter.
/// </summary>
public record MeterCreateRequest
{
    /// <summary>
    /// The name of the meter.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the meter.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The aggregation type of the meter.
    /// </summary>
    [Required]
    public MeterAggregationType AggregationType { get; init; }

    /// <summary>
    /// The unit of measurement for the meter.
    /// </summary>
    [Required]
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// The metadata to associate with the meter.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to update an existing meter.
/// </summary>
public record MeterUpdateRequest
{
    /// <summary>
    /// The name of the meter.
    /// </summary>
    [StringLength(255, ErrorMessage = "Meter name cannot exceed 255 characters.")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the meter.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Meter description cannot exceed 1000 characters.")]
    public string? Description { get; init; }

    /// <summary>
    /// The aggregation type of the meter.
    /// </summary>
    public MeterAggregationType? AggregationType { get; init; }

    /// <summary>
    /// The unit of measurement for the meter.
    /// </summary>
    [StringLength(50, ErrorMessage = "Meter unit cannot exceed 50 characters.")]
    public string? Unit { get; init; }

    /// <summary>
    /// Whether the meter is active.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// The metadata to associate with the meter.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to create a new meter quantity.
/// </summary>
public record MeterQuantityCreateRequest
{
    /// <summary>
    /// The meter ID.
    /// </summary>
    [Required(ErrorMessage = "Meter ID is required.")]
    [JsonPropertyName("meter_id")]
    public string MeterId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID.
    /// </summary>
    [Required(ErrorMessage = "Customer ID is required.")]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The quantity value.
    /// </summary>
    [Required(ErrorMessage = "Quantity is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
    [JsonPropertyName("quantity")]
    public decimal Quantity { get; init; }

    /// <summary>
    /// The period start date.
    /// </summary>
    [Required(ErrorMessage = "Period start date is required.")]
    [JsonPropertyName("period_start")]
    public DateTime PeriodStart { get; init; }

    /// <summary>
    /// The period end date.
    /// </summary>
    [Required(ErrorMessage = "Period end date is required.")]
    [JsonPropertyName("period_end")]
    public DateTime PeriodEnd { get; init; }

    /// <summary>
    /// The metadata to associate with the meter quantity.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to update an existing meter quantity.
/// </summary>
public record MeterQuantityUpdateRequest
{
    /// <summary>
    /// The quantity value.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; init; }

    /// <summary>
    /// The period start date.
    /// </summary>
    [JsonPropertyName("period_start")]
    public DateTime? PeriodStart { get; init; }

    /// <summary>
    /// The period end date.
    /// </summary>
    [JsonPropertyName("period_end")]
    public DateTime? PeriodEnd { get; init; }

    /// <summary>
    /// The metadata to associate with the meter quantity.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}