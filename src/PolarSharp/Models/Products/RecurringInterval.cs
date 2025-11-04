using System.Text.Json.Serialization;

namespace PolarSharp.Models.Products;

/// <summary>
/// The recurring interval for subscription prices.
/// </summary>

public enum RecurringInterval
{
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