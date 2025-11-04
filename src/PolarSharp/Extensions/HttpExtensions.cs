using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using PolarSharp.Models;
using PolarSharp.Models.Common;

namespace PolarSharp.Extensions;

/// <summary>
/// Extension methods for HttpClient to support additional HTTP operations.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Sends a PATCH request with JSON content.
    /// </summary>
    /// <typeparam name="T">The type of the request content.</typeparam>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="value">The value to serialize as JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HttpResponseMessage.</returns>
    public static Task<HttpResponseMessage> PatchAsJsonAsync<T>(
        this HttpClient httpClient,
        string? requestUri,
        T value,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, requestUri)
        {
            Content = JsonContent.Create(value)
        };
        
        return httpClient.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sends a PATCH request with JSON content and returns the response content as the specified type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="value">The value to serialize as JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response content.</returns>
    public static async Task<TResponse> PatchAsJsonAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        string? requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Sends a POST request with JSON content and returns the response content as the specified type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request content.</typeparam>
    /// <typeparam name="TResponse">The type of the response content.</typeparam>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="value">The value to serialize as JSON.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response content.</returns>
    public static async Task<TResponse> PostAsJsonAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        string? requestUri,
        TRequest value,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(requestUri, value, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Sends an HTTP request and returns the response content as a PaginatedResponse.
    /// </summary>
    /// <typeparam name="T">The type of the items in the paginated response.</typeparam>
    /// <param name="httpClient">The HttpClient instance.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A PaginatedResponse containing the items.</returns>
    public static async Task<PaginatedResponse<T>> GetFromJsonAsPaginatedAsync<T>(
        this HttpClient httpClient,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<T>>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}