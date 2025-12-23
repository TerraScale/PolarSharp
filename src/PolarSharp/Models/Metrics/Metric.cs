using System.Text.Json.Serialization;

namespace PolarSharp.Models.Metrics;

/// <summary>
/// Time interval for metrics aggregation.
/// </summary>
public enum TimeInterval
{
    [JsonPropertyName("year")]
    Year,
    [JsonPropertyName("month")]
    Month,
    [JsonPropertyName("week")]
    Week,
    [JsonPropertyName("day")]
    Day,
    [JsonPropertyName("hour")]
    Hour
}

/// <summary>
/// Represents a metric period data point in Polar system.
/// </summary>
public record MetricPeriod
{
    /// <summary>
    /// The timestamp of the metric period.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Number of orders.
    /// </summary>
    [JsonPropertyName("orders")]
    public int Orders { get; init; }

    /// <summary>
    /// Total revenue in cents.
    /// </summary>
    [JsonPropertyName("revenue")]
    public long Revenue { get; init; }

    /// <summary>
    /// Net revenue in cents (after fees).
    /// </summary>
    [JsonPropertyName("net_revenue")]
    public long NetRevenue { get; init; }

    /// <summary>
    /// Cumulative revenue in cents.
    /// </summary>
    [JsonPropertyName("cumulative_revenue")]
    public long CumulativeRevenue { get; init; }

    /// <summary>
    /// Net cumulative revenue in cents.
    /// </summary>
    [JsonPropertyName("net_cumulative_revenue")]
    public long NetCumulativeRevenue { get; init; }

    /// <summary>
    /// Total costs in cents.
    /// </summary>
    [JsonPropertyName("costs")]
    public long Costs { get; init; }

    /// <summary>
    /// Cumulative costs in cents.
    /// </summary>
    [JsonPropertyName("cumulative_costs")]
    public long CumulativeCosts { get; init; }

    /// <summary>
    /// Average order value in cents.
    /// </summary>
    [JsonPropertyName("average_order_value")]
    public long AverageOrderValue { get; init; }

    /// <summary>
    /// Net average order value in cents.
    /// </summary>
    [JsonPropertyName("net_average_order_value")]
    public long NetAverageOrderValue { get; init; }

    /// <summary>
    /// Average revenue per user in cents.
    /// </summary>
    [JsonPropertyName("average_revenue_per_user")]
    public long AverageRevenuePerUser { get; init; }

    /// <summary>
    /// Cost per user in cents.
    /// </summary>
    [JsonPropertyName("cost_per_user")]
    public long CostPerUser { get; init; }

    /// <summary>
    /// Active users by event.
    /// </summary>
    [JsonPropertyName("active_user_by_event")]
    public int ActiveUserByEvent { get; init; }

    /// <summary>
    /// Number of one-time product purchases.
    /// </summary>
    [JsonPropertyName("one_time_products")]
    public int OneTimeProducts { get; init; }

    /// <summary>
    /// One-time products revenue in cents.
    /// </summary>
    [JsonPropertyName("one_time_products_revenue")]
    public long OneTimeProductsRevenue { get; init; }

    /// <summary>
    /// One-time products net revenue in cents.
    /// </summary>
    [JsonPropertyName("one_time_products_net_revenue")]
    public long OneTimeProductsNetRevenue { get; init; }

    /// <summary>
    /// Number of new subscriptions.
    /// </summary>
    [JsonPropertyName("new_subscriptions")]
    public int NewSubscriptions { get; init; }

    /// <summary>
    /// New subscriptions revenue in cents.
    /// </summary>
    [JsonPropertyName("new_subscriptions_revenue")]
    public long NewSubscriptionsRevenue { get; init; }

    /// <summary>
    /// New subscriptions net revenue in cents.
    /// </summary>
    [JsonPropertyName("new_subscriptions_net_revenue")]
    public long NewSubscriptionsNetRevenue { get; init; }

    /// <summary>
    /// Number of renewed subscriptions.
    /// </summary>
    [JsonPropertyName("renewed_subscriptions")]
    public int RenewedSubscriptions { get; init; }

    /// <summary>
    /// Renewed subscriptions revenue in cents.
    /// </summary>
    [JsonPropertyName("renewed_subscriptions_revenue")]
    public long RenewedSubscriptionsRevenue { get; init; }

