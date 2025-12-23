using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;
using PolarSharp.Models.Common;

namespace PolarSharp.Extensions;

/// <summary>
/// A delegating handler that implements rate limiting with request queuing and automatic retry.
/// Uses an unbounded channel-based approach for high-performance request queuing.
/// </summary>
public class RateLimitedHttpHandler : DelegatingHandler
{
    private readonly Channel<RequestQueueItem> _requestQueue;
    private readonly ConcurrentQueue<long> _requestTimestamps;
    private readonly SemaphoreSlim _concurrencySemaphore;
    private readonly int _maxRequestsPerMinute;
    private readonly int _maxRetryAttempts;
    private readonly int _initialRetryDelayMs;
    private readonly int _maxRetryDelayMs;
    private readonly double _jitterFactor;
    private readonly bool _respectRetryAfterHeader;
    private readonly int _maxConcurrentRequests;
    private readonly long _windowTicks;
    private long _cleanupThreshold;

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
    /// <param name="maxConcurrentRequests">Maximum concurrent requests (default: 50).</param>
    public RateLimitedHttpHandler(
        int maxRequestsPerMinute = 300,
        int maxRetryAttempts = 3,
        int initialRetryDelayMs = 1000,
        int maxRetryDelayMs = 30000,
        double jitterFactor = 0.1,
        bool respectRetryAfterHeader = true,
        int maxConcurrentRequests = 50)
    {
        _maxRequestsPerMinute = maxRequestsPerMinute;
        _maxRetryAttempts = maxRetryAttempts;
        _initialRetryDelayMs = initialRetryDelayMs;
        _maxRetryDelayMs = maxRetryDelayMs;
        _jitterFactor = jitterFactor;
        _respectRetryAfterHeader = respectRetryAfterHeader;
        _maxConcurrentRequests = Math.Min(maxConcurrentRequests, maxRequestsPerMinute);
        _windowTicks = TimeSpan.FromMinutes(1).Ticks;
        _cleanupThreshold = 0;

        // Use unbounded channel for high-performance request queuing
        _requestQueue = Channel.CreateUnbounded<RequestQueueItem>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false,
            AllowSynchronousContinuations = true
        });

        // Lock-free concurrent queue for timestamp tracking
        _requestTimestamps = new ConcurrentQueue<long>();

        // Semaphore for limiting concurrent requests
        _concurrencySemaphore = new SemaphoreSlim(_maxConcurrentRequests, _maxConcurrentRequests);
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
                // Wait for rate limit slot using sliding window
                await WaitForRateLimitSlotAsync(cancellationToken);

                // Acquire concurrency semaphore
                await _concurrencySemaphore.WaitAsync(cancellationToken);

                try
                {
                    // Clone the request for retry scenarios
                    var clonedRequest = await CloneRequestAsync(request, cancellationToken);

                    // Record timestamp (lock-free)
                    RecordRequestTimestamp();

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
                    _concurrencySemaphore.Release();
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

        return response ?? throw new HttpRequestException("Request failed after maximum retry attempts");
    }

    /// <summary>
    /// Records the current timestamp in a lock-free manner.
    /// </summary>
    private void RecordRequestTimestamp()
    {
        var now = DateTime.UtcNow.Ticks;
        _requestTimestamps.Enqueue(now);

        // Periodic cleanup (every ~100 requests or when queue gets large)
        if (Interlocked.Increment(ref _cleanupThreshold) % 100 == 0 || 
            _requestTimestamps.Count > _maxRequestsPerMinute * 3)
        {
            CleanupOldTimestamps();
        }
    }

    /// <summary>
    /// Removes timestamps older than the sliding window.
    /// </summary>
    private void CleanupOldTimestamps()
    {
        var cutoff = DateTime.UtcNow.Ticks - _windowTicks;
        
        // Remove old timestamps from the front of the queue
        while (_requestTimestamps.TryPeek(out var oldestTicks) && oldestTicks < cutoff)
        {
            _requestTimestamps.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Waits until a rate limit slot is available using sliding window algorithm.
    /// </summary>
    private async Task WaitForRateLimitSlotAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var now = DateTime.UtcNow.Ticks;
            var windowStart = now - _windowTicks;

            // Clean up old timestamps
            CleanupOldTimestamps();

            // Count requests in the current window
            var requestsInWindow = CountRequestsInWindow(windowStart);

            if (requestsInWindow < _maxRequestsPerMinute)
            {
                // Slot available
                return;
            }

            // Calculate wait time until the oldest request exits the window
            if (_requestTimestamps.TryPeek(out var oldestTicks))
            {
                var waitUntil = oldestTicks + _windowTicks;
                var waitTicks = waitUntil - now;

                if (waitTicks > 0)
                {
                    var waitTime = TimeSpan.FromTicks(waitTicks);
                    // Add small buffer to ensure we're past the window
                    waitTime = waitTime.Add(TimeSpan.FromMilliseconds(50));
                    
                    // Cap wait time to prevent excessive delays
                    if (waitTime > TimeSpan.FromSeconds(30))
                    {
                        waitTime = TimeSpan.FromSeconds(30);
                    }

                    await Task.Delay(waitTime, cancellationToken);
                }
            }
            else
            {
                // Queue is empty but we thought we were at limit - race condition, just continue
                return;
            }
        }
    }

    /// <summary>
    /// Counts the number of requests in the current sliding window.
    /// </summary>
    private int CountRequestsInWindow(long windowStart)
    {
        var count = 0;
        foreach (var timestamp in _requestTimestamps)
        {
            if (timestamp >= windowStart)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Clones an HTTP request for retry scenarios.
    /// </summary>
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
            _concurrencySemaphore.Dispose();
            _requestQueue.Writer.Complete();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Represents a queued request item.
/// </summary>
internal sealed class RequestQueueItem
{
    public HttpRequestMessage Request { get; init; } = null!;
    public TaskCompletionSource<HttpResponseMessage> CompletionSource { get; init; } = null!;
    public CancellationToken CancellationToken { get; init; }
}
