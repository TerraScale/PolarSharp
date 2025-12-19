using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Subscriptions;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing subscriptions in the Polar system.
/// </summary>
public class SubscriptionsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal SubscriptionsApi(
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
    /// Lists all subscriptions with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by subscription status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing subscriptions.</returns>
    public async Task<PolarResult<PaginatedResponse<Subscription>>> ListAsync(
        int page = 1,
        int limit = 10,
        string? customerId = null,
        string? productId = null,
        SubscriptionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (!string.IsNullOrEmpty(customerId))
            queryParams["customer_id"] = customerId;

        if (!string.IsNullOrEmpty(productId))
            queryParams["product_id"] = productId;

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/subscriptions/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Subscription>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a subscription by ID.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription, or null if not found.</returns>
    public async Task<PolarResult<Subscription>> GetAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/subscriptions/{subscriptionId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Subscription>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new subscription.
    /// </summary>
    /// <param name="request">The subscription creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created subscription, or null if the operation could not complete (e.g., sandbox limitations).</returns>
    public async Task<PolarResult<Subscription>> CreateAsync(
        SubscriptionCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/subscriptions/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Subscription>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="request">The subscription update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated subscription, or null if not found.</returns>
    public async Task<PolarResult<Subscription>> UpdateAsync(
        string subscriptionId,
        SubscriptionUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/subscriptions/{subscriptionId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Subscription>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Revokes (cancels) a subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revoked subscription, or null if not found.</returns>
    public async Task<PolarResult<Subscription>> RevokeAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/subscriptions/{subscriptionId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Subscription>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Exports subscriptions to a file.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response, or null if the operation could not complete.</returns>
    public async Task<PolarResult<SubscriptionExportResponse>> ExportAsync(
        SubscriptionExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/subscriptions/export", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<SubscriptionExportResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all subscriptions across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by subscription status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all subscriptions.</returns>
    public async IAsyncEnumerable<PolarResult<Subscription>> ListAllAsync(
        string? customerId = null,
        string? productId = null,
        SubscriptionStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, customerId, productId, status, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Subscription>.Failure(result.Error!);
                yield break;
            }

            foreach (var subscription in result.Value!.Items)
            {
                yield return PolarResult<Subscription>.Success(subscription);
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
    /// Creates a query builder for subscriptions with fluent filtering.
    /// </summary>
    /// <returns>A new SubscriptionsQueryBuilder instance.</returns>
    public SubscriptionsQueryBuilder Query() => new();

    /// <summary>
    /// Lists subscriptions using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered subscriptions.</returns>
    public async Task<PolarResult<PaginatedResponse<Subscription>>> ListAsync(
        SubscriptionsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/subscriptions/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Subscription>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}



/// <summary>
/// Response for subscription export.
/// </summary>
public record SubscriptionExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export ID.
    /// </summary>
    public string ExportId { get; init; } = string.Empty;

    /// <summary>
    /// The format of the export.
    /// </summary>
    public ExportFormat Format { get; init; }

    /// <summary>
    /// The size of the export in bytes.
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// The number of records in the export.
    /// </summary>
    public int RecordCount { get; init; }
}