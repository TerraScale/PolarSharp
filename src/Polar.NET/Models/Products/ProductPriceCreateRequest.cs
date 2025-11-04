using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Polar.NET.Models.Products;

/// <summary>
/// Request to create a new product price.
/// </summary>
public record ProductPriceCreateRequest
{
    /// <summary>
    /// The amount in the smallest currency unit (e.g., cents for USD).
    /// </summary>
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    [JsonPropertyName("price_amount")]
    public decimal Amount { get; init; }

    /// <summary>
    /// The currency code (e.g., "USD", "EUR").
    /// </summary>
    [Required(ErrorMessage = "Currency is required.")]
    [RegularExpression("^[a-z]{3}$", ErrorMessage = "Currency must be a valid 3-letter ISO 4217 code in lowercase.")]
    [JsonPropertyName("price_currency")]
    public string Currency { get; init; } = string.Empty;

    /// <summary>
    /// The type of price amount.
    /// </summary>
    [Required]
    [JsonPropertyName("amount_type")]
    public ProductPriceType Type { get; init; }

    /// <summary>
    /// The recurring interval for recurring prices.
    /// </summary>
    [RegularExpression("^(day|week|month|year)$", ErrorMessage = "Recurring interval must be day, week, month, or year.")]
    [JsonPropertyName("recurring_interval")]
    public string? RecurringInterval { get; init; }

    /// <summary>
    /// The recurring interval count for recurring prices.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Recurring interval count must be a positive number.")]
    [JsonPropertyName("recurring_interval_count")]
    public int? RecurringIntervalCount { get; init; }

    /// <summary>
    /// The trial period in days for recurring prices.
    /// </summary>
    [Range(0, 365, ErrorMessage = "Trial period must be between 0 and 365 days.")]
    [JsonPropertyName("trial_period_days")]
    public int? TrialPeriodDays { get; init; }
}