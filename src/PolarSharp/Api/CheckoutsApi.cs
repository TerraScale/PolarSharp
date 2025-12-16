using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Checkouts;
using PolarSharp.Models.Common;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing checkout sessions in the Polar system.
/// </summary>
public class CheckoutsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CheckoutsApi(
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
    /// Lists all checkout sessions with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by checkout status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing checkout sessions.</returns>
    public async Task<PaginatedResponse<Checkout>> ListAsync(
        int page = 1,
        int limit = 10,
        string? customerId = null,
        string? productId = null,
        CheckoutStatus? status = null,
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
            () => _httpClient.GetAsync($"v1/checkouts/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        var result = await response.HandleErrorsAsync<PaginatedResponse<Checkout>>(_jsonOptions, cancellationToken);
        var (value, error) = result.EnsureSuccess();
        if (error != null)
            throw error.ToPolarApiException();
        
        return value ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a query builder for checkouts with fluent filtering.
    /// </summary>
    /// <returns>A new CheckoutsQueryBuilder instance.</returns>
    public CheckoutsQueryBuilder Query() => new();

    /// <summary>
    /// Lists checkouts using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered checkouts.</returns>
    public async Task<PaginatedResponse<Checkout>> ListAsync(
        CheckoutsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/checkouts/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        var result = await response.HandleErrorsAsync<PaginatedResponse<Checkout>>(_jsonOptions, cancellationToken);
        var (value, error) = result.EnsureSuccess();
        if (error != null)
            throw error.ToPolarApiException();
        
        return value ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a checkout session by ID.
    /// </summary>
    /// <param name="checkoutId">The checkout session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkout session, or null if not found.</returns>
    public async Task<Checkout?> GetAsync(
        string checkoutId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/checkouts/{checkoutId}/", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Checkout>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new checkout session.
    /// </summary>
    /// <param name="request">The checkout creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created checkout session.</returns>
    public async Task<Checkout> CreateAsync(
        CheckoutCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/checkouts/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        var result = await response.HandleErrorsAsync<Checkout>(_jsonOptions, cancellationToken);
        var (value, error) = result.EnsureSuccess();
        if (error != null)
            throw error.ToPolarApiException();
        
        return value ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing checkout session.
    /// </summary>
    /// <param name="checkoutId">The checkout session ID.</param>
    /// <param name="request">The checkout update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated checkout session, or null if not found.</returns>
    public async Task<Checkout?> UpdateAsync(
        string checkoutId,
        CheckoutUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/checkouts/{checkoutId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Checkout>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a checkout session from client side.
    /// </summary>
    /// <param name="checkoutId">The checkout session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The checkout session, or null if not found.</returns>
    public async Task<Checkout?> GetFromClientAsync(
        string checkoutId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/checkouts/client/{checkoutId}", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Checkout>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates a checkout session from client side.
    /// </summary>
    /// <param name="checkoutId">The checkout session ID.</param>
    /// <param name="request">The checkout update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated checkout session, or null if not found.</returns>
    public async Task<Checkout?> UpdateFromClientAsync(
        string checkoutId,
        CheckoutUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/checkouts/client/{checkoutId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Checkout>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Confirms a checkout session from client side.
    /// </summary>
    /// <param name="checkoutId">The checkout session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The confirmed checkout session, or null if not found.</returns>
    public async Task<Checkout?> ConfirmFromClientAsync(
        string checkoutId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsync($"v1/checkouts/client/{checkoutId}/confirm", null, cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Checkout>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a checkout session.
    /// </summary>
    /// <param name="checkoutId">The checkout session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted checkout session, or null if not found.</returns>
    public async Task<Checkout?> DeleteAsync(
        string checkoutId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/checkouts/{checkoutId}", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Checkout>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all checkout sessions across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by checkout status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all checkout sessions.</returns>
    public async IAsyncEnumerable<Checkout> ListAllAsync(
        string? customerId = null,
        string? productId = null,
        CheckoutStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, customerId, productId, status, cancellationToken);
            
            foreach (var checkout in response.Items)
            {
                yield return checkout;
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