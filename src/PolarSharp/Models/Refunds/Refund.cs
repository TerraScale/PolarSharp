using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Refunds;

/// <summary>
/// Represents a refund in the Polar system.
/// </summary>
public record Refund
{
    /// <summary>
    /// The unique identifier of the refund.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The amount of the refund in the smallest currency unit (e.g., cents).
    /// </summary>
    [Required]
    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    /// <summary>
    /// The currency of the refund (ISO 4217 format).
    /// </summary>
    [Required]
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The payment ID associated with the refund.
    /// </summary>
    [Required]
    [JsonPropertyName("payment_id")]
    public string PaymentId { get; init; } = string.Empty;

    /// <summary>
    /// The order ID associated with the refund.
    /// </summary>
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }

    /// <summary>
    /// The reason for the refund.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    /// The status of the refund.
    /// </summary>
    [Required]
    [JsonPropertyName("status")]
    public RefundStatus Status { get; init; }

    /// <summary>
    /// The metadata associated with the refund.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the refund.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the refund.
    /// </summary>
    [Required]
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The receipt URL for the refund.
    /// </summary>
    [JsonPropertyName("receipt_url")]
    public string? ReceiptUrl { get; init; }
}

/// <summary>
/// Represents the status of a refund.
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// The refund is pending.
    /// </summary>
    Pending,

    /// <summary>
    /// The refund succeeded.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The refund failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The refund was canceled.
    /// </summary>
    Canceled
}

/// <summary>
/// Request to create a new refund.
/// </summary>
public record RefundCreateRequest
{
    /// <summary>
    /// The payment ID to refund.
    /// </summary>
    [Required]
    [JsonPropertyName("payment_id")]
    public string PaymentId { get; init; } = string.Empty;

    /// <summary>
    /// The amount to refund in the smallest currency unit (e.g., cents).
    /// </summary>
    [Required]
    [Range(1, long.MaxValue)]
    public long Amount { get; init; }

    /// <summary>
    /// The reason for the refund.
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; init; }

    /// <summary>
    /// The metadata to associate with the refund.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}