namespace SeleniumTraining.Core.Services.Drivers.Contracts;

/// <summary>
/// Defines the contract for a service that manages the lifecycle of an <see cref="IWebDriver"/> instance
/// for the scope of a single test execution.
/// This includes initialization, retrieval of the active driver, and cleanup (quitting the driver).
/// </summary>
/// <remarks>
/// Implementations of this interface are typically scoped per test. They ensure that each test
/// (or parallel test execution thread) has its own isolated WebDriver instance.
/// This service often coordinates with other services like <see cref="IDriverInitializationService"/>
/// for creating the driver and <see cref="IDriverLifecycleService"/> for tearing it down.
/// It also provides a way to check if a driver is currently active for the scope.
/// Being <see cref="IDisposable"/>, it's expected that the <see cref="Dispose"/> method
/// will handle the necessary cleanup, typically by calling <see cref="QuitDriver"/>.
/// </remarks>
public interface ITestWebDriverManager : IDisposable
{
    /// <summary>
    /// Initializes a WebDriver instance for the specified browser type, associating it with the given
    /// test name and correlation ID for the current test scope.
    /// If a driver is already active within this manager's scope, it should typically be quit first
    /// before initializing a new one, or an exception should be thrown to indicate an improper state.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> for which to initialize the WebDriver (e.g., Chrome, Firefox).</param>
    /// <param name="testName">The name of the current test or test class, used for logging, reporting, or context-specific configurations.</param>
    /// <param name="correlationId">A unique identifier for the current test execution, useful for tracing and correlating logs.</param>
    /// <exception cref="InvalidOperationException">Thrown if an attempt is made to initialize a driver when one is already active and not handled,
    /// or if the initialization process itself encounters an unexpected state.</exception>
    /// <exception cref="WebDriverException">Thrown if WebDriver instantiation or configuration fails (e.g., driver executable issues, browser launch failures).</exception>
    public void InitializeDriver(BrowserType browserType, string testName, string correlationId);

    /// <summary>
    /// Retrieves the currently active <see cref="IWebDriver"/> instance managed by this service.
    /// </summary>
    /// <returns>The active <see cref="IWebDriver"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no WebDriver instance is currently active or initialized
    /// (i.e., if <see cref="InitializeDriver"/> has not been successfully called or <see cref="QuitDriver"/> has already been called).</exception>
    public IWebDriver GetDriver();

    /// <summary>
    /// Quits the currently active <see cref="IWebDriver"/> instance managed by this service.
    /// This involves closing all associated browser windows and ending the browser session.
    /// After this call, <see cref="IsDriverActive"/> should return false.
    /// </summary>
    /// <remarks>
    /// This method should be called at the end of a test's scope to ensure proper resource cleanup.
    /// It's often invoked by the <see cref="Dispose"/> method of this interface.
    /// Implementations should handle cases where the driver might already be null or unresponsive.
    /// </remarks>
    /// <exception cref="WebDriverException">May be thrown if an error occurs during the process of quitting the driver,
    /// though implementations are encouraged to handle such exceptions internally for robust cleanup.</exception>
    public void QuitDriver();

    /// <summary>
    /// Gets a value indicating whether a WebDriver instance is currently active and managed by this manager.
    /// </summary>
    /// <value>
    /// <c>true</c> if an <see cref="IWebDriver"/> instance is active; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property can be checked before attempting to get or quit the driver to avoid exceptions
    /// if no driver has been initialized or if it has already been quit.
    /// </remarks>
    public bool IsDriverActive { get; }
}
