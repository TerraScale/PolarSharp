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
    public async Task<PaginatedResponse<Subscription>> ListAsync(
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

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Subscription>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a subscription by ID.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription.</returns>
    public async Task<Subscription> GetAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/subscriptions/{subscriptionId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Subscription>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new subscription.
    /// </summary>
    /// <param name="request">The subscription creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created subscription.</returns>
    public async Task<Subscription> CreateAsync(
        SubscriptionCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("subscriptions", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Subscription>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="request">The subscription update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated subscription.</returns>
    public async Task<Subscription> UpdateAsync(
        string subscriptionId,
        SubscriptionUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/subscriptions/{subscriptionId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Subscription>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Revokes (cancels) a subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revoked subscription.</returns>
    public async Task<Subscription> RevokeAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/subscriptions/{subscriptionId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Subscription>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Exports subscriptions to a file.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<SubscriptionExportResponse> ExportAsync(
        SubscriptionExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("subscriptions/export", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<SubscriptionExportResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all subscriptions across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by subscription status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all subscriptions.</returns>
    public async IAsyncEnumerable<Subscription> ListAllAsync(
        string? customerId = null,
        string? productId = null,
        SubscriptionStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, customerId, productId, status, cancellationToken);
            
            foreach (var subscription in response.Items)
            {
                yield return subscription;
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
    public async Task<PaginatedResponse<Subscription>> ListAsync(
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

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Subscription>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
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