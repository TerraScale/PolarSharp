using System.Net.Http.Json;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.OAuth2;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing OAuth2 operations in the Polar system.
/// </summary>
public class OAuth2Api
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal OAuth2Api(
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
    /// Creates a new OAuth2 client.
    /// </summary>
    /// <param name="request">The OAuth2 client creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created OAuth2 client.</returns>
    public async Task<PolarResult<OAuth2Client>> CreateClientAsync(
        OAuth2ClientCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/oauth2/clients", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<OAuth2Client>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets an OAuth2 client by ID.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OAuth2 client, or null if not found.</returns>
    public async Task<PolarResult<OAuth2Client>> GetClientAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/oauth2/clients/{clientId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<OAuth2Client>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates an existing OAuth2 client.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="request">The OAuth2 client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated OAuth2 client, or null if not found.</returns>
    public async Task<PolarResult<OAuth2Client>> UpdateClientAsync(
        string clientId,
        OAuth2ClientUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync($"v1/oauth2/clients/{clientId}", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<OAuth2Client>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Deletes an OAuth2 client.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, or null if not found.</returns>
    public async Task<PolarResult> DeleteClientAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/oauth2/clients/{clientId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Requests an OAuth2 token.
    /// </summary>
    /// <param name="request">The token request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OAuth2 token response.</returns>
    public async Task<PolarResult<OAuth2TokenResponse>> RequestTokenAsync(
        OAuth2TokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/oauth2/token", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<OAuth2TokenResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Revokes an OAuth2 token.
    /// </summary>
    /// <param name="request">The revoke request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the operation.</returns>
    public async Task<PolarResult> RevokeTokenAsync(
        OAuth2RevokeRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/oauth2/revoke", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Introspects an OAuth2 token.
    /// </summary>
    /// <param name="request">The introspection request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OAuth2 token introspection response.</returns>
    public async Task<PolarResult<OAuth2IntrospectResponse>> IntrospectTokenAsync(
        OAuth2IntrospectRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/oauth2/introspect", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<OAuth2IntrospectResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets OAuth2 user info.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The OAuth2 user info, or null if not found.</returns>
    public async Task<PolarResult<OAuth2UserInfo>> GetUserInfoAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/oauth2/userinfo", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<OAuth2UserInfo>(_jsonOptions, cancellationToken);
    }

    private async Task<HttpResponseMessage> ExecuteWithPoliciesAsync(
        Func<Task<HttpResponseMessage>> operation,
        CancellationToken cancellationToken)
    {
        // Rate limiting and retry is now handled by RateLimitedHttpHandler
        return await operation();
    }
}