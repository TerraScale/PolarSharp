using System.Text.Json.Serialization;

namespace Polar.NET.Models.Common;

/// <summary>
/// Represents a paginated response from the Polar API.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public record PaginatedResponse<T>
{
    /// <summary>
    /// The list of items for the current page.
    /// </summary>
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; init; } = new List<T>();

    /// <summary>
    /// Pagination information.
    /// </summary>
    [JsonPropertyName("pagination")]
    public PaginationInfo Pagination { get; init; } = new PaginationInfo();
}