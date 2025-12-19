using System.Net;

namespace PolarSharp.Results;

/// <summary>
/// Extension methods for PolarResult types.
/// </summary>
public static class PolarResultExtensions
{
    /// <summary>
    /// Converts a nullable value to a result, treating null as not found.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="notFoundMessage">The message for not found error.</param>
    /// <returns>A PolarResult containing the value or a not found error.</returns>
    public static PolarResult<T> ToResult<T>(this T? value, string notFoundMessage = "Resource not found")
        where T : class
    {
        return value is not null
            ? PolarResult<T>.Success(value)
            : PolarResult<T>.Failure(PolarError.NotFound(notFoundMessage));
    }

    /// <summary>
    /// Gets the value or null for not found errors. Throws for other errors.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>The value if successful, null if not found, throws for other errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown for non-not-found errors.</exception>
    public static T? ValueOrNullIfNotFound<T>(this PolarResult<T> result) where T : class
    {
        if (result.IsSuccess) return result.Value;
        if (result.IsNotFoundError) return null;
        throw new InvalidOperationException($"API Error: {result.Error!.Message} (Status: {result.Error.StatusCode})");
    }

    /// <summary>
    /// Ensures the result is successful, throwing if not.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>The same result if successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is failed.</exception>
    public static PolarResult<T> EnsureSuccess<T>(this PolarResult<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"API Error: {result.Error!.Message} (Status: {result.Error.StatusCode})");
        return result;
    }

    /// <summary>
    /// Ensures the result is successful, throwing if not.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>The same result if successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is failed.</exception>
    public static PolarResult EnsureSuccess(this PolarResult result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"API Error: {result.Error!.Message} (Status: {result.Error.StatusCode})");
        return result;
    }

    /// <summary>
    /// Gets the value or throws with detailed error information.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>The value if successful.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the result is failed.</exception>
    public static T GetValueOrThrow<T>(this PolarResult<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"API Error: {result.Error!.Message} (Status: {result.Error.StatusCode})");
        return result.Value;
    }

    /// <summary>
    /// Combines multiple results, returning success only if all succeed.
    /// Returns the first failure encountered.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>Success if all results succeeded, otherwise the first failure.</returns>
    public static PolarResult Combine(params PolarResult[] results)
    {
        foreach (var result in results)
        {
            if (result.IsFailure) return result;
        }
        return PolarResult.Success();
    }

    /// <summary>
    /// Awaitable extension for getting value or throwing.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <returns>The value if successful.</returns>
    public static async Task<T> GetValueOrThrowAsync<T>(this Task<PolarResult<T>> resultTask)
    {
        var result = await resultTask;
        return result.GetValueOrThrow();
    }

    /// <summary>
    /// Awaitable extension for getting value or null for not found.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <returns>The value if successful, null if not found.</returns>
    public static async Task<T?> ValueOrNullIfNotFoundAsync<T>(this Task<PolarResult<T>> resultTask)
        where T : class
    {
        var result = await resultTask;
        return result.ValueOrNullIfNotFound();
    }

    /// <summary>
    /// Awaitable extension for ensuring success.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <returns>The result if successful.</returns>
    public static async Task<PolarResult<T>> EnsureSuccessAsync<T>(this Task<PolarResult<T>> resultTask)
    {
        var result = await resultTask;
        return result.EnsureSuccess();
    }

    /// <summary>
    /// Awaitable extension for ensuring success.
    /// </summary>
    /// <param name="resultTask">The task containing the result.</param>
    /// <returns>The result if successful.</returns>
    public static async Task<PolarResult> EnsureSuccessAsync(this Task<PolarResult> resultTask)
    {
        var result = await resultTask;
        return result.EnsureSuccess();
    }

    /// <summary>
    /// Maps the value if successful (async).
    /// </summary>
    /// <typeparam name="T">The type of the original value.</typeparam>
    /// <typeparam name="TNew">The type of the new value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new result with the mapped value.</returns>
    public static async Task<PolarResult<TNew>> MapAsync<T, TNew>(
        this Task<PolarResult<T>> resultTask,
        Func<T, TNew> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Binds to another result if successful (async).
    /// </summary>
    /// <typeparam name="T">The type of the original value.</typeparam>
    /// <typeparam name="TNew">The type of the new value.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound result.</returns>
    public static async Task<PolarResult<TNew>> BindAsync<T, TNew>(
        this Task<PolarResult<T>> resultTask,
        Func<T, Task<PolarResult<TNew>>> binder)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return PolarResult<TNew>.Failure(result.Error!);
        return await binder(result.Value);
    }

    /// <summary>
    /// Checks if the result indicates an API limitation (auth error, method not allowed, not found).
    /// Useful for sandbox environment tests.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is an API limitation error.</returns>
    public static bool IsApiLimitationError<T>(this PolarResult<T> result)
    {
        if (result.IsSuccess) return false;

        var error = result.Error;
        if (error == null) return false;

        return error.IsAuthError ||
               error.IsMethodNotAllowedError ||
               error.IsNotFoundError ||
               (error.Message?.Contains("RequestValidationError") ?? false) ||
               (error.Message?.Contains("NotOpenToPublic") ?? false);
    }

    /// <summary>
    /// Checks if the result indicates an API limitation (non-generic version).
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns>True if the result is an API limitation error.</returns>
    public static bool IsApiLimitationError(this PolarResult result)
    {
        if (result.IsSuccess) return false;

        var error = result.Error;
        if (error == null) return false;

        return error.IsAuthError ||
               error.IsMethodNotAllowedError ||
               error.IsNotFoundError ||
               (error.Message?.Contains("RequestValidationError") ?? false) ||
               (error.Message?.Contains("NotOpenToPublic") ?? false);
    }

    /// <summary>
    /// Converts a failed result to a different type while preserving the error.
    /// </summary>
    /// <typeparam name="T">The original type.</typeparam>
    /// <typeparam name="TNew">The new type.</typeparam>
    /// <param name="result">The failed result.</param>
    /// <returns>A new result with the same error.</returns>
    public static PolarResult<TNew> ToFailure<T, TNew>(this PolarResult<T> result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Cannot convert a successful result to a failure.");
        return PolarResult<TNew>.Failure(result.Error!);
    }
}
