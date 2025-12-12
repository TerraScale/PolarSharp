using System.Text.Json.Serialization;

namespace PolarSharp.Models.Products;

/// <summary>
/// The recurring interval for subscription prices.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RecurringInterval
{
    /// <summary>
    /// Daily recurring interval.
    /// </summary>
    [JsonPropertyName("day")]
    Day,

    /// <summary>
    /// Weekly recurring interval.
    /// </summary>
    [JsonPropertyName("week")]
    Week,

    /// <summary>
    /// Monthly recurring interval.
    /// </summary>
    [JsonPropertyName("month")]
    Month,
    
    /// <summary>
    /// Yearly recurring interval.
    /// </summary>
    [JsonPropertyName("year")]
    Year
}