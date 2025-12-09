using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PolarSharp.Models.Common;
using System;
using System.Text.Json;

namespace PolarSharp.Extensions;

/// <summary>
/// Builder for creating PolarClientOptions with mutable properties.
/// </summary>
public class PolarClientOptionsBuilder
{
    /// <summary>
    /// Gets or sets the API access token for authentication.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base URL for the Polar API.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the API environment to use.
    /// </summary>
    public PolarEnvironment Environment { get; set; } = PolarEnvironment.Production;

    /// <summary>
    /// Gets or sets the timeout for HTTP requests in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the initial delay for retry attempts in milliseconds.
    /// </summary>
    public int InitialRetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum number of requests per minute.
    /// </summary>
    public int RequestsPerMinute { get; set; } = 300;

    /// <summary>
    /// Gets or sets the custom user agent string.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the custom JSON serializer options.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry delay in milliseconds.
    /// </summary>
    public int MaxRetryDelayMs { get; set; } = 30000;

    /// <summary>
    /// Gets or sets the jitter factor for retry delays.
    /// </summary>
    public double JitterFactor { get; set; } = 0.1;

    /// <summary>
    /// Gets or sets whether to respect server-provided Retry-After header.
    /// </summary>
    public bool RespectRetryAfterHeader { get; set; } = true;

    /// <summary>
    /// Builds the immutable PolarClientOptions from the builder settings.
    /// </summary>
    /// <returns>A configured PolarClientOptions instance.</returns>
    public PolarClientOptions Build()
    {
        return new PolarClientOptions
        {
            AccessToken = AccessToken,
            BaseUrl = BaseUrl,
            Environment = Environment,
            TimeoutSeconds = TimeoutSeconds,
            MaxRetryAttempts = MaxRetryAttempts,
            InitialRetryDelayMs = InitialRetryDelayMs,
            RequestsPerMinute = RequestsPerMinute,
            UserAgent = UserAgent,
            JsonSerializerOptions = JsonSerializerOptions,
            MaxRetryDelayMs = MaxRetryDelayMs,
            JitterFactor = JitterFactor,
            RespectRetryAfterHeader = RespectRetryAfterHeader
        };
    }
}

/// <summary>
/// Extension methods for registering PolarClient with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PolarClient to the service collection with default configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="accessToken">The API access token for authentication.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPolarClient(
        this IServiceCollection services,
        string accessToken)
    {
        return services.AddPolarClient(options =>
        {
            options.AccessToken = accessToken;
        });
    }

    /// <summary>
    /// Adds PolarClient to the service collection with configuration from action.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure PolarClient options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPolarClient(
        this IServiceCollection services,
        Action<PolarClientOptionsBuilder>? configureOptions = null)
    {
        // Configure options using builder pattern
        var optionsBuilder = new PolarClientOptionsBuilder();
        configureOptions?.Invoke(optionsBuilder);
        var options = optionsBuilder.Build();

        // Validate required options
        if (string.IsNullOrWhiteSpace(options.AccessToken))
        {
            throw new ArgumentException("Access token is required.", nameof(options.AccessToken));
        }

        // Register options as singleton
        services.AddSingleton(options);

        // Add HttpClient with proper configuration
        services.AddHttpClient("PolarClient", client =>
        {
            var baseUrl = options.BaseUrl ?? options.Environment switch
            {
                PolarEnvironment.Production => new Uri("https://api.polar.sh"),
                PolarEnvironment.Sandbox => new Uri("https://sandbox-api.polar.sh"),
                _ => new Uri("https://api.polar.sh")
            };

            client.BaseAddress = baseUrl;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.AccessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent ?? "PolarSharp/1.0.0");
        });

        // Register PolarClient as scoped service
        services.AddScoped<IPolarClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var clientOptions = provider.GetRequiredService<PolarClientOptions>();
            
            return new PolarClient(
                httpClientFactory,
                clientOptions.AccessToken,
                clientOptions.BaseUrl,
                "PolarClient",
                clientOptions);
        });
        services.AddScoped<PolarClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var clientOptions = provider.GetRequiredService<PolarClientOptions>();
            
            return new PolarClient(
                httpClientFactory,
                clientOptions.AccessToken,
                clientOptions.BaseUrl,
                "PolarClient",
                clientOptions);
        });

        return services;
    }

    /// <summary>
    /// Adds PolarClient to the service collection with pre-configured options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">Pre-configured PolarClient options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPolarClient(
        this IServiceCollection services,
        PolarClientOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        // Validate required options
        if (string.IsNullOrWhiteSpace(options.AccessToken))
        {
            throw new ArgumentException("Access token is required.", nameof(options.AccessToken));
        }

        // Register options as singleton
        services.AddSingleton(options);

        // Add HttpClient with proper configuration
        services.AddHttpClient("PolarClient", client =>
        {
            var baseUrl = options.BaseUrl ?? options.Environment switch
            {
                PolarEnvironment.Production => new Uri("https://api.polar.sh"),
                PolarEnvironment.Sandbox => new Uri("https://sandbox-api.polar.sh"),
                _ => new Uri("https://api.polar.sh")
            };

            client.BaseAddress = baseUrl;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.AccessToken);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", options.UserAgent ?? "PolarSharp/1.0.0");
        });

        // Register PolarClient as scoped service
        services.AddScoped<IPolarClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var clientOptions = provider.GetRequiredService<PolarClientOptions>();
            
            return new PolarClient(
                httpClientFactory,
                clientOptions.AccessToken,
                clientOptions.BaseUrl,
                "PolarClient",
                clientOptions);
        });
        services.AddScoped<PolarClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var clientOptions = provider.GetRequiredService<PolarClientOptions>();
            
            return new PolarClient(
                httpClientFactory,
                clientOptions.AccessToken,
                clientOptions.BaseUrl,
                "PolarClient",
                clientOptions);
        });

        return services;
    }

    /// <summary>
    /// Adds PolarClient to the service collection for sandbox environment.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="accessToken">The API access token for authentication.</param>
    /// <param name="configureOptions">Optional action to configure additional options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPolarClientSandbox(
        this IServiceCollection services,
        string accessToken,
        Action<PolarClientOptionsBuilder>? configureOptions = null)
    {
        return services.AddPolarClient(options =>
        {
            options.AccessToken = accessToken;
            options.Environment = PolarEnvironment.Sandbox;
            configureOptions?.Invoke(options);
        });
    }

    /// <summary>
    /// Adds PolarClient to the service collection for production environment.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="accessToken">The API access token for authentication.</param>
    /// <param name="configureOptions">Optional action to configure additional options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddPolarClientProduction(
        this IServiceCollection services,
        string accessToken,
        Action<PolarClientOptionsBuilder>? configureOptions = null)
    {
        return services.AddPolarClient(options =>
        {
            options.AccessToken = accessToken;
            options.Environment = PolarEnvironment.Production;
            configureOptions?.Invoke(options);
        });
    }
}

/// <summary>
/// Interface for PolarClient to support dependency injection and mocking.
/// </summary>
public interface IPolarClient
{
    /// <summary>
    /// Gets the products API client.
    /// </summary>
    Api.ProductsApi Products { get; }

    /// <summary>
    /// Gets the orders API client.
    /// </summary>
    Api.OrdersApi Orders { get; }

    /// <summary>
    /// Gets the subscriptions API client.
    /// </summary>
    Api.SubscriptionsApi Subscriptions { get; }

    /// <summary>
    /// Gets the checkouts API client.
    /// </summary>
    Api.CheckoutsApi Checkouts { get; }

    /// <summary>
    /// Gets the checkout links API client.
    /// </summary>
    Api.CheckoutLinksApi CheckoutLinks { get; }

    /// <summary>
    /// Gets the benefits API client.
    /// </summary>
    Api.BenefitsApi Benefits { get; }

    /// <summary>
    /// Gets the customers API client.
    /// </summary>
    Api.CustomersApi Customers { get; }

    /// <summary>
    /// Gets the customer sessions API client.
    /// </summary>
    Api.CustomerSessionsApi CustomerSessions { get; }

    /// <summary>
    /// Gets the license keys API client.
    /// </summary>
    Api.LicenseKeysApi LicenseKeys { get; }

    /// <summary>
    /// Gets the files API client.
    /// </summary>
    Api.FilesApi Files { get; }

    /// <summary>
    /// Gets the organizations API client.
    /// </summary>
    Api.OrganizationsApi Organizations { get; }

    /// <summary>
    /// Gets the payments API client.
    /// </summary>
    Api.PaymentsApi Payments { get; }

    /// <summary>
    /// Gets the refunds API client.
    /// </summary>
    Api.RefundsApi Refunds { get; }

    /// <summary>
    /// Gets the discounts API client.
    /// </summary>
    Api.DiscountsApi Discounts { get; }

    /// <summary>
    /// Gets the webhooks API client.
    /// </summary>
    Api.WebhooksApi Webhooks { get; }

    /// <summary>
    /// Gets the meters API client.
    /// </summary>
    Api.MetersApi Meters { get; }

    /// <summary>
    /// Gets the customer meters API client.
    /// </summary>
    Api.CustomerMetersApi CustomerMeters { get; }

    /// <summary>
    /// Gets the events API client.
    /// </summary>
    Api.EventsApi Events { get; }

    /// <summary>
    /// Gets the metrics API client.
    /// </summary>
    Api.MetricsApi Metrics { get; }

    /// <summary>
    /// Gets the custom fields API client.
    /// </summary>
    Api.CustomFieldsApi CustomFields { get; }

    /// <summary>
    /// Gets the OAuth2 API client.
    /// </summary>
    Api.OAuth2Api OAuth2 { get; }

    /// <summary>
    /// Gets the seats API client.
    /// </summary>
    Api.SeatsApi Seats { get; }

    /// <summary>
    /// Gets the customer seats API client.
    /// </summary>
    Api.CustomerSeatsApi CustomerSeats { get; }

    /// <summary>
    /// Gets the current rate limit status.
    /// </summary>
    (int Available, int Limit, TimeSpan? ResetTime) RateLimitStatus { get; }
}