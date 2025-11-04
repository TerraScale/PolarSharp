using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.CustomFields;

/// <summary>
/// Represents a custom field in the Polar system.
/// </summary>
public record CustomField
{
    /// <summary>
    /// The unique identifier of the custom field.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the custom field.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the custom field.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The type of the custom field.
    /// </summary>
    [JsonPropertyName("type")]
    public CustomFieldType Type { get; init; }

    /// <summary>
    /// Whether the custom field is required.
    /// </summary>
    [JsonPropertyName("is_required")]
    public bool IsRequired { get; init; }

    /// <summary>
    /// Whether the custom field is active.
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    /// <summary>
    /// The organization ID that owns the custom field.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string OrganizationId { get; init; } = string.Empty;

    /// <summary>
    /// The creation date of the custom field.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last modification date of the custom field.
    /// </summary>
    [JsonPropertyName("modified_at")]
    public DateTime ModifiedAt { get; init; }

    /// <summary>
    /// The metadata associated with the custom field.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The available options for select/multi-select fields.
    /// </summary>
    [JsonPropertyName("options")]
    public List<CustomFieldOption>? Options { get; init; }
}

/// <summary>
/// Represents an option for select/multi-select custom fields.
/// </summary>
public record CustomFieldOption
{
    /// <summary>
    /// The unique identifier of the option.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The label of the option.
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// The value of the option.
    /// </summary>
    [JsonPropertyName("value")]
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// The sort order of the option.
    /// </summary>
    [JsonPropertyName("sort_order")]
    public int SortOrder { get; init; }
}

/// <summary>
/// Represents the type of a custom field.
/// </summary>
public enum CustomFieldType
{
    /// <summary>
    /// Text input field.
    /// </summary>
    [JsonPropertyName("text")]
    Text,

    /// <summary>
    /// Number input field.
    /// </summary>
    [JsonPropertyName("number")]
    Number,

    /// <summary>
    /// Boolean checkbox field.
    /// </summary>
    [JsonPropertyName("boolean")]
    Boolean,

    /// <summary>
    /// Date picker field.
    /// </summary>
    [JsonPropertyName("date")]
    Date,

    /// <summary>
    /// Select dropdown field.
    /// </summary>
    [JsonPropertyName("select")]
    Select,

    /// <summary>
    /// Multi-select field.
    /// </summary>
    [JsonPropertyName("multi_select")]
    MultiSelect,

    /// <summary>
    /// Textarea field.
    /// </summary>
    [JsonPropertyName("textarea")]
    Textarea,

    /// <summary>
    /// URL input field.
    /// </summary>
    [JsonPropertyName("url")]
    Url,

    /// <summary>
    /// Email input field.
    /// </summary>
    [JsonPropertyName("email")]
    Email
}

/// <summary>
/// Request to create a new custom field.
/// </summary>
public record CustomFieldCreateRequest
{
    /// <summary>
    /// The name of the custom field.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the custom field.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The type of the custom field.
    /// </summary>
    [Required]
    public CustomFieldType Type { get; init; }

    /// <summary>
    /// Whether the custom field is required.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// The metadata to associate with the custom field.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The options for select/multi-select fields.
    /// </summary>
    public List<CustomFieldOptionCreateRequest>? Options { get; init; }
}

/// <summary>
/// Request to update an existing custom field.
/// </summary>
public record CustomFieldUpdateRequest
{
    /// <summary>
    /// The name of the custom field.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The description of the custom field.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The type of the custom field.
    /// </summary>
    public CustomFieldType? Type { get; init; }

    /// <summary>
    /// Whether the custom field is required.
    /// </summary>
    public bool? IsRequired { get; init; }

    /// <summary>
    /// Whether the custom field is active.
    /// </summary>
    public bool? IsActive { get; init; }

    /// <summary>
    /// The metadata to associate with the custom field.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The options for select/multi-select fields.
    /// </summary>
    public List<CustomFieldOptionCreateRequest>? Options { get; init; }
}

/// <summary>
/// Request to create a custom field option.
/// </summary>
public record CustomFieldOptionCreateRequest
{
    /// <summary>
    /// The label of the option.
    /// </summary>
    [Required]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// The value of the option.
    /// </summary>
    [Required]
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// The sort order of the option.
    /// </summary>
    public int SortOrder { get; init; }
}