using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.CustomerSessions;

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
    public async Task<CustomerSession> CreateAsync(
        CustomerSessionCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer-sessions", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomerSession>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Introspects a customer session to validate the access token.
    /// </summary>
    /// <param name="request">The customer session introspection request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The introspection response.</returns>
    public async Task<CustomerSessionIntrospectResponse> IntrospectAsync(
        CustomerSessionIntrospectRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer-sessions/introspect", request, _jsonOptions, cancellationToken),
            cancellationToken);

        await response.HandleErrorsAsync(_jsonOptions, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CustomerSessionIntrospectResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
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
}