namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service that orchestrates the complete lifecycle
/// of a single test, from setup and initialization to finalization and reporting.
/// </summary>
/// <remarks>
/// This service acts as a central coordinator, ensuring that the driver is initialized,
/// reports are set up, and all resources are properly cleaned up in a consistent sequence for each test.
/// </remarks>
public interface ITestLifecycleManager
{
    /// <summary>
    /// Gets the service for managing the WebDriver lifecycle for the current test.
    /// </summary>
    public ITestWebDriverManager WebDriverManager { get; }

    /// <summary>
    /// Gets the service for performing visual regression testing.
    /// </summary>
    public IVisualTestService VisualTester { get; }

    /// <summary>
    /// Initializes the test environment for a single test execution. This includes
    /// running CI environment checks, preparing directories, initializing reports,
    /// and creating the WebDriver instance.
    /// </summary>
    /// <param name="testName">The name of the current test class, used for logging and directory creation.</param>
    /// <param name="testContext">The NUnit TestContext for the currently running test.</param>
    /// <param name="browserType">The browser the test is configured to run on.</param>
    public void InitializeTestScope(string testName, TestContext testContext, BrowserType browserType);

    /// <summary>
    /// Finalizes the test execution. This includes generating reports (with screenshots
    /// on failure) and safely quitting the WebDriver.
    /// </summary>
    /// <param name="testContext">The NUnit TestContext for the completed test, containing the test outcome.</param>
    public void FinalizeTestScope(TestContext testContext);
}