    /// <summary>
    /// Renewed subscriptions net revenue in cents.
    /// </summary>
    [JsonPropertyName("renewed_subscriptions_net_revenue")]
    public long RenewedSubscriptionsNetRevenue { get; init; }

    /// <summary>
    /// Number of active subscriptions.
    /// </summary>
    [JsonPropertyName("active_subscriptions")]
    public int ActiveSubscriptions { get; init; }

    /// <summary>
    /// Monthly recurring revenue in cents.
    /// </summary>
    [JsonPropertyName("monthly_recurring_revenue")]
    public long MonthlyRecurringRevenue { get; init; }

    /// <summary>
    /// Committed monthly recurring revenue in cents.
    /// </summary>
    [JsonPropertyName("committed_monthly_recurring_revenue")]
    public long CommittedMonthlyRecurringRevenue { get; init; }

    /// <summary>
    /// Number of checkouts.
    /// </summary>
    [JsonPropertyName("checkouts")]
    public int Checkouts { get; init; }

    /// <summary>
    /// Number of succeeded checkouts.
    /// </summary>
    [JsonPropertyName("succeeded_checkouts")]
    public int SucceededCheckouts { get; init; }

    /// <summary>
    /// Checkout conversion rate.
    /// </summary>
    [JsonPropertyName("checkouts_conversion")]
    public decimal CheckoutsConversion { get; init; }

    /// <summary>
    /// Number of canceled subscriptions.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions")]
    public int CanceledSubscriptions { get; init; }

    /// <summary>
    /// Canceled due to customer service issues.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_customer_service")]
    public int CanceledSubscriptionsCustomerService { get; init; }

    /// <summary>
    /// Canceled due to low quality.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_low_quality")]
    public int CanceledSubscriptionsLowQuality { get; init; }

    /// <summary>
    /// Canceled due to missing features.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_missing_features")]
    public int CanceledSubscriptionsMissingFeatures { get; init; }

    /// <summary>
    /// Canceled due to switching to another service.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_switched_service")]
    public int CanceledSubscriptionsSwitchedService { get; init; }

    /// <summary>
    /// Canceled because too complex.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_too_complex")]
    public int CanceledSubscriptionsTooComplex { get; init; }

    /// <summary>
    /// Canceled because too expensive.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_too_expensive")]
    public int CanceledSubscriptionsTooExpensive { get; init; }

    /// <summary>
    /// Canceled due to being unused.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_unused")]
    public int CanceledSubscriptionsUnused { get; init; }

    /// <summary>
    /// Canceled for other reasons.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_other")]
    public int CanceledSubscriptionsOther { get; init; }

    /// <summary>
    /// Gross margin in cents.
    /// </summary>
    [JsonPropertyName("gross_margin")]
    public long GrossMargin { get; init; }

    /// <summary>
    /// Gross margin percentage.
    /// </summary>
    [JsonPropertyName("gross_margin_percentage")]
    public decimal GrossMarginPercentage { get; init; }

    /// <summary>
    /// Cashflow in cents.
    /// </summary>
    [JsonPropertyName("cashflow")]
    public long Cashflow { get; init; }
}

/// <summary>
/// Represents totals for the metrics period.
/// </summary>
public record MetricsTotals
{
    /// <summary>
    /// Total number of orders.
    /// </summary>
    [JsonPropertyName("orders")]
    public int Orders { get; init; }

    /// <summary>
    /// Total revenue in cents.
    /// </summary>
    [JsonPropertyName("revenue")]
    public long Revenue { get; init; }

    /// <summary>
    /// Total net revenue in cents.
    /// </summary>
    [JsonPropertyName("net_revenue")]
    public long NetRevenue { get; init; }

    /// <summary>
    /// Cumulative revenue in cents.
    /// </summary>
    [JsonPropertyName("cumulative_revenue")]
    public long CumulativeRevenue { get; init; }

    /// <summary>
    /// Net cumulative revenue in cents.
    /// </summary>
    [JsonPropertyName("net_cumulative_revenue")]
    public long NetCumulativeRevenue { get; init; }

    /// <summary>
    /// Total costs in cents.
    /// </summary>
    [JsonPropertyName("costs")]
    public long Costs { get; init; }

    /// <summary>
    /// Cumulative costs in cents.
    /// </summary>
    [JsonPropertyName("cumulative_costs")]
    public long CumulativeCosts { get; init; }

    /// <summary>
    /// Average order value in cents.
    /// </summary>
    [JsonPropertyName("average_order_value")]
    public long AverageOrderValue { get; init; }

    /// <summary>
    /// Net average order value in cents.
    /// </summary>
    [JsonPropertyName("net_average_order_value")]
    public long NetAverageOrderValue { get; init; }

    /// <summary>
    /// Average revenue per user in cents.
    /// </summary>
    [JsonPropertyName("average_revenue_per_user")]
    public long AverageRevenuePerUser { get; init; }

    /// <summary>
    /// Cost per user in cents.
    /// </summary>
    [JsonPropertyName("cost_per_user")]
    public long CostPerUser { get; init; }

    /// <summary>
    /// Active users by event.
    /// </summary>
    [JsonPropertyName("active_user_by_event")]
    public int ActiveUserByEvent { get; init; }

    /// <summary>
    /// Number of one-time product purchases.
    /// </summary>
    [JsonPropertyName("one_time_products")]
    public int OneTimeProducts { get; init; }

    /// <summary>
    /// One-time products revenue in cents.
    /// </summary>
    [JsonPropertyName("one_time_products_revenue")]
    public long OneTimeProductsRevenue { get; init; }

    /// <summary>
    /// One-time products net revenue in cents.
    /// </summary>
    [JsonPropertyName("one_time_products_net_revenue")]
    public long OneTimeProductsNetRevenue { get; init; }

    /// <summary>
    /// Number of new subscriptions.
    /// </summary>
    [JsonPropertyName("new_subscriptions")]
    public int NewSubscriptions { get; init; }

    /// <summary>
    /// New subscriptions revenue in cents.
    /// </summary>
    [JsonPropertyName("new_subscriptions_revenue")]
    public long NewSubscriptionsRevenue { get; init; }

    /// <summary>
    /// New subscriptions net revenue in cents.
    /// </summary>
    [JsonPropertyName("new_subscriptions_net_revenue")]
    public long NewSubscriptionsNetRevenue { get; init; }

    /// <summary>
    /// Number of renewed subscriptions.
    /// </summary>
    [JsonPropertyName("renewed_subscriptions")]
    public int RenewedSubscriptions { get; init; }

    /// <summary>
    /// Renewed subscriptions revenue in cents.
    /// </summary>
    [JsonPropertyName("renewed_subscriptions_revenue")]
    public long RenewedSubscriptionsRevenue { get; init; }

    /// <summary>
    /// Renewed subscriptions net revenue in cents.
    /// </summary>
    [JsonPropertyName("renewed_subscriptions_net_revenue")]
    public long RenewedSubscriptionsNetRevenue { get; init; }

    /// <summary>
    /// Number of active subscriptions.
    /// </summary>
    [JsonPropertyName("active_subscriptions")]
    public int ActiveSubscriptions { get; init; }

    /// <summary>
    /// Monthly recurring revenue in cents.
    /// </summary>
    [JsonPropertyName("monthly_recurring_revenue")]
    public long MonthlyRecurringRevenue { get; init; }

    /// <summary>
    /// Committed monthly recurring revenue in cents.
    /// </summary>
    [JsonPropertyName("committed_monthly_recurring_revenue")]
    public long CommittedMonthlyRecurringRevenue { get; init; }

    /// <summary>
    /// Number of checkouts.
    /// </summary>
    [JsonPropertyName("checkouts")]
    public int Checkouts { get; init; }

    /// <summary>
    /// Number of succeeded checkouts.
    /// </summary>
    [JsonPropertyName("succeeded_checkouts")]
    public int SucceededCheckouts { get; init; }

    /// <summary>
    /// Checkout conversion rate.
    /// </summary>
    [JsonPropertyName("checkouts_conversion")]
    public decimal CheckoutsConversion { get; init; }

    /// <summary>
    /// Number of canceled subscriptions.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions")]
    public int CanceledSubscriptions { get; init; }

    /// <summary>
    /// Canceled due to customer service issues.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_customer_service")]
    public int CanceledSubscriptionsCustomerService { get; init; }

    /// <summary>
    /// Canceled due to low quality.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_low_quality")]
    public int CanceledSubscriptionsLowQuality { get; init; }

    /// <summary>
    /// Canceled due to missing features.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_missing_features")]
    public int CanceledSubscriptionsMissingFeatures { get; init; }

    /// <summary>
    /// Canceled due to switching to another service.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_switched_service")]
    public int CanceledSubscriptionsSwitchedService { get; init; }

    /// <summary>
    /// Canceled because too complex.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_too_complex")]
    public int CanceledSubscriptionsTooComplex { get; init; }

    /// <summary>
    /// Canceled because too expensive.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_too_expensive")]
    public int CanceledSubscriptionsTooExpensive { get; init; }

    /// <summary>
    /// Canceled due to being unused.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_unused")]
    public int CanceledSubscriptionsUnused { get; init; }

    /// <summary>
    /// Canceled for other reasons.
    /// </summary>
    [JsonPropertyName("canceled_subscriptions_other")]
    public int CanceledSubscriptionsOther { get; init; }

    /// <summary>
    /// Gross margin in cents.
    /// </summary>
    [JsonPropertyName("gross_margin")]
    public long GrossMargin { get; init; }

    /// <summary>
    /// Gross margin percentage.
    /// </summary>
    [JsonPropertyName("gross_margin_percentage")]
    public decimal GrossMarginPercentage { get; init; }

    /// <summary>
    /// Cashflow in cents.
    /// </summary>
    [JsonPropertyName("cashflow")]
    public long Cashflow { get; init; }
}

/// <summary>
/// Represents information about a metric.
/// </summary>
public record MetricInfo
{
    /// <summary>
    /// The slug identifier for the metric.
    /// </summary>
    [JsonPropertyName("slug")]
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// The display name of the metric.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// The type of metric (e.g., "scalar", "currency").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;
}

/// <summary>
/// Represents metric definitions.
/// </summary>
public record MetricsDefinitions
{
    [JsonPropertyName("orders")]
    public MetricInfo? Orders { get; init; }

    [JsonPropertyName("revenue")]
    public MetricInfo? Revenue { get; init; }

    [JsonPropertyName("net_revenue")]
    public MetricInfo? NetRevenue { get; init; }

    [JsonPropertyName("cumulative_revenue")]
    public MetricInfo? CumulativeRevenue { get; init; }

    [JsonPropertyName("net_cumulative_revenue")]
    public MetricInfo? NetCumulativeRevenue { get; init; }

    [JsonPropertyName("costs")]
    public MetricInfo? Costs { get; init; }

    [JsonPropertyName("cumulative_costs")]
    public MetricInfo? CumulativeCosts { get; init; }

    [JsonPropertyName("average_order_value")]
    public MetricInfo? AverageOrderValue { get; init; }

    [JsonPropertyName("net_average_order_value")]
    public MetricInfo? NetAverageOrderValue { get; init; }

    [JsonPropertyName("average_revenue_per_user")]
    public MetricInfo? AverageRevenuePerUser { get; init; }

    [JsonPropertyName("cost_per_user")]
    public MetricInfo? CostPerUser { get; init; }

    [JsonPropertyName("active_user_by_event")]
    public MetricInfo? ActiveUserByEvent { get; init; }

    [JsonPropertyName("one_time_products")]
    public MetricInfo? OneTimeProducts { get; init; }

    [JsonPropertyName("one_time_products_revenue")]
    public MetricInfo? OneTimeProductsRevenue { get; init; }

    [JsonPropertyName("one_time_products_net_revenue")]
    public MetricInfo? OneTimeProductsNetRevenue { get; init; }

    [JsonPropertyName("new_subscriptions")]
    public MetricInfo? NewSubscriptions { get; init; }

    [JsonPropertyName("new_subscriptions_revenue")]
    public MetricInfo? NewSubscriptionsRevenue { get; init; }

    [JsonPropertyName("new_subscriptions_net_revenue")]
    public MetricInfo? NewSubscriptionsNetRevenue { get; init; }

    [JsonPropertyName("renewed_subscriptions")]
    public MetricInfo? RenewedSubscriptions { get; init; }

    [JsonPropertyName("renewed_subscriptions_revenue")]
    public MetricInfo? RenewedSubscriptionsRevenue { get; init; }

