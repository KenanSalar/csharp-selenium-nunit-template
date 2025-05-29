namespace SeleniumTraining.Core.Services.Drivers;

public class TestWebDriverManager : BaseService, ITestWebDriverManager
{
    private readonly IDriverInitializationService _driverInitializer;
    private readonly IDriverLifecycleService _driverLifecycleManager;
    private readonly IThreadLocalDriverStorageService _driverStore;
    private bool _disposed;

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
        Logger.LogInformation("{ServiceName} initialized.", nameof(TestWebDriverManager));
    }

    public bool IsDriverActive => _driverStore.IsDriverInitialized();

    public void InitializeDriver(BrowserType browserType, string testName, string correlationId)
    {
        if (_driverStore.IsDriverInitialized())
        {
            Logger.LogWarning("InitializeDriver called for test {TestName}, but a driver is already active for this thread. This may indicate an issue in test setup/teardown logic. The existing driver will be overwritten.", testName);
        }

        Logger.LogInformation("Orchestrating WebDriver initialization for test: {TestName}, browser: {BrowserType}", testName, browserType);
        IWebDriver driver = _driverInitializer.InitializeDriver(browserType, testName, correlationId);
        _driverStore.SetDriverContext(driver, testName, correlationId);
        Logger.LogInformation("WebDriver initialization orchestrated successfully for test: {TestName}", testName);
    }

    public IWebDriver GetDriver()
    {
        return _driverStore.GetDriver();
    }

    public void QuitDriver()
    {
        if (_driverStore.IsDriverInitialized())
        {
            string testName = _driverStore.GetTestName();
            string correlationId = _driverStore.GetCorrelationId();
            IWebDriver driver = _driverStore.GetDriver();

            Logger.LogInformation("Orchestrating WebDriver quit for test: {TestName}", testName);
            _driverLifecycleManager.QuitDriver(driver, testName, correlationId);
            _driverStore.ClearDriverContext();
            Logger.LogInformation("WebDriver quit orchestrated successfully for test: {TestName}", testName);
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
                Logger.LogDebug(ex, "Failed to retrieve TestName while attempting to quit a non-active driver. Defaulting TestName for warning log.");
            }

            Logger.LogWarning("Attempted to quit WebDriver for test {TestName}, but no active driver was found for the current thread.", currentTestName);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Logger.LogInformation("Disposing {ServiceName}.", nameof(TestWebDriverManager));
            if (_driverStore.IsDriverInitialized())
            {
                string testName = "UnknownTest_Dispose";
                try
                {
                    testName = _driverStore.GetTestName();
                }
                catch (Exception ex)
                {
                    Logger.LogDebug(ex, "Failed to retrieve TestName during Dispose. Defaulting TestName for warning log.");
                }

                Logger.LogWarning("{ServiceName} is disposing, but a driver instance was still active for test {TestName}. Attempting force quit.", nameof(TestWebDriverManager), testName);
                QuitDriver();
            }

            _driverStore.Dispose();
            Logger.LogInformation("{ServiceName} dispose complete.", nameof(TestWebDriverManager));
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
