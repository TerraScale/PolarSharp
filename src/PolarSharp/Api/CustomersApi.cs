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
using PolarSharp.Models.Customers;
using PolarSharp.Results;

namespace PolarSharp.Api;

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
    public async Task<PolarResult<PaginatedResponse<Customer>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/customers/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Customer>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a customer by ID.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer, or null if not found.</returns>
    public async Task<PolarResult<Customer>> GetAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/{customerId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer, or null if not found.</returns>
    public async Task<PolarResult<Customer>> GetByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/external/{externalId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">The customer creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer.</returns>
    public async Task<PolarResult<Customer>> CreateAsync(
        CustomerCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate request before sending
        request.ValidateAndThrow(nameof(request));

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customers/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="request">The customer update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated customer, or null if not found.</returns>
    public async Task<PolarResult<Customer>> UpdateAsync(
        string customerId,
        CustomerUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/customers/{customerId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="request">The customer update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated customer, or null if not found.</returns>
    public async Task<PolarResult<Customer>> UpdateByExternalIdAsync(
        string externalId,
        CustomerUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/customers/external/{externalId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a customer by ID.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, or null if not found.</returns>
    public async Task<PolarResult> DeleteAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/customers/{customerId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted customer, or null if not found.</returns>
    public async Task<PolarResult<Customer>> DeleteByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/customers/external/{externalId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets the state of a customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer state, or null if not found.</returns>
    public async Task<PolarResult<CustomerState>> GetStateAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/{customerId}/state", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerState>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets the state of a customer by external ID.
    /// </summary>
    /// <param name="externalId">The external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer state, or null if not found.</returns>
    public async Task<PolarResult<CustomerState>> GetStateByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/external/{externalId}/state", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerState>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets the balance of a customer.
    /// </summary>
    /// <param name="customerId">The customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer balance, or null if not found.</returns>
    public async Task<PolarResult<CustomerBalance>> GetBalanceAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customers/{customerId}/balance", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerBalance>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Exports customers to a file.
    /// </summary>
    /// <param name="request">The export request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<PolarResult<CustomerExportResponse>> ExportAsync(
        CustomerExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customers/export/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerExportResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all customers across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="email">Filter by email address.</param>
    /// <param name="externalId">Filter by external ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all customers.</returns>
    public async IAsyncEnumerable<PolarResult<Customer>> ListAllAsync(
        string? email = null,
        string? externalId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, email, externalId, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Customer>.Failure(result.Error!);
                yield break;
            }

            foreach (var customer in result.Value!.Items)
            {
                yield return PolarResult<Customer>.Success(customer);
            }

            if (page >= result.Value!.Pagination.MaxPage)
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
    public async Task<PolarResult<PaginatedResponse<Customer>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/customers/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Customer>>(_jsonOptions, cancellationToken);
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