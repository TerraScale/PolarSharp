using System.Diagnostics.CodeAnalysis;

namespace PolarSharp.Results;

/// <summary>
/// Represents the result of a Polar API operation that does not return a value.
/// </summary>
public readonly struct PolarResult
{
    private readonly PolarError? _error;

    private PolarResult(PolarError? error) => _error = error;

    /// <summary>
    /// Returns true if the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => _error is null;

    /// <summary>
    /// Returns true if the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => _error is not null;

    /// <summary>
    /// The error if the operation failed, null otherwise.
    /// </summary>
    public PolarError? Error => _error;

    /// <summary>
    /// Returns true if this result failed due to rate limiting (HTTP 429).
    /// </summary>
    public bool IsRateLimitError => _error?.IsRateLimitError ?? false;

    /// <summary>
    /// Returns true if this result failed due to authentication/authorization issues.
    /// </summary>
    public bool IsAuthError => _error?.IsAuthError ?? false;

    /// <summary>
    /// Returns true if this result failed because the resource was not found.
    /// </summary>
    public bool IsNotFoundError => _error?.IsNotFoundError ?? false;

    /// <summary>
    /// Returns true if this result failed due to a server-side error (HTTP 5xx).
    /// </summary>
    public bool IsServerError => _error?.IsServerError ?? false;

    /// <summary>
    /// Returns true if this result failed due to a client-side error (HTTP 4xx).
    /// </summary>
    public bool IsClientError => _error?.IsClientError ?? false;

    /// <summary>
    /// Returns true if this result failed due to a validation error.
    /// </summary>
    public bool IsValidationError => _error?.IsValidationError ?? false;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful PolarResult.</returns>
    public static PolarResult Success() => new(null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error details.</param>
    /// <returns>A failed PolarResult.</returns>
    public static PolarResult Failure(PolarError error) => new(error);

    /// <summary>
    /// Implicit conversion from PolarError to a failed PolarResult.
    /// </summary>
    public static implicit operator PolarResult(PolarError error) => Failure(error);

    /// <summary>
    /// Deconstructs the result for pattern matching.
    /// </summary>
    /// <param name="isSuccess">True if the operation succeeded.</param>
    /// <param name="error">The error if the operation failed.</param>
    public void Deconstruct(out bool isSuccess, out PolarError? error)
    {
        isSuccess = IsSuccess;
        error = _error;
    }

    /// <summary>
    /// Matches on success or failure with corresponding functions.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the matching function.</returns>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<PolarError, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(_error!);

    /// <summary>
    /// Executes an action based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure.</param>
    public void Switch(Action onSuccess, Action<PolarError> onFailure)
    {
        if (IsSuccess) onSuccess();
        else onFailure(_error!);
    }

    /// <summary>
    /// Returns the error message if failed, or an empty string if successful.
    /// </summary>
    public override string ToString() => IsSuccess ? "Success" : $"Failure: {_error!.Message}";
}

/// <summary>
/// Represents the result of a Polar API operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public readonly struct PolarResult<T>
{
    private readonly T? _value;
    private readonly PolarError? _error;
    private readonly bool _hasValue;

    private PolarResult(T value)
    {
        _value = value;
        _error = null;
        _hasValue = true;
    }

    private PolarResult(PolarError error)
    {
        _value = default;
        _error = error;
        _hasValue = false;
    }

    /// <summary>
    /// Returns true if the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => _hasValue;

    /// <summary>
    /// Returns true if the operation failed.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !_hasValue;

    /// <summary>
    /// The value if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if accessing value on failed result.</exception>
    public T Value => _hasValue
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failed result. Error: {_error?.Message}");

    /// <summary>
    /// The error if failed, null otherwise.
    /// </summary>
    public PolarError? Error => _error;

    /// <summary>
    /// Returns true if this result failed due to rate limiting (HTTP 429).
    /// </summary>
    public bool IsRateLimitError => _error?.IsRateLimitError ?? false;

    /// <summary>
    /// Returns true if this result failed due to authentication/authorization issues.
    /// </summary>
    public bool IsAuthError => _error?.IsAuthError ?? false;

    /// <summary>
    /// Returns true if this result failed because the resource was not found.
    /// </summary>
    public bool IsNotFoundError => _error?.IsNotFoundError ?? false;

    /// <summary>
    /// Returns true if this result failed due to a server-side error (HTTP 5xx).
    /// </summary>
    public bool IsServerError => _error?.IsServerError ?? false;

    /// <summary>
    /// Returns true if this result failed due to a client-side error (HTTP 4xx).
    /// </summary>
    public bool IsClientError => _error?.IsClientError ?? false;

    /// <summary>
    /// Returns true if this result failed due to a validation error.
    /// </summary>
    public bool IsValidationError => _error?.IsValidationError ?? false;

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A successful PolarResult.</returns>
    public static PolarResult<T> Success(T value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error details.</param>
    /// <returns>A failed PolarResult.</returns>
    public static PolarResult<T> Failure(PolarError error) => new(error);

    /// <summary>
    /// Implicit conversion from T to a successful PolarResult.
    /// </summary>
    public static implicit operator PolarResult<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from PolarError to a failed PolarResult.
    /// </summary>
    public static implicit operator PolarResult<T>(PolarError error) => Failure(error);

    /// <summary>
    /// Gets the value or a default if failed.
    /// </summary>
    /// <param name="defaultValue">The default value to return if failed.</param>
    /// <returns>The value if successful, otherwise the default value.</returns>
    public T? ValueOrDefault(T? defaultValue = default) => _hasValue ? _value : defaultValue;

    /// <summary>
    /// Deconstructs the result for pattern matching (3 values).
    /// </summary>
    /// <param name="isSuccess">True if the operation succeeded.</param>
    /// <param name="value">The value if successful.</param>
    /// <param name="error">The error if failed.</param>
    public void Deconstruct(out bool isSuccess, out T? value, out PolarError? error)
    {
        isSuccess = _hasValue;
        value = _value;
        error = _error;
    }

    /// <summary>
    /// Deconstructs the result for pattern matching (2 values).
    /// </summary>
    /// <param name="value">The value if successful.</param>
    /// <param name="error">The error if failed.</param>
    public void Deconstruct(out T? value, out PolarError? error)
    {
        value = _value;
        error = _error;
    }

    /// <summary>
    /// Matches on success or failure with corresponding functions.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="onSuccess">Function to execute on success.</param>
    /// <param name="onFailure">Function to execute on failure.</param>
    /// <returns>The result of the matching function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<PolarError, TResult> onFailure)
        => _hasValue ? onSuccess(_value!) : onFailure(_error!);

    /// <summary>
    /// Executes an action based on success or failure.
    /// </summary>
    /// <param name="onSuccess">Action to execute on success.</param>
    /// <param name="onFailure">Action to execute on failure.</param>
    public void Switch(Action<T> onSuccess, Action<PolarError> onFailure)
    {
        if (_hasValue) onSuccess(_value!);
        else onFailure(_error!);
    }

    /// <summary>
    /// Maps the value if successful.
    /// </summary>
    /// <typeparam name="TNew">The type of the new value.</typeparam>
    /// <param name="mapper">The mapping function.</param>
    /// <returns>A new PolarResult with the mapped value.</returns>
    public PolarResult<TNew> Map<TNew>(Func<T, TNew> mapper)
        => _hasValue ? PolarResult<TNew>.Success(mapper(_value!)) : PolarResult<TNew>.Failure(_error!);

    /// <summary>
    /// Binds to another result if successful.
    /// </summary>
    /// <typeparam name="TNew">The type of the new value.</typeparam>
    /// <param name="binder">The binding function.</param>
    /// <returns>The bound result.</returns>
    public PolarResult<TNew> Bind<TNew>(Func<T, PolarResult<TNew>> binder)
        => _hasValue ? binder(_value!) : PolarResult<TNew>.Failure(_error!);

    /// <summary>
    /// Converts to non-generic result.
    /// </summary>
    /// <returns>A PolarResult without the value.</returns>
    public PolarResult ToResult() => _hasValue ? PolarResult.Success() : PolarResult.Failure(_error!);

    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    public override string ToString() => _hasValue ? $"Success: {_value}" : $"Failure: {_error!.Message}";
}
