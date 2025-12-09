using System;
using System.Collections.Generic;
using System.Linq;

namespace PolarSharp.Extensions;

/// <summary>
/// Builder for constructing query strings with fluent API.
/// </summary>
/// <typeparam name="TBuilder">The builder type for method chaining.</typeparam>
public class QueryBuilder<TBuilder> where TBuilder : QueryBuilder<TBuilder>
{
    private readonly Dictionary<string, string> _parameters = new();

    /// <summary>
    /// Initializes a new instance of QueryBuilder.
    /// </summary>
    protected QueryBuilder()
    {
    }

    /// <summary>
    /// Adds a parameter to query.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    protected TBuilder AddParameter(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _parameters[key] = value;
        }
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a parameter to query.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="value">The parameter value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    protected TBuilder AddParameter(string key, object? value)
    {
        if (value != null)
        {
            _parameters[key] = value.ToString() ?? string.Empty;
        }
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds multiple values for same parameter key.
    /// </summary>
    /// <param name="key">The parameter key.</param>
    /// <param name="values">The parameter values.</param>
    /// <returns>The builder instance for method chaining.</returns>
    protected TBuilder AddParameters(string key, IEnumerable<string>? values)
    {
        if (values != null)
        {
            var nonEmptyValues = values.Where(v => !string.IsNullOrEmpty(v));
            if (nonEmptyValues.Any())
            {
                _parameters[key] = string.Join(",", nonEmptyValues);
            }
        }
        return (TBuilder)this;
    }

    /// <summary>
    /// Builds query string.
    /// </summary>
    /// <returns>The query string.</returns>
    public string Build()
    {
        if (!_parameters.Any())
            return string.Empty;

        return string.Join("&", _parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    /// <summary>
    /// Gets current parameters.
    /// </summary>
    /// <returns>The parameters dictionary.</returns>
    public IReadOnlyDictionary<string, string> GetParameters() => _parameters;
}

/// <summary>
/// Query builder for products API.
/// </summary>
public class ProductsQueryBuilder : QueryBuilder<ProductsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of ProductsQueryBuilder.
    /// </summary>
    public ProductsQueryBuilder() : base() { }

    /// <summary>
    /// Filters products that are active.
    /// </summary>
    /// <param name="active">Whether to filter by active status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ProductsQueryBuilder WithActive(bool? active)
    {
        return AddParameter("is_active", active?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters products by type.
    /// </summary>
    /// <param name="type">The product type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ProductsQueryBuilder WithType(string? type)
    {
        return AddParameter("type", type);
    }

    /// <summary>
    /// Filters products by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ProductsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters products by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ProductsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for orders API.
/// </summary>
public class OrdersQueryBuilder : QueryBuilder<OrdersQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of OrdersQueryBuilder.
    /// </summary>
    public OrdersQueryBuilder() : base() { }

    /// <summary>
    /// Filters orders by status.
    /// </summary>
    /// <param name="status">The order status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrdersQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters orders by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrdersQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters orders by product ID.
    /// </summary>
    /// <param name="productId">The product ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrdersQueryBuilder WithProductId(string? productId)
    {
        return AddParameter("product_id", productId);
    }

    /// <summary>
    /// Filters orders by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrdersQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters orders by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrdersQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for subscriptions API.
/// </summary>
public class SubscriptionsQueryBuilder : QueryBuilder<SubscriptionsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of SubscriptionsQueryBuilder.
    /// </summary>
    public SubscriptionsQueryBuilder() : base() { }

    /// <summary>
    /// Filters subscriptions by status.
    /// </summary>
    /// <param name="status">The subscription status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SubscriptionsQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters subscriptions by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SubscriptionsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters subscriptions by product ID.
    /// </summary>
    /// <param name="productId">The product ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SubscriptionsQueryBuilder WithProductId(string? productId)
    {
        return AddParameter("product_id", productId);
    }

    /// <summary>
    /// Filters subscriptions that are canceled.
    /// </summary>
    /// <param name="canceled">Whether to filter by canceled status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SubscriptionsQueryBuilder WithCanceled(bool? canceled)
    {
        return AddParameter("canceled", canceled?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters subscriptions by external ID.
    /// </summary>
    /// <param name="externalId">The external ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SubscriptionsQueryBuilder WithExternalId(string? externalId)
    {
        return AddParameter("external_id", externalId);
    }
}

/// <summary>
/// Query builder for customers API.
/// </summary>
public class CustomersQueryBuilder : QueryBuilder<CustomersQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CustomersQueryBuilder.
    /// </summary>
    public CustomersQueryBuilder() : base() { }

    /// <summary>
    /// Filters customers by email.
    /// </summary>
    /// <param name="email">The email to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomersQueryBuilder WithEmail(string? email)
    {
        return AddParameter("email", email);
    }

    /// <summary>
    /// Filters customers by external ID.
    /// </summary>
    /// <param name="externalId">The external ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomersQueryBuilder WithExternalId(string? externalId)
    {
        return AddParameter("external_id", externalId);
    }

    /// <summary>
    /// Filters customers by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomersQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters customers by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomersQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for files API.
/// </summary>
public class FilesQueryBuilder : QueryBuilder<FilesQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of FilesQueryBuilder.
    /// </summary>
    public FilesQueryBuilder() : base() { }

    /// <summary>
    /// Filters files by name.
    /// </summary>
    /// <param name="name">The name to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FilesQueryBuilder WithName(string? name)
    {
        return AddParameter("name", name);
    }

    /// <summary>
    /// Filters files by MIME type.
    /// </summary>
    /// <param name="mimeType">The MIME type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FilesQueryBuilder WithMimeType(string? mimeType)
    {
        return AddParameter("mime_type", mimeType);
    }

    /// <summary>
    /// Filters files that are public.
    /// </summary>
    /// <param name="isPublic">Whether to filter by public status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FilesQueryBuilder WithPublic(bool? isPublic)
    {
        return AddParameter("is_public", isPublic?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters files by organization ID.
    /// </summary>
    /// <param name="organizationId">The organization ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FilesQueryBuilder WithOrganizationId(string? organizationId)
    {
        return AddParameter("organization_id", organizationId);
    }

    /// <summary>
    /// Filters files by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FilesQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters files by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public FilesQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for payments API.
/// </summary>
public class PaymentsQueryBuilder : QueryBuilder<PaymentsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of PaymentsQueryBuilder.
    /// </summary>
    public PaymentsQueryBuilder() : base() { }

    /// <summary>
    /// Filters payments by status.
    /// </summary>
    /// <param name="status">The payment status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters payments by order ID.
    /// </summary>
    /// <param name="orderId">The order ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder WithOrderId(string? orderId)
    {
        return AddParameter("order_id", orderId);
    }

    /// <summary>
    /// Filters payments by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters payments by amount.
    /// </summary>
    /// <param name="amount">The amount to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder WithAmount(decimal? amount)
    {
        return AddParameter("amount", amount?.ToString());
    }

    /// <summary>
    /// Filters payments by currency.
    /// </summary>
    /// <param name="currency">The currency to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder WithCurrency(string? currency)
    {
        return AddParameter("currency", currency);
    }

    /// <summary>
    /// Filters payments by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters payments by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PaymentsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for refunds API.
/// </summary>
public class RefundsQueryBuilder : QueryBuilder<RefundsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of RefundsQueryBuilder.
    /// </summary>
    public RefundsQueryBuilder() : base() { }

    /// <summary>
    /// Filters refunds by status.
    /// </summary>
    /// <param name="status">The refund status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters refunds by payment ID.
    /// </summary>
    /// <param name="paymentId">The payment ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder WithPaymentId(string? paymentId)
    {
        return AddParameter("payment_id", paymentId);
    }

    /// <summary>
    /// Filters refunds by order ID.
    /// </summary>
    /// <param name="orderId">The order ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder WithOrderId(string? orderId)
    {
        return AddParameter("order_id", orderId);
    }

    /// <summary>
    /// Filters refunds by amount.
    /// </summary>
    /// <param name="amount">The amount to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder WithAmount(decimal? amount)
    {
        return AddParameter("amount", amount?.ToString());
    }

    /// <summary>
    /// Filters refunds by currency.
    /// </summary>
    /// <param name="currency">The currency to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder WithCurrency(string? currency)
    {
        return AddParameter("currency", currency);
    }

    /// <summary>
    /// Filters refunds by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters refunds by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public RefundsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for discounts API.
/// </summary>
public class DiscountsQueryBuilder : QueryBuilder<DiscountsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of DiscountsQueryBuilder.
    /// </summary>
    public DiscountsQueryBuilder() : base() { }

    /// <summary>
    /// Filters discounts by code.
    /// </summary>
    /// <param name="code">The discount code to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder WithCode(string? code)
    {
        return AddParameter("code", code);
    }

    /// <summary>
    /// Filters discounts by type.
    /// </summary>
    /// <param name="type">The discount type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder WithType(string? type)
    {
        return AddParameter("type", type?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters discounts that are active.
    /// </summary>
    /// <param name="isActive">Whether to filter by active status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder WithActive(bool? isActive)
    {
        return AddParameter("is_active", isActive?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters discounts that are expired.
    /// </summary>
    /// <param name="isExpired">Whether to filter by expired status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder WithExpired(bool? isExpired)
    {
        return AddParameter("is_expired", isExpired?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters discounts by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters discounts by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters discounts that expire after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder ExpiresAfter(DateTime? date)
    {
        return AddParameter("expires_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters discounts that expire before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public DiscountsQueryBuilder ExpiresBefore(DateTime? date)
    {
        return AddParameter("expires_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for checkouts API.
/// </summary>
public class CheckoutsQueryBuilder : QueryBuilder<CheckoutsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CheckoutsQueryBuilder.
    /// </summary>
    public CheckoutsQueryBuilder() : base() { }

    /// <summary>
    /// Filters checkouts by status.
    /// </summary>
    /// <param name="status">The checkout status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutsQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters checkouts by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters checkouts by product ID.
    /// </summary>
    /// <param name="productId">The product ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutsQueryBuilder WithProductId(string? productId)
    {
        return AddParameter("product_id", productId);
    }

    /// <summary>
    /// Filters checkouts by success URL.
    /// </summary>
    /// <param name="successUrl">The success URL to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutsQueryBuilder WithSuccessUrl(string? successUrl)
    {
        return AddParameter("success_url", successUrl);
    }

    /// <summary>
    /// Filters checkouts by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters checkouts by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for checkout links API.
/// </summary>
public class CheckoutLinksQueryBuilder : QueryBuilder<CheckoutLinksQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CheckoutLinksQueryBuilder.
    /// </summary>
    public CheckoutLinksQueryBuilder() : base() { }

    /// <summary>
    /// Filters checkout links by product ID.
    /// </summary>
    /// <param name="productId">The product ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutLinksQueryBuilder WithProductId(string? productId)
    {
        return AddParameter("product_id", productId);
    }

    /// <summary>
    /// Filters checkout links that are enabled.
    /// </summary>
    /// <param name="enabled">Whether to filter by enabled status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutLinksQueryBuilder WithEnabled(bool? enabled)
    {
        return AddParameter("enabled", enabled?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters checkout links that are archived.
    /// </summary>
    /// <param name="archived">Whether to filter by archived status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutLinksQueryBuilder WithArchived(bool? archived)
    {
        return AddParameter("archived", archived?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters checkout links by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutLinksQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters checkout links by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CheckoutLinksQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for benefits API.
/// </summary>
public class BenefitsQueryBuilder : QueryBuilder<BenefitsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of BenefitsQueryBuilder.
    /// </summary>
    public BenefitsQueryBuilder() : base() { }

    /// <summary>
    /// Filters benefits by type.
    /// </summary>
    /// <param name="type">The benefit type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public BenefitsQueryBuilder WithType(string? type)
    {
        return AddParameter("type", type?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters benefits that are selectable.
    /// </summary>
    /// <param name="selectable">Whether to filter by selectable status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public BenefitsQueryBuilder WithSelectable(bool? selectable)
    {
        return AddParameter("selectable", selectable?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters benefits by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public BenefitsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters benefits by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public BenefitsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for license keys API.
/// </summary>
public class LicenseKeysQueryBuilder : QueryBuilder<LicenseKeysQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of LicenseKeysQueryBuilder.
    /// </summary>
    public LicenseKeysQueryBuilder() : base() { }

    /// <summary>
    /// Filters license keys by status.
    /// </summary>
    /// <param name="status">The license key status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LicenseKeysQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters license keys by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LicenseKeysQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters license keys by benefit ID.
    /// </summary>
    /// <param name="benefitId">The benefit ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LicenseKeysQueryBuilder WithBenefitId(string? benefitId)
    {
        return AddParameter("benefit_id", benefitId);
    }

    /// <summary>
    /// Filters license keys by key.
    /// </summary>
    /// <param name="key">The license key to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LicenseKeysQueryBuilder WithKey(string? key)
    {
        return AddParameter("key", key);
    }

    /// <summary>
    /// Filters license keys by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LicenseKeysQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters license keys by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public LicenseKeysQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for webhooks API.
/// </summary>
public class WebhooksQueryBuilder : QueryBuilder<WebhooksQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of WebhooksQueryBuilder.
    /// </summary>
    public WebhooksQueryBuilder() : base() { }

    /// <summary>
    /// Filters webhook endpoints by URL.
    /// </summary>
    /// <param name="url">The URL to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhooksQueryBuilder WithUrl(string? url)
    {
        return AddParameter("url", url);
    }

    /// <summary>
    /// Filters webhook endpoints that are active.
    /// </summary>
    /// <param name="isActive">Whether to filter by active status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhooksQueryBuilder WithActive(bool? isActive)
    {
        return AddParameter("is_active", isActive?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters webhook endpoints by event type.
    /// </summary>
    /// <param name="eventType">The event type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhooksQueryBuilder WithEventType(string? eventType)
    {
        return AddParameter("event_type", eventType);
    }

    /// <summary>
    /// Filters webhook deliveries by success status.
    /// </summary>
    /// <param name="isSuccess">Whether to filter by success status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhooksQueryBuilder WithSuccess(bool? isSuccess)
    {
        return AddParameter("is_success", isSuccess?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters webhook endpoints by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhooksQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters webhook endpoints by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhooksQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for meters API.
/// </summary>
public class MetersQueryBuilder : QueryBuilder<MetersQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of MetersQueryBuilder.
    /// </summary>
    public MetersQueryBuilder() : base() { }

    /// <summary>
    /// Filters meters by type.
    /// </summary>
    /// <param name="type">The meter type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetersQueryBuilder WithType(string? type)
    {
        return AddParameter("type", type?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters meters that are active.
    /// </summary>
    /// <param name="active">Whether to filter by active status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetersQueryBuilder WithActive(bool? active)
    {
        return AddParameter("is_active", active?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters meters by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetersQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters meters by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetersQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for customer meters API.
/// </summary>
public class CustomerMetersQueryBuilder : QueryBuilder<CustomerMetersQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CustomerMetersQueryBuilder.
    /// </summary>
    public CustomerMetersQueryBuilder() : base() { }

    /// <summary>
    /// Filters customer meters by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerMetersQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters customer meters by meter ID.
    /// </summary>
    /// <param name="meterId">The meter ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerMetersQueryBuilder WithMeterId(string? meterId)
    {
        return AddParameter("meter_id", meterId);
    }

    /// <summary>
    /// Filters customer meters by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerMetersQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters customer meters by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerMetersQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for events API.
/// </summary>
public class EventsQueryBuilder : QueryBuilder<EventsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of EventsQueryBuilder.
    /// </summary>
    public EventsQueryBuilder() : base() { }

    /// <summary>
    /// Filters events by name.
    /// </summary>
    /// <param name="name">The event name to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EventsQueryBuilder WithName(string? name)
    {
        return AddParameter("name", name);
    }

    /// <summary>
    /// Filters events by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EventsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters events by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EventsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters events by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public EventsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for metrics API.
/// </summary>
public class MetricsQueryBuilder : QueryBuilder<MetricsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of MetricsQueryBuilder.
    /// </summary>
    public MetricsQueryBuilder() : base() { }

    /// <summary>
    /// Filters metrics by type.
    /// </summary>
    /// <param name="type">The metric type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetricsQueryBuilder WithType(string? type)
    {
        return AddParameter("type", type?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters metrics by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetricsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters metrics by start date.
    /// </summary>
    /// <param name="date">The start date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetricsQueryBuilder StartDate(DateTime? date)
    {
        return AddParameter("start_date", date?.ToString("yyyy-MM-dd"));
    }

    /// <summary>
    /// Filters metrics by end date.
    /// </summary>
    /// <param name="date">The end date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public MetricsQueryBuilder EndDate(DateTime? date)
    {
        return AddParameter("end_date", date?.ToString("yyyy-MM-dd"));
    }
}

/// <summary>
/// Query builder for custom fields API.
/// </summary>
public class CustomFieldsQueryBuilder : QueryBuilder<CustomFieldsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CustomFieldsQueryBuilder.
    /// </summary>
    public CustomFieldsQueryBuilder() : base() { }

    /// <summary>
    /// Filters custom fields by key.
    /// </summary>
    /// <param name="key">The custom field key to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomFieldsQueryBuilder WithKey(string? key)
    {
        return AddParameter("key", key);
    }

    /// <summary>
    /// Filters custom fields by type.
    /// </summary>
    /// <param name="type">The custom field type to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomFieldsQueryBuilder WithType(string? type)
    {
        return AddParameter("type", type?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters custom fields that are required.
    /// </summary>
    /// <param name="required">Whether to filter by required status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomFieldsQueryBuilder WithRequired(bool? required)
    {
        return AddParameter("required", required?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters custom fields by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomFieldsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters custom fields by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomFieldsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for seats API.
/// </summary>
public class SeatsQueryBuilder : QueryBuilder<SeatsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of SeatsQueryBuilder.
    /// </summary>
    public SeatsQueryBuilder() : base() { }

    /// <summary>
    /// Filters seats by user ID.
    /// </summary>
    /// <param name="userId">The user ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SeatsQueryBuilder WithUserId(string? userId)
    {
        return AddParameter("user_id", userId);
    }

    /// <summary>
    /// Filters seats by email.
    /// </summary>
    /// <param name="email">The email to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SeatsQueryBuilder WithEmail(string? email)
    {
        return AddParameter("email", email);
    }

    /// <summary>
    /// Filters seats by status.
    /// </summary>
    /// <param name="status">The seat status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SeatsQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters seats by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SeatsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters seats by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SeatsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for customer seats API.
/// </summary>
public class CustomerSeatsQueryBuilder : QueryBuilder<CustomerSeatsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CustomerSeatsQueryBuilder.
    /// </summary>
    public CustomerSeatsQueryBuilder() : base() { }

    /// <summary>
    /// Filters customer seats by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSeatsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters customer seats by seat ID.
    /// </summary>
    /// <param name="seatId">The seat ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSeatsQueryBuilder WithSeatId(string? seatId)
    {
        return AddParameter("seat_id", seatId);
    }

    /// <summary>
    /// Filters customer seats by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSeatsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters customer seats by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSeatsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}

/// <summary>
/// Query builder for customer sessions API.
/// </summary>
public class CustomerSessionsQueryBuilder : QueryBuilder<CustomerSessionsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of CustomerSessionsQueryBuilder.
    /// </summary>
    public CustomerSessionsQueryBuilder() : base() { }

    /// <summary>
    /// Filters customer sessions by customer ID.
    /// </summary>
    /// <param name="customerId">The customer ID to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSessionsQueryBuilder WithCustomerId(string? customerId)
    {
        return AddParameter("customer_id", customerId);
    }

    /// <summary>
    /// Filters customer sessions by status.
    /// </summary>
    /// <param name="status">The session status to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSessionsQueryBuilder WithStatus(string? status)
    {
        return AddParameter("status", status?.ToLowerInvariant());
    }

    /// <summary>
    /// Filters customer sessions by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSessionsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters customer sessions by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSessionsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters customer sessions that are expired.
    /// </summary>
    /// <param name="expired">Whether to filter by expired status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CustomerSessionsQueryBuilder WithExpired(bool? expired)
    {
        return AddParameter("expired", expired?.ToString().ToLowerInvariant());
    }
}

/// <summary>
/// Query builder for organizations API.
/// </summary>
public class OrganizationsQueryBuilder : QueryBuilder<OrganizationsQueryBuilder>
{
    /// <summary>
    /// Initializes a new instance of OrganizationsQueryBuilder.
    /// </summary>
    public OrganizationsQueryBuilder() : base() { }

    /// <summary>
    /// Filters organizations by name.
    /// </summary>
    /// <param name="name">The organization name to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrganizationsQueryBuilder WithName(string? name)
    {
        return AddParameter("name", name);
    }

    /// <summary>
    /// Filters organizations that are active.
    /// </summary>
    /// <param name="active">Whether to filter by active status.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrganizationsQueryBuilder WithActive(bool? active)
    {
        return AddParameter("is_active", active?.ToString().ToLowerInvariant());
    }

    /// <summary>
    /// Filters organizations by created after date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrganizationsQueryBuilder CreatedAfter(DateTime? date)
    {
        return AddParameter("created_after", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }

    /// <summary>
    /// Filters organizations by created before date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public OrganizationsQueryBuilder CreatedBefore(DateTime? date)
    {
        return AddParameter("created_before", date?.ToString("yyyy-MM-ddTHH:mm:ssZ"));
    }
}