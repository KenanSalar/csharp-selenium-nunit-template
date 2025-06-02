using Polly;
using Polly.Retry;

namespace SeleniumTraining.Core.Services;

/// <summary>
/// Provides retry capabilities for actions and functions using policies primarily based on Polly.
/// It handles a predefined set of common Selenium exceptions and uses an exponential backoff strategy.
/// </summary>
/// <remarks>
/// This service implements <see cref="IRetryService"/> ([2]) and centralizes the logic for retrying operations
/// that might be prone to transient failures. It uses helper methods to build Polly policies
/// based on a configurable list of default Selenium exceptions.
/// Logging of retry attempts and outcomes is performed using the provided or internal logger.
/// It inherits from <see cref="BaseService"/> for common logging infrastructure.
/// </remarks>
public class RetryService : BaseService, IRetryService
{
    private static readonly Type[] _defaultSeleniumExceptions = [
        typeof(NoSuchElementException),
        typeof(StaleElementReferenceException),
        typeof(ElementNotInteractableException),
        typeof(WebDriverTimeoutException),
        typeof(ElementClickInterceptedException)
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    public RetryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("RetryService initialized.");
    }

    /// <inheritdoc cref="IRetryService.ExecuteWithRetry(Action, int, TimeSpan?, ILogger?)" />
    /// <remarks>
    /// This implementation builds a Polly <see cref="RetryPolicy"/> using <see cref="BuildPolicyForDefaultExceptions()"/>
    /// and configures it with <see cref="WaitAndRetry"/> using an exponential backoff strategy
    /// calculated by <see cref="CalculateExponentialBackoff(TimeSpan, int)"/>.
    /// Retry attempts and exceptions are logged.
    /// </remarks>
    public void ExecuteWithRetry(
        Action action,
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null,
        ILogger? actionLogger = null
    )
    {
        ILogger logger = actionLogger ?? ServiceLogger;
        TimeSpan delay = initialDelay ?? TimeSpan.FromSeconds(1);

        RetryPolicy retryPolicy = BuildPolicyForDefaultExceptions().WaitAndRetry(
            maxRetryAttempts,
            retryAttempt => CalculateExponentialBackoff(delay, retryAttempt),
            (exception, timeSpan, attemptNumber, context) => logger.LogWarning(
                exception,
                "Retry {AttemptNumber}/{MaxAttempts} for action due to {ExceptionType}. Waiting {TimeSpan} before next attempt. Context: {ContextKeys}",
                attemptNumber,
                maxRetryAttempts,
                exception.GetType().Name,
                timeSpan,
                context.Keys
            )
        );

        retryPolicy.Execute(action);
    }

    /// <inheritdoc cref="IRetryService.ExecuteWithRetry{TResult}(Func{TResult}, int, TimeSpan?, ILogger?, Func{TResult, bool}?)" />
    /// <remarks>
    /// This implementation dynamically builds a Polly <see cref="RetryPolicy{TResult}"/> using
    /// <see cref="BuildPolicyForDefaultExceptions{TResult}()"/>.
    /// If a <paramref name="resultCondition"/> is provided, the policy also retries if the condition evaluates to false
    /// using <c>OrResult()</c>. The <see cref="WaitAndRetry"/> strategy employs exponential backoff
    /// via <see cref="CalculateExponentialBackoff(TimeSpan, int)"/>.
    /// Detailed logging is performed for both exception-based and result-based retries.
    /// </remarks>
    public TResult ExecuteWithRetry<TResult>(
        Func<TResult> func,
        int maxRetryAttempts = 3,
        TimeSpan? initialDelay = null,
        ILogger? actionLogger = null,
        Func<TResult, bool>? resultCondition = null
    )
    {
        ILogger logger = actionLogger ?? ServiceLogger;
        TimeSpan baseDelay = initialDelay ?? TimeSpan.FromSeconds(1);

        PolicyBuilder<TResult> policyBuilder = BuildPolicyForDefaultExceptions<TResult>();

        if (resultCondition != null)
        {
            return policyBuilder.OrResult(res => !resultCondition(res)).WaitAndRetry(
                maxRetryAttempts,
                retryAttempt => CalculateExponentialBackoff(baseDelay, retryAttempt),
                (delegateResult, timeSpan, attemptNumber, context) =>
                {
                    if (delegateResult.Exception != null)
                    {
                        logger.LogWarning(
                            delegateResult.Exception,
                            "Retry {AttemptNumber}/{MaxAttempts} for FUNCTION (exception) due to {ExceptionType}. Waiting {TimeSpan}. Context Keys: {ContextKeysString}",
                            attemptNumber,
                            maxRetryAttempts,
                            delegateResult.Exception.GetType().Name,
                            timeSpan,
                            context.Keys.Count != 0
                                ? string.Join(",", context.Keys)
                                : "N/A"
                        );
                    }
                    else
                    {
                        logger.LogWarning(
                            "Retry {AttemptNumber}/{MaxAttempts} for FUNCTION (result condition) because result was '{Result}'. Waiting {TimeSpan}. Context Keys: {ContextKeysString}",
                            attemptNumber,
                            maxRetryAttempts,
                            delegateResult.Result,
                            timeSpan,
                            context.Keys.Count != 0 ? string.Join(",", context.Keys) : "N/A"
                        );
                    }
                }
            ).Execute(func);
        }
        else
        {
            return policyBuilder.WaitAndRetry(
                maxRetryAttempts,
                retryAttempt => CalculateExponentialBackoff(baseDelay, retryAttempt),
                (delegateResult, timeSpan, attemptNumber, context) =>
                {
                    if (delegateResult.Exception != null)
                    {
                        logger.LogWarning(
                            delegateResult.Exception,
                            "Retry {AttemptNumber}/{MaxAttempts} for FUNCTION (exception only) due to {ExceptionType}. Waiting {TimeSpanString} ({TimeSpanSeconds}s) before next attempt. Context Keys: {ContextKeysString}",
                            attemptNumber,
                            maxRetryAttempts,
                            delegateResult.Exception.GetType().Name,
                            timeSpan.ToString(),
                            timeSpan.TotalSeconds,
                            context.Keys.Count != 0 ? string.Join(",", context.Keys) : "N/A"
                        );
                    }
                    else
                    {
                        logger.LogWarning(
                            "Retry {AttemptNumber}/{MaxAttempts} for FUNCTION (exception only) triggered unexpectedly without an exception. Waiting {TimeSpanString} ({TimeSpanSeconds}s). Context Keys: {ContextKeysString}",
                            attemptNumber,
                            maxRetryAttempts,
                            timeSpan.ToString(),
                            timeSpan.TotalSeconds,
                            context.Keys.Count != 0 ? string.Join(",", context.Keys) : "N/A"
                        );
                    }
                }
            ).Execute(func);
        }
    }

    /// <summary>
    /// Calculates the duration for an exponential backoff strategy based on the attempt number.
    /// The delay increases exponentially with each attempt (e.g., initial, initial*2, initial*4).
    /// </summary>
    /// <param name="initialDelay">The base delay for the first attempt.</param>
    /// <param name="attemptNumber">The current retry attempt number (should be 1-based).</param>
    /// <returns>A <see cref="TimeSpan"/> representing the calculated delay for the current attempt.</returns>
    /// <remarks>
    /// The calculated delay is capped at a maximum of 30 seconds to prevent excessively long waits.
    /// </remarks>
    private static TimeSpan CalculateExponentialBackoff(TimeSpan initialDelay, int attemptNumber)
    {
        return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, attemptNumber - 1));
    }

    /// <summary>
    /// Builds a Polly <see cref="PolicyBuilder"/> configured to handle a default set of
    /// common Selenium exceptions defined in <see cref="_defaultSeleniumExceptions"/>.
    /// </summary>
    /// <returns>A <see cref="PolicyBuilder"/> instance configured with the default exception handling rules.</returns>
    /// <remarks>
    /// This method iterates through the <see cref="_defaultSeleniumExceptions"/> array, adding each
    /// exception type to the policy builder's handling logic using <c>IsAssignableFrom</c>
    /// for flexibility with derived exception types.
    /// </remarks>
    private static PolicyBuilder BuildPolicyForDefaultExceptions()
    {
        if (_defaultSeleniumExceptions == null || _defaultSeleniumExceptions.Length == 0)
        {
            return Policy.Handle<Exception>();
        }

        PolicyBuilder policyBuilder = Policy.Handle((Exception ex) => _defaultSeleniumExceptions[0].IsAssignableFrom(ex.GetType()));

        foreach (Type? exType in _defaultSeleniumExceptions.Skip(1))
        {
            policyBuilder = policyBuilder.Or((Exception ex) => exType.IsAssignableFrom(ex.GetType()));
        }
        return policyBuilder;
    }

    /// <summary>
    /// Builds a Polly <see cref="PolicyBuilder{TResult}"/> configured to handle a default set of
    /// common Selenium exceptions defined in <see cref="_defaultSeleniumExceptions"/> for functions returning <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the function to be retried.</typeparam>
    /// <returns>A <see cref="PolicyBuilder{TResult}"/> instance configured with the default exception handling rules.</returns>
    /// <remarks>
    /// This method is analogous to <see cref="BuildPolicyForDefaultExceptions()"/> but for policies
    /// that operate on functions returning a result. It uses <see cref="Policy{TResult}.Handle(Func{Exception, bool})"/>.
    /// </remarks>
    private static PolicyBuilder<TResult> BuildPolicyForDefaultExceptions<TResult>()
    {
        if (_defaultSeleniumExceptions == null || _defaultSeleniumExceptions.Length == 0)
        {
            return Policy<TResult>.Handle<Exception>();
        }

        PolicyBuilder<TResult> policyBuilder = Policy<TResult>.Handle((Exception ex) => _defaultSeleniumExceptions[0].IsAssignableFrom(ex.GetType()));

        foreach (Type? exType in _defaultSeleniumExceptions.Skip(1))
        {
            policyBuilder = policyBuilder.Or((Exception ex) => exType.IsAssignableFrom(ex.GetType()));
        }

        return policyBuilder;
    }
}
