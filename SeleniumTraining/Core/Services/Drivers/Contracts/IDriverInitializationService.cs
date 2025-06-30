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
/// </remarks>
public interface IDriverInitializationService
{
    /// <summary>
    /// Initializes and returns a new <see cref="IWebDriver"/> instance tailored for the specified
    /// browser type, test context, and correlation ID.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> for which the driver should be initialized.</param>
    /// <param name="testName">The name of the current test or test class.</param>
    /// <param name="correlationId">A unique identifier for the current test execution.</param>
    /// <returns>
    /// A <see cref="Result{TSuccess, TFailure}"/> object that, on success, contains the initialized <see cref="IWebDriver"/> instance,
    /// or on failure, contains a <see cref="string"/> with the error message.
    /// </returns>
    /// <remarks>
    /// This method encapsulates the entire driver creation process. Instead of throwing exceptions for predictable
    /// failures (e.g., a factory unable to create a driver), it returns a Failure result, making error handling
    /// more explicit for the consumer.
    /// </remarks>
    public Result<IWebDriver, string> InitializeDriver(BrowserType browserType, string testName, string correlationId);
}
