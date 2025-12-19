using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Refunds;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing refunds in Polar system.
/// </summary>
public class RefundsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal RefundsApi(
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
    /// Lists all refunds with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing refunds.</returns>
    public async Task<PolarResult<PaginatedResponse<Refund>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/refunds?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Refund>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a refund by ID.
    /// </summary>
    /// <param name="refundId">The refund ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refund, or null if not found.</returns>
    public async Task<PolarResult<Refund>> GetAsync(
        string refundId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/refunds/{refundId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Refund>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new refund.
    /// </summary>
    /// <param name="request">The refund creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created refund.</returns>
    public async Task<PolarResult<Refund>> CreateAsync(
        RefundCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("refunds", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Refund>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all refunds across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all refunds.</returns>
    public async IAsyncEnumerable<PolarResult<Refund>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Refund>.Failure(result.Error!);
                yield break;
            }

            foreach (var refund in result.Value!.Items)
            {
                yield return PolarResult<Refund>.Success(refund);
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
    /// Creates a query builder for refunds with fluent filtering.
    /// </summary>
    /// <returns>A new RefundsQueryBuilder instance.</returns>
    public RefundsQueryBuilder Query() => new();

    /// <summary>
    /// Lists refunds using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered refunds.</returns>
    public async Task<PolarResult<PaginatedResponse<Refund>>> ListAsync(
        RefundsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/refunds?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Refund>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}