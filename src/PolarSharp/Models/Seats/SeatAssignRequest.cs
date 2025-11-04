using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.Seats;

/// <summary>
/// Request to assign a seat to a user.
/// </summary>
public record CustomerSeatAssignRequest
{
    /// <summary>
    /// The seat ID.
    /// </summary>
    [Required]
    public string SeatId { get; init; } = string.Empty;

    /// <summary>
    /// The user ID to assign the seat to.
    /// </summary>
    [Required]
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The email of the user to assign the seat to.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The metadata to associate with the seat assignment.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}