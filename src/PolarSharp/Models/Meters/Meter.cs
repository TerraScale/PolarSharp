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
    /// The filter criteria for the meter.
    /// </summary>
    [JsonPropertyName("filter")]
    public MeterFilter? Filter { get; init; }

    /// <summary>
    /// The aggregation configuration for the meter.
    /// </summary>
    [JsonPropertyName("aggregation")]
    public MeterAggregation? Aggregation { get; init; }

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
    /// Alias for ModifiedAt for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public DateTime UpdatedAt => ModifiedAt;

    /// <summary>
    /// The date when the meter was archived, if applicable.
    /// </summary>
    [JsonPropertyName("archived_at")]
    public DateTime? ArchivedAt { get; init; }

    /// <summary>
    /// The metadata associated with the meter.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The description of the meter (for backward compatibility).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The unit of measurement (for backward compatibility).
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; init; }

    /// <summary>
    /// The aggregation type (for backward compatibility, use Aggregation.Func instead).
    /// </summary>
    [JsonIgnore]
    public MeterAggregationFunc? AggregationType => Aggregation?.Func;

    /// <summary>
    /// Whether the meter is active (for backward compatibility).
    /// </summary>
    [JsonIgnore]
    public bool IsActive => ArchivedAt == null;
}

/// <summary>
/// Represents the filter configuration for a meter.
/// </summary>
public record MeterFilter
{
    /// <summary>
    /// The logical conjunction for combining clauses (and/or).
    /// </summary>
    [JsonPropertyName("conjunction")]
    public string Conjunction { get; init; } = "and";

    /// <summary>
    /// The filter clauses.
    /// </summary>
    [JsonPropertyName("clauses")]
    public List<MeterFilterClause> Clauses { get; init; } = new();
}

/// <summary>
/// Represents a single filter clause for a meter.
/// </summary>
public record MeterFilterClause
{
    /// <summary>
    /// The property to filter on.
    /// </summary>
    [JsonPropertyName("property")]
    public string Property { get; init; } = string.Empty;

    /// <summary>
    /// The operator to use for comparison.
    /// </summary>
    [JsonPropertyName("operator")]
    public string Operator { get; init; } = string.Empty;

    /// <summary>
    /// The value to compare against.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; init; }
}

/// <summary>
/// Represents the aggregation configuration for a meter.
/// </summary>
public record MeterAggregation
{
    /// <summary>
    /// The aggregation function (sum, count, max, min, avg, latest).
    /// </summary>
    [JsonPropertyName("func")]
    public MeterAggregationFunc Func { get; init; }

    /// <summary>
    /// The property to aggregate on (optional, for sum/max/min/avg).
    /// </summary>
    [JsonPropertyName("property")]
    public string? Property { get; init; }
}

/// <summary>
/// Represents the aggregation function for a meter.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MeterAggregationFunc
{
    /// <summary>
    /// Sum of all values.
    /// </summary>
    [JsonPropertyName("sum")]
    Sum,

    /// <summary>
    /// Count of events.
    /// </summary>
    [JsonPropertyName("count")]
    Count,

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
    /// Average of all values.
    /// </summary>
    [JsonPropertyName("avg")]
    Avg,

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
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the meter (for backward compatibility).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The unit of measurement (for backward compatibility).
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; init; }

    /// <summary>
    /// The filter criteria for the meter.
    /// </summary>
    [JsonPropertyName("filter")]
    public MeterFilter? Filter { get; init; }

    /// <summary>
    /// The aggregation configuration for the meter.
    /// </summary>
    [Required]
    [JsonPropertyName("aggregation")]
    public MeterAggregation Aggregation { get; init; } = new();

    /// <summary>
    /// The aggregation type (for backward compatibility, use Aggregation.Func instead).
    /// </summary>
    [JsonIgnore]
    public MeterAggregationFunc? AggregationType
    {
        get => Aggregation?.Func;
        init => Aggregation = new MeterAggregation { Func = value ?? MeterAggregationFunc.Sum };
    }

    /// <summary>
    /// The metadata to associate with the meter.
    /// </summary>
    [JsonPropertyName("metadata")]
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
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The description of the meter (for backward compatibility).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The metadata to associate with the meter.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Alias for MeterAggregationFunc for backward compatibility.
/// </summary>
public static class MeterAggregationType
{
    public const MeterAggregationFunc Sum = MeterAggregationFunc.Sum;
    public const MeterAggregationFunc Count = MeterAggregationFunc.Count;
    public const MeterAggregationFunc Max = MeterAggregationFunc.Max;
    public const MeterAggregationFunc Min = MeterAggregationFunc.Min;
    public const MeterAggregationFunc Avg = MeterAggregationFunc.Avg;
    public const MeterAggregationFunc Latest = MeterAggregationFunc.Latest;
}


