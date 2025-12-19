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
using PolarSharp.Models.LicenseKeys;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing license keys in the Polar system.
/// </summary>
public class LicenseKeysApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal LicenseKeysApi(
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
    /// Lists all license keys with optional pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="benefitId">Filter by benefit ID.</param>
    /// <param name="status">Filter by license key status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing license keys.</returns>
    public async Task<PolarResult<PaginatedResponse<LicenseKey>>> ListAsync(
        int page = 1,
        int limit = 10,
        string? customerId = null,
        string? benefitId = null,
        LicenseKeyStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (!string.IsNullOrEmpty(customerId))
            queryParams["customer_id"] = customerId;

        if (!string.IsNullOrEmpty(benefitId))
            queryParams["benefit_id"] = benefitId;

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/license-keys/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<LicenseKey>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a license key by ID.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The license key, or null if not found.</returns>
    public async Task<PolarResult<LicenseKey?>> GetAsync(
        string licenseKeyId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/license-keys/{licenseKeyId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<LicenseKey>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing license key.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="request">The license key update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated license key, or null if not found.</returns>
    public async Task<PolarResult<LicenseKey?>> UpdateAsync(
        string licenseKeyId,
        LicenseKeyUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/license-keys/{licenseKeyId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<LicenseKey>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets the activation information for a license key.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The activation information, or null if not found.</returns>
    public async Task<PolarResult<LicenseKeyActivation?>> GetActivationAsync(
        string licenseKeyId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/license-keys/{licenseKeyId}/activation", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<LicenseKeyActivation>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Validates a license key.
    /// </summary>
    /// <param name="request">The license key validation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation response.</returns>
    public async Task<PolarResult<LicenseKeyValidateResponse>> ValidateAsync(
        LicenseKeyValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("license-keys/validate", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<LicenseKeyValidateResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Activates a license key.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="request">The license key activation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The activation response.</returns>
    public async Task<PolarResult<LicenseKeyActivateResponse>> ActivateAsync(
        string licenseKeyId,
        LicenseKeyActivateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/license-keys/{licenseKeyId}/activate", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<LicenseKeyActivateResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deactivates a license key.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="request">The license key deactivation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deactivation response.</returns>
    public async Task<PolarResult<LicenseKeyDeactivateResponse>> DeactivateAsync(
        string licenseKeyId,
        LicenseKeyDeactivateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/license-keys/{licenseKeyId}/deactivate", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<LicenseKeyDeactivateResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all license keys across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="benefitId">Filter by benefit ID.</param>
    /// <param name="status">Filter by license key status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all license keys.</returns>
    public async IAsyncEnumerable<PolarResult<LicenseKey>> ListAllAsync(
        string? customerId = null,
        string? benefitId = null,
        LicenseKeyStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, customerId, benefitId, status, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<LicenseKey>.Failure(result.Error!);
                yield break;
            }

            foreach (var licenseKey in result.Value.Items)
            {
                yield return PolarResult<LicenseKey>.Success(licenseKey);
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
    /// Creates a query builder for license keys with fluent filtering.
    /// </summary>
    /// <returns>A new LicenseKeysQueryBuilder instance.</returns>
    public LicenseKeysQueryBuilder Query() => new();

    /// <summary>
    /// Lists license keys using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered license keys.</returns>
    public async Task<PolarResult<PaginatedResponse<LicenseKey>>> ListAsync(
        LicenseKeysQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/license-keys/?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<LicenseKey>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}