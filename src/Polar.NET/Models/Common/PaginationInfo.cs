using System.Text.Json.Serialization;

namespace Polar.NET.Models.Common;

/// <summary>
/// Represents pagination information returned by list endpoints.
/// </summary>
public record PaginationInfo
{
    /// <summary>
    /// Current page number.
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; init; }

    /// <summary>
    /// Total number of items matching the query across all pages.
    /// </summary>
    [JsonPropertyName("total_count")]
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages available, given the current limit value.
    /// </summary>
    [JsonPropertyName("max_page")]
    public int MaxPage { get; init; }
}