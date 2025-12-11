using System.Net;
using System.Threading.Channels;
using PolarSharp.Models.Common;

namespace PolarSharp.Extensions;

/// <summary>
/// A delegating handler that implements rate limiting with request queuing and automatic retry.
/// Uses a channel-based approach to queue requests and process them at a controlled rate.
/// </summary>
public class RateLimitedHttpHandler : DelegatingHandler
{
    private readonly SemaphoreSlim _requestSemaphore;
    private readonly Channel<DateTime> _requestTimestamps;
    private readonly int _maxRequestsPerMinute;
    private readonly int _maxRetryAttempts;
    private readonly int _initialRetryDelayMs;
    private readonly int _maxRetryDelayMs;
    private readonly double _jitterFactor;
    private readonly bool _respectRetryAfterHeader;
    private readonly SemaphoreSlim _cleanupSemaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitedHttpHandler"/> class.
    /// </summary>
    /// <param name="options">The client options containing rate limiting configuration.</param>
    public RateLimitedHttpHandler(PolarClientOptions options)
        : this(options.RequestsPerMinute, options.MaxRetryAttempts, options.InitialRetryDelayMs, 
               options.MaxRetryDelayMs, options.JitterFactor, options.RespectRetryAfterHeader)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitedHttpHandler"/> class.
    /// </summary>
    /// <param name="maxRequestsPerMinute">Maximum requests per minute.</param>
    /// <param name="maxRetryAttempts">Maximum retry attempts.</param>
    /// <param name="initialRetryDelayMs">Initial retry delay in milliseconds.</param>
    /// <param name="maxRetryDelayMs">Maximum retry delay in milliseconds.</param>
    /// <param name="jitterFactor">Jitter factor for retry delays.</param>
    /// <param name="respectRetryAfterHeader">Whether to respect Retry-After header.</param>
    public RateLimitedHttpHandler(
        int maxRequestsPerMinute = 300,
        int maxRetryAttempts = 3,
        int initialRetryDelayMs = 1000,
        int maxRetryDelayMs = 30000,
        double jitterFactor = 0.1,
        bool respectRetryAfterHeader = true)
    {
        _maxRequestsPerMinute = maxRequestsPerMinute;
        _maxRetryAttempts = maxRetryAttempts;
        _initialRetryDelayMs = initialRetryDelayMs;
        _maxRetryDelayMs = maxRetryDelayMs;
        _jitterFactor = jitterFactor;
        _respectRetryAfterHeader = respectRetryAfterHeader;

        // Allow some concurrent requests but not more than the rate limit
        _requestSemaphore = new SemaphoreSlim(Math.Min(maxRequestsPerMinute, 50), Math.Min(maxRequestsPerMinute, 50));
        
        // Bounded channel to track request timestamps for sliding window rate limiting
        _requestTimestamps = Channel.CreateBounded<DateTime>(new BoundedChannelOptions(maxRequestsPerMinute * 2)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false
        });
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var retryCount = 0;
        HttpResponseMessage? response = null;

        while (retryCount <= _maxRetryAttempts)
        {
            try
            {
                // Wait for rate limit slot
                await WaitForRateLimitSlotAsync(cancellationToken);
                
                // Acquire semaphore to limit concurrent requests
                await _requestSemaphore.WaitAsync(cancellationToken);
                
                try
                {
                    // Clone the request for retry scenarios (request content can only be read once)
                    var clonedRequest = await CloneRequestAsync(request, cancellationToken);
                    
                    // Record this request timestamp
                    await _requestTimestamps.Writer.WriteAsync(DateTime.UtcNow, cancellationToken);
                    
                    // Send the request
                    response = await base.SendAsync(clonedRequest, cancellationToken);
                    
                    // Check if we should retry
                    if (!ShouldRetry(response.StatusCode) || retryCount >= _maxRetryAttempts)
                    {
                        return response;
                    }
                    
                    // Calculate retry delay
                    var delay = CalculateRetryDelay(retryCount + 1, response);
                    
                    // Dispose the response before retry
                    response.Dispose();
                    
                    // Wait before retrying
                    await Task.Delay(delay, cancellationToken);
                    retryCount++;
                }
                finally
                {
                    _requestSemaphore.Release();
                }
            }
            catch (HttpRequestException) when (retryCount < _maxRetryAttempts)
            {
                // Network error - retry with backoff
                var delay = CalculateRetryDelay(retryCount + 1, null);
                await Task.Delay(delay, cancellationToken);
                retryCount++;
            }
        }

        // Return the last response or throw if null
        return response ?? throw new HttpRequestException("Request failed after maximum retry attempts");
    }

    private async Task WaitForRateLimitSlotAsync(CancellationToken cancellationToken)
    {
        // Clean up old timestamps and calculate if we need to wait
        await _cleanupSemaphore.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var oneMinuteAgo = now.AddMinutes(-1);
            var timestamps = new List<DateTime>();
            
            // Read all current timestamps
            while (_requestTimestamps.Reader.TryRead(out var timestamp))
            {
                if (timestamp > oneMinuteAgo)
                {
                    timestamps.Add(timestamp);
                }
            }
            
            // Write back valid timestamps
            foreach (var ts in timestamps)
            {
                await _requestTimestamps.Writer.WriteAsync(ts, cancellationToken);
            }
            
            // If we're at or over the limit, calculate wait time
            if (timestamps.Count >= _maxRequestsPerMinute)
            {
                var oldestTimestamp = timestamps.Min();
                var waitUntil = oldestTimestamp.AddMinutes(1);
                var waitTime = waitUntil - now;
                
                if (waitTime > TimeSpan.Zero)
                {
                    // Add a small buffer
                    waitTime = waitTime.Add(TimeSpan.FromMilliseconds(100));
                    await Task.Delay(waitTime, cancellationToken);
                }
            }
        }
        finally
        {
            _cleanupSemaphore.Release();
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content if present
        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            clone.Content = new ByteArrayContent(contentBytes);
            
            // Copy content headers
            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Copy options
        foreach (var option in request.Options)
        {
            clone.Options.TryAdd(option.Key, option.Value);
        }

        return clone;
    }

    private bool ShouldRetry(HttpStatusCode statusCode)
    {
        return RateLimitHelper.ShouldRetry(statusCode);
    }

    private TimeSpan CalculateRetryDelay(int retryAttempt, HttpResponseMessage? response)
    {
        // Check for Retry-After header on 429 responses
        if (_respectRetryAfterHeader && response?.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryDelay = RateLimitHelper.ExtractRetryDelay(response.Headers.RetryAfter, _maxRetryDelayMs);
            if (retryDelay.HasValue)
            {
                // Add a small buffer to ensure we wait long enough
                return retryDelay.Value + TimeSpan.FromMilliseconds(100);
            }
        }

        // Use exponential backoff with jitter
        return RateLimitHelper.CalculateExponentialBackoffWithJitter(
            retryAttempt,
            _initialRetryDelayMs,
            _maxRetryDelayMs,
            _jitterFactor);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _requestSemaphore.Dispose();
            _cleanupSemaphore.Dispose();
        }
        base.Dispose(disposing);
    }
}
