using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Files;
using PolarSharp.Results;
using File = PolarSharp.Models.Files.File;
using Files_File = PolarSharp.Models.Files.File;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing files in the Polar system.
/// </summary>
public class FilesApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal FilesApi(
        HttpClient httpClient,
        JsonSerializerOptions jsonOptions,
        AsyncRetryPolicy<HttpResponseMessage> retryPolicy,
        AsyncRateLimitPolicy<HttpResponseMessage> rateLimitPolicy)
    {
        _httpClient = httpClient;
        _jsonOptions = jsonOptions;
        _retryPolicy = retryPolicy;
        _rateLimitPolicy = rateLimitPolicy;
    }

    /// <summary>
    /// Lists all files with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing files.</returns>
    public async Task<PolarResult<PaginatedResponse<Files_File>>> ListAsync(
        int page = 1,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/files?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Files_File>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a file by ID.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file, or null if not found.</returns>
    public async Task<PolarResult<Files_File?>> GetAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/files/{fileId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Files_File>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new file.
    /// </summary>
    /// <param name="request">The file creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created file.</returns>
    public async Task<PolarResult<Files_File>> CreateAsync(
        FileCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/files", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Files_File>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing file.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <param name="request">The file update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated file, or null if not found.</returns>
    public async Task<PolarResult<Files_File?>> UpdateAsync(
        string fileId,
        FileUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/files/{fileId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Files_File>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted file, or null if not found.</returns>
    public async Task<PolarResult<Files_File?>> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/files/{fileId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Files_File>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Marks a file upload as completed.
    /// </summary>
    /// <param name="fileId">The file ID.</param>
    /// <param name="request">The upload completion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated file.</returns>
    public async Task<PolarResult<Files_File>> CompleteUploadAsync(
        string fileId,
        FileUploadCompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/files/{fileId}/uploaded", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Files_File>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all files across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all files.</returns>
    public async IAsyncEnumerable<PolarResult<Files_File>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Files_File>.Failure(result.Error!);
                yield break;
            }

            foreach (var file in result.Value.Items)
            {
                yield return PolarResult<Files_File>.Success(file);
            }

            if (page >= result.Value.Pagination.MaxPage)
                break;

            page++;
        }
    }

    private async Task<HttpResponseMessage> ExecuteWithPoliciesAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        // Rate limiting and retry is now handled by RateLimitedHttpHandler
        return await operation();
    }

    /// <summary>
    /// Creates a query builder for files with fluent filtering.
    /// </summary>
    /// <returns>A new FilesQueryBuilder instance.</returns>
    public FilesQueryBuilder Query() => new();

    /// <summary>
    /// Lists files using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered files.</returns>
    public async Task<PolarResult<PaginatedResponse<Files_File>>> ListAsync(
        FilesQueryBuilder builder,
        int page = 1,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        // Add query builder parameters
        foreach (var param in builder.GetParameters())
        {
            queryParams[param.Key] = param.Value;
        }

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/files?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Files_File>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}