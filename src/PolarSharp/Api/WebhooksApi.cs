using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Exceptions;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Webhooks;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing webhooks in Polar system.
/// </summary>
public class WebhooksApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal WebhooksApi(
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
    /// Lists all webhook endpoints with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing webhook endpoints.</returns>
    public async Task<PaginatedResponse<WebhookEndpoint>> ListEndpointsAsync(
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
            () => _httpClient.GetAsync($"v1/webhooks/endpoints?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<WebhookEndpoint>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a webhook endpoint by ID.
    /// </summary>
    /// <param name="endpointId">The webhook endpoint ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The webhook endpoint.</returns>
    public async Task<WebhookEndpoint> GetEndpointAsync(
        string endpointId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/webhooks/endpoints/{endpointId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WebhookEndpoint>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new webhook endpoint.
    /// </summary>
    /// <param name="request">The webhook endpoint creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created webhook endpoint.</returns>
    public async Task<WebhookEndpoint> CreateEndpointAsync(
        WebhookEndpointCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/webhooks/endpoints", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WebhookEndpoint>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing webhook endpoint.
    /// </summary>
    /// <param name="endpointId">The webhook endpoint ID.</param>
    /// <param name="request">The webhook endpoint update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated webhook endpoint.</returns>
    public async Task<WebhookEndpoint> UpdateEndpointAsync(
        string endpointId,
        WebhookEndpointUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/webhooks/endpoints/{endpointId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WebhookEndpoint>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a webhook endpoint.
    /// </summary>
    /// <param name="endpointId">The webhook endpoint ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted webhook endpoint.</returns>
    public async Task<WebhookEndpoint> DeleteEndpointAsync(
        string endpointId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/webhooks/endpoints/{endpointId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WebhookEndpoint>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Resets the secret for a webhook endpoint.
    /// </summary>
    /// <param name="endpointId">The webhook endpoint ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated webhook endpoint with new secret.</returns>
    public async Task<WebhookEndpoint> ResetEndpointSecretAsync(
        string endpointId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsync($"v1/webhooks/endpoints/{endpointId}/reset_secret", null, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WebhookEndpoint>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all webhook deliveries with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing webhook deliveries.</returns>
    public async Task<PaginatedResponse<WebhookDelivery>> ListDeliveriesAsync(
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
            () => _httpClient.GetAsync($"v1/webhooks/deliveries?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<WebhookDelivery>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Redelivers a webhook event.
    /// </summary>
    /// <param name="deliveryId">The webhook delivery ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The redelivery result.</returns>
    public async Task<WebhookDelivery> RedeliverAsync(
        string deliveryId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsync($"v1/webhooks/deliveries/redeliver", null, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<WebhookDelivery>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all webhook endpoints across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all webhook endpoints.</returns>
    public async IAsyncEnumerable<WebhookEndpoint> ListAllEndpointsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListEndpointsAsync(page, limit, cancellationToken);
            
            foreach (var endpoint in response.Items)
            {
                yield return endpoint;
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
    /// Creates a query builder for webhooks with fluent filtering.
    /// </summary>
    /// <returns>A new WebhooksQueryBuilder instance.</returns>
    public WebhooksQueryBuilder Query() => new();

    /// <summary>
    /// Lists webhook endpoints using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered webhook endpoints.</returns>
    public async Task<PaginatedResponse<WebhookEndpoint>> ListAsync(
        WebhooksQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/webhooks/endpoints?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<WebhookEndpoint>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}