namespace SeleniumTraining.Core.Services;

/// <summary>
/// Provides a base class for services within the framework, offering common
/// functionalities such as integrated logging.
/// </summary>
/// <remarks>
/// Derived service classes should call the base constructor, passing an <see cref="ILoggerFactory"/>,
/// which is then used to create a specific <see cref="ILogger"/> instance for that derived service.
/// This promotes consistent logging practices across different services.
/// The <see cref="ServiceLogger"/> property is available to derived classes for their logging needs.
/// </remarks>
public abstract class BaseService
{
    /// <summary>
    /// Gets the <see cref="ILoggerFactory"/> instance used to create loggers.
    /// This factory is provided during the construction of the service.
    /// </summary>
    /// <value>The logger factory.</value>
    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance specific to the derived service type.
    /// This logger is created using the <see cref="LoggerFactory"/> and is configured
    /// with the type of the derived class, allowing for contextual logging.
    /// </summary>
    /// <value>The logger instance for the derived service.</value>
    protected ILogger ServiceLogger { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory to be used for creating logger instances. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    /// <remarks>
    /// Upon construction, this base class initializes the <see cref="LoggerFactory"/> property
    /// and creates a <see cref="ServiceLogger"/> instance specifically for the concrete derived service type.
    /// A debug log message is also emitted indicating the initialization of the derived service.
    /// </remarks>
    protected BaseService(ILoggerFactory loggerFactory)
    {
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        ServiceLogger = LoggerFactory.CreateLogger(GetType());

        ServiceLogger.LogDebug("{DerivedServiceName} (service) initialized.", GetType().Name);
    }
}
