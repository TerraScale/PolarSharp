using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Events;

/// <summary>
/// Represents an event in the Polar system.
/// </summary>
public record Event
{
    /// <summary>
    /// The unique identifier of the event.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the event.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID associated with the event.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The organization ID associated with the event.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The event data.
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// The metadata associated with the event.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the event.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the event.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }
}

/// <summary>
/// Represents an event name in the Polar system.
/// </summary>
public record EventName
{
    /// <summary>
    /// The name of the event.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The count of events with this name.
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>
    /// The last occurrence date of this event.
    /// </summary>
    [JsonPropertyName("last_occurred_at")]
    public DateTime LastOccurredAt { get; init; }
}

/// <summary>
/// Request to ingest events.
/// </summary>
public record EventIngestRequest
{
    /// <summary>
    /// The list of events to ingest.
    /// </summary>
    [Required]
    public List<EventData> Events { get; init; } = new();
}

/// <summary>
/// Represents event data for ingestion.
/// </summary>
public record EventData
{
    /// <summary>
    /// The name of the event.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID associated with the event.
    /// </summary>
    public string? CustomerId { get; init; }

    /// <summary>
    /// The event data.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// The metadata associated with the event.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The timestamp of the event (optional, defaults to current time).
    /// </summary>
    public DateTime? Timestamp { get; init; }
}