namespace SeleniumTraining.Core.Services;

/// <summary>
/// Orchestrates the setup and teardown logic for a single test execution.
/// This class is responsible for initializing the driver, setting up reporting,
/// and ensuring everything is cleaned up correctly.
/// </summary>
/// <remarks>
/// This is a scoped service that uses constructor injection to receive all its
/// dependencies. It is resolved once per test and contains the core logic
/// that was previously in BaseTest, adhering to the Single Responsibility Principle.
/// </remarks>
public class TestLifecycleManager : BaseService, ITestLifecycleManager
{
    private readonly ITestReporterService _testReporter;
    private readonly IDirectoryManagerService _directoryManager;

    /// <inheritdoc/>
    public ITestWebDriverManager WebDriverManager { get; }

    /// <inheritdoc/>
    public IVisualTestService VisualTester { get; }

    /// <summary>
    /// Gets the browser type for the current test execution.
    /// </summary>
    public BrowserType CurrentBrowserType { get; private set; }

    /// <summary>
    /// Gets the full path to the screenshot directory for the current test.
    /// </summary>
    public string CurrentTestScreenshotDirectory { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique correlation ID for the current test execution.
    /// </summary>
    public string CorrelationId { get; private set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestLifecycleManager"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="webDriverManager">The service for managing the WebDriver instance.</param>
    /// <param name="testReporter">The service for initializing and finalizing test reports.</param>
    /// <param name="directoryManager">The service for managing test artifact directories.</param>
    /// <param name="visualTester">The service for performing visual regression testing.</param>
    public TestLifecycleManager(
        ILoggerFactory loggerFactory,
        ITestWebDriverManager webDriverManager,
        ITestReporterService testReporter,
        IDirectoryManagerService directoryManager,
        IVisualTestService visualTester
    )
        : base(loggerFactory)
    {
        WebDriverManager = webDriverManager;
        _testReporter = testReporter;
        _directoryManager = directoryManager;
        VisualTester = visualTester;
    }

    /// <inheritdoc/>
    public void InitializeTestScope(string testName, TestContext testContext, BrowserType browserType)
    {
        CurrentBrowserType = browserType;

        // CI Browser Check Logic
        string? targetBrowserCiEnv = Environment.GetEnvironmentVariable("TARGET_BROWSER_CI");
        if (!string.IsNullOrEmpty(targetBrowserCiEnv) && Enum.TryParse(targetBrowserCiEnv, true, out BrowserType ciBrowserType))
        {
            if (browserType != ciBrowserType)
            {
                string skipMessage = $"Skipping test fixture: Fixture is for '{browserType}', but CI job is targeting '{ciBrowserType}'.";
                ServiceLogger.LogWarning(
                    "Skipping test fixture: Fixture is for '{BrowserType}', but CI job is targeting '{CiBrowserType}'.",
                    browserType,
                    ciBrowserType
                );
                Assert.Ignore(skipMessage);
                return;
            }
        }

        CorrelationId = Guid.NewGuid().ToString("N")[..12];
        CurrentTestScreenshotDirectory = _directoryManager.GetAndEnsureTestScreenshotDirectory(testName);
        ServiceLogger.LogInformation("Screenshot directory prepared: {ScreenshotDir}", CurrentTestScreenshotDirectory);

        // Initialize Reporting
        string? baseMethodName = testContext.Test.MethodName ?? "UnknownMethod";
        string allureDisplayName = $"{baseMethodName} ({browserType})";
        _testReporter.InitializeTestReport(allureDisplayName, browserType.ToString(), CorrelationId);
        ServiceLogger.LogInformation("Test report initialized for Allure.");

        // Initialize WebDriver
        ServiceLogger.LogDebug("Initializing WebDriver via WebDriverManager for {Browser}.", browserType);
        WebDriverManager.InitializeDriver(browserType, testName, CorrelationId);
        ServiceLogger.LogInformation("WebDriver initialization requested for {Browser}.", browserType);
    }

    /// <inheritdoc/>
    public void FinalizeTestScope(TestContext testContext)
    {
        IWebDriver? driverForActions = null;
        if (WebDriverManager.IsDriverActive)
        {
            try
            {
                driverForActions = WebDriverManager.GetDriver();
            }
            catch (Exception ex)
            {
                ServiceLogger.LogWarning(ex, "Failed to get WebDriver during FinalizeTestScope. Screenshot may be missed.");
            }
        }

        // Finalize Reporting (takes screenshot on failure)
        _testReporter.FinalizeTestReport(
            testContext,
            driverForActions,
            CurrentBrowserType,
            CurrentTestScreenshotDirectory,
            CorrelationId
        );

        // Quit WebDriver
        if (WebDriverManager.IsDriverActive)
        {
            ServiceLogger.LogDebug("Quitting WebDriver via WebDriverManager in FinalizeTestScope.");
            WebDriverManager.QuitDriver();
        }
    }
}
