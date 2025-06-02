namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service responsible for initializing and finalizing test reports,
/// typically for integration with reporting frameworks like Allure.
/// </summary>
/// <remarks>
/// This service encapsulates the logic for setting up report metadata at the beginning
/// of a test and capturing relevant information (like screenshots on failure) at the end.
/// It helps in generating comprehensive and informative test execution reports.
/// </remarks>
public interface ITestReporterService
{
    /// <summary>
    /// Initializes the test report for the currently executing test case.
    /// This method should be called at the beginning of each test (e.g., in SetUp).
    /// </summary>
    /// <param name="allureDisplayName">The display name for the test case as it should appear in the Allure report.
    /// This often includes the test method name and browser type.</param>
    /// <param name="browserName">The name of the browser on which the test is being executed (e.g., "Chrome", "Firefox").
    /// Used for categorizing or tagging results in the report.</param>
    /// <param name="correlationId">A unique identifier for the test execution, used for correlating logs and report entries.</param>
    /// <remarks>
    /// This method sets up the necessary context for Allure or other reporting tools,
    /// allowing subsequent logs, steps, and attachments to be correctly associated with the current test.
    /// </remarks>
    public void InitializeTestReport(string allureDisplayName, string browserName, string correlationId);

    /// <summary>
    /// Finalizes the test report for the currently executing test case.
    /// This method should be called at the end of each test (e.g., in TearDown),
    /// typically after the test outcome is known.
    /// </summary>
    /// <param name="testContext">The NUnit <see cref="TestContext"/> for the current test, providing access to test results and metadata.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance used in the test. Can be null if the driver failed to initialize or was already quit.
    /// Used for taking screenshots on failure.</param>
    /// <param name="browserType">The <see cref="BrowserType"/> on which the test was executed. Used for report categorization.</param>
    /// <param name="screenshotDirectory">The directory where screenshots (e.g., on failure) should be saved.
    /// This path is then typically used for attaching screenshots to the report.</param>
    /// <param name="correlationId">The unique correlation ID for the test execution, used to link report entries with logs.</param>
    /// <remarks>
    /// This method is responsible for actions like:
    /// <list type="bullet">
    ///   <item><description>Determining the test status (pass/fail/skipped).</description></item>
    ///   <item><description>Taking and attaching screenshots if the test failed (if a driver instance is available).</description></item>
    ///   <item><description>Adding any final environment details or test outcome information to the report.</description></item>
    /// </list>
    /// </remarks>
    public void FinalizeTestReport(
        TestContext testContext,
        IWebDriver? driver,
        BrowserType browserType,
        string screenshotDirectory,
        string correlationId
    );
}
