using System.Text.Json.Serialization;

namespace PolarSharp.Models.Common;

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// CSV format.
    /// </summary>
    [JsonPropertyName("csv")]
    Csv,

    /// <summary>
    /// JSON format.
    /// </summary>
    [JsonPropertyName("json")]
    Json,

    /// <summary>
    /// Excel format.
    /// </summary>
    [JsonPropertyName("excel")]
    Excel
}