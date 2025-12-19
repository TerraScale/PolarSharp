using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.CustomFields;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing custom fields in the Polar system.
/// </summary>
public class CustomFieldsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CustomFieldsApi(
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
    /// Lists all custom fields with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing custom fields.</returns>
    public async Task<PolarResult<PaginatedResponse<CustomField>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/custom_fields?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<CustomField>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new custom field.
    /// </summary>
    /// <param name="request">The custom field creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created custom field.</returns>
    public async Task<PolarResult<CustomField>> CreateAsync(
        CustomFieldCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/custom_fields", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomField>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a custom field by ID.
    /// </summary>
    /// <param name="customFieldId">The custom field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The custom field, or null if not found.</returns>
    public async Task<PolarResult<CustomField>> GetAsync(
        string customFieldId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/custom_fields/{customFieldId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomField>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing custom field.
    /// </summary>
    /// <param name="customFieldId">The custom field ID.</param>
    /// <param name="request">The custom field update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated custom field, or null if not found.</returns>
    public async Task<PolarResult<CustomField>> UpdateAsync(
        string customFieldId,
        CustomFieldUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/custom_fields/{customFieldId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomField>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a custom field.
    /// </summary>
    /// <param name="customFieldId">The custom field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, or null if not found.</returns>
    public async Task<PolarResult> DeleteAsync(
        string customFieldId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/custom_fields/{customFieldId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all custom fields across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all custom fields.</returns>
    public async IAsyncEnumerable<PolarResult<CustomField>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<CustomField>.Failure(result.Error!);
                yield break;
            }

            foreach (var customField in result.Value!.Items)
            {
                yield return PolarResult<CustomField>.Success(customField);
            }

            if (page >= result.Value!.Pagination.MaxPage)
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
    /// Creates a query builder for custom fields with fluent filtering.
    /// </summary>
    /// <returns>A new CustomFieldsQueryBuilder instance.</returns>
    public CustomFieldsQueryBuilder Query() => new();

    /// <summary>
    /// Lists custom fields using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered custom fields.</returns>
    public async Task<PolarResult<PaginatedResponse<CustomField>>> ListAsync(
        CustomFieldsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/custom_fields?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<CustomField>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}