using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using PolarSharp.Models.Common;

namespace PolarSharp.Extensions;

/// <summary>
/// Extension methods for common PolarSharp patterns.
/// </summary>
public static class PolarExtensions
{
    /// <summary>
    /// Executes an operation with retry logic for fire-and-forget scenarios.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ValueTask representing the operation.</returns>
    public static ValueTask<T> WithRetryAsync<T>(this Task<T> task, CancellationToken cancellationToken = default)
    {
        return new ValueTask<T>(task);
    }

    /// <summary>
    /// Executes an operation with retry logic for fire-and-forget scenarios.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A ValueTask representing the operation.</returns>
    public static ValueTask WithRetryAsync(this Task task, CancellationToken cancellationToken = default)
    {
        return new ValueTask(task);
    }

    /// <summary>
    /// Converts a paginated response to an async enumerable for efficient streaming.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="firstPageTask">Task that returns the first page.</param>
    /// <param name="getNextPage">Function to get the next page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of all items.</returns>
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
        this Task<PaginatedResponse<T>> firstPageTask,
        Func<PaginatedResponse<T>, Task<PaginatedResponse<T>>> getNextPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var currentPage = await firstPageTask;
        
        foreach (var item in currentPage.Items)
        {
            yield return item;
        }

        while (currentPage.Pagination.Page < currentPage.Pagination.MaxPage)
        {
            currentPage = await getNextPage(currentPage);
            foreach (var item in currentPage.Items)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Executes a batch operation with automatic chunking for large datasets.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="items">The items to process.</param>
    /// <param name="batchSize">The size of each batch.</param>
    /// <param name="processBatch">Function to process each batch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the batch operation.</returns>
    public static async Task<List<TResult>> ProcessInBatchesAsync<T, TResult>(
        this IEnumerable<T> items,
        int batchSize,
        Func<IReadOnlyList<T>, Task<TResult>> processBatch,
        CancellationToken cancellationToken = default)
    {
        var results = new List<TResult>();
        var batch = new List<T>(batchSize);

        foreach (var item in items)
        {
            batch.Add(item);
            
            if (batch.Count >= batchSize)
            {
                var result = await processBatch(batch);
                results.Add(result);
                batch.Clear();
            }
        }

        // Process remaining items
        if (batch.Count > 0)
        {
            var result = await processBatch(batch);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Creates a rate limiter for high-frequency operations.
    /// </summary>
    /// <param name="requestsPerSecond">Maximum requests per second.</param>
    /// <param name="burstCapacity">Burst capacity for the token bucket.</param>
    /// <returns>A rate limiter instance.</returns>
    public static RateLimiter CreateRateLimiter(int requestsPerSecond, int burstCapacity = 10)
    {
        return new RateLimiter(requestsPerSecond, burstCapacity);
    }
}

/// <summary>
/// Simple rate limiter implementation using token bucket algorithm.
/// </summary>
public class RateLimiter
{
    private readonly int _requestsPerSecond;
    private readonly int _burstCapacity;
    private readonly SemaphoreSlim _semaphore;
    private int _tokens;
    private DateTime _lastRefill;

    /// <summary>
    /// Initializes a new instance of RateLimiter.
    /// </summary>
    /// <param name="requestsPerSecond">Maximum requests per second.</param>
    /// <param name="burstCapacity">Burst capacity for the token bucket.</param>
    public RateLimiter(int requestsPerSecond, int burstCapacity)
    {
        _requestsPerSecond = requestsPerSecond;
        _burstCapacity = burstCapacity;
        _semaphore = new SemaphoreSlim(1, 1);
        _tokens = burstCapacity;
        _lastRefill = DateTime.UtcNow;
    }

    /// <summary>
    /// Acquires a token from the rate limiter.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when a token is available.</returns>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            RefillTokens();
            
            if (_tokens > 0)
            {
                _tokens--;
                return;
            }

            // Calculate wait time for next token
            var waitTime = TimeSpan.FromSeconds(1.0 / _requestsPerSecond);
            await Task.Delay(waitTime, cancellationToken);
            RefillTokens();
            _tokens--;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var timePassed = (now - _lastRefill).TotalSeconds;
        var tokensToAdd = (int)(timePassed * _requestsPerSecond);
        
        _tokens = Math.Min(_burstCapacity, _tokens + tokensToAdd);
        _lastRefill = now;
    }
}