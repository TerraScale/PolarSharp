using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.CustomerSessions;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing customer sessions in the Polar system.
/// </summary>
public class CustomerSessionsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CustomerSessionsApi(
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
    /// Creates a new customer session.
    /// </summary>
    /// <param name="request">The customer session creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer session.</returns>
    public async Task<PolarResult<CustomerSession>> CreateAsync(
        CustomerSessionCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer-sessions", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerSession>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Introspects a customer session to validate the access token.
    /// </summary>
    /// <param name="request">The customer session introspection request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The introspection response.</returns>
    public async Task<PolarResult<CustomerSessionIntrospectResponse>> IntrospectAsync(
        CustomerSessionIntrospectRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer-sessions/introspect", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<CustomerSessionIntrospectResponse>(_jsonOptions, cancellationToken);
    }

    private async Task<HttpResponseMessage> ExecuteWithPoliciesAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        // Rate limiting and retry is now handled by RateLimitedHttpHandler
        return await operation();
    }
}