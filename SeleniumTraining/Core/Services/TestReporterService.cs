using NUnit.Framework.Interfaces;

namespace SeleniumTraining.Core.Services;

/// <summary>
/// Service responsible for initializing and finalizing test reports,
/// with a primary focus on integration with the Allure reporting framework.
/// This service delegates screenshot capture to an <see cref="IScreenshotService"/>.
/// </summary>
/// <remarks>
/// This service implements <see cref="ITestReporterService"/> and handles the setup of Allure test case
/// details at the beginning of a test. At the end of a test, it processes the test outcome,
/// logs failure information, and coordinates with the <see cref="IScreenshotService"/> to obtain
/// a screenshot on failure, which is then attached to NUnit and Allure reports.
/// It utilizes NUnit's <see cref="TestContext"/> for result information.
/// This class inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class TestReporterService : BaseService, ITestReporterService
{
    private readonly IScreenshotService _screenshotService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestReporterService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <param name="screenshotService">The service responsible for capturing and saving screenshots. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> or <paramref name="screenshotService"/> is null.</exception>
    public TestReporterService(ILoggerFactory loggerFactory, IScreenshotService screenshotService)
        : base(loggerFactory)
    {
        _screenshotService = screenshotService ?? throw new ArgumentNullException(nameof(screenshotService));
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(TestReporterService));
    }

    /// <inheritdoc cref="ITestReporterService.InitializeTestReport(string, string, string)" />
    /// <remarks>
    /// This implementation updates the current Allure test case with the provided display name
    /// and adds the browser name as a parameter to the Allure report.
    /// Exceptions during Allure interaction are caught and logged.
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
    /// For failed tests where a <paramref name="driver"/> instance is available, it:
    /// <list type="bullet">
    ///   <item><description>Logs detailed failure information.</description></item>
    ///   <item><description>Constructs a unique file name for the screenshot.</description></item>
    ///   <item><description>Invokes the injected <see cref="IScreenshotService"/> to capture and save the screenshot to the provided <paramref name="screenshotDirectory"/>.</description></item>
    ///   <item><description>If screenshot capture is successful, attaches the screenshot file to NUnit test results and the Allure report.</description></item>
    /// </list>
    /// For passed or other outcomes, it logs the status.
    /// All significant operations and potential errors during finalization are logged.
    /// </remarks>
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
                        string screenshotFileNameWithoutExtension = $"{testName}_{browserType}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";

                        ServiceLogger.LogDebug(
                            "Requesting screenshot capture for failed test {TestFullName}. Directory: {ScreenshotDir}, Base FileName: {ScreenshotBaseName}",
                            testName,
                            screenshotDirectory,
                            screenshotFileNameWithoutExtension
                        );

                        string? screenshotFilePath = _screenshotService.CaptureAndSaveScreenshot(driver, screenshotDirectory, screenshotFileNameWithoutExtension);

                        if (!string.IsNullOrEmpty(screenshotFilePath))
                        {
                            TestContext.AddTestAttachment(screenshotFilePath, "Screenshot on failure");
                            ServiceLogger.LogDebug("Screenshot {ScreenshotFilePath} added as NUnit test attachment.", screenshotFilePath);

                            string allureAttachmentName = Path.GetFileName(screenshotFilePath);
                            AllureApi.AddAttachment(allureAttachmentName, "image/png", screenshotFilePath);
                            ServiceLogger.LogInformation("Screenshot attached to Allure as {AllureAttachmentName} from {ScreenshotFilePath} for {TestName}", allureAttachmentName, screenshotFilePath, testName);
                        }
                        else
                        {
                            ServiceLogger.LogWarning("Screenshot capture or save failed for test {TestName}. ScreenshotService did not return a file path.", testName);
                        }
                    }
                    else
                    {
                        ServiceLogger.LogWarning("Test {TestName} failed but WebDriver was not available for screenshot.", testName);
                    }
                }
                else if (testOutcome.Status == TestStatus.Passed)
                {
                    ServiceLogger.LogInformation(
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
}
