namespace SeleniumTraining.Core.Services;

public abstract class BaseService
{
    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger ServiceLogger { get; }

    protected BaseService(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        ServiceLogger = LoggerFactory.CreateLogger(GetType());

        ServiceLogger.LogDebug("{DerivedServiceName} (service) initialized.", GetType().Name);
    }
}
