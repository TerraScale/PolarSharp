using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PolarSharp.Api;

namespace PolarSharp.Models.Subscriptions;

/// <summary>
/// Request to export subscriptions.
/// </summary>
public record SubscriptionExportRequest
{
    /// <summary>
    /// The export format.
    /// </summary>
    [Required]
    [JsonPropertyName("format")]
    public ExportFormat Format { get; init; }

    /// <summary>
    /// Filter by product ID.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; init; }

    /// <summary>
    /// Filter by customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; init; }

    /// <summary>
    /// Filter by subscription status.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Filter by subscription created after this date.
    /// </summary>
    [JsonPropertyName("created_after")]
    public DateTime? CreatedAfter { get; init; }

    /// <summary>
    /// Filter by subscription created before this date.
    /// </summary>
    [JsonPropertyName("created_before")]
    public DateTime? CreatedBefore { get; init; }

    /// <summary>
    /// Filter by start date.
    /// </summary>
    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Filter by end date.
    /// </summary>
    [JsonPropertyName("end_date")]
    public DateTime? EndDate { get; init; }
}