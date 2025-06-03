namespace SeleniumTraining.Utils.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IWebDriver"/> instances,
/// offering enhanced or safer ways to interact with the WebDriver.
/// </summary>
/// <remarks>
/// This static class centralizes utility functions that extend the functionality of <see cref="IWebDriver"/>.
/// Currently, it includes <see cref="QuitSafely"/>, which is designed to make WebDriver termination
/// more resilient by handling potential exceptions during the <c>Quit()</c> operation.
/// Such robust cleanup is particularly important in automated test environments,
/// including CI/CD pipelines, to prevent orphaned browser processes.
/// </remarks>
public static class WebDriverExtensions
{
    /// <summary>
    /// Attempts to quit the provided <see cref="IWebDriver"/> instance gracefully,
    /// logging any exceptions that occur during the quit process without re-throwing them.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance to quit.
    /// This method handles cases where <paramref name="driver"/> might be null.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance to use for logging messages and exceptions.
    /// Must not be null if <paramref name="driver"/> is not null and an operation is attempted.</param>
    /// <param name="contextMessage">A message providing context for the quit operation (e.g., test name, reason for quit),
    /// used in log messages for better traceability.</param>
    /// <remarks>
    /// This extension method is designed to prevent tests or cleanup processes from failing due to
    /// exceptions thrown by <c>driver.Quit()</c>. This can happen if the driver is already in an
    /// invalid state, has been terminated externally, or if the browser process is unresponsive.
    /// <list type="bullet">
    ///   <item><description>If <paramref name="driver"/> is null, the method logs a debug message (if logger available) and returns without action.</description></item>
    ///   <item><description>If <paramref name="driver"/> is not null, it calls <c>driver.Quit()</c>.</description></item>
    ///   <item><description>Successful quit operations are logged at the Debug level.</description></item>
    ///   <item><description>Any <see cref="Exception"/> during <c>driver.Quit()</c> is caught, logged as a Warning, and then suppressed.</description></item>
    /// </list>
    /// This method ensures that the attempt to release WebDriver resources is made without risking the stability of the overall test run or teardown sequence.
    /// </remarks>
    public static void QuitSafely(this IWebDriver driver, ILogger logger, string contextMessage)
    {
        if (driver == null)
            return;

        try
        {
            driver.Quit();
            logger.LogDebug("WebDriver QuitSafely successful for context: {Context}", contextMessage);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception during WebDriver QuitSafely ({Context}). Driver might not have been fully initialized or already closed.", contextMessage);
        }
    }
}
