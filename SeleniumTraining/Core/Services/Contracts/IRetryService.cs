namespace SeleniumTraining.Core.Services.Contracts;

public interface IRetryService
{
    /// <summary>
    /// Executes an action with a retry policy that includes exponential backoff.
    /// Retries on specific exceptions.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxRetryAttempts">Maximum number of retry attempts.</param>
    /// <param name="initialDelay">Initial delay for backoff.</param>
    /// <param name="logger">Logger for retry attempts.</param>
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
    /// <param name="logger">Logger for retry attempts.</param>
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
