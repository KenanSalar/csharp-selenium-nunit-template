namespace SeleniumTraining.Core.Models;

/// <summary>
/// A non-generic companion class for the Result pattern.
/// This class provides static factory methods to create instances of the generic Result type.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a new success result with the specified value.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the successful result.</typeparam>
    /// <typeparam name="TFailure">The type of the failure result.</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>A new instance of Result representing a success.</returns>
    public static Result<TSuccess, TFailure> Success<TSuccess, TFailure>(TSuccess value)
        => new Result<TSuccess, TFailure>.SuccessResult(value);

    /// <summary>
    /// Creates a new failure result with the specified error.
    /// </summary>
    /// <typeparam name="TSuccess">The type of the successful result.</typeparam>
    /// <typeparam name="TFailure">The type of the failure result.</typeparam>
    /// <param name="error">The error value.</param>
    /// <returns>A new instance of Result representing a failure.</returns>
    public static Result<TSuccess, TFailure> Failure<TSuccess, TFailure>(TFailure error)
        => new Result<TSuccess, TFailure>.FailureResult(error);
}

/// <summary>
/// A discriminated union to represent the result of an operation that can either succeed or fail.
/// </summary>
/// <typeparam name="TSuccess">The type of the successful result.</typeparam>
/// <typeparam name="TFailure">The type of the failure result (e.g., an error message string, an enum).</typeparam>
/// <remarks>
/// This abstract record serves as the base for two nested, sealed records: <see cref="SuccessResult"/> and <see cref="FailureResult"/>.
/// It forces the consumer to handle both possibilities, making error handling more explicit and robust.
/// This pattern is used to avoid throwing exceptions for predictable, operational failures.
/// </remarks>
public abstract record Result<TSuccess, TFailure>
{
    private Result() { }

    /// <summary>
    /// Represents a successful result. This record is sealed to prevent further inheritance.
    /// </summary>
    /// <param name="Value">The value of the successful operation.</param>
    public sealed record SuccessResult(TSuccess Value) : Result<TSuccess, TFailure>;

    /// <summary>
    /// Represents a failed result. This record is sealed to prevent further inheritance.
    /// </summary>
    /// <param name="Error">The value describing the failure.</param>
    public sealed record FailureResult(TFailure Error) : Result<TSuccess, TFailure>;

    /// <summary>
    /// Gets whether the result is a success.
    /// </summary>
    public bool IsSuccess => this is SuccessResult;

    /// <summary>
    /// Gets whether the result is a failure.
    /// </summary>
    public bool IsFailure => this is FailureResult;
}
