using System.ComponentModel.DataAnnotations;

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
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the file.
    /// </summary>
    [Required]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    [Required]
    public long Size { get; init; }

    /// <summary>
    /// The MIME type of the file.
    /// </summary>
    [Required]
    public string MimeType { get; init; } = string.Empty;

    /// <summary>
    /// The status of the file.
    /// </summary>
    [Required]
    public FileStatus Status { get; init; }

    /// <summary>
    /// The upload URL for the file.
    /// </summary>
    public string? UploadUrl { get; init; }

    /// <summary>
    /// The download URL for the file.
    /// </summary>
    public string? DownloadUrl { get; init; }

    /// <summary>
    /// The checksum of the file.
    /// </summary>
    public string? Checksum { get; init; }

    /// <summary>
    /// The metadata associated with the file.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The creation date of the file.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The last update date of the file.
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The expiration date of the file.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The public URL of the file.
    /// </summary>
    public string? PublicUrl { get; init; }

    /// <summary>
    /// Whether the file is public.
    /// </summary>
    public bool Public { get; init; }
}

/// <summary>
/// Represents the status of a file.
/// </summary>
public enum FileStatus
{
    /// <summary>
    /// The file is pending upload.
    /// </summary>
    Pending,

    /// <summary>
    /// The file is uploading.
    /// </summary>
    Uploading,

    /// <summary>
    /// The file is uploaded.
    /// </summary>
    Uploaded,

    /// <summary>
    /// The file upload failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The file is expired.
    /// </summary>
    Expired
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
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The MIME type of the file.
    /// </summary>
    [Required]
    public string MimeType { get; init; } = string.Empty;

    /// <summary>
    /// The size of the file in bytes.
    /// </summary>
    [Required]
    [Range(1, long.MaxValue)]
    public long Size { get; init; }

    /// <summary>
    /// The metadata to associate with the file.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Whether the file should be public.
    /// </summary>
    public bool Public { get; init; }

    /// <summary>
    /// The expiration date of the file.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// The checksum of the file.
    /// </summary>
    public string? Checksum { get; init; }
}

/// <summary>
/// Request to update an existing file.
/// </summary>
public record FileUpdateRequest
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// The description of the file.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Whether the file should be public.
    /// </summary>
    public bool? Public { get; init; }

    /// <summary>
    /// The metadata to update on the file.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// The expiration date to update on the file.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }
}

/// <summary>
/// Request to complete file upload.
/// </summary>
public record FileUploadCompleteRequest
{
    /// <summary>
    /// The checksum of the uploaded file.
    /// </summary>
    public string? Checksum { get; init; }

    /// <summary>
    /// The metadata to associate with the file.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }
}