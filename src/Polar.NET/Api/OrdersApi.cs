using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using Polar.NET.Extensions;
using Polar.NET.Exceptions;
using Polar.NET.Models.Common;
using Polar.NET.Models.Orders;

namespace Polar.NET.Api;

/// <summary>
/// API client for managing orders in the Polar system.
/// </summary>
public class OrdersApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal OrdersApi(
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
    /// Lists all orders with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by order status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing orders.</returns>
    public async Task<PaginatedResponse<Order>> ListAsync(
        int page = 1,
        int limit = 10,
        string? customerId = null,
        string? productId = null,
        OrderStatus? status = null,
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
            () => _httpClient.GetAsync($"v1/orders?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Order>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets an order by ID.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order.</returns>
    public async Task<Order> GetAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/orders/{orderId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Order>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">The order creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created order.</returns>
    public async Task<Order> CreateAsync(
        OrderCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("orders", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Order>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="request">The order update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated order.</returns>
    public async Task<Order> UpdateAsync(
        string orderId,
        OrderUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/orders/{orderId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Order>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes an order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted order.</returns>
    public async Task<Order> DeleteAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/orders/{orderId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Order>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Generates an invoice for an order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated invoice.</returns>
    public async Task<OrderInvoice> GenerateInvoiceAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsync($"v1/orders/{orderId}/generate_invoice", null, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OrderInvoice>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets the invoice for an order.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order invoice.</returns>
    public async Task<OrderInvoice> GetInvoiceAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/orders/{orderId}/invoice", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OrderInvoice>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Exports orders to a file.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by order status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<OrderExportResponse> ExportAsync(
        ExportFormat format = ExportFormat.Csv,
        string? customerId = null,
        string? productId = null,
        OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        if (!string.IsNullOrEmpty(customerId))
            queryParams["customer_id"] = customerId;
        
        if (!string.IsNullOrEmpty(productId))
            queryParams["product_id"] = productId;
        
        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/orders/export?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<OrderExportResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all orders across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="status">Filter by order status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all orders.</returns>
    public async IAsyncEnumerable<Order> ListAllAsync(
        string? customerId = null,
        string? productId = null,
        OrderStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, customerId, productId, status, cancellationToken);
            
            foreach (var order in response.Items)
            {
                yield return order;
            }

            if (page >= response.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Creates a query builder for orders with fluent filtering.
    /// </summary>
    /// <returns>A new OrdersQueryBuilder instance.</returns>
    public OrdersQueryBuilder Query() => new();

    /// <summary>
    /// Lists orders using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered orders.</returns>
    public async Task<PaginatedResponse<Order>> ListAsync(
        OrdersQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/orders?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Order>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    private async Task<HttpResponseMessage> ExecuteWithPoliciesAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        return await _rateLimitPolicy.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await operation();
            });
        });
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}

/// <summary>
/// Response for order export.
/// </summary>
public record OrderExportResponse
{
    /// <summary>
    /// The export URL.
    /// </summary>
    public string ExportUrl { get; init; } = string.Empty;

    /// <summary>
    /// The export file size in bytes.
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// The number of records in the export.
    /// </summary>
    public int RecordCount { get; init; }
}

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// CSV format.
    /// </summary>
    Csv,

    /// <summary>
    /// JSON format.
    /// </summary>
    Json,

    /// <summary>
    /// Excel format.
    /// </summary>
    Excel
}