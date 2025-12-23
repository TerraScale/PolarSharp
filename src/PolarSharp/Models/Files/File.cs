using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PolarSharp.Models.Files;

/// <summary>
/// Represents a file in the Polar system.
/// </summary>
public record File
{
    /// <summary>
    /// The unique identifier of the file.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The organization ID that owns the file.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; init; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The path of the file.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; init; }

    /// <summary>
    /// The MIME type of the file.
    /// </summary>
    [Required]
    [JsonPropertyName("mime_type")]
    public string MimeType { get; init; } = string.Empty;

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    [Required]
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// Human-readable file size.
    /// </summary>
    [JsonPropertyName("size_readable")]
    public string? SizeReadable { get; init; }

    /// <summary>
    /// The storage version of the file.
    /// </summary>
    [JsonPropertyName("storage_version")]
    public string? StorageVersion { get; init; }

    /// <summary>
    /// The ETag checksum of the file.
    /// </summary>
    [JsonPropertyName("checksum_etag")]
    public string? ChecksumEtag { get; init; }

    /// <summary>
    /// The SHA256 checksum of the file in base64.
    /// </summary>
    [JsonPropertyName("checksum_sha256_base64")]
    public string? ChecksumSha256Base64 { get; init; }

    /// <summary>
    /// The SHA256 checksum of the file in hex.
    /// </summary>
    [JsonPropertyName("checksum_sha256_hex")]
    public string? ChecksumSha256Hex { get; init; }

    /// <summary>
    /// The last modification date of the file.
    /// </summary>
    [JsonPropertyName("last_modified_at")]
    public DateTime? LastModifiedAt { get; init; }

    /// <summary>
    /// The version of the file.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>
    /// The service that stores the file.
    /// </summary>
    [JsonPropertyName("service")]
    public FileService Service { get; init; }

    /// <summary>
    /// Whether the file has been uploaded.
    /// </summary>
    [JsonPropertyName("is_uploaded")]
    public bool IsUploaded { get; init; }

    /// <summary>
    /// The creation date of the file.
    /// </summary>
    [Required]
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The upload details for completing the upload.
    /// </summary>
    [JsonPropertyName("upload")]
    public FileUpload? Upload { get; init; }

    /// <summary>
    /// The description of the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the file is public (for backward compatibility).
    /// </summary>
    [JsonPropertyName("public")]
    public bool? Public { get; init; }

    /// <summary>
    /// The status of the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("status")]
    public FileStatus? Status { get; init; }

    /// <summary>
    /// The upload URL (for backward compatibility, use Upload.Url instead).
    /// </summary>
    [JsonIgnore]
    public string? UploadUrl => Upload?.Url;

    /// <summary>
    /// The metadata associated with the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Represents the status of a file.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileStatus
{
    /// <summary>
    /// The file is pending upload.
    /// </summary>
    [JsonPropertyName("pending")]
    Pending,

    /// <summary>
    /// The file has been uploaded.
    /// </summary>
    [JsonPropertyName("uploaded")]
    Uploaded,

    /// <summary>
    /// The file upload failed.
    /// </summary>
    [JsonPropertyName("failed")]
    Failed
}

/// <summary>
/// Represents the service that stores the file.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileService
{
    /// <summary>
    /// Downloadable file service.
    /// </summary>
    [JsonPropertyName("downloadable")]
    Downloadable,

    /// <summary>
    /// Product media file service.
    /// </summary>
    [JsonPropertyName("product_media")]
    ProductMedia,

    /// <summary>
    /// Organization avatar file service.
    /// </summary>
    [JsonPropertyName("organization_avatar")]
    OrganizationAvatar
}

/// <summary>
/// Represents file upload information.
/// </summary>
public record FileUpload
{
    /// <summary>
    /// The upload ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// The upload URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>
    /// The upload headers.
    /// </summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// The upload parts for multipart uploads.
    /// </summary>
    [JsonPropertyName("parts")]
    public List<FileUploadPart>? Parts { get; init; }
}

/// <summary>
/// Represents a part of a multipart file upload.
/// </summary>
public record FileUploadPart
{
    /// <summary>
    /// The part number.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; init; }

    /// <summary>
    /// The chunk start byte.
    /// </summary>
    [JsonPropertyName("chunk_start")]
    public long ChunkStart { get; init; }

    /// <summary>
    /// The chunk end byte.
    /// </summary>
    [JsonPropertyName("chunk_end")]
    public long ChunkEnd { get; init; }

    /// <summary>
    /// The checksum for the part.
    /// </summary>
    [JsonPropertyName("checksum_sha256_base64")]
    public string? ChecksumSha256Base64 { get; init; }

    /// <summary>
    /// The upload URL for the part.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>
    /// The upload headers for the part.
    /// </summary>
    [JsonPropertyName("headers")]
    public Dictionary<string, string>? Headers { get; init; }
}

/// <summary>
/// Request to create a new file.
/// </summary>
public record FileCreateRequest
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    [Required]
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The MIME type of the file.
    /// </summary>
    [Required]
    [JsonPropertyName("mime_type")]
    public string MimeType { get; init; } = string.Empty;

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    [Required]
    [Range(1, long.MaxValue)]
    [JsonPropertyName("size")]
    public long Size { get; init; }

    /// <summary>
    /// The service type for the file.
    /// </summary>
    [JsonPropertyName("service")]
    public FileService Service { get; init; }

    /// <summary>
    /// The checksum of the file (SHA256 base64).
    /// </summary>
    [JsonPropertyName("checksum_sha256_base64")]
    public string? ChecksumSha256Base64 { get; init; }

    /// <summary>
    /// The organization ID.
    /// </summary>
    [JsonPropertyName("organization_id")]
    public string? OrganizationId { get; init; }

    /// <summary>
    /// The description of the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the file is public (for backward compatibility).
    /// </summary>
    [JsonPropertyName("public")]
    public bool? Public { get; init; }

    /// <summary>
    /// The metadata associated with the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to update an existing file.
/// </summary>
public record FileUpdateRequest
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// The version of the file.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>
    /// The description of the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Whether the file is public (for backward compatibility).
    /// </summary>
    [JsonPropertyName("public")]
    public bool? Public { get; init; }

    /// <summary>
    /// The metadata associated with the file (for backward compatibility).
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to complete file upload.
/// </summary>
public record FileUploadCompleteRequest
{
    /// <summary>
    /// The file ID.
    /// </summary>
    [Required]
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The upload path.
    /// </summary>
    [JsonPropertyName("path")]
    public string? Path { get; init; }

    /// <summary>
    /// The completed parts for multipart uploads.
    /// </summary>
    [JsonPropertyName("parts")]
    public List<FileUploadCompletedPart>? Parts { get; init; }
}

/// <summary>
/// Represents a completed part of a multipart file upload.
/// </summary>
public record FileUploadCompletedPart
{
    /// <summary>
    /// The part number.
    /// </summary>
    [JsonPropertyName("number")]
    public int Number { get; init; }

    /// <summary>
    /// The checksum for the part.
    /// </summary>
    [JsonPropertyName("checksum_sha256_base64")]
    public string? ChecksumSha256Base64 { get; init; }

    /// <summary>
    /// The ETag for the part.
    /// </summary>
    [JsonPropertyName("checksum_etag")]
    public string? ChecksumEtag { get; init; }
}
