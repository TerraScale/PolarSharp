using System.Collections.Generic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Polly.RateLimit;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;

namespace PolarSharp.Api;

/// <summary>
/// API client for customer portal operations in the Polar system.
/// This API uses customer access tokens for authentication.
/// </summary>
public class CustomerPortalApi
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;

    internal CustomerPortalApi(
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
    /// Creates a new CustomerPortalApi instance with customer access token.
    /// </summary>
    /// <param name="customerAccessToken">The customer access token.</param>
    /// <param name="baseUrl">The base URL for the Polar API.</param>
    /// <param name="options">Optional client options.</param>
    /// <returns>A configured CustomerPortalApi instance.</returns>
    public static CustomerPortalApi Create(
        string customerAccessToken,
        Uri? baseUrl = null,
        PolarClientOptions? options = null)
    {
        var finalOptions = options ?? new PolarClientOptions();
        
        var httpClient = new HttpClient();
        httpClient.BaseAddress = baseUrl ?? new Uri("https://api.polar.sh/v1");
        httpClient.Timeout = TimeSpan.FromSeconds(finalOptions.TimeoutSeconds);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", customerAccessToken);
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("User-Agent", finalOptions.UserAgent ?? "PolarSharp/1.0.0");

        var jsonOptions = CreateJsonSerializerOptions(finalOptions.JsonSerializerOptions);
        var retryPolicy = CreateRetryPolicy(finalOptions);
        var rateLimitPolicy = CreateRateLimitPolicy(finalOptions);

        return new CustomerPortalApi(httpClient, jsonOptions, retryPolicy, rateLimitPolicy);
    }

    /// <summary>
    /// Creates a new CustomerPortalApi instance for sandbox environment.
    /// </summary>
    /// <param name="customerAccessToken">The customer access token.</param>
    /// <param name="options">Optional client options.</param>
    /// <returns>A configured CustomerPortalApi instance for sandbox.</returns>
    public static CustomerPortalApi CreateSandbox(
        string customerAccessToken,
        PolarClientOptions? options = null)
    {
        return Create(customerAccessToken, new Uri("https://sandbox-api.polar.sh/v1"), options);
    }

    /// <summary>
    /// Gets the current customer information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer information, or null if not found.</returns>
    public async Task<Models.Customers.Customer?> GetCustomerAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/customer-portal/customers", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Models.Customers.Customer>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Updates the current customer information.
    /// </summary>
    /// <param name="request">The customer update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated customer information.</returns>
    public async Task<Models.Customers.Customer> UpdateCustomerAsync(
        Models.Customers.CustomerUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PatchAsJsonAsync("v1/customer-portal/customers", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.Customers.Customer>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists customer's payment methods.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of payment methods.</returns>
    public async Task<List<Models.Payments.PaymentMethod>> ListPaymentMethodsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/customer-portal/customers/payment-methods", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Models.Payments.PaymentMethod>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Adds a new payment method for the customer.
    /// </summary>
    /// <param name="request">The payment method creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created payment method.</returns>
    public async Task<Models.Payments.PaymentMethod> AddPaymentMethodAsync(
        Models.Payments.PaymentMethodCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer-portal/customers/payment-methods", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.Payments.PaymentMethod>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Confirms a customer payment method.
    /// </summary>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The confirmed payment method.</returns>
    public async Task<Models.Payments.PaymentMethod> ConfirmPaymentMethodAsync(
        string paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsync($"v1/customer-portal/customers/payment-methods/{paymentMethodId}/confirm", null, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.Payments.PaymentMethod>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deletes a customer payment method.
    /// </summary>
    /// <param name="paymentMethodId">The payment method ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted payment method.</returns>
    public async Task<Models.Payments.PaymentMethod> DeletePaymentMethodAsync(
        string paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.DeleteAsync($"v1/customer-portal/customers/payment-methods/{paymentMethodId}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.Payments.PaymentMethod>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists customer's orders.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="status">Filter by order status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing orders.</returns>
    public async Task<PaginatedResponse<Models.Orders.Order>> ListOrdersAsync(
        int page = 1,
        int limit = 10,
        Models.Orders.OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/orders?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Models.Orders.Order>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all customer orders across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="status">Filter by order status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all orders.</returns>
    public async IAsyncEnumerable<Models.Orders.Order> ListAllOrdersAsync(
        Models.Orders.OrderStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListOrdersAsync(page, limit, status, cancellationToken);
            
            foreach (var order in response.Items)
            {
                yield return order;
            }

            if (page >= response.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Gets a customer order by ID.
    /// </summary>
    /// <param name="orderId">The order ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order, or null if not found.</returns>
    public async Task<Models.Orders.Order?> GetOrderAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/orders/{orderId}", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Models.Orders.Order>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists customer's subscriptions.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="status">Filter by subscription status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing subscriptions.</returns>
    public async Task<PaginatedResponse<Models.Subscriptions.Subscription>> ListSubscriptionsAsync(
        int page = 1,
        int limit = 10,
        Models.Subscriptions.SubscriptionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/subscriptions?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Models.Subscriptions.Subscription>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all customer subscriptions across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="status">Filter by subscription status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all subscriptions.</returns>
    public async IAsyncEnumerable<Models.Subscriptions.Subscription> ListAllSubscriptionsAsync(
        Models.Subscriptions.SubscriptionStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListSubscriptionsAsync(page, limit, status, cancellationToken);
            
            foreach (var subscription in response.Items)
            {
                yield return subscription;
            }

            if (page >= response.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Gets a customer subscription by ID.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The subscription, or null if not found.</returns>
    public async Task<Models.Subscriptions.Subscription?> GetSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/subscriptions/{subscriptionId}", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Models.Subscriptions.Subscription>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Cancels a customer subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The canceled subscription.</returns>
    public async Task<Models.Subscriptions.Subscription> CancelSubscriptionAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsync($"v1/customer-portal/subscriptions/{subscriptionId}/cancel", null, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.Subscriptions.Subscription>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists customer's benefit grants.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="status">Filter by benefit grant status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing benefit grants.</returns>
    public async Task<PaginatedResponse<Models.Benefits.BenefitGrant>> ListBenefitGrantsAsync(
        int page = 1,
        int limit = 10,
        Models.Benefits.BenefitGrantStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/benefit-grants?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Models.Benefits.BenefitGrant>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all customer benefit grants across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="status">Filter by benefit grant status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all benefit grants.</returns>
    public async IAsyncEnumerable<Models.Benefits.BenefitGrant> ListAllBenefitGrantsAsync(
        Models.Benefits.BenefitGrantStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListBenefitGrantsAsync(page, limit, status, cancellationToken);
            
            foreach (var benefitGrant in response.Items)
            {
                yield return benefitGrant;
            }

            if (page >= response.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Gets a customer benefit grant by ID.
    /// </summary>
    /// <param name="benefitGrantId">The benefit grant ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The benefit grant, or null if not found.</returns>
    public async Task<Models.Benefits.BenefitGrant?> GetBenefitGrantAsync(
        string benefitGrantId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/benefit-grants/{benefitGrantId}", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Models.Benefits.BenefitGrant>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Lists customer's license keys.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="limit">Number of items per page (default: 10, max: 100).</param>
    /// <param name="status">Filter by license key status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated response containing license keys.</returns>
    public async Task<PaginatedResponse<Models.LicenseKeys.LicenseKey>> ListLicenseKeysAsync(
        int page = 1,
        int limit = 10,
        Models.LicenseKeys.LicenseKeyStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["page"] = page.ToString(),
            ["limit"] = Math.Min(limit, 100).ToString()
        };

        if (status.HasValue)
            queryParams["status"] = status.Value.ToString().ToLowerInvariant();

        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/license-keys?{GetQueryString(queryParams)}", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<PaginatedResponse<Models.LicenseKeys.LicenseKey>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists all customer license keys across all pages using IAsyncEnumerable.
    /// </summary>
    /// <param name="status">Filter by license key status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all license keys.</returns>
    public async IAsyncEnumerable<Models.LicenseKeys.LicenseKey> ListAllLicenseKeysAsync(
        Models.LicenseKeys.LicenseKeyStatus? status = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var page = 1;
        const int limit = 100; // Use maximum page size for efficiency

        while (true)
        {
            var response = await ListLicenseKeysAsync(page, limit, status, cancellationToken);
            
            foreach (var licenseKey in response.Items)
            {
                yield return licenseKey;
            }

            if (page >= response.Pagination.MaxPage)
                break;

            page++;
        }
    }

    /// <summary>
    /// Gets a customer license key by ID.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The license key, or null if not found.</returns>
    public async Task<Models.LicenseKeys.LicenseKey?> GetLicenseKeyAsync(
        string licenseKeyId,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync($"v1/customer-portal/license-keys/{licenseKeyId}", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Models.LicenseKeys.LicenseKey>(_jsonOptions, cancellationToken);
    }

    /// <summary>
    /// Validates a customer license key.
    /// </summary>
    /// <param name="request">The license key validation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validation response.</returns>
    public async Task<Models.LicenseKeys.LicenseKeyValidateResponse> ValidateLicenseKeyAsync(
        Models.LicenseKeys.LicenseKeyValidateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync("v1/customer-portal/license-keys/validate", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.LicenseKeys.LicenseKeyValidateResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Activates a customer license key.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="request">The license key activation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The activation response.</returns>
    public async Task<Models.LicenseKeys.LicenseKeyActivateResponse> ActivateLicenseKeyAsync(
        string licenseKeyId,
        Models.LicenseKeys.LicenseKeyActivateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/customer-portal/license-keys/{licenseKeyId}/activate", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.LicenseKeys.LicenseKeyActivateResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Deactivates a customer license key.
    /// </summary>
    /// <param name="licenseKeyId">The license key ID.</param>
    /// <param name="request">The license key deactivation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deactivation response.</returns>
    public async Task<Models.LicenseKeys.LicenseKeyDeactivateResponse> DeactivateLicenseKeyAsync(
        string licenseKeyId,
        Models.LicenseKeys.LicenseKeyDeactivateRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.PostAsJsonAsync($"v1/customer-portal/license-keys/{licenseKeyId}/deactivate", request, _jsonOptions, cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Models.LicenseKeys.LicenseKeyDeactivateResponse>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Lists customer's downloadable files.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of downloadable files.</returns>
    public async Task<List<Models.Files.File>> ListDownloadablesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/customer-portal/downloadables", cancellationToken),
            cancellationToken);

        (await response.HandleErrorsAsync(_jsonOptions, cancellationToken)).EnsureSuccess();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<List<Models.Files.File>>(content, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
    }

    /// <summary>
    /// Gets customer's organization information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organization information, or null if not found.</returns>
    public async Task<Models.Organizations.Organization?> GetOrganizationAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await ExecuteWithPoliciesAsync(
            () => _httpClient.GetAsync("v1/customer-portal/organizations", cancellationToken),
            cancellationToken);

        return await response.HandleNotFoundAsNullAsync<Models.Organizations.Organization>(_jsonOptions, cancellationToken);
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

    private static JsonSerializerOptions CreateJsonSerializerOptions(JsonSerializerOptions? customOptions = null)
    {
        var options = customOptions ?? new JsonSerializerOptions();
        
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        return options;
    }

    private static AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy(PolarClientOptions options)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                options.MaxRetryAttempts,
                retryAttempt => TimeSpan.FromMilliseconds(options.InitialRetryDelayMs * Math.Pow(2, retryAttempt - 1)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempt if needed
                });
    }

    private static AsyncRateLimitPolicy<HttpResponseMessage> CreateRateLimitPolicy(PolarClientOptions options)
    {
        return Policy.RateLimitAsync<HttpResponseMessage>(
            options.RequestsPerMinute,
            TimeSpan.FromMinutes(1));
    }
}

