namespace SeleniumTraining.Core.Services.Drivers.Contracts;

/// <summary>
/// Defines the contract for a service responsible for initializing a WebDriver instance
/// for a specified browser type, test context, and correlation ID.
/// </summary>
/// <remarks>
/// This service typically orchestrates the entire driver setup process, which may include:
/// <list type="bullet">
///   <item><description>Retrieving browser-specific settings and capabilities.</description></item>
///   <item><description>Invoking an appropriate browser factory (<see cref="IBrowserDriverFactoryService"/>) to create the WebDriver.</description></item>
///   <item><description>Applying common WebDriver configurations (e.g., timeouts, window size) after creation.</description></item>
///   <item><description>Managing the lifecycle of the driver instance for the current test scope.</description></item>
/// </list>
/// It acts as a central point for initiating a browser session for test execution.
/// </remarks>
public interface IDriverInitializationService
{
    /// <summary>
    /// Initializes and returns a new or existing <see cref="IWebDriver"/> instance tailored for the specified
    /// browser type, test context, and correlation ID.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> for which the driver should be initialized (e.g., Chrome, Firefox).</param>
    /// <param name="testName">The name of the current test or test class, used for logging, reporting, or context-specific configurations.</param>
    /// <param name="correlationId">A unique identifier for the current test execution, useful for tracing and correlating logs across services.</param>
    /// <returns>A configured <see cref="IWebDriver"/> instance ready for use in a test.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="testName"/> or <paramref name="correlationId"/> is null or empty.</exception>
    /// <exception cref="NotSupportedException">Thrown if the specified <paramref name="browserType"/> is not supported.</exception>
    /// <exception cref="WebDriverException">Thrown if any part of the WebDriver creation or configuration process fails (e.g., issues with driver executables, browser launch failures).</exception>
    /// <remarks>
    /// If a driver instance is already active for the current context (e.g., within a test scope),
    /// an implementation might choose to return the existing instance or quit it and create a new one,
    /// depending on the desired lifecycle management strategy.
    /// </remarks>
    public IWebDriver InitializeDriver(BrowserType browserType, string testName, string correlationId);
}