    [JsonPropertyName("renewed_subscriptions_net_revenue")]
    public MetricInfo? RenewedSubscriptionsNetRevenue { get; init; }

    [JsonPropertyName("active_subscriptions")]
    public MetricInfo? ActiveSubscriptions { get; init; }

    [JsonPropertyName("monthly_recurring_revenue")]
    public MetricInfo? MonthlyRecurringRevenue { get; init; }

    [JsonPropertyName("committed_monthly_recurring_revenue")]
    public MetricInfo? CommittedMonthlyRecurringRevenue { get; init; }

    [JsonPropertyName("checkouts")]
    public MetricInfo? Checkouts { get; init; }

    [JsonPropertyName("succeeded_checkouts")]
    public MetricInfo? SucceededCheckouts { get; init; }

    [JsonPropertyName("checkouts_conversion")]
    public MetricInfo? CheckoutsConversion { get; init; }

    [JsonPropertyName("canceled_subscriptions")]
    public MetricInfo? CanceledSubscriptions { get; init; }

    [JsonPropertyName("canceled_subscriptions_customer_service")]
    public MetricInfo? CanceledSubscriptionsCustomerService { get; init; }

    [JsonPropertyName("canceled_subscriptions_low_quality")]
    public MetricInfo? CanceledSubscriptionsLowQuality { get; init; }

    [JsonPropertyName("canceled_subscriptions_missing_features")]
    public MetricInfo? CanceledSubscriptionsMissingFeatures { get; init; }

    [JsonPropertyName("canceled_subscriptions_switched_service")]
    public MetricInfo? CanceledSubscriptionsSwitchedService { get; init; }

    [JsonPropertyName("canceled_subscriptions_too_complex")]
    public MetricInfo? CanceledSubscriptionsTooComplex { get; init; }

    [JsonPropertyName("canceled_subscriptions_too_expensive")]
    public MetricInfo? CanceledSubscriptionsTooExpensive { get; init; }

    [JsonPropertyName("canceled_subscriptions_unused")]
    public MetricInfo? CanceledSubscriptionsUnused { get; init; }

    [JsonPropertyName("canceled_subscriptions_other")]
    public MetricInfo? CanceledSubscriptionsOther { get; init; }

    [JsonPropertyName("gross_margin")]
    public MetricInfo? GrossMargin { get; init; }

    [JsonPropertyName("gross_margin_percentage")]
    public MetricInfo? GrossMarginPercentage { get; init; }

    [JsonPropertyName("cashflow")]
    public MetricInfo? Cashflow { get; init; }
}

/// <summary>
/// Represents the full metrics response from the Polar API.
/// </summary>
public record MetricsResponse
{
    /// <summary>
    /// List of data for each timestamp period.
    /// </summary>
    [JsonPropertyName("periods")]
    public List<MetricPeriod> Periods { get; init; } = new();

    /// <summary>
    /// Totals for the whole selected period.
    /// </summary>
    [JsonPropertyName("totals")]
    public MetricsTotals Totals { get; init; } = new();

    /// <summary>
    /// Information about the returned metrics.
    /// </summary>
    [JsonPropertyName("metrics")]
    public MetricsDefinitions Metrics { get; init; } = new();
}

/// <summary>
/// Represents a single metric interval limit.
/// </summary>
public record MetricIntervalLimit
{
    /// <summary>
    /// The time interval (e.g., "hour", "day", "week", "month", "year").
    /// </summary>
    [JsonPropertyName("interval")]
    public string Interval { get; init; } = string.Empty;

    /// <summary>
    /// The minimum number of days required for this interval.
    /// </summary>
    [JsonPropertyName("min_days")]
    public int MinDays { get; init; }
}

/// <summary>
/// Represents the metrics limits response.
/// </summary>
public record MetricsLimitsResponse
{
    /// <summary>
    /// List of interval limits.
    /// </summary>
    [JsonPropertyName("limits")]
    public List<MetricIntervalLimit> Limits { get; init; } = new();
}

/// <summary>
/// Billing type filter for metrics.
/// </summary>
public enum BillingType
{
    [JsonPropertyName("one_time")]
    OneTime,
    [JsonPropertyName("recurring")]
    Recurring
}
