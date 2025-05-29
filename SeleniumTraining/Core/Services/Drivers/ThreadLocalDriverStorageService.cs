namespace SeleniumTraining.Core.Services.Drivers;

public class ThreadLocalDriverStorageService : BaseService, IThreadLocalDriverStorageService
{
    private readonly ThreadLocal<IWebDriver?> _webDriver = new();
    private readonly ThreadLocal<string?> _threadLocalTestName = new();
    private readonly ThreadLocal<string?> _threadLocalCorrelationId = new();
    private bool _disposed;

    public ThreadLocalDriverStorageService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{ServiceName} initialized.", nameof(ThreadLocalDriverStorageService));
    }

    public void SetDriverContext(IWebDriver driver, string testName, string correlationId)
    {
        _webDriver.Value = driver ?? throw new ArgumentNullException(nameof(driver));
        _threadLocalTestName.Value = testName ?? throw new ArgumentNullException(nameof(testName));
        _threadLocalCorrelationId.Value = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        Logger.LogDebug("WebDriver, TestName, and CorrelationId set for the current thread.");
    }

    public IWebDriver GetDriver()
    {
        if (!_webDriver.IsValueCreated || _webDriver.Value == null)
        {
            string? testName = _threadLocalTestName.Value ?? "UnknownTest (GetDriver)";
            string? correlationId = _threadLocalCorrelationId.Value ?? "N/A (GetDriver)";
            Logger.LogError("Attempted to get WebDriver for test {TestName} (CorrelationId: {CorrelationId}), but it was not initialized or already disposed for the current thread.", testName, correlationId);
            throw new InvalidOperationException($"WebDriver is not initialized or has been disposed for the current thread (Test: {testName}).");
        }
        return _webDriver.Value;
    }

    public string GetTestName()
    {
        return _threadLocalTestName.Value ?? throw new InvalidOperationException("TestName is not set for the current thread.");
    }

    public string GetCorrelationId()
    {
        return _threadLocalCorrelationId.Value ?? throw new InvalidOperationException("CorrelationId is not set for the current thread.");
    }

    public bool IsDriverInitialized()
    {
        return _webDriver.IsValueCreated && _webDriver.Value != null;
    }

    public void ClearDriverContext()
    {
        _webDriver.Value = null;
        _threadLocalTestName.Value = null;
        _threadLocalCorrelationId.Value = null;
        Logger.LogDebug("WebDriver, TestName, and CorrelationId cleared for the current thread.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            Logger.LogInformation("Disposing {ServiceName}. Cleaning up ThreadLocal instances.", nameof(ThreadLocalDriverStorageService));
            _webDriver.Dispose();
            _threadLocalTestName.Dispose();
            _threadLocalCorrelationId.Dispose();
            Logger.LogInformation("ThreadLocal resources disposed within {ServiceName}.", nameof(ThreadLocalDriverStorageService));
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
