namespace SeleniumTraining.Core.Services.Drivers.Contracts;

/// <summary>
/// Defines the contract for a service that provides thread-local storage for WebDriver instances
/// and associated test context information (test name, correlation ID).
/// This ensures that each execution thread in a parallel testing scenario
/// has its own isolated WebDriver context.
/// </summary>
/// <remarks>
/// This service is critical for enabling parallel test execution without interference
/// between threads. It uses thread-local storage mechanisms to store and retrieve
/// driver instances and context data specific to the current thread.
/// As an <see cref="IDisposable"/> service, it's also responsible for cleaning up
/// any thread-local data or resources when the service instance is disposed,
/// which typically occurs at the end of a test or test scope.
/// The <see cref="ClearDriverContext"/> method is often used for explicit cleanup
/// before or after a test.
/// </remarks>
public interface IThreadLocalDriverStorageService : IDisposable
{
    /// <summary>
    /// Sets the WebDriver instance and its associated context (test name, correlation ID)
    /// for the current thread. This effectively stores the driver and its metadata
    /// in thread-local storage.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance to store for the current thread. Must not be null.</param>
    /// <param name="testName">The name of the test currently being executed on this thread. Must not be null or empty.</param>
    /// <param name="correlationId">A unique correlation ID for the current test execution on this thread. Must not be null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="driver"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="testName"/> or <paramref name="correlationId"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a driver context is already set for the current thread and an attempt is made to set it again without clearing first (behavior depends on implementation).</exception>
    public void SetDriverContext(IWebDriver driver, string testName, string correlationId);

    /// <summary>
    /// Retrieves the <see cref="IWebDriver"/> instance stored for the current thread.
    /// </summary>
    /// <returns>The <see cref="IWebDriver"/> instance associated with the current thread.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no WebDriver instance has been set
    /// (via <see cref="SetDriverContext"/>) for the current thread, or if it has been cleared.</exception>
    public IWebDriver GetDriver();

    /// <summary>
    /// Retrieves the test name associated with the WebDriver context for the current thread.
    /// </summary>
    /// <returns>The test name string stored for the current thread's driver context.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no driver context (and thus no test name)
    /// has been set for the current thread.</exception>
    public string GetTestName();

    /// <summary>
    /// Retrieves the correlation ID associated with the WebDriver context for the current thread.
    /// </summary>
    /// <returns>The correlation ID string stored for the current thread's driver context.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no driver context (and thus no correlation ID)
    /// has been set for the current thread.</exception>
    public string GetCorrelationId();

    /// <summary>
    /// Checks if a WebDriver instance has been initialized and set in the context for the current thread.
    /// </summary>
    /// <returns><c>true</c> if a driver context (including a driver instance) is currently set for the thread; otherwise, <c>false</c>.</returns>
    public bool IsDriverInitialized();

    /// <summary>
    /// Clears the WebDriver instance and all associated context (test name, correlation ID)
    /// from the thread-local storage for the current thread.
    /// </summary>
    /// <remarks>
    /// This method should be called to ensure that thread-local data is cleaned up,
    /// typically at the end of a test or when a driver session is no longer needed for the thread.
    /// It prevents data from one test execution leaking into another on the same thread if threads are reused.
    /// Often, the <see cref="IDisposable.Dispose"/> method of this service might also call this internally.
    /// </remarks>
    public void ClearDriverContext();
}
