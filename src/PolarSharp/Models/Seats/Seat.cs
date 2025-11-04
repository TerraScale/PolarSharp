using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Seats;

/// <summary>
/// Represents a seat in the Polar system.
/// </summary>
public record Seat
{
    /// <summary>
    /// The unique identifier of the seat.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The user ID assigned to the seat.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }

    /// <summary>
    /// The user email.
    /// </summary>
    [JsonPropertyName("user_email")]
    public string? UserEmail { get; init; }

    /// <summary>
    /// The user name.
    /// </summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; init; }

    /// <summary>
    /// The subscription ID.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The seat status.
    /// </summary>
    [JsonPropertyName("status")]
    public SeatStatus Status { get; init; }

    /// <summary>
    /// The invitation token.
    /// </summary>
    [JsonPropertyName("invitation_token")]
    public string? InvitationToken { get; init; }

    /// <summary>
    /// The invitation expires at.
    /// </summary>
    [JsonPropertyName("invitation_expires_at")]
    public DateTime? InvitationExpiresAt { get; init; }

    /// <summary>
    /// The last invited at.
    /// </summary>
    [JsonPropertyName("last_invited_at")]
    public DateTime? LastInvitedAt { get; init; }

    /// <summary>
    /// The creation date of the seat.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the seat.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The subscription information.
    /// </summary>
    [JsonPropertyName("subscription")]
    public Subscriptions.Subscription? Subscription { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }
}

/// <summary>
/// Represents a customer seat in the Polar system.
/// </summary>
public record CustomerSeat
{
    /// <summary>
    /// The unique identifier of the customer seat.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The user ID.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }

    /// <summary>
    /// The user email.
    /// </summary>
    [JsonPropertyName("user_email")]
    public string? UserEmail { get; init; }

    /// <summary>
    /// The user name.
    /// </summary>
    [JsonPropertyName("user_name")]
    public string? UserName { get; init; }

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The subscription ID.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The seat status.
    /// </summary>
    [JsonPropertyName("status")]
    public SeatStatus Status { get; init; }

    /// <summary>
    /// The invitation token.
    /// </summary>
    [JsonPropertyName("invitation_token")]
    public string? InvitationToken { get; init; }

    /// <summary>
    /// The invitation expires at.
    /// </summary>
    [JsonPropertyName("invitation_expires_at")]
    public DateTime? InvitationExpiresAt { get; init; }

    /// <summary>
    /// The last invited at.
    /// </summary>
    [JsonPropertyName("last_invited_at")]
    public DateTime? LastInvitedAt { get; init; }

    /// <summary>
    /// The creation date of the customer seat.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the customer seat.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The customer information.
    /// </summary>
    [JsonPropertyName("customer")]
    public Customers.Customer? Customer { get; init; }

    /// <summary>
    /// The subscription information.
    /// </summary>
    [JsonPropertyName("subscription")]
    public Subscriptions.Subscription? Subscription { get; init; }
}

/// <summary>
/// Represents the status of a seat.
/// </summary>
public enum SeatStatus
{
    /// <summary>
    /// The seat is active.
    /// </summary>
    [JsonPropertyName("active")]
    Active,

    /// <summary>
    /// The seat is invited.
    /// </summary>
    [JsonPropertyName("invited")]
    Invited,

    /// <summary>
    /// The seat is revoked.
    /// </summary>
    [JsonPropertyName("revoked")]
    Revoked
}

/// <summary>
/// Request to assign a seat.
/// </summary>
public record SubscriptionSeatAssignRequest
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    [Required]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The user email to assign the seat to.
    /// </summary>
    [Required]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Request to revoke a seat.
/// </summary>
public record SeatRevokeRequest
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    [Required]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The seat ID to revoke.
    /// </summary>
    [Required]
    public string SeatId { get; init; } = string.Empty;
}

/// <summary>
/// Request to resend a seat invitation.
/// </summary>
public record SeatResendInvitationRequest
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    [Required]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The seat ID to resend invitation for.
    /// </summary>
    [Required]
    public string SeatId { get; init; } = string.Empty;
}

/// <summary>
/// Represents seat claim information.
/// </summary>
public record SeatClaimInfo
{
    /// <summary>
    /// The seat ID.
    /// </summary>
    [JsonPropertyName("seat_id")]
    public string SeatId { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The subscription ID.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The invitation token.
    /// </summary>
    [JsonPropertyName("invitation_token")]
    public string InvitationToken { get; init; } = string.Empty;

    /// <summary>
    /// The invitation expires at.
    /// </summary>
    [JsonPropertyName("invitation_expires_at")]
    public DateTime InvitationExpiresAt { get; init; }
}

/// <summary>
/// Request to claim a seat.
/// </summary>
public record SeatClaimRequest
{
    /// <summary>
    /// The invitation token.
    /// </summary>
    [Required]
    public string InvitationToken { get; init; } = string.Empty;
}

/// <summary>
/// Represents claimed subscriptions for seats.
/// </summary>
public record ClaimedSubscription
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    [JsonPropertyName("subscription_id")]
    public string SubscriptionId { get; init; } = string.Empty;

    /// <summary>
    /// The subscription name.
    /// </summary>
    [JsonPropertyName("subscription_name")]
    public string SubscriptionName { get; init; } = string.Empty;

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The customer name.
    /// </summary>
    [JsonPropertyName("customer_name")]
    public string CustomerName { get; init; } = string.Empty;

    /// <summary>
    /// The available seats.
    /// </summary>
    [JsonPropertyName("available_seats")]
    public int AvailableSeats { get; init; }

    /// <summary>
    /// The used seats.
    /// </summary>
    [JsonPropertyName("used_seats")]
    public int UsedSeats { get; init; }
}