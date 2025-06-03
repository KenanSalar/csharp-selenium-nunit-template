namespace SeleniumTraining.Core.Services.Drivers.Contracts;

/// <summary>
/// Defines the contract for a service responsible for managing the termination
/// (quitting) of WebDriver instances.
/// </summary>
/// <remarks>
/// This service encapsulates the logic required to properly close browser sessions
/// and release associated resources at the end of a test or test scope.
/// It complements services responsible for driver initialization by handling the teardown phase.
/// </remarks>
public interface IDriverLifecycleService
{
    /// <summary>
    /// Quits the provided <see cref="IWebDriver"/> instance, closing all associated browser windows
    /// and ending the browser session.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance to quit. If null, the method may log a warning or do nothing,
    /// depending on the implementation's error handling strategy.</param>
    /// <param name="testClassName">The name of the test class associated with this driver instance.
    /// Used for logging and context, helping to identify which test's driver is being quit.</param>
    /// <param name="correlationId">A unique identifier for the test execution associated with this driver.
    /// Used for correlating logs and tracing the driver's lifecycle.</param>
    /// <remarks>
    /// It is crucial to call this method at the end of each test or test session to prevent
    /// resource leaks (e.g., orphaned browser processes or driver executables).
    /// Implementations should handle potential exceptions during the quit process gracefully (e.g., if the driver is already unresponsive).
    /// This method should effectively call <c>driver.Quit()</c>.
    /// </remarks>
    /// <exception cref="WebDriverException">May be thrown if an error occurs during the process of quitting the driver,
    /// though implementations are encouraged to handle such exceptions internally to ensure cleanup proceeds as much as possible.</exception>
    public void QuitDriver(IWebDriver driver, string testClassName, string correlationId);
}
