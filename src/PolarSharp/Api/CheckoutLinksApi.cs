using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Checkouts;
using PolarSharp.Models.Common;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing checkout links in the Polar system.
/// </summary>
public class CheckoutLinksApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CheckoutLinksApi(
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
    /// Lists all checkout links with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="enabled">Filter by enabled status.</param>
    /// <param name="archived">Filter by archived status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing checkout links.</returns>
    public async Task<PaginatedResponse<CheckoutLink>> ListAsync(
        int page = 1,
        int limit = 10,
        string? productId = null,
        bool? enabled = null,
        bool? archived = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (!string.IsNullOrEmpty(productId))
            queryParams["product_id"] = productId;
        
        if (enabled.HasValue)
            queryParams["enabled"] = enabled.Value.ToString().ToLowerInvariant();
        
        if (archived.HasValue)
            queryParams["archived"] = archived.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/checkout_links?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<CheckoutLink>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a query builder for checkout links with fluent filtering.
    /// </summary>
    /// <returns>A new CheckoutLinksQueryBuilder instance.</returns>
    public CheckoutLinksQueryBuilder Query() => new();

    /// <summary>
    /// Lists checkout links using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered checkout links.</returns>
    public async Task<PaginatedResponse<CheckoutLink>> ListAsync(
        CheckoutLinksQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/checkout_links?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<CheckoutLink>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a checkout link by ID.
    /// </summary>
    /// <param name="checkoutLinkId">The checkout link ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkout link.</returns>
    public async Task<CheckoutLink> GetAsync(
        string checkoutLinkId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/checkout_links/{checkoutLinkId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CheckoutLink>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new checkout link.
    /// </summary>
    /// <param name="request">The checkout link creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created checkout link.</returns>
    public async Task<CheckoutLink> CreateAsync(
        CheckoutLinkCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/checkout_links", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CheckoutLink>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing checkout link.
    /// </summary>
    /// <param name="checkoutLinkId">The checkout link ID.</param>
    /// <param name="request">The checkout link update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated checkout link.</returns>
    public async Task<CheckoutLink> UpdateAsync(
        string checkoutLinkId,
        CheckoutLinkUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/checkout_links/{checkoutLinkId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CheckoutLink>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a checkout link.
    /// </summary>
    /// <param name="checkoutLinkId">The checkout link ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted checkout link.</returns>
    public async Task<CheckoutLink> DeleteAsync(
        string checkoutLinkId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/checkout_links/{checkoutLinkId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CheckoutLink>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all checkout links across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="enabled">Filter by enabled status.</param>
    /// <param name="archived">Filter by archived status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all checkout links.</returns>
    public async IAsyncEnumerable<CheckoutLink> ListAllAsync(
        string? productId = null,
        bool? enabled = null,
        bool? archived = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, productId, enabled, archived, cancellationToken);
            
            foreach (var checkoutLink in response.Items)
            {
                yield return checkoutLink;
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

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}