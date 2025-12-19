using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Events;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing events in the Polar system.
/// </summary>
public class EventsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal EventsApi(
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
    /// Lists all events with optional pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing events.</returns>
    public async Task<PolarResult<PaginatedResponse<Event>>> ListAsync(
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
            () => _httpClient.GetAsync($"v1/events?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Event>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all event names.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of event names.</returns>
    public async Task<PolarResult<List<EventName>>> ListNamesAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/events/names", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<List<EventName>>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets an event by ID.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The event, or null if not found.</returns>
    public async Task<PolarResult<Event?>> GetAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/events/{eventId}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultWithNullableAsync<Event>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Ingests multiple events.
    /// </summary>
    /// <param name="request">The event ingestion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response indicating success.</returns>
    public async Task<PolarResult> IngestAsync(
        EventIngestRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/events/ingest", request, _jsonOptions, cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists all events across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all events.</returns>
    public async IAsyncEnumerable<PolarResult<Event>> ListAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var result = await ListAsync(page, limit, cancellationToken);

            if (result.IsFailure)
            {
                yield return PolarResult<Event>.Failure(result.Error!);
                yield break;
            }

            foreach (var @event in result.Value.Items)
            {
                yield return PolarResult<Event>.Success(@event);
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
    /// Creates a query builder for events with fluent filtering.
    /// </summary>
    /// <returns>A new EventsQueryBuilder instance.</returns>
    public EventsQueryBuilder Query() => new();

    /// <summary>
    /// Lists events using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing filtered events.</returns>
    public async Task<PolarResult<PaginatedResponse<Event>>> ListAsync(
        EventsQueryBuilder builder,
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
            () => _httpClient.GetAsync($"v1/events?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<PaginatedResponse<Event>>(_jsonOptions, cancellationToken);
    }

    private static string GetQueryString(Dictionary<string, string> parameters)
    {
        return string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}