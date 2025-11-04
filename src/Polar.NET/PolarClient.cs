using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Http;
using Polar.NET.Api;
using Polar.NET.Extensions;
using Polar.NET.Models.Common;
using Polar.NET.Models.Products;
using Polly;
using Polly.RateLimit;
using Polly.Retry;

namespace Polar.NET;

/// <summary>
/// A highly efficient and easy-to-use REST API client for the Polar.sh payments platform.
/// </summary>
public class PolarClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PolarClientOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly DateTime[] _requestTimestamps;
    private int _requestIndex;
    private readonly object _lockObject = new();
    private bool _disposed;

    /// <summary>
    /// Gets the products API client.
    /// </summary>
    public ProductsApi Products { get; }

    /// <summary>
    /// Gets the orders API client.
    /// </summary>
    public OrdersApi Orders { get; }

    /// <summary>
    /// Gets the subscriptions API client.
    /// </summary>
    public SubscriptionsApi Subscriptions { get; }

    /// <summary>
    /// Gets the checkouts API client.
    /// </summary>
    public CheckoutsApi Checkouts { get; }

    /// <summary>
    /// Gets the checkout links API client.
    /// </summary>
    public CheckoutLinksApi CheckoutLinks { get; }

    /// <summary>
    /// Gets the benefits API client.
    /// </summary>
    public BenefitsApi Benefits { get; }

    /// <summary>
    /// Gets the customers API client.
    /// </summary>
    public CustomersApi Customers { get; }

    /// <summary>
    /// Gets the customer sessions API client.
    /// </summary>
    public CustomerSessionsApi CustomerSessions { get; }

    /// <summary>
    /// Gets the license keys API client.
    /// </summary>
    public LicenseKeysApi LicenseKeys { get; }

    /// <summary>
    /// Gets the files API client.
    /// </summary>
    public FilesApi Files { get; }

    /// <summary>
    /// Gets the organizations API client.
    /// </summary>
    public OrganizationsApi Organizations { get; }

    /// <summary>
    /// Gets the payments API client.
    /// </summary>
    public PaymentsApi Payments { get; }

    /// <summary>
    /// Gets the refunds API client.
    /// </summary>
    public RefundsApi Refunds { get; }

    /// <summary>
    /// Gets the discounts API client.
    /// </summary>
    public DiscountsApi Discounts { get; }

    /// <summary>
    /// Gets the webhooks API client.
    /// </summary>
    public WebhooksApi Webhooks { get; }

    /// <summary>
    /// Gets the meters API client.
    /// </summary>
    public MetersApi Meters { get; }

    /// <summary>
    /// Gets the customer meters API client.
    /// </summary>
    public CustomerMetersApi CustomerMeters { get; }

    /// <summary>
    /// Gets the events API client.
    /// </summary>
    public EventsApi Events { get; }

    /// <summary>
    /// Gets the metrics API client.
    /// </summary>
    public MetricsApi Metrics { get; }

    /// <summary>
    /// Gets the custom fields API client.
    /// </summary>
    public CustomFieldsApi CustomFields { get; }

    /// <summary>
    /// Gets the OAuth2 API client.
    /// </summary>
    public OAuth2Api OAuth2 { get; }

    /// <summary>
    /// Gets the seats API client.
    /// </summary>
    public SeatsApi Seats { get; }

    /// <summary>
    /// Gets the customer seats API client.
    /// </summary>
    public CustomerSeatsApi CustomerSeats { get; }

    /// <summary>
    /// Gets the current rate limit status.
    /// </summary>
    public (int Available, int Limit) RateLimitStatus
    {
        get
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;
                var oneMinuteAgo = now.AddMinutes(-1);
                var recentRequests = _requestTimestamps.Count(t => t > oneMinuteAgo);
                return (_options.RequestsPerMinute - recentRequests, _options.RequestsPerMinute);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolarClient"/> class.
    /// </summary>
    /// <param name="accessToken">The API access token for authentication.</param>
    /// <param name="baseUrl">The base URL for the Polar API. If not specified, uses the default production URL.</param>
    /// <param name="httpClient">Optional HttpClient instance. If not provided, one will be created.</param>
    /// <param name="options">Optional client options.</param>
    public PolarClient(
        string accessToken,
        Uri? baseUrl = null,
        HttpClient? httpClient = null,
        PolarClientOptions? options = null)
    {
        _options = options ?? new PolarClientOptions { AccessToken = accessToken };
        
        if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            throw new ArgumentException("Access token is required.", nameof(accessToken));
        }

        _httpClient = httpClient ?? CreateHttpClient();
        ConfigureHttpClient(_httpClient, baseUrl ?? GetDefaultBaseUrl());
        
        _jsonOptions = CreateJsonSerializerOptions(_options.JsonSerializerOptions);
        _retryPolicy = CreateRetryPolicy();
        _rateLimitPolicy = CreateRateLimitPolicy();
        
        // Initialize rate limiting
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        _requestTimestamps = new DateTime[_options.RequestsPerMinute];
        _requestIndex = 0;

        Products = new ProductsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Orders = new OrdersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Subscriptions = new SubscriptionsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Checkouts = new CheckoutsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CheckoutLinks = new CheckoutLinksApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Benefits = new BenefitsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Customers = new CustomersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomerSessions = new CustomerSessionsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        LicenseKeys = new LicenseKeysApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Files = new FilesApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Organizations = new OrganizationsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Payments = new PaymentsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Refunds = new RefundsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Discounts = new DiscountsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Webhooks = new WebhooksApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Meters = new MetersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomerMeters = new CustomerMetersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Events = new EventsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Metrics = new MetricsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomFields = new CustomFieldsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        OAuth2 = new OAuth2Api(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Seats = new SeatsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomerSeats = new CustomerSeatsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PolarClient"/> class using IHttpClientFactory.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
    /// <param name="accessToken">The API access token for authentication.</param>
    /// <param name="baseUrl">The base URL for Polar API. If not specified, uses the default production URL.</param>
    /// <param name="httpClientName">Optional name for the HTTP client. If not specified, uses "PolarClient".</param>
    /// <param name="options">Optional client options.</param>
    public PolarClient(
        IHttpClientFactory httpClientFactory,
        string accessToken,
        Uri? baseUrl = null,
        string? httpClientName = null,
        PolarClientOptions? options = null)
    {
        _options = options ?? new PolarClientOptions { AccessToken = accessToken };
        
        if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            throw new ArgumentException("Access token is required.", nameof(accessToken));
        }

        var clientName = httpClientName ?? "PolarClient";
        _httpClient = httpClientFactory.CreateClient(clientName);
        ConfigureHttpClient(_httpClient, baseUrl ?? GetDefaultBaseUrl());
        
        _jsonOptions = CreateJsonSerializerOptions(_options.JsonSerializerOptions);
        _retryPolicy = CreateRetryPolicy();
        _rateLimitPolicy = CreateRateLimitPolicy();
        
        // Initialize rate limiting
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        _requestTimestamps = new DateTime[_options.RequestsPerMinute];
        _requestIndex = 0;

        Products = new ProductsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Orders = new OrdersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Subscriptions = new SubscriptionsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Checkouts = new CheckoutsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CheckoutLinks = new CheckoutLinksApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Benefits = new BenefitsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Customers = new CustomersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomerSessions = new CustomerSessionsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        LicenseKeys = new LicenseKeysApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Files = new FilesApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Organizations = new OrganizationsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Payments = new PaymentsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Refunds = new RefundsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Discounts = new DiscountsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Webhooks = new WebhooksApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Meters = new MetersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomerMeters = new CustomerMetersApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Events = new EventsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Metrics = new MetricsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomFields = new CustomFieldsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        OAuth2 = new OAuth2Api(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        Seats = new SeatsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
        CustomerSeats = new CustomerSeatsApi(_httpClient, _jsonOptions, _retryPolicy, _rateLimitPolicy);
    }

    /// <summary>
    /// Creates a new <see cref="PolarClientBuilder"/> for fluent configuration.
    /// </summary>
    /// <returns>A new instance of <see cref="PolarClientBuilder"/>.</returns>
    public static PolarClientBuilder Create() => new();

    private static Uri GetDefaultBaseUrl()
    {
        return new Uri("https://api.polar.sh");
    }

    private HttpClient CreateHttpClient()
    {
        return new HttpClient();
    }

    private void ConfigureHttpClient(HttpClient client, Uri baseUrl)
    {
        client.BaseAddress = baseUrl;
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        
        var token = string.IsNullOrWhiteSpace(_options.AccessToken) ? string.Empty : _options.AccessToken;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", _options.UserAgent ?? "Polar.NET/1.0.0");
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions(JsonSerializerOptions? customOptions = null)
    {
        var options = customOptions ?? new JsonSerializerOptions();
        
        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        // Add custom enum converter to handle JsonPropertyName attributes
        options.Converters.Add(new JsonStringEnumConverterWithAttributeNames());
        
        return options;
    }

    private AsyncRetryPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(response => 
                response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                response.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                response.StatusCode == System.Net.HttpStatusCode.BadGateway ||
                response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
            .WaitAndRetryAsync(
                _options.MaxRetryAttempts,
                retryAttempt => CalculateRetryDelay(retryAttempt),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Could add logging here
                    if (outcome.Result?.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        // Handle rate limit specifically
                        var retryAfter = outcome.Result.Headers.RetryAfter;
                        if (retryAfter?.Delta.HasValue == true)
                        {
                            // Use the server-provided retry delay
                        }
                    }
                });
    }

    private TimeSpan CalculateRetryDelay(int retryAttempt)
    {
        // Exponential backoff with jitter
        var baseDelay = TimeSpan.FromMilliseconds(_options.InitialRetryDelayMs * Math.Pow(2, retryAttempt - 1));
        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, (int)(baseDelay.TotalMilliseconds * 0.1)));
        return baseDelay + jitter;
    }

    private AsyncRateLimitPolicy<HttpResponseMessage> CreateRateLimitPolicy()
    {
        return Policy.RateLimitAsync<HttpResponseMessage>(
            _options.RequestsPerMinute,
            TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Implements rate limiting using sliding window approach.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the rate limiting operation.</returns>
    private async Task WaitForRateLimitAsync(CancellationToken cancellationToken = default)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        try
        {
            DateTime waitTime;
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;
                var oneMinuteAgo = now.AddMinutes(-1);
                
                // Clean up old timestamps
                for (int i = 0; i < _requestTimestamps.Length; i++)
                {
                    if (_requestTimestamps[i] <= oneMinuteAgo)
                    {
                        _requestTimestamps[i] = DateTime.MinValue;
                    }
                }
                
                // Count recent requests
                var recentRequests = _requestTimestamps.Count(t => t > oneMinuteAgo);
                
                if (recentRequests >= _options.RequestsPerMinute)
                {
                    // Calculate wait time until oldest request expires
                    var oldestRequest = _requestTimestamps.Where(t => t > DateTime.MinValue).Min();
                    waitTime = oldestRequest.AddMinutes(1);
                }
                else
                {
                    waitTime = now;
                }
                
                // Record this request
                _requestTimestamps[_requestIndex] = now;
                _requestIndex = (_requestIndex + 1) % _requestTimestamps.Length;
            }
            
            var actualWaitTime = waitTime - DateTime.UtcNow;
            if (actualWaitTime > TimeSpan.Zero)
            {
                await Task.Delay(actualWaitTime, cancellationToken);
            }
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    /// <summary>
    /// Releases all resources used by the PolarClient.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _rateLimitSemaphore?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Builder for creating and configuring a PolarClient with fluent API.
/// </summary>
public class PolarClientBuilder
{
    private string _accessToken = string.Empty;
    private Uri? _baseUrl;
    private HttpClient? _httpClient;
    private IHttpClientFactory? _httpClientFactory;
    private string _httpClientName = "PolarClient";
    private PolarClientOptions _options = new();

    /// <summary>
    /// Sets the access token for authentication.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithToken(string accessToken)
    {
        _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        return this;
    }

    /// <summary>
    /// Sets the base URL for the Polar API.
    /// </summary>
    /// <param name="baseUrl">The base URL.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithBaseUrl(Uri baseUrl)
    {
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        return this;
    }

    /// <summary>
    /// Sets the base URL for the Polar API.
    /// </summary>
    /// <param name="baseUrl">The base URL string.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithBaseUrl(string baseUrl)
    {
        _baseUrl = new Uri(baseUrl);
        return this;
    }

    /// <summary>
    /// Sets the API environment.
    /// </summary>
    /// <param name="environment">The environment to use.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithEnvironment(PolarEnvironment environment)
    {
        _options = _options with { Environment = environment };
        _baseUrl = environment switch
        {
            PolarEnvironment.Production => new Uri("https://api.polar.sh"),
            PolarEnvironment.Sandbox => new Uri("https://sandbox-api.polar.sh"),
            _ => throw new ArgumentException($"Unknown environment: {environment}")
        };
        return this;
    }

    /// <summary>
    /// Sets the HTTP client to use.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        return this;
    }

    /// <summary>
    /// Sets the HTTP client factory to use for creating HTTP clients.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="clientName">Optional name for the HTTP client. Defaults to "PolarClient".</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithHttpClientFactory(IHttpClientFactory httpClientFactory, string? clientName = null)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _httpClientName = clientName ?? "PolarClient";
        return this;
    }

    /// <summary>
    /// Sets the timeout for HTTP requests.
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithTimeout(int timeoutSeconds)
    {
        _options = _options with { TimeoutSeconds = timeoutSeconds };
        return this;
    }

    /// <summary>
    /// Sets the maximum number of retry attempts.
    /// </summary>
    /// <param name="maxRetries">Maximum retry attempts.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithMaxRetries(int maxRetries)
    {
        _options = _options with { MaxRetryAttempts = maxRetries };
        return this;
    }



    /// <summary>
    /// Builds the configured PolarClient instance.
    /// </summary>
    /// <returns>A configured PolarClient instance.</returns>
    public PolarClient Build()
    {
        var finalOptions = string.IsNullOrWhiteSpace(_accessToken) 
            ? _options 
            : _options with { AccessToken = _accessToken };
            
        if (_httpClientFactory != null)
        {
            return new PolarClient(_httpClientFactory, _accessToken, _baseUrl, _httpClientName, finalOptions);
        }
        
        return new PolarClient(_accessToken, _baseUrl, _httpClient, finalOptions);
    }

    /// <summary>
    /// Sets the custom user agent string.
    /// </summary>
    /// <param name="userAgent">User agent string.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithUserAgent(string userAgent)
    {
        _options = _options with { UserAgent = userAgent };
        return this;
    }

    /// <summary>
    /// Sets custom JSON serializer options.
    /// </summary>
    /// <param name="jsonOptions">JSON serializer options.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithJsonOptions(JsonSerializerOptions jsonOptions)
    {
        _options = _options with { JsonSerializerOptions = jsonOptions };
        return this;
    }

    /// <summary>
    /// Sets the access token for authentication.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithAccessToken(string accessToken)
    {
        _options = _options with { AccessToken = accessToken };
        return this;
    }

    /// <summary>
    /// Sets the initial retry delay in milliseconds.
    /// </summary>
    /// <param name="delayMs">Initial delay in milliseconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithInitialRetryDelay(int delayMs)
    {
        _options = _options with { InitialRetryDelayMs = delayMs };
        return this;
    }

    /// <summary>
    /// Sets the requests per minute limit.
    /// </summary>
    /// <param name="requestsPerMinute">Maximum requests per minute.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder WithRequestsPerMinute(int requestsPerMinute)
    {
        _options = _options with { RequestsPerMinute = requestsPerMinute };
        return this;
    }

    /// <summary>
    /// Configures the builder to use the sandbox environment.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder UseSandbox()
    {
        return WithEnvironment(PolarEnvironment.Sandbox);
    }

    /// <summary>
    /// Configures the builder to use the production environment.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public PolarClientBuilder UseProduction()
    {
        return WithEnvironment(PolarEnvironment.Production);
    }


}