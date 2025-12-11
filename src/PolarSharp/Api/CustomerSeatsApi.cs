using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Exceptions;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Seats;

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
    public async Task<PaginatedResponse<CustomerSeat>> ListAsync(
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

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<CustomerSeat>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets a customer seat by ID.
    /// </summary>
    /// <param name="customerSeatId">The customer seat ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer seat.</returns>
    public async Task<CustomerSeat> GetAsync(
        string customerSeatId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer_seats/{customerSeatId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<CustomerSeat>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Assigns a customer seat.
    /// </summary>
    /// <param name="request">The seat assignment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
public async Task AssignAsync(
        CustomerSeatAssignRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/assign", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();
    }

    /// <summary>
    /// Revokes a customer seat.
    /// </summary>
    /// <param name="request">The seat revoke request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task RevokeAsync(
        SeatRevokeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/revoke", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();
    }

    /// <summary>
    /// Resends a customer seat invitation.
    /// </summary>
    /// <param name="request">The seat invitation resend request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task ResendInvitationAsync(
        SeatResendInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/resend_invitation", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();
    }

    /// <summary>
    /// Gets seat claim information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The seat claim information.</returns>
    public async Task<SeatClaimInfo> GetClaimInfoAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/customer_seats/claim_info", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<SeatClaimInfo>(stream, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Claims a seat.
    /// </summary>
    /// <param name="request">The seat claim request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task ClaimAsync(
        SeatClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer_seats/claim", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();
    }

    /// <summary>
    /// Lists all customer seats across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all customer seats.</returns>
    public async IAsyncEnumerable<CustomerSeat> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListAsync(page, limit, cancellationToken);
            
            foreach (var customerSeat in response.Items)
            {
                yield return customerSeat;
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
    public async Task<PaginatedResponse<CustomerSeat>> ListAsync(
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

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<CustomerSeat>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}