namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service that provides retry capabilities for executing actions or functions.
/// Implementations typically use policies like exponential backoff to handle transient failures.
/// </summary>
/// <remarks>
/// This service is essential for improving the robustness of interactions that might
/// intermittently fail due to transient issues such as network latency, temporary unavailability
/// of elements, or race conditions in web applications. It often wraps operations that
/// interact with external systems or UI elements.
/// </remarks>
public interface IRetryService
{
    /// <summary>
    /// Executes an action with a retry policy that includes exponential backoff.
    /// Retries on specific exceptions.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts.</param>
    /// <param name="initialDelay">Initial delay for backoff.</param>
    /// <param name="actionLogger">Logger for retry attempts.</param>
    public void ExecuteWithRetry(
        Action action,
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null,
        ILogger? actionLogger = null
    );

    /// <summary>
    /// Executes a function that returns a result with a retry policy.
    /// Retries on specific exceptions or if the result does not meet a condition.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts.</param>
    /// <param name="initialDelay">Initial delay for backoff.</param>
    /// <param name="actionLogger">Logger for retry attempts.</param>
    /// <param name="resultCondition">Optional condition to check on the result. If it returns false, a retry is triggered.</param>
    /// <returns>The result of the function if successful.</returns>
    public TResult ExecuteWithRetry<TResult>(
        Func<TResult> func,
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null,
        ILogger? actionLogger = null,
        Func<TResult, bool>? resultCondition = null
    );
}
