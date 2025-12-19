using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Seats;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing customer seats in the Polar system.
/// </summary>
public class CustomerSeatsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CustomerSeatsApi(
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
    /// Lists all customer seats with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing customer seats.</returns>
    public async Task<PolarResult<PaginatedResponse<CustomerSeat>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/customer_seats?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<CustomerSeat>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets a customer seat by ID.
    /// </summary>
    /// <param name="customerSeatId">The customer seat ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer seat, or null if not found.</returns>
    public async Task<PolarResult<CustomerSeat>> GetAsync(
        string customerSeatId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer_seats/{customerSeatId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerSeat>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Assigns a customer seat.
    /// </summary>
    /// <param name="request">The seat assignment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> AssignAsync(
        CustomerSeatAssignRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/assign", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Revokes a customer seat.
    /// </summary>
    /// <param name="request">The seat revoke request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> RevokeAsync(
        SeatRevokeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/revoke", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Resends a customer seat invitation.
    /// </summary>
    /// <param name="request">The seat invitation resend request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> ResendInvitationAsync(
        SeatResendInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/resend_invitation", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets seat claim information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The seat claim information, or null if not found.</returns>
    public async Task<PolarResult<SeatClaimInfo>> GetClaimInfoAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/customer_seats/claim_info", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<SeatClaimInfo>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Claims a seat.
    /// </summary>
    /// <param name="request">The seat claim request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> ClaimAsync(
        SeatClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/claim", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all customer seats across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all customer seats.</returns>
    public async IAsyncEnumerable<PolarResult<CustomerSeat>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<CustomerSeat>.Failure(result.Error!);
                yield break;
            }

            foreach (var customerSeat in result.Value!.Items)
            {
                yield return PolarResult<CustomerSeat>.Success(customerSeat);
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
    /// Creates a query builder for customer seats with fluent filtering.
    /// </summary>
    /// <returns>A new CustomerSeatsQueryBuilder instance.</returns>
    public CustomerSeatsQueryBuilder Query() => new();

    /// <summary>
    /// Lists customer seats using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered customer seats.</returns>
    public async Task<PolarResult<PaginatedResponse<CustomerSeat>>> ListAsync(
        CustomerSeatsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/customer_seats?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<CustomerSeat>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}