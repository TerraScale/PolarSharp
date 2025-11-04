using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using Polar.NET.Extensions;
using Polar.NET.Exceptions;
using Polar.NET.Models.Common;
using Polar.NET.Models.CustomFields;

namespace Polar.NET.Api;

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
    public async Task<PaginatedResponse<CustomField>> ListAsync(
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

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<CustomField>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new custom field.
    /// </summary>
    /// <param name="request">The custom field creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created custom field.</returns>
    public async Task<CustomField> CreateAsync(
        CustomFieldCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("custom_fields", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<CustomField>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a custom field by ID.
    /// </summary>
    /// <param name="customFieldId">The custom field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The custom field.</returns>
    public async Task<CustomField> GetAsync(
        string customFieldId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/custom_fields/{customFieldId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<CustomField>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing custom field.
    /// </summary>
    /// <param name="customFieldId">The custom field ID.</param>
    /// <param name="request">The custom field update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated custom field.</returns>
    public async Task<CustomField> UpdateAsync(
        string customFieldId,
        CustomFieldUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/custom_fields/{customFieldId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomField>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a custom field.
    /// </summary>
    /// <param name="customFieldId">The custom field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task DeleteAsync(
        string customFieldId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/custom_fields/{customFieldId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all custom fields across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all custom fields.</returns>
    public async IAsyncEnumerable<CustomField> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, cancellationToken);
            
            foreach (var customField in response.Items)
            {
                yield return customField;
            }

            if (page >= response.Pagination.MaxPage)
                break;

            page++;
        }
    }

    private async Task<HttpResponseMessage> ExecuteWithPoliciesAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        return await _rateLimitPolicy.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await operation();
            });
        });
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
    public async Task<PaginatedResponse<CustomField>> ListAsync(
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

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<CustomField>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}