using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Benefits;
using PolarSharp.Models.Common;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing benefits/ in the Polar system.
/// </summary>
public class BenefitsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal BenefitsApi(
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
    /// Lists all benefits/ with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="type">Filter by benefit type.</param>
    /// <param name="active">Filter by active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing benefits/.</returns>
    public async Task<PolarResult<PaginatedResponse<Benefit>>> ListAsync(
        int page = 1,
        int limit = 10,
        BenefitType? type = null,
        bool? active = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (type.HasValue)
            queryParams["type"] = type.Value.ToString().ToLowerInvariant();

        if (active.HasValue)
            queryParams["active"] = active.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/benefits/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Benefit>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a query builder for benefits/ with fluent filtering.
    /// </summary>
    /// <returns>A new BenefitsQueryBuilder instance.</returns>
    public BenefitsQueryBuilder Query() => new();

    /// <summary>
    /// Lists benefits/ using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered benefits/.</returns>
    public async Task<PolarResult<PaginatedResponse<Benefit>>> ListAsync(
        BenefitsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/benefits/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Benefit>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a benefit by ID.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The benefit, or null if not found.</returns>
    public async Task<PolarResult<Benefit?>> GetAsync(
        string benefitId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/benefits/{benefitId}/", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Benefit>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new benefit.
    /// </summary>
    /// <param name="request">The benefit creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created benefit.</returns>
    public async Task<PolarResult<Benefit>> CreateAsync(
        BenefitCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/benefits/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Benefit>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing benefit.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="request">The benefit update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated benefit, or null if not found.</returns>
    public async Task<PolarResult<Benefit?>> UpdateAsync(
        string benefitId,
        BenefitUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/benefits/{benefitId}/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Benefit>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes a benefit.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted benefit, or null if not found.</returns>
    public async Task<PolarResult<Benefit?>> DeleteAsync(
        string benefitId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/benefits/{benefitId}/", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Benefit>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists benefit grants for a specific benefit.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="status">Filter by grant status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing benefit grants.</returns>
    public async Task<PolarResult<PaginatedResponse<BenefitGrant>>> ListGrantsAsync(
        string benefitId,
        int page = 1,
        int limit = 10,
        string? customerId = null,
        BenefitGrantStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (!string.IsNullOrEmpty(customerId))
            queryParams["customer_id"] = customerId;

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/benefits/{benefitId}/grants/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<BenefitGrant>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all benefits/ across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="type">Filter by benefit type.</param>
    /// <param name="active">Filter by active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all benefits/.</returns>
    public async IAsyncEnumerable<PolarResult<Benefit>> ListAllAsync(
        BenefitType? type = null,
        bool? active = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, type, active, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Benefit>.Failure(result.Error!);
                yield break;
            }

            foreach (var benefit in result.Value.Items)
            {
                yield return PolarResult<Benefit>.Success(benefit);
            }

            if (page >= result.Value.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Lists all benefit grants across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="status">Filter by grant status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all benefit grants.</returns>
    public async IAsyncEnumerable<PolarResult<BenefitGrant>> ListAllGrantsAsync(
        string benefitId,
        string? customerId = null,
        BenefitGrantStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListGrantsAsync(benefitId, page, limit, customerId, status, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<BenefitGrant>.Failure(result.Error!);
                yield break;
            }

            foreach (var grant in result.Value.Items)
            {
                yield return PolarResult<BenefitGrant>.Success(grant);
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
    /// Exports benefits/ to a file.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <param name="type">Filter by benefit type.</param>
    /// <param name="active">Filter by active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<PolarResult<BenefitExportResponse>> ExportAsync(
        ExportFormat format = ExportFormat.Csv,
        BenefitType? type = null,
        bool? active = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        if (type.HasValue)
            queryParams["type"] = type.Value.ToString().ToLowerInvariant();

        if (active.HasValue)
            queryParams["active"] = active.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/benefits/export/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<BenefitExportResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Exports benefit grants to a file.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="format">The export format.</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="status">Filter by grant status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The export response.</returns>
    public async Task<PolarResult<BenefitGrantExportResponse>> ExportGrantsAsync(
        string benefitId,
        ExportFormat format = ExportFormat.Csv,
        string? customerId = null,
        BenefitGrantStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["format"] = format.ToString().ToLowerInvariant()
        };

        if (!string.IsNullOrEmpty(customerId))
            queryParams["customer_id"] = customerId;

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/benefits/{benefitId}/grants/export/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<BenefitGrantExportResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Grants a benefit to a customer.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="request">The benefit grant request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created benefit grant.</returns>
    public async Task<PolarResult<BenefitGrant>> GrantAsync(
        string benefitId,
        BenefitGrantRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/benefits/{benefitId}/grant/", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<BenefitGrant>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Revokes a benefit grant.
    /// </summary>
    /// <param name="benefitId">The benefit ID.</param>
    /// <param name="grantId">The grant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revoked benefit grant.</returns>
    public async Task<PolarResult<BenefitGrant>> RevokeGrantAsync(
        string benefitId,
        string grantId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/benefits/{benefitId}/grants/{grantId}/", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<BenefitGrant>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}