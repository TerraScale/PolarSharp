using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Customers;

/// <summary>
/// Request to create a new customer.
/// </summary>
public record CustomerCreateRequest
{
    /// <summary>
    /// The email address of customer.
    /// </summary>
    [Required]
    [EmailAddress]
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The name of customer.
    /// </summary>
    [StringLength(100, ErrorMessage = "Customer name cannot exceed 100 characters.")]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The external ID for the customer.
    /// </summary>
    [StringLength(100, ErrorMessage = "External ID cannot exceed 100 characters.")]
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The metadata to associate with customer.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The avatar URL of customer.
    /// </summary>
    [Url(ErrorMessage = "Avatar URL must be a valid URL.")]
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The billing address of customer.
    /// </summary>
    [JsonPropertyName("billing_address")]
    public Address? BillingAddress { get; init; }

    /// <summary>
    /// The shipping address of customer.
    /// </summary>
    [JsonPropertyName("shipping_address")]
    public Address? ShippingAddress { get; init; }
}

/// <summary>
/// Request to update an existing customer.
/// </summary>


/// <summary>
/// Represents the state of a customer.
/// A customer along with additional state information including active subscriptions, granted benefits, and active meters.
/// </summary>
public record CustomerState
{
    /// <summary>
    /// The ID of the customer.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the customer.
    /// </summary>
    [Required]
    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The name of the customer.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The external ID for the customer.
    /// </summary>
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; init; }

    /// <summary>
    /// The metadata associated with the customer.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether customer has a verified email address.
    /// </summary>
    [JsonPropertyName("email_verified")]
    public bool EmailVerified { get; init; }

    /// <summary>
    /// The avatar URL of customer.
    /// </summary>
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// Creation timestamp of the object.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modification timestamp of the object.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// Timestamp for when the customer was soft deleted.
    /// </summary>
    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; init; }

    /// <summary>
    /// The customer's billing address.
    /// </summary>
    [JsonPropertyName("billing_address")]
    public Address? BillingAddress { get; init; }

    /// <summary>
    /// The customer's tax ID.
    /// </summary>
    [JsonPropertyName("tax_id")]
    public List<object>? TaxId { get; init; }

    /// <summary>
    /// The ID of the organization owning the customer.
    /// </summary>
    [Required]
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The customer's active subscriptions.
    /// </summary>
    [JsonPropertyName("active_subscriptions")]
    public List<CustomerStateSubscription>? ActiveSubscriptions { get; init; }

    /// <summary>
    /// The customer's active benefit grants.
    /// </summary>
    [JsonPropertyName("granted_benefits")]
    public List<CustomerStateBenefitGrant>? GrantedBenefits { get; init; }

    /// <summary>
    /// The customer's active meters.
    /// </summary>
    [JsonPropertyName("active_meters")]
    public List<CustomerStateMeter>? ActiveMeters { get; init; }
}

/// <summary>
/// Represents a subscription in customer state.
/// </summary>
public record CustomerStateSubscription
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// Metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The subscription status.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// The amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public long? Amount { get; init; }

    /// <summary>
    /// The currency.
    /// </summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>
    /// The recurring interval.
    /// </summary>
    [JsonPropertyName("recurring_interval")]
    public string? RecurringInterval { get; init; }

    /// <summary>
    /// Current period start.
    /// </summary>
    [JsonPropertyName("current_period_start")]
    public DateTime? CurrentPeriodStart { get; init; }

    /// <summary>
    /// Current period end.
    /// </summary>
    [JsonPropertyName("current_period_end")]
    public DateTime? CurrentPeriodEnd { get; init; }

    /// <summary>
    /// Trial start.
    /// </summary>
    [JsonPropertyName("trial_start")]
    public DateTime? TrialStart { get; init; }

    /// <summary>
    /// Trial end.
    /// </summary>
    [JsonPropertyName("trial_end")]
    public DateTime? TrialEnd { get; init; }

    /// <summary>
    /// Whether to cancel at period end.
    /// </summary>
    [JsonPropertyName("cancel_at_period_end")]
    public bool CancelAtPeriodEnd { get; init; }

    /// <summary>
    /// Canceled at timestamp.
    /// </summary>
    [JsonPropertyName("canceled_at")]
    public DateTime? CanceledAt { get; init; }

    /// <summary>
    /// Started at timestamp.
    /// </summary>
    [JsonPropertyName("started_at")]
    public DateTime? StartedAt { get; init; }

    /// <summary>
    /// Ends at timestamp.
    /// </summary>
    [JsonPropertyName("ends_at")]
    public DateTime? EndsAt { get; init; }

    /// <summary>
    /// The product ID.
    /// </summary>
    [JsonPropertyName("product_id")]
    public string? ProductId { get; init; }

    /// <summary>
    /// The discount ID.
    /// </summary>
    [JsonPropertyName("discount_id")]
    public string? DiscountId { get; init; }

    /// <summary>
    /// Custom field data.
    /// </summary>
    [JsonPropertyName("custom_field_data")]
    public Dictionary<string, object>? CustomFieldData { get; init; }
}

/// <summary>
/// Represents a benefit grant in customer state.
/// </summary>
public record CustomerStateBenefitGrant
{
    /// <summary>
    /// The benefit grant ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// Granted at timestamp.
    /// </summary>
    [JsonPropertyName("granted_at")]
    public DateTime? GrantedAt { get; init; }

    /// <summary>
    /// The benefit ID.
    /// </summary>
    [JsonPropertyName("benefit_id")]
    public string? BenefitId { get; init; }

    /// <summary>
    /// The benefit type.
    /// </summary>
    [JsonPropertyName("benefit_type")]
    public string? BenefitType { get; init; }

    /// <summary>
    /// Benefit metadata.
    /// </summary>
    [JsonPropertyName("benefit_metadata")]
    public Dictionary<string, object>? BenefitMetadata { get; init; }

    /// <summary>
    /// Properties.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; init; }
}

/// <summary>
/// Represents a meter in customer state.
/// </summary>
public record CustomerStateMeter
{
    /// <summary>
    /// The meter ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; init; }

    /// <summary>
    /// The meter ID.
    /// </summary>
    [JsonPropertyName("meter_id")]
    public string? MeterId { get; init; }

    /// <summary>
    /// Consumed units.
    /// </summary>
    [JsonPropertyName("consumed_units")]
    public long? ConsumedUnits { get; init; }

    /// <summary>
    /// Credited units.
    /// </summary>
    [JsonPropertyName("credited_units")]
    public long? CreditedUnits { get; init; }

    /// <summary>
    /// Balance.
    /// </summary>
    [JsonPropertyName("balance")]
    public long? Balance { get; init; }
}

/// <summary>
/// Represents the balance of a customer.
/// </summary>
public record CustomerBalance
{
    /// <summary>
    /// The customer ID.
    /// </summary>
    [Required]
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; init; } = string.Empty;

    /// <summary>
    /// The current balance of the customer in cents.
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; init; }

    /// <summary>
    /// The currency of the balance.
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; init; } = string.Empty;
}