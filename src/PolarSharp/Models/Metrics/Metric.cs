using System.Text.Json.Serialization;

namespace PolarSharp.Models.Metrics;

/// <summary>
/// Represents metrics data in Polar system.
/// </summary>
public record Metric
{
    /// <summary>
    /// The metric name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The metric value.
    /// </summary>
    [JsonPropertyName("value")]
    public decimal Value { get; init; }

    /// <summary>
    /// The metric period.
    /// </summary>
    [JsonPropertyName("period")]
    public string Period { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp of the metric.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Represents metric limits in Polar system.
/// </summary>
public record MetricLimit
{
    /// <summary>
    /// The metric name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The maximum allowed value.
    /// </summary>
    [JsonPropertyName("max_value")]
    public decimal MaxValue { get; init; }

    /// <summary>
    /// The current value.
    /// </summary>
    [JsonPropertyName("current_value")]
    public decimal CurrentValue { get; init; }

    /// <summary>
    /// The percentage of limit used.
    /// </summary>
    [JsonPropertyName("percentage_used")]
    public decimal PercentageUsed { get; init; }

    /// <summary>
    /// The reset date for the limit.
    /// </summary>
    [JsonPropertyName("resets_at")]
    public DateTime? ResetsAt { get; init; }
}