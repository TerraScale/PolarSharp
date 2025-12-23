using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Metrics;
using PolarSharp.Results;

namespace PolarSharp.Api;

/// <summary>
/// API client for managing metrics in the Polar system.
/// </summary>
public class MetricsApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal MetricsApi(
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
    /// Gets metrics data for a specific date range and interval.
    /// Currency values are returned in cents.
    /// </summary>
    /// <param name="startDate">Start date for the metrics period.</param>
    /// <param name="endDate">End date for the metrics period.</param>
    /// <param name="interval">Time interval for data aggregation.</param>
    /// <param name="timezone">Timezone to use for timestamps (default: UTC).</param>
    /// <param name="organizationId">Filter by organization ID.</param>
    /// <param name="productId">Filter by product ID.</param>
    /// <param name="billingType">Filter by billing type (one_time or recurring).</param>
    /// <param name="customerId">Filter by customer ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A metrics response containing periods, totals, and metric definitions.</returns>
    public async Task<PolarResult<MetricsResponse>> GetAsync(
        DateOnly startDate,
        DateOnly endDate,
        TimeInterval interval,
        string timezone = "UTC",
        string? organizationId = null,
        string? productId = null,
        BillingType? billingType = null,
        string? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["start_date"] = startDate.ToString("yyyy-MM-dd"),
            ["end_date"] = endDate.ToString("yyyy-MM-dd"),
            ["interval"] = interval.ToApiString(),
            ["timezone"] = timezone
        };

        if (!string.IsNullOrEmpty(organizationId))
            queryParams["organization_id"] = organizationId;
        
        if (!string.IsNullOrEmpty(productId))
            queryParams["product_id"] = productId;
        
        if (billingType.HasValue)
            queryParams["billing_type"] = billingType.Value.ToApiString();
        
        if (!string.IsNullOrEmpty(customerId))
            queryParams["customer_id"] = customerId;

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/metrics?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<MetricsResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Gets metrics limits which define the minimum number of days required for each interval.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The metrics limits response containing interval constraints.</returns>
    public async Task<PolarResult<MetricsLimitsResponse>> GetLimitsAsync(CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/metrics/limits", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<MetricsLimitsResponse>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Creates a query builder for metrics with fluent filtering.
    /// </summary>
    /// <returns>A new MetricsQueryBuilder instance.</returns>
    public MetricsQueryBuilder Query() => new();

    /// <summary>
    /// Gets metrics using a query builder for advanced filtering.
    /// </summary>
    /// <param name="builder">The query builder containing filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A metrics response containing periods, totals, and metric definitions.</returns>
    public async Task<PolarResult<MetricsResponse>> GetAsync(
        MetricsQueryBuilder builder,
        CancellationToken cancellationToken = default)
    {
        var queryParams = builder.GetParameters();

        // Validate required parameters
        if (!queryParams.ContainsKey("start_date"))
            return PolarResult<MetricsResponse>.Failure(PolarError.ValidationError("start_date is required"));
        
        if (!queryParams.ContainsKey("end_date"))
            return PolarResult<MetricsResponse>.Failure(PolarError.ValidationError("end_date is required"));
        
        if (!queryParams.ContainsKey("interval"))
            return PolarResult<MetricsResponse>.Failure(PolarError.ValidationError("interval is required"));

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/metrics?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        return await response.ToPolarResultAsync<MetricsResponse>(_jsonOptions, cancellationToken);
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

/// <summary>
/// Query builder for metrics API with fluent filtering options.
/// </summary>
public class MetricsQueryBuilder
{
    private readonly Dictionary<string, string> _parameters = new();

    /// <summary>
    /// Sets the start date for the metrics period (required).
    /// </summary>
    public MetricsQueryBuilder WithStartDate(DateOnly startDate)
    {
        _parameters["start_date"] = startDate.ToString("yyyy-MM-dd");
        return this;
    }

    /// <summary>
    /// Sets the start date for the metrics period (required).
    /// </summary>
    public MetricsQueryBuilder StartDate(DateTime startDate)
    {
        _parameters["start_date"] = DateOnly.FromDateTime(startDate).ToString("yyyy-MM-dd");
        return this;
    }

    /// <summary>
    /// Sets the end date for the metrics period (required).
    /// </summary>
    public MetricsQueryBuilder WithEndDate(DateOnly endDate)
    {
        _parameters["end_date"] = endDate.ToString("yyyy-MM-dd");
        return this;
    }

    /// <summary>
    /// Sets the end date for the metrics period (required).
    /// </summary>
    public MetricsQueryBuilder EndDate(DateTime endDate)
    {
        _parameters["end_date"] = DateOnly.FromDateTime(endDate).ToString("yyyy-MM-dd");
        return this;
    }

    /// <summary>
    /// Sets the time interval for data aggregation (required).
    /// </summary>
    public MetricsQueryBuilder WithInterval(TimeInterval interval)
    {
        _parameters["interval"] = interval.ToApiString();
        return this;
    }

    /// <summary>
    /// Sets the timezone for timestamps.
    /// </summary>
    public MetricsQueryBuilder WithTimezone(string timezone)
    {
        _parameters["timezone"] = timezone;
        return this;
    }

    /// <summary>
    /// Filters by organization ID.
    /// </summary>
    public MetricsQueryBuilder WithOrganizationId(Guid organizationId)
    {
        _parameters["organization_id"] = organizationId.ToString();
        return this;
    }

    /// <summary>
    /// Filters by product ID.
    /// </summary>
    public MetricsQueryBuilder WithProductId(Guid productId)
    {
        _parameters["product_id"] = productId.ToString();
        return this;
    }

    /// <summary>
    /// Filters by billing type.
    /// </summary>
    public MetricsQueryBuilder WithBillingType(BillingType billingType)
    {
        _parameters["billing_type"] = billingType.ToApiString();
        return this;
    }

    /// <summary>
    /// Filters by customer ID.
    /// </summary>
    public MetricsQueryBuilder WithCustomerId(Guid customerId)
    {
        _parameters["customer_id"] = customerId.ToString();
        return this;
    }

    /// <summary>
    /// Legacy method for compatibility - sets metric type filter.
    /// </summary>
    [Obsolete("Use WithBillingType instead. This method is kept for backwards compatibility.")]
    public MetricsQueryBuilder WithType(string type)
    {
        // Map old type parameter to billing type if possible
        if (type.Equals("recurring", StringComparison.OrdinalIgnoreCase))
            _parameters["billing_type"] = "recurring";
        else if (type.Equals("one_time", StringComparison.OrdinalIgnoreCase))
            _parameters["billing_type"] = "one_time";
        return this;
    }

    /// <summary>
    /// Gets the query parameters dictionary.
    /// </summary>
    internal Dictionary<string, string> GetParameters() => new(_parameters);
}

/// <summary>
/// Extension methods for enum to API string conversion.
/// </summary>
internal static class MetricsEnumExtensions
{
    public static string ToApiString(this TimeInterval interval) => interval switch
    {
        TimeInterval.Year => "year",
        TimeInterval.Month => "month",
        TimeInterval.Week => "week",
        TimeInterval.Day => "day",
        TimeInterval.Hour => "hour",
        _ => "day"
    };

    public static string ToApiString(this BillingType billingType) => billingType switch
    {
        BillingType.OneTime => "one_time",
        BillingType.Recurring => "recurring",
        _ => "one_time"
    };
}
