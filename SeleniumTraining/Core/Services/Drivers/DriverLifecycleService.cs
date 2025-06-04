namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Service responsible for managing the termination (quitting) of WebDriver instances.
/// </summary>
/// <remarks>
/// This service implements <see cref="IDriverLifecycleService"/> and provides a robust
/// mechanism for quitting WebDriver instances, including attempts to close, quit, and dispose
/// of the driver, with comprehensive logging for each step.
/// It inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class DriverLifecycleService : BaseService, IDriverLifecycleService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DriverLifecycleService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    public DriverLifecycleService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(DriverLifecycleService));
    }

    /// <inheritdoc cref="IDriverLifecycleService.QuitDriver(IWebDriver, string, string)" />
    /// <remarks>
    /// This implementation first checks if the provided <paramref name="driver"/> is null.
    /// If not null, it attempts to <c>driver.Close()</c> followed by <c>driver.Quit()</c>.
    /// If an exception occurs during these operations, it logs the error and then attempts
    /// a fallback to <c>driver.Dispose()</c>. Further exceptions during the dispose fallback are also logged.
    /// This multi-step approach aims to maximize the chances of successfully terminating the browser
    /// and releasing resources, even if the driver is in an unstable state.
    /// All operations are logged with the provided <paramref name="testClassName"/> and <paramref name="correlationId"/>.
    /// </remarks>
    public void QuitDriver(IWebDriver driver, string testClassName, string correlationId)
    {
        if (driver == null)
        {
            ServiceLogger.LogWarning("Attempted to quit a null WebDriver instance for test: {TestName}. This should not happen if logic is correct.", testClassName);
            return;
        }

        var logProps = new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["TestClassName"] = testClassName
        };

        using (ServiceLogger.BeginScope(logProps!))
        {
            ServiceLogger.LogInformation("Attempting to quit WebDriver for test: {TestName}", testClassName);
            try
            {
                driver.Close();
                driver.Quit();
                ServiceLogger.LogInformation("WebDriver quit successfully for test: {TestName}", testClassName);
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "WebDriver Quit failed for test: {TestName}. Attempting Dispose as fallback.", testClassName);
                try
                {
                    driver.Dispose();
                    ServiceLogger.LogInformation("WebDriver Dispose fallback succeeded for test: {TestName}", testClassName);
                }
                catch (Exception disposeEx)
                {
                    ServiceLogger.LogError(disposeEx, "WebDriver Dispose fallback also failed for test: {TestName}", testClassName);
                }
            }
        }
    }
}
