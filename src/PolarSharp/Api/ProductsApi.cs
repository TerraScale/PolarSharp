using System.Collections.Generic;
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
using PolarSharp.Models.Products;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing products in the Polar system.
/// </summary>
public class ProductsApi
{
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
    public async Task<PolarResult<PaginatedResponse<Product>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/products/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Product>>(_jsonOptions, cancellationToken);
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
    public async Task<PolarResult<PaginatedResponse<Product>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/products/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Product>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product, or null if not found.</returns>
    public async Task<PolarResult<Product>> GetAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/products/{productId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Product>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created product.</returns>
    public async Task<PolarResult<Product>> CreateAsync(
        ProductCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate request before sending
        request.ValidateAndThrow(nameof(request));

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/products/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Product>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">The product update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated product, or null if not found.</returns>
    public async Task<PolarResult<Product>> UpdateAsync(
        string productId,
        ProductUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/products/{productId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Product>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Archives a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The archived product, or null if not found.</returns>
    public async Task<PolarResult<Product>> ArchiveAsync(
        string productId,
        CancellationToken cancellationToken = default)
    {
        var archiveRequest = new { is_archived = true };
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/products/{productId}", archiveRequest, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Product>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new price for a product.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="request">The price creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created price, or null if the product was not found.</returns>
    public async Task<PolarResult<ProductPrice>> CreatePriceAsync(
        string productId,
        ProductPriceCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/products/{productId}/prices/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<ProductPrice>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all products across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all products.</returns>
    public async IAsyncEnumerable<PolarResult<Product>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Product>.Failure(result.Error!);
                yield break;
            }

            foreach (var product in result.Value!.Items)
            {
                yield return PolarResult<Product>.Success(product);
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
    /// Executes an operation with streaming response for better memory efficiency.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    private async Task<PolarResult<T>> ExecuteWithStreamingAsync<T>(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        var response = await ExecuteWithPoliciesAsync(operation, cancellationToken);
        return await response.ToPolarResultAsync<T>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Exports products to a file.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<PolarResult<ProductExportResponse>> ExportAsync(
        ExportFormat format = ExportFormat.Csv,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/products/export/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<ProductExportResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Exports product prices to a file.
    /// </summary>
    /// <param name="productId">The product ID.</param>
    /// <param name="format">The export format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<PolarResult<ProductPriceExportResponse>> ExportPricesAsync(
        string productId,
        ExportFormat format = ExportFormat.Csv,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/products/{productId}/prices/export/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<ProductPriceExportResponse>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}