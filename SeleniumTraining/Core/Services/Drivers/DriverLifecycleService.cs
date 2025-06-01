namespace SeleniumTraining.Core.Services.Drivers;

public class DriverLifecycleService : BaseService, IDriverLifecycleService
{
    public DriverLifecycleService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(DriverLifecycleService));
    }

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
