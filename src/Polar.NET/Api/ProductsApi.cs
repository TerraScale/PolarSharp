using System.Collections.Generic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using Polar.NET.Extensions;
using Polar.NET.Exceptions;
using Polar.NET.Models.Common;
using Polar.NET.Models.Products;

namespace Polar.NET.Api;

/// <summary>
/// API client for managing products in the Polar system.
/// </summary>
public class ProductsApi
{
    private const string ApiVersion = "v1";
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal ProductsApi(
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
    /// Lists all products with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing products.</returns>
    public async Task<PaginatedResponse<Product>> ListAsync(
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
            () => _httpClient.GetAsync($"products?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Product>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a query builder for products with fluent filtering.
    /// </summary>
    /// <returns>A new ProductsQueryBuilder instance.</returns>
    public ProductsQueryBuilder Query() => new();

    /// <summary>
    /// Lists products using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered products.</returns>
    public async Task<PaginatedResponse<Product>> ListAsync(
        ProductsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"products?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Product>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product.</returns>
    public async Task<Product> GetAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"products?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<Product>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created product.</returns>
    public async Task<Product> CreateAsync(
        ProductCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("products", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<Product>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">The product update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated product.</returns>
    public async Task<Product> UpdateAsync(
        string productId,
        ProductUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"products/{productId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Product>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Archives a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The archived product.</returns>
    public async Task<Product> ArchiveAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"products/{productId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Product>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new price for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">The price creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created price.</returns>
    public async Task<ProductPrice> CreatePriceAsync(
        string productId,
        ProductPriceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"products/{productId}/prices", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProductPrice>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all products across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all products.</returns>
    public async IAsyncEnumerable<Product> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, cancellationToken);
            
            foreach (var product in response.Items)
            {
                yield return product;
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
        return await _rateLimitPolicy.ExecuteAsync(async () =>
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await operation();
            });
        });
    }

    /// <summary>
    /// Executes an operation with streaming response for better memory efficiency.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    private async Task<T> ExecuteWithStreamingAsync<T>(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        var response = await ExecuteWithPoliciesAsync(operation, cancellationToken);
        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Exports products to a file.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<ProductExportResponse> ExportAsync(
        ExportFormat format = ExportFormat.Csv,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"products/export?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProductExportResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Exports product prices to a file.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="format">The export format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<ProductPriceExportResponse> ExportPricesAsync(
        string productId,
        ExportFormat format = ExportFormat.Csv,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"products/{productId}/prices/export?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ProductPriceExportResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}