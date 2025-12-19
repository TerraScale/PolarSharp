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
/// API client for managing seats in the Polar system.
/// </summary>
public class SeatsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal SeatsApi(
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
    /// Lists all seats with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing seats.</returns>
    public async Task<PolarResult<PaginatedResponse<Seat>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/seats?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Seat>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Assigns a seat to a user.
    /// </summary>
    /// <param name="request">The seat assignment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> AssignAsync(
        SubscriptionSeatAssignRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/seats/assign", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Revokes a seat.
    /// </summary>
    /// <param name="request">The seat revoke request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> RevokeAsync(
        SeatRevokeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/seats/revoke", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Resends a seat invitation.
    /// </summary>
    /// <param name="request">The seat invitation resend request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> ResendInvitationAsync(
        SeatResendInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/seats/resend_invitation", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists claimed subscriptions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of claimed subscriptions.</returns>
    public async Task<PolarResult<List<ClaimedSubscription>>> ListClaimedSubscriptionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/seats/claimed_subscriptions", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<List<ClaimedSubscription>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all seats across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all seats.</returns>
    public async IAsyncEnumerable<PolarResult<Seat>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Seat>.Failure(result.Error!);
                yield break;
            }

            foreach (var seat in result.Value!.Items)
            {
                yield return PolarResult<Seat>.Success(seat);
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
    /// Creates a query builder for seats with fluent filtering.
    /// </summary>
    /// <returns>A new SeatsQueryBuilder instance.</returns>
    public SeatsQueryBuilder Query() => new();

    /// <summary>
    /// Lists seats using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered seats.</returns>
    public async Task<PolarResult<PaginatedResponse<Seat>>> ListAsync(
        SeatsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/seats?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Seat>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}