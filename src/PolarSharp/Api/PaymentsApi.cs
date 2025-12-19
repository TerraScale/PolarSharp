using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Payments;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing payments in Polar system.
/// </summary>
public class PaymentsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal PaymentsApi(
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
    /// Lists all payments with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing payments.</returns>
    public async Task<PolarResult<PaginatedResponse<Payment>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/payments?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Payment>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a payment by ID.
    /// </summary>
    /// <param name="paymentId">The payment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payment, or null if not found.</returns>
    public async Task<PolarResult<Payment>> GetAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/payments/{paymentId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Payment>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all payments across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all payments.</returns>
    public async IAsyncEnumerable<PolarResult<Payment>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Payment>.Failure(result.Error!);
                yield break;
            }

            foreach (var payment in result.Value!.Items)
            {
                yield return PolarResult<Payment>.Success(payment);
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
    /// Creates a query builder for payments with fluent filtering.
    /// </summary>
    /// <returns>A new PaymentsQueryBuilder instance.</returns>
    public PaymentsQueryBuilder Query() => new();

    /// <summary>
    /// Lists payments using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered payments.</returns>
    public async Task<PolarResult<PaginatedResponse<Payment>>> ListAsync(
        PaymentsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/payments?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Payment>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}