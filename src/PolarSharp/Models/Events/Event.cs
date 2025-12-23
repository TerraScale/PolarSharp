using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Models.Customers;

namespace PolarSharp.Models.Events;

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
    /// The timestamp when the event occurred.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The name of the event.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The source of the event (system or user).
    /// </summary>
    [JsonPropertyName("source")]
    public EventSource Source { get; init; }

    /// <summary>
    /// The customer ID associated with the event.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The external customer ID associated with the event.
    /// </summary>
    [JsonPropertyName("external_customer_id")]
    public string? ExternalCustomerId { get; init; }

    /// <summary>
    /// The organization ID associated with the event.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The customer associated with the event.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customer? Customer { get; init; }

    /// <summary>
    /// The metadata associated with the event.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Alias for Metadata for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, object>? Data => Metadata;

    /// <summary>
    /// Alias for Timestamp for backward compatibility.
    /// </summary>
    [JsonIgnore]
    public DateTime CreatedAt => Timestamp;

    /// <summary>
    /// The number of child events.
    /// </summary>
    [JsonPropertyName("child_count")]
    public int ChildCount { get; init; }

    /// <summary>
    /// The parent event ID (for hierarchical events).
    /// </summary>
    [JsonPropertyName("parent_id")]
    public string? ParentId { get; init; }
}

/// <summary>
/// The source of an event.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EventSource
{
    /// <summary>
    /// System-generated event.
    /// </summary>
    [JsonPropertyName("system")]
    System,

    /// <summary>
    /// User-generated event.
    /// </summary>
    [JsonPropertyName("user")]
    User
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
    [JsonPropertyName("events")]
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
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The external customer ID associated with the event.
    /// </summary>
    [JsonPropertyName("external_customer_id")]
    public string? ExternalCustomerId { get; init; }

    /// <summary>
    /// The customer ID associated with the event.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// The organization ID associated with the event.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; init; }

    /// <summary>
    /// The event payload data.
    /// </summary>
    [JsonPropertyName("data")]
    public Dictionary<string, object>? Data { get; init; }

    /// <summary>
    /// The metadata associated with the event.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The timestamp of the event (optional, defaults to current time).
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; init; }
}
