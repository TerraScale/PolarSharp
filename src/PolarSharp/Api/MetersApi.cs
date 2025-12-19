using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Meters;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing meters in the Polar system.
/// </summary>
public class MetersApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal MetersApi(
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
    /// Lists all meters with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing meters.</returns>
    public async Task<PolarResult<PaginatedResponse<Meter>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/meters?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Meter>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new meter.
    /// </summary>
    /// <param name="request">The meter creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created meter.</returns>
    public async Task<PolarResult<Meter>> CreateAsync(
        MeterCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/meters", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Meter>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a meter by ID.
    /// </summary>
    /// <param name="meterId">The meter ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The meter, or null if not found.</returns>
    public async Task<PolarResult<Meter?>> GetAsync(
        string meterId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/meters/{meterId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Meter>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing meter.
    /// </summary>
    /// <param name="meterId">The meter ID.</param>
    /// <param name="request">The meter update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated meter, or null if not found.</returns>
    public async Task<PolarResult<Meter?>> UpdateAsync(
        string meterId,
        MeterUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/meters/{meterId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Meter>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a meter.
    /// </summary>
    /// <param name="meterId">The meter ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted meter, or null if not found.</returns>
    public async Task<PolarResult<Meter?>> DeleteAsync(
        string meterId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/meters/{meterId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Meter>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets meter quantities for a specific meter.
    /// </summary>
    /// <param name="meterId">The meter ID.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing meter quantities.</returns>
    public async Task<PolarResult<PaginatedResponse<MeterQuantity>>> GetQuantitiesAsync(
        string meterId,
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
            () => _httpClient.GetAsync($"v1/meters/{meterId}/quantities?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<MeterQuantity>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all meters across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all meters.</returns>
    public async IAsyncEnumerable<PolarResult<Meter>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Meter>.Failure(result.Error!);
                yield break;
            }

            foreach (var meter in result.Value.Items)
            {
                yield return PolarResult<Meter>.Success(meter);
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
    /// Creates a query builder for meters with fluent filtering.
    /// </summary>
    /// <returns>A new MetersQueryBuilder instance.</returns>
    public MetersQueryBuilder Query() => new();

    /// <summary>
    /// Lists meters using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered meters.</returns>
    public async Task<PolarResult<PaginatedResponse<Meter>>> ListAsync(
        MetersQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/meters?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Meter>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}