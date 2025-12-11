using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Exceptions;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Discounts;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing discounts in Polar system.
/// </summary>
public class DiscountsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal DiscountsApi(
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
    /// Lists all discounts with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing discounts.</returns>
    public async Task<PaginatedResponse<Discount>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/discounts?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Discount>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a discount by ID.
    /// </summary>
    /// <param name="discountId">The discount ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The discount.</returns>
    public async Task<Discount> GetAsync(
        string discountId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/discounts/{discountId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Discount>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new discount.
    /// </summary>
    /// <param name="request">The discount creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created discount.</returns>
    public async Task<Discount> CreateAsync(
        DiscountCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/discounts", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Discount>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing discount.
    /// </summary>
    /// <param name="discountId">The discount ID.</param>
    /// <param name="request">The discount update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated discount.</returns>
    public async Task<Discount> UpdateAsync(
        string discountId,
        DiscountUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/discounts/{discountId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Discount>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a discount.
    /// </summary>
    /// <param name="discountId">The discount ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted discount.</returns>
    public async Task<Discount> DeleteAsync(
        string discountId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/discounts/{discountId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Discount>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all discounts across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all discounts.</returns>
    public async IAsyncEnumerable<Discount> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, cancellationToken);
            
            foreach (var discount in response.Items)
            {
                yield return discount;
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
    /// Creates a query builder for discounts with fluent filtering.
    /// </summary>
    /// <returns>A new DiscountsQueryBuilder instance.</returns>
    public DiscountsQueryBuilder Query() => new();

    /// <summary>
    /// Lists discounts using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered discounts.</returns>
    public async Task<PaginatedResponse<Discount>> ListAsync(
        DiscountsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/discounts?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Discount>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}