namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Manages the lifecycle of a WebDriver instance within the scope of a single test.
/// It handles initialization, retrieval, and quitting of the driver for a specific test scope.
/// </summary>
/// <remarks>
/// This class implements <see cref="ITestWebDriverManager"/> and acts as a scoped orchestrator.
/// It delegates driver creation to <see cref="IDriverInitializationService"/> and termination
/// to <see cref="IDriverLifecycleService"/>. For thread-safe storage, it consumes the singleton
/// <see cref="IThreadLocalDriverStorageService"/> but does not manage its lifecycle, adhering to DI best practices.
/// The <see cref="IDisposable"/> implementation ensures that any driver initialized within its scope is properly quit.
/// </remarks>
public class TestWebDriverManager : BaseService, ITestWebDriverManager
{
    private readonly IDriverInitializationService _driverInitializer;
    private readonly IDriverLifecycleService _driverLifecycleManager;
    private readonly IThreadLocalDriverStorageService _driverStore;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestWebDriverManager"/> class.
    /// </summary>
    /// <param name="driverInitializer">The service responsible for initializing WebDriver instances. Must not be null.</param>
    /// <param name="driverLifecycleManager">The service responsible for quitting WebDriver instances. Must not be null.</param>
    /// <param name="driverStore">The service providing thread-local storage for WebDriver instances and context. Must not be null.</param>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="driverInitializer"/>, <paramref name="driverLifecycleManager"/>,
    /// <paramref name="driverStore"/>, or <paramref name="loggerFactory"/> is null.
    /// </exception>
    public TestWebDriverManager(
        IDriverInitializationService driverInitializer,
        IDriverLifecycleService driverLifecycleManager,
        IThreadLocalDriverStorageService driverStore,
        ILoggerFactory loggerFactory
    )
        : base(loggerFactory)
    {
        _driverInitializer = driverInitializer ?? throw new ArgumentNullException(nameof(driverInitializer));
        _driverLifecycleManager = driverLifecycleManager ?? throw new ArgumentNullException(nameof(driverLifecycleManager));
        _driverStore = driverStore ?? throw new ArgumentNullException(nameof(driverStore));
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(TestWebDriverManager));
    }

    /// <inheritdoc cref="ITestWebDriverManager.IsDriverActive" />
    public bool IsDriverActive => _driverStore.IsDriverInitialized();

    /// <inheritdoc cref="ITestWebDriverManager.InitializeDriver(BrowserType, string, string)" />
    /// <remarks>
    /// This implementation checks if a driver is already active for the current thread.
    /// If so, a warning is logged, and the existing driver context will be overwritten by the new one.
    /// It then delegates the actual driver creation to the <see cref="IDriverInitializationService"/>
    /// and stores the created driver and its context using <see cref="IThreadLocalDriverStorageService"/>.
    /// </remarks>
    public void InitializeDriver(BrowserType browserType, string testName, string correlationId)
    {
        if (_driverStore.IsDriverInitialized())
        {
            ServiceLogger.LogWarning("InitializeDriver called for test {TestName}, but a driver is already active for this thread. This may indicate an issue in test setup/teardown logic. The existing driver will be overwritten.", testName);
        }

        ServiceLogger.LogInformation("Orchestrating WebDriver initialization for test: {TestName}, browser: {BrowserType}", testName, browserType);
        IWebDriver driver = _driverInitializer.InitializeDriver(browserType, testName, correlationId);
        _driverStore.SetDriverContext(driver, testName, correlationId);
        ServiceLogger.LogInformation("WebDriver initialization orchestrated successfully for test: {TestName}", testName);
    }

    /// <inheritdoc cref="ITestWebDriverManager.GetDriver()" />
    /// <remarks>
    /// This implementation retrieves the driver directly from the configured <see cref="IThreadLocalDriverStorageService"/>.
    /// An <see cref="InvalidOperationException"/> will be thrown by the storage service if no driver is currently set for the thread.
    /// </remarks>
    public IWebDriver GetDriver()
    {
        return _driverStore.GetDriver();
    }

    /// <inheritdoc cref="ITestWebDriverManager.QuitDriver()" />
    /// <remarks>
    /// This implementation first checks if a driver is active using <see cref="IsDriverActive"/>.
    /// If active, it retrieves the driver and context information from <see cref="IThreadLocalDriverStorageService"/>,
    /// then delegates the quitting operation to <see cref="IDriverLifecycleService"/>,
    /// and finally clears the driver context from the thread-local store.
    /// If no driver is found to be active, a warning is logged.
    /// </remarks>
    public void QuitDriver()
    {
        if (_driverStore.IsDriverInitialized())
        {
            string testName = _driverStore.GetTestName();
            string correlationId = _driverStore.GetCorrelationId();
            IWebDriver driver = _driverStore.GetDriver();

            ServiceLogger.LogInformation("Orchestrating WebDriver quit for test: {TestName}", testName);
            _driverLifecycleManager.QuitDriver(driver, testName, correlationId);
            _driverStore.ClearDriverContext();
            ServiceLogger.LogInformation("WebDriver quit orchestrated successfully for test: {TestName}", testName);
        }
        else
        {
            string currentTestName = "UnknownTest (QuitDriver)";
            try
            {
                currentTestName = _driverStore.GetTestName();
            }
            catch (Exception ex)
            {
                ServiceLogger.LogDebug(ex, "Failed to retrieve TestName while attempting to quit a non-active driver. Defaulting TestName for warning log.");
            }

            ServiceLogger.LogWarning("Attempted to quit WebDriver for test {TestName}, but no active driver was found for the current thread.", currentTestName);
        }
    }

    /// <summary>
    /// Releases managed and unmanaged resources. Ensures that any active WebDriver instance
    /// managed by this service's scope is quit.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/> (managed and unmanaged resources);
    /// false if called from a finalizer (unmanaged resources only).</param>
    /// <remarks>
    /// If <paramref name="disposing"/> is true and a driver is still active for the current scope, this method
    /// will attempt to quit the driver using <see cref="QuitDriver"/> to ensure proper cleanup.
    /// It does **not** dispose of injected singleton services like the driver store, as their
    /// lifecycle is managed by the root DI container.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            ServiceLogger.LogInformation("Disposing {ServiceName}.", nameof(TestWebDriverManager));
            if (_driverStore.IsDriverInitialized())
            {
                string testName = "UnknownTest_Dispose";
                try
                {
                    testName = _driverStore.GetTestName();
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogDebug(ex, "Failed to retrieve TestName during Dispose. Defaulting TestName for warning log.");
                }

                ServiceLogger.LogWarning("{ServiceName} is disposing, but a driver instance was still active for test {TestName}. Attempting force quit.", nameof(TestWebDriverManager), testName);
                QuitDriver();
            }

            ServiceLogger.LogInformation("{ServiceName} dispose complete.", nameof(TestWebDriverManager));
        }
        _disposed = true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// This implementation ensures that any active WebDriver is quit.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
