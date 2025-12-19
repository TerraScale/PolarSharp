using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Organizations;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing organizations in Polar system.
/// </summary>
public class OrganizationsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal OrganizationsApi(
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
    /// Lists all organizations with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing organizations.</returns>
    public async Task<PolarResult<PaginatedResponse<Organization>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/organizations?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Organization>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets an organization by ID.
    /// </summary>
    /// <param name="organizationId">The organization ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organization, or null if not found.</returns>
    public async Task<PolarResult<Organization>> GetAsync(
        string organizationId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/organizations/{organizationId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Organization>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    /// <param name="request">The organization creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created organization.</returns>
    public async Task<PolarResult<Organization>> CreateAsync(
        OrganizationCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/organizations", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Organization>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing organization.
    /// </summary>
    /// <param name="organizationId">The organization ID.</param>
    /// <param name="request">The organization update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated organization, or null if not found.</returns>
    public async Task<PolarResult<Organization>> UpdateAsync(
        string organizationId,
        OrganizationUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/organizations/{organizationId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Organization>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes an organization.
    /// </summary>
    /// <param name="organizationId">The organization ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted organization.</returns>
    public async Task<PolarResult<Organization>> DeleteAsync(
        string organizationId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/organizations/{organizationId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<Organization>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all organizations across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all organizations.</returns>
    public async IAsyncEnumerable<PolarResult<Organization>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Organization>.Failure(result.Error!);
                yield break;
            }

            foreach (var organization in result.Value!.Items)
            {
                yield return PolarResult<Organization>.Success(organization);
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

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}