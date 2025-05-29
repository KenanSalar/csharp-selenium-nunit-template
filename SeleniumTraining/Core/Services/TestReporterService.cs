using NUnit.Framework.Interfaces;

namespace SeleniumTraining.Core.Services;

public class TestReporterService : BaseService, ITestReporterService
{
    public TestReporterService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{ServiceName} initialized.", nameof(TestReporterService));
    }

    public void InitializeTestReport(string allureDisplayName, string browserName, string correlationId)
    {
        var scopeProperties = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["AllureDisplayName"] = allureDisplayName,
            ["BrowserName"] = browserName
        };

        using (Logger.BeginScope(scopeProperties!))
        {
            Logger.LogInformation("Initializing Allure test case: {AllureDisplayName}, Browser: {BrowserName}", allureDisplayName, browserName);
            try
            {
                _ = AllureLifecycle.Instance.UpdateTestCase(tc =>
                {
                    tc.name = allureDisplayName;
                    tc.parameters.Add(new Parameter { name = "Browser", value = browserName, mode = ParameterMode.Default });
                });
                Logger.LogDebug("Allure test case details updated successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to update Allure test case details for {AllureDisplayName}.", allureDisplayName);
            }
        }
    }

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

        using (Logger.BeginScope(scopeProperties!))
        {
            Logger.LogInformation("Finalizing report for test: {TestName}, Outcome: {Outcome}", testName, testOutcome.Status);

            try
            {
                if (testOutcome.Status == TestStatus.Failed)
                {
                    Logger.LogError(
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

                        Logger.LogDebug(
                            "Attempting to save screenshot for failed test {TestFullName} to {ScreenshotFilePath}",
                            testName,
                            screenshotFilePath
                        );
                        SaveScreenshotInternal(driver, screenshotFilePath, testName);

                        // Attach to NUnit test result
                        TestContext.AddTestAttachment(screenshotFilePath, "Screenshot on failure");
                        Logger.LogDebug("Screenshot {ScreenshotFilePath} added as NUnit test attachment.", screenshotFilePath);

                        // Attach to Allure report
                        if (File.Exists(screenshotFilePath))
                        {
                            AllureApi.AddAttachment(screenshotFileName, "image/png", screenshotFilePath);
                            Logger.LogInformation("Screenshot saved to {ScreenshotFilePath} for {TestName}", screenshotFilePath, testName);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Test {TestName} failed but WebDriver was not available for screenshot.", testName);
                    }
                }
                else if (testOutcome.Status == TestStatus.Passed)
                {
                    Logger.LogWarning(
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
                    Logger.LogWarning("Test {TestName} had outcome {Outcome} with message: {Message}", testName, testOutcome.Label, testContext.Result.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected exception during report finalization for test {TestFullName}.", testName);
            }
            finally
            {
                Logger.LogInformation("Report finalization process complete for test {TestFullName}.", testName);
            }
        }
    }

    private void SaveScreenshotInternal(IWebDriver driver, string filePath, string testNameForLog)
    {
        if (driver is ITakesScreenshot screenshotDriver)
        {
            try
            {
                Logger.LogDebug("Taking screenshot for test {TestNameForLog}, target path: {FilePath}", testNameForLog, filePath);
                Screenshot screenshot = screenshotDriver.GetScreenshot();

                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                }
                else
                {
                    Logger.LogWarning(
                        "Could not determine directory for screenshot path {FilePath} for test {TestNameForLog}. Screenshot may fail.",
                        filePath,
                        testNameForLog
                    );
                }
                screenshot.SaveAsFile(filePath);
                Logger.LogInformation("Screenshot successfully saved to {FilePath} for test {TestNameForLog}", filePath, testNameForLog);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to save screenshot for test {TestName} to {FilePath}.", testNameForLog, filePath);
            }
        }
        else
        {
            Logger.LogWarning("WebDriver instance does not support ITakesScreenshot for test {TestName}.", testNameForLog);
        }
    }
}
