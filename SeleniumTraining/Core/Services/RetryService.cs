using Polly;
using Polly.Retry;

namespace SeleniumTraining.Core.Services;

public class RetryService : BaseService, IRetryService
{
    private static readonly Type[] _defaultSeleniumExceptions = [
        typeof(NoSuchElementException),
        typeof(StaleElementReferenceException),
        typeof(ElementNotInteractableException),
        typeof(WebDriverTimeoutException),
        typeof(ElementClickInterceptedException)
    ];

    public RetryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("RetryService initialized.");
    }

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

    private static TimeSpan CalculateExponentialBackoff(TimeSpan initialDelay, int attemptNumber)
    {
        return TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, attemptNumber - 1));
    }

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
