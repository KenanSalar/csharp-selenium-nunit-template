namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Provides thread-local storage for WebDriver instances and associated test context,
/// enabling isolated browser sessions in parallel test execution environments.
/// </summary>
/// <remarks>
/// This service implements <see cref="IThreadLocalDriverStorageService"/> and is designed to be
/// registered as a **Singleton** in the DI container. Its singleton nature ensures a single, consistent
/// point of storage for the entire application, while its internal use of <see cref="ThreadLocal{T}"/>
/// guarantees that each thread's data (WebDriver, test name, correlation ID) is kept completely separate.
/// This combination is critical for maintaining test integrity when running tests in parallel.
/// The <see cref="Dispose()"/> method is intended to be called once by the root service provider when the test run ends.
/// </remarks>
public class ThreadLocalDriverStorageService : BaseService, IThreadLocalDriverStorageService
{
    private readonly ThreadLocal<IWebDriver?> _webDriver = new();
    private readonly ThreadLocal<string?> _threadLocalTestName = new();
    private readonly ThreadLocal<string?> _threadLocalCorrelationId = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThreadLocalDriverStorageService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    public ThreadLocalDriverStorageService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(ThreadLocalDriverStorageService));
    }

    /// <inheritdoc cref="IThreadLocalDriverStorageService.SetDriverContext(IWebDriver, string, string)" />
    /// <remarks>
    /// This implementation stores the provided driver and context strings into separate <see cref="ThreadLocal{T}"/> instances.
    /// It performs null checks on all input parameters.
    /// </remarks>
    public void SetDriverContext(IWebDriver driver, string testName, string correlationId)
    {
        _webDriver.Value = driver ?? throw new ArgumentNullException(nameof(driver));
        _threadLocalTestName.Value = testName ?? throw new ArgumentNullException(nameof(testName));
        _threadLocalCorrelationId.Value = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        ServiceLogger.LogDebug("WebDriver, TestName, and CorrelationId set for the current thread.");
    }

    /// <inheritdoc cref="IThreadLocalDriverStorageService.GetDriver()" />
    /// <remarks>
    /// This implementation checks if a value has been created for the WebDriver <see cref="ThreadLocal{T}"/> instance
    /// and if that value is not null. If not, it throws an <see cref="InvalidOperationException"/> with contextual information.
    /// </remarks>
    public IWebDriver GetDriver()
    {
        if (!_webDriver.IsValueCreated || _webDriver.Value == null)
        {
            string? testName = _threadLocalTestName.Value ?? "UnknownTest (GetDriver)";
            string? correlationId = _threadLocalCorrelationId.Value ?? "N/A (GetDriver)";
            ServiceLogger.LogError("Attempted to get WebDriver for test {TestName} (CorrelationId: {CorrelationId}), but it was not initialized or already disposed for the current thread.", testName, correlationId);
            throw new InvalidOperationException($"WebDriver is not initialized or has been disposed for the current thread (Test: {testName}).");
        }
        return _webDriver.Value;
    }

    /// <inheritdoc cref="IThreadLocalDriverStorageService.GetTestName()" />
    /// <remarks>
    /// This implementation retrieves the test name from its <see cref="ThreadLocal{T}"/> storage.
    /// It throws an <see cref="InvalidOperationException"/> if no test name has been set for the current thread.
    /// </remarks>
    public string GetTestName()
    {
        return _threadLocalTestName.Value ?? throw new InvalidOperationException("TestName is not set for the current thread.");
    }

    /// <inheritdoc cref="IThreadLocalDriverStorageService.GetCorrelationId()" />
    /// <remarks>
    /// This implementation retrieves the correlation ID from its <see cref="ThreadLocal{T}"/> storage.
    /// It throws an <see cref="InvalidOperationException"/> if no correlation ID has been set for the current thread.
    /// </remarks>
    public string GetCorrelationId()
    {
        return _threadLocalCorrelationId.Value ?? throw new InvalidOperationException("CorrelationId is not set for the current thread.");
    }

    /// <inheritdoc cref="IThreadLocalDriverStorageService.IsDriverInitialized()" />
    /// <remarks>
    /// This implementation checks if the <see cref="ThreadLocal{T}"/> instance for the WebDriver has a value created
    /// and that this value is not null.
    /// </remarks>
    public bool IsDriverInitialized()
    {
        return _webDriver.IsValueCreated && _webDriver.Value != null;
    }

    /// <inheritdoc cref="IThreadLocalDriverStorageService.ClearDriverContext()" />
    /// <remarks>
    /// This implementation explicitly sets the <c>Value</c> property of each <see cref="ThreadLocal{T}"/>
    /// instance (for WebDriver, test name, and correlation ID) to <c>null</c>.
    /// This effectively removes the data associated with the current thread, making it eligible for garbage collection
    /// if not referenced elsewhere and preparing the thread for potential reuse with a new context.
    /// </remarks>
    public void ClearDriverContext()
    {
        _webDriver.Value = null;
        _threadLocalTestName.Value = null;
        _threadLocalCorrelationId.Value = null;
        ServiceLogger.LogDebug("WebDriver, TestName, and CorrelationId cleared for the current thread.");
    }

    /// <summary>
    /// Releases managed resources, specifically by disposing the underlying <see cref="ThreadLocal{T}"/> instances.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/> (managed resources);
    /// false if called from a finalizer (unmanaged resources only, though this class doesn't have a finalizer).</param>
    /// <remarks>
    /// If <paramref name="disposing"/> is true, this method disposes each of the <see cref="ThreadLocal{T}"/>
    /// fields to ensure proper cleanup of thread-local data structures.
    /// It is protected by a flag to ensure it's only executed once.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            ServiceLogger.LogInformation("Disposing {ServiceName}. Cleaning up ThreadLocal instances.", nameof(ThreadLocalDriverStorageService));
            _webDriver.Dispose();
            _threadLocalTestName.Dispose();
            _threadLocalCorrelationId.Dispose();
            ServiceLogger.LogInformation("ThreadLocal resources disposed within {ServiceName}.", nameof(ThreadLocalDriverStorageService));
        }
        _disposed = true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// This implementation ensures that the underlying <see cref="ThreadLocal{T}"/> instances are disposed.
    /// </summary>
    /// <inheritdoc cref="IDisposable.Dispose()" />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
