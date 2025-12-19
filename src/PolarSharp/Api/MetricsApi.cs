using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Metrics;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing metrics in the Polar system.
/// </summary>
public class MetricsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal MetricsApi(
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
    /// Gets metrics data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of metrics.</returns>
    public async Task<PolarResult<List<Metric>>> GetAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/metrics", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<List<Metric>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets metrics limits.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of metric limits.</returns>
    public async Task<PolarResult<List<MetricLimit>>> GetLimitsAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/metrics/limits", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<List<MetricLimit>>(_jsonOptions, cancellationToken);
    }

    private async Task<HttpResponseMessage> ExecuteWithPoliciesAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        // Rate limiting and retry is now handled by RateLimitedHttpHandler
        return await operation();
    }

    /// <summary>
    /// Creates a query builder for metrics with fluent filtering.
    /// </summary>
    /// <returns>A new MetricsQueryBuilder instance.</returns>
    public MetricsQueryBuilder Query() => new();

    /// <summary>
    /// Lists metrics using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered metrics.</returns>
    public async Task<PolarResult<PaginatedResponse<Metric>>> ListAsync(
        MetricsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/metrics?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Metric>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all metrics across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all metrics.</returns>
    public async IAsyncEnumerable<PolarResult<Metric>> ListAllAsync(
        MetricsQueryBuilder? builder = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(builder ?? new MetricsQueryBuilder(), page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Metric>.Failure(result.Error!);
                yield break;
            }

            foreach (var metric in result.Value!.Items)
            {
                yield return PolarResult<Metric>.Success(metric);
            }

            if (page >= result.Value!.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Lists all metrics across all pages using IAsyncEnumerable (without filtering).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all metrics.</returns>
    public async IAsyncEnumerable<PolarResult<Metric>> ListAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var metric in ListAllAsync(new MetricsQueryBuilder(), cancellationToken))
        {
            yield return metric;
        }
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}