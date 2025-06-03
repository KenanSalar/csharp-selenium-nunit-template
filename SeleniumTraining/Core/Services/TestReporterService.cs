using NUnit.Framework.Interfaces;

namespace SeleniumTraining.Core.Services;

/// <summary>
/// Service responsible for initializing and finalizing test reports,
/// with a primary focus on integration with the Allure reporting framework.
/// </summary>
/// <remarks>
/// This service implements <see cref="ITestReporterService"/> and handles the setup of Allure test case
/// details at the beginning of a test and the processing of test outcomes, including
/// capturing and attaching screenshots on failure, at the end of a test.
/// It utilizes NUnit's <see cref="TestContext"/> for result information and Selenium's
/// <see cref="ITakesScreenshot"/> for screen capture.
/// This class inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class TestReporterService : BaseService, ITestReporterService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestReporterService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    public TestReporterService(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(TestReporterService));
    }

    /// <inheritdoc cref="ITestReporterService.InitializeTestReport(string, string, string)" />
    /// <remarks>
    /// This implementation updates the current Allure test case with the provided display name
    /// and adds the browser name as a parameter to the Allure report.
    /// Exceptions during Allure interaction are logged.
    /// </remarks>
    public void InitializeTestReport(string allureDisplayName, string browserName, string correlationId)
    {
        var scopeProperties = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["AllureDisplayName"] = allureDisplayName,
            ["BrowserName"] = browserName
        };

        using (ServiceLogger.BeginScope(scopeProperties!))
        {
            ServiceLogger.LogInformation("Initializing Allure test case: {AllureDisplayName}, Browser: {BrowserName}", allureDisplayName, browserName);
            try
            {
                _ = AllureLifecycle.Instance.UpdateTestCase(tc =>
                {
                    tc.name = allureDisplayName;
                    tc.parameters.Add(new Parameter { name = "Browser", value = browserName, mode = ParameterMode.Default });
                });
                ServiceLogger.LogDebug("Allure test case details updated successfully.");
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "Failed to update Allure test case details for {AllureDisplayName}.", allureDisplayName);
            }
        }
    }

    /// <inheritdoc cref="ITestReporterService.FinalizeTestReport(TestContext, IWebDriver, BrowserType, string, string)" />
    /// <remarks>
    /// This implementation processes the NUnit <see cref="TestContext.Result"/> to determine the test outcome.
    /// For failed tests, it logs detailed failure information, attempts to save a screenshot using
    /// the private helper <see cref="SaveScreenshotInternal(IWebDriver, string, string)"/>,
    /// adds the screenshot as an NUnit test attachment, and attaches it to the Allure report.
    /// For passed or other outcomes, it logs the status.
    /// All significant operations and potential errors during finalization are logged.
    public void FinalizeTestReport(
        TestContext testContext,
        IWebDriver? driver,
        BrowserType browserType,
        string screenshotDirectory,
        string correlationId
    )
    {
        ResultState testOutcome = testContext.Result.Outcome;
        string testName = testContext.Test.Name;

        var scopeProperties = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TestName"] = testName,
            ["TestOutcome"] = testOutcome.Status.ToString(),
            ["BrowserType"] = browserType.ToString()
        };

        using (ServiceLogger.BeginScope(scopeProperties!))
        {
            ServiceLogger.LogInformation("Finalizing report for test: {TestName}, Outcome: {Outcome}", testName, testOutcome.Status);

            try
            {
                if (testOutcome.Status == TestStatus.Failed)
                {
                    ServiceLogger.LogError(
                        "Test FAILED: {TestFullName} on {BrowserType}. Label: {OutcomeLabel}. Message: {ResultMessage}. StackTrace: {StackTrace}",
                        testName,
                        browserType.ToString(),
                        testOutcome.Label,
                        testContext.Result.Message,
                        testContext.Result.StackTrace
                    );

                    if (driver != null)
                    {
                        string screenshotFileName = $"{testName}_{browserType}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
                        string screenshotFilePath = Path.Combine(screenshotDirectory, screenshotFileName);

                        ServiceLogger.LogDebug(
                            "Attempting to save screenshot for failed test {TestFullName} to {ScreenshotFilePath}",
                            testName,
                            screenshotFilePath
                        );
                        SaveScreenshotInternal(driver, screenshotFilePath, testName);

                        // Attach to NUnit test result
                        TestContext.AddTestAttachment(screenshotFilePath, "Screenshot on failure");
                        ServiceLogger.LogDebug("Screenshot {ScreenshotFilePath} added as NUnit test attachment.", screenshotFilePath);

                        // Attach to Allure report
                        if (File.Exists(screenshotFilePath))
                        {
                            AllureApi.AddAttachment(screenshotFileName, "image/png", screenshotFilePath);
                            ServiceLogger.LogInformation("Screenshot saved to {ScreenshotFilePath} for {TestName}", screenshotFilePath, testName);
                        }
                    }
                    else
                    {
                        ServiceLogger.LogWarning("Test {TestName} failed but WebDriver was not available for screenshot.", testName);
                    }
                }
                else if (testOutcome.Status == TestStatus.Passed)
                {
                    ServiceLogger.LogWarning(
                        "Test {TestName} on {BrowserType} completed with outcome: {OutcomeStatus} ({OutcomeLabel}). Message: {ResultMessage}",
                        testName,
                        browserType.ToString(),
                        testOutcome.Status,
                        testOutcome.Label,
                        testContext.Result.Message
                    );
                }
                else
                {
                    ServiceLogger.LogWarning("Test {TestName} had outcome {Outcome} with message: {Message}", testName, testOutcome.Label, testContext.Result.Message);
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "Unexpected exception during report finalization for test {TestFullName}.", testName);
            }
            finally
            {
                ServiceLogger.LogInformation("Report finalization process complete for test {TestFullName}.", testName);
            }
        }
    }

    /// <summary>
    /// Saves a screenshot from the provided WebDriver instance to the specified file path.
    /// This is a helper method used internally, typically for capturing evidence on test failures.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance from which to take the screenshot.
    /// Must implement <see cref="ITakesScreenshot"/>.</param>
    /// <param name="filePath">The full path (including filename and extension) where the screenshot will be saved.</param>
    /// <param name="testNameForLog">The name of the test, used for contextual logging of the screenshot operation.</param>
    /// <remarks>
    /// This method first checks if the driver supports <see cref="ITakesScreenshot"/>.
    /// It ensures the target directory exists before attempting to save the screenshot.
    /// Any exceptions during the process (directory creation, screenshot capture, file save) are logged.
    /// </remarks>
    private void SaveScreenshotInternal(IWebDriver driver, string filePath, string testNameForLog)
    {
        if (driver is ITakesScreenshot screenshotDriver)
        {
            try
            {
                ServiceLogger.LogDebug("Taking screenshot for test {TestNameForLog}, target path: {FilePath}", testNameForLog, filePath);
                Screenshot screenshot = screenshotDriver.GetScreenshot();

                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                }
                else
                {
                    ServiceLogger.LogWarning(
                        "Could not determine directory for screenshot path {FilePath} for test {TestNameForLog}. Screenshot may fail.",
                        filePath,
                        testNameForLog
                    );
                }
                screenshot.SaveAsFile(filePath);
                ServiceLogger.LogInformation("Screenshot successfully saved to {FilePath} for test {TestNameForLog}", filePath, testNameForLog);
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "Failed to save screenshot for test {TestName} to {FilePath}.", testNameForLog, filePath);
            }
        }
        else
        {
            ServiceLogger.LogWarning("WebDriver instance does not support ITakesScreenshot for test {TestName}.", testNameForLog);
        }
    }
}
