using System.ComponentModel.DataAnnotations;

namespace PolarSharp.Models.Organizations;

/// <summary>
/// Represents an organization in the Polar system.
/// </summary>
public record Organization
{
    /// <summary>
    /// The unique identifier of the organization.
    /// </summary>
    [Required]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the organization.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The slug of the organization.
    /// </summary>
    [Required]
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// The description of the organization.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The avatar URL of the organization.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The banner URL of the organization.
    /// </summary>
    public string? BannerUrl { get; init; }

    /// <summary>
    /// The website URL of the organization.
    /// </summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// The Twitter handle of the organization.
    /// </summary>
    public string? TwitterHandle { get; init; }

    /// <summary>
    /// The GitHub URL of the organization.
    /// </summary>
    public string? GithubUrl { get; init; }

    /// <summary>
    /// The metadata associated with the organization.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the organization is verified.
    /// </summary>
    public bool Verified { get; init; }

    /// <summary>
    /// Whether the organization is public.
    /// </summary>
    public bool Public { get; init; }

    /// <summary>
    /// The creation date of the organization.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the organization.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The organization settings.
    /// </summary>
    public OrganizationSettings? Settings { get; init; }

    /// <summary>
    /// The organization's default currency.
    /// </summary>
    public string? DefaultCurrency { get; init; }

    /// <summary>
    /// The organization's country.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// The organization's timezone.
    /// </summary>
    public string? Timezone { get; init; }
}

/// <summary>
/// Represents the settings of an organization.
/// </summary>
public record OrganizationSettings
{
    /// <summary>
    /// Whether to enable custom domains.
    /// </summary>
    public bool EnableCustomDomains { get; init; }

    /// <summary>
    /// Whether to enable webhooks.
    /// </summary>
    public bool EnableWebhooks { get; init; }

    /// <summary>
    /// Whether to enable customer portal.
    /// </summary>
    public bool EnableCustomerPortal { get; init; }

    /// <summary>
    /// Whether to enable license keys.
    /// </summary>
    public bool EnableLicenseKeys { get; init; }

    /// <summary>
    /// Whether to enable downloads.
    /// </summary>
    public bool EnableDownloads { get; init; }

    /// <summary>
    /// Whether to enable subscriptions.
    /// </summary>
    public bool EnableSubscriptions { get; init; }

    /// <summary>
    /// Whether to enable discounts.
    /// </summary>
    public bool EnableDiscounts { get; init; }

    /// <summary>
    /// Whether to enable custom fields.
    /// </summary>
    public bool EnableCustomFields { get; init; }

    /// <summary>
    /// Whether to enable metrics.
    /// </summary>
    public bool EnableMetrics { get; init; }

    /// <summary>
    /// Whether to enable events.
    /// </summary>
    public bool EnableEvents { get; init; }

    /// <summary>
    /// Whether to enable OAuth2.
    /// </summary>
    public bool EnableOAuth2 { get; init; }

    /// <summary>
    /// Whether to enable customer seats.
    /// </summary>
    public bool EnableCustomerSeats { get; init; }

    /// <summary>
    /// Whether to enable meters.
    /// </summary>
    public bool EnableMeters { get; init; }

    /// <summary>
    /// Whether to enable customer meters.
    /// </summary>
    public bool EnableCustomerMeters { get; init; }
}

/// <summary>
/// Request to create a new organization.
/// </summary>
public record OrganizationCreateRequest
{
    /// <summary>
    /// The name of the organization.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The slug of the organization.
    /// </summary>
    [Required]
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// The description of the organization.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The avatar URL of the organization.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The banner URL of the organization.
    /// </summary>
    public string? BannerUrl { get; init; }

    /// <summary>
    /// The website URL of the organization.
    /// </summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// The Twitter handle of the organization.
    /// </summary>
    public string? TwitterHandle { get; init; }

    /// <summary>
    /// The GitHub URL of the organization.
    /// </summary>
    public string? GithubUrl { get; init; }

    /// <summary>
    /// The metadata to associate with the organization.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the organization should be public.
    /// </summary>
    public bool Public { get; init; }

    /// <summary>
    /// The organization's default currency.
    /// </summary>
    public string? DefaultCurrency { get; init; }

    /// <summary>
    /// The organization's country.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// The organization's timezone.
    /// </summary>
    public string? Timezone { get; init; }

    /// <summary>
    /// The organization settings.
    /// </summary>
    public OrganizationSettings? Settings { get; init; }
}

/// <summary>
/// Request to update an existing organization.
/// </summary>
public record OrganizationUpdateRequest
{
    /// <summary>
    /// The name of the organization.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The description of the organization.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The avatar URL of the organization.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The banner URL of the organization.
    /// </summary>
    public string? BannerUrl { get; init; }

    /// <summary>
    /// The website URL of the organization.
    /// </summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>
    /// The Twitter handle of the organization.
    /// </summary>
    public string? TwitterHandle { get; init; }

    /// <summary>
    /// The GitHub URL of the organization.
    /// </summary>
    public string? GithubUrl { get; init; }

    /// <summary>
    /// The metadata to update on the organization.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the organization should be public.
    /// </summary>
    public bool? Public { get; init; }

    /// <summary>
    /// The organization's default currency.
    /// </summary>
    public string? DefaultCurrency { get; init; }

    /// <summary>
    /// The organization's country.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// The organization's timezone.
    /// </summary>
    public string? Timezone { get; init; }

    /// <summary>
    /// The organization settings.
    /// </summary>
    public OrganizationSettings? Settings { get; init; }
}