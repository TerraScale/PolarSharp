using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using Polar.NET.Extensions;
using Polar.NET.Models.Common;
using Polar.NET.Models.Customers;

namespace Polar.NET.Api;

/// <summary>
/// API client for managing customers in the Polar system.
/// </summary>
public class CustomersApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CustomersApi(
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
    /// Lists all customers with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="email">Filter by email address.</param>
    /// <param name="externalId">Filter by external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing customers.</returns>
    public async Task<PaginatedResponse<Customer>> ListAsync(
        int page = 1,
        int limit = 10,
        string? email = null,
        string? externalId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (!string.IsNullOrEmpty(email))
            queryParams["email"] = email;
        
        if (!string.IsNullOrEmpty(externalId))
            queryParams["external_id"] = externalId;

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Customer>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer.</returns>
    public async Task<Customer> GetAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/{customerId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer.</returns>
    public async Task<Customer> GetByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/external/{externalId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">The customer creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer.</returns>
    public async Task<Customer> CreateAsync(
        CustomerCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("customers", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="request">The customer update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated customer.</returns>
    public async Task<Customer> UpdateAsync(
        string customerId,
        CustomerUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/customers/{customerId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Updates a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="request">The customer update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated customer.</returns>
    public async Task<Customer> UpdateByExternalIdAsync(
        string externalId,
        CustomerUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/customers/external/{externalId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted customer.</returns>
    public async Task<Customer> DeleteAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/customers/{customerId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted customer.</returns>
    public async Task<Customer> DeleteByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/customers/external/{externalId}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets the state of a customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer state.</returns>
    public async Task<CustomerState> GetStateAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/{customerId}/state", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomerState>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets the state of a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer state.</returns>
    public async Task<CustomerState> GetStateByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/external/{externalId}/state", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomerState>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets the balance of a customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer balance.</returns>
    public async Task<CustomerBalance> GetBalanceAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/{customerId}/balance", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomerBalance>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Exports customers to a file.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<CustomerExportResponse> ExportAsync(
        CustomerExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("customers/export", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomerExportResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all customers across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="email">Filter by email address.</param>
    /// <param name="externalId">Filter by external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all customers.</returns>
    public async IAsyncEnumerable<Customer> ListAllAsync(
        string? email = null,
        string? externalId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, email, externalId, cancellationToken);
            
            foreach (var customer in response.Items)
            {
                yield return customer;
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
    /// Creates a query builder for customers with fluent filtering.
    /// </summary>
    /// <returns>A new CustomersQueryBuilder instance.</returns>
    public CustomersQueryBuilder Query() => new();

    /// <summary>
    /// Lists customers using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered customers.</returns>
    public async Task<PaginatedResponse<Customer>> ListAsync(
        CustomersQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/customers?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Customer>>(content, _jsonOptions)
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
/// Request to export customers.
/// </summary>
public record CustomerExportRequest
{
    /// <summary>
    /// The format of export.
    /// </summary>
    public ExportFormat Format { get; init; } = ExportFormat.Csv;

    /// <summary>
    /// Filter by email address.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Filter by external ID.
    /// </summary>
    public string? ExternalId { get; init; }

    /// <summary>
    /// Filter by start date.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Filter by end date.
    /// </summary>
    public DateTime? EndDate { get; init; }
}

/// <summary>
/// Response for customer export.
/// </summary>
public record CustomerExportResponse
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