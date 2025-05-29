namespace SeleniumTraining.Core.Services;

public abstract class BaseService
{
    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger Logger { get; }

    protected BaseService(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        Logger = LoggerFactory.CreateLogger(GetType());

        Logger.LogDebug("{DerivedServiceName} (service) initialized.", GetType().Name);
    }
}
