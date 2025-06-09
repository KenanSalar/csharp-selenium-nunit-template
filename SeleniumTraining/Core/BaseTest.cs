namespace SeleniumTraining.Core;

/// <summary>
/// Provides a base class for all Selenium UI tests, encapsulating common setup,
/// teardown, dependency injection, and test context management logic.
/// It supports browser type specification for test fixtures and integrates with Allure reporting.
/// </summary>
/// <remarks>
/// This abstract class is designed to be inherited by concrete test fixture classes.
/// It handles:
/// <list type="bullet">
///   <item><description>Dependency injection scope management per test using <see cref="TestHost.Services"/>.</description></item>
///   <item><description>Resolution of core services like <see cref="ISettingsProviderService"/>, <see cref="ITestWebDriverManager"/>, <see cref="IResourceMonitorService"/>, etc.</description></item>
///   <item><description>Initialization and cleanup of WebDriver via <see cref="ITestWebDriverManager"/>.</description></item>
///   <item><description>Reporting setup and finalization via <see cref="ITestReporterService"/>, primarily for Allure.</description></item>
///   <item><description>Resource monitoring via <see cref="IResourceMonitorService"/>, often integrated with <see cref="PerformanceTimer"/>.</description></item>
///   <item><description>Correlation ID generation for tracing.</description></item>
///   <item><description>Logging setup using <see cref="ILogger"/> specific to the test class.</description></item>
///   <item><description>Handling of CI environment variables (TARGET_BROWSER_CI) to skip non-matching test fixtures.</description></item>
///   <item><description>Proper resource disposal via <see cref="IDisposable"/>.</description></item>
/// </list>
/// Concrete test classes must provide a <see cref="BrowserType"/> to the constructor.
/// </remarks>
[AllureNUnit]
public abstract class BaseTest : IDisposable
{
    private bool _disposed;
    private IServiceScope? _testScope; // DI scope for the current test

    /// <summary>
    /// Gets the <see cref="BrowserType"/> for which this test fixture is configured to run.
    /// This is set via the constructor by derived test classes.
    /// </summary>
    /// <value>The configured browser type.</value>
    protected BrowserType BrowserType { get; }

    /// <summary>
    /// Gets the browser-specific settings loaded for the current <see cref="BrowserType"/>.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The browser settings. Null until <see cref="SetUp"/> completes successfully.</value>
    protected BaseBrowserSettings BrowserSettings { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="ILoggerFactory"/> instance used for creating loggers for page objects.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The logger factory for page objects. Null until <see cref="SetUp"/> completes successfully.</value>
    protected ILoggerFactory PageObjectLoggerFactory { get; private set; } = null!;

    /// <summary>
    /// Gets the service for accessing application and framework settings.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The settings provider service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected ISettingsProviderService SettingsProvider { get; private set; } = null!;

    /// <summary>
    /// Gets the service for managing directory paths for test artifacts.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The directory manager service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected IDirectoryManagerService DirectoryManager { get; private set; } = null!;

    /// <summary>
    /// Gets the service for managing the WebDriver lifecycle for the current test.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The WebDriver manager service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected ITestWebDriverManager WebDriverManager { get; private set; } = null!;

    /// <summary>
    /// Gets the service for initializing and finalizing test reports (e.g., for Allure).
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The test reporter service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected ITestReporterService TestReporter { get; private set; } = null!;

    /// <summary>
    /// Gets the service for performing visual regression testing.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The visual test service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected IVisualTestService VisualTester { get; private set; } = null!;

    /// <summary>
    /// Gets the service for executing actions with retry policies.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The retry service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected IRetryService RetryService { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance specifically for logging messages from the current test class.
    /// It is initialized in <see cref="SetUp"/> and configured with the test class name.
    /// </summary>
    /// <value>The logger for the current test. Null until <see cref="SetUp"/> completes successfully.</value>
    public ILogger TestLogger { get; protected set; } = null!;

    /// <summary>
    /// Gets the full path to the directory where screenshots for the current test (or test class) will be saved.
    /// This path is determined and ensured to exist during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The screenshot directory path. Empty until <see cref="SetUp"/> completes successfully.</value>
    protected string CurrentTestScreenshotDirectory { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the name of the current test class. This is typically derived from the type name of the concrete test fixture.
    /// </summary>
    /// <value>The name of the test class.</value>
    protected string TestName { get; }

    /// <summary>
    /// Gets a unique correlation ID generated for the current test execution.
    /// This ID is used for tracing logs and correlating activities across different services.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The correlation ID. Empty until <see cref="SetUp"/> completes successfully.</value>
    protected string CorrelationId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the service for monitoring process resource usage, such as memory.
    /// Initialized during <see cref="SetUp"/>.
    /// </summary>
    /// <value>The resource monitor service. Null until <see cref="SetUp"/> completes successfully.</value>
    protected IResourceMonitorService ResourceMonitor { get; private set; } = null!;

    private IDisposable? _loggingScope; // For structured logging scope


    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTest"/> class for a specific browser type.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> on which the tests in this fixture are intended to run.</param>
    /// <remarks>
    /// This constructor sets the <see cref="BrowserType"/> and determines the <see cref="TestName"/>
    /// based on the derived class's type name. Core service resolution and other setup occur in the <see cref="SetUp"/> method.
    /// </remarks>
    protected BaseTest(BrowserType browserType)
    {
        BrowserType = browserType;
        TestName = GetType().Name;
    }

    /// <summary>
    /// Performs setup operations common to all tests before each test method execution.
    /// This includes:
    /// <list type="bullet">
    ///   <item><description>Creating a new DI scope and resolving necessary services (including <see cref="IResourceMonitorService"/>).</description></item>
    ///   <item><description>Initializing the <see cref="TestLogger"/> with a logging scope including correlation ID and test context.</description></item>
    ///   <item><description>Checking CI environment variables (TARGET_BROWSER_CI) and skipping tests if the fixture's <see cref="BrowserType"/> does not match.</description></item>
    ///   <item><description>Loading browser-specific settings.</description></item>
    ///   <item><description>Ensuring necessary output directories exist.</description></item>
    ///   <item><description>Initializing the test report (e.g., for Allure).</description></item>
    ///   <item><description>Initializing the WebDriver instance for the configured <see cref="BrowserType"/>.</description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// This method is decorated with <see cref="SetUpAttribute"/> and will be called by NUnit before each test.
    /// If a test fixture is skipped due to a browser mismatch in CI, <see cref="Assert.Ignore(string)"/> is called.
    /// </remarks>
    [SetUp]
    public virtual void SetUp()
    {
        _testScope = TestHost.Services.CreateScope();
        IServiceProvider scopedServiceProvider = _testScope.ServiceProvider;

        PageObjectLoggerFactory = scopedServiceProvider.GetRequiredService<ILoggerFactory>()!;
        RetryService = scopedServiceProvider.GetRequiredService<IRetryService>()!;
        ResourceMonitor = scopedServiceProvider.GetRequiredService<IResourceMonitorService>()!;
        SettingsProvider = scopedServiceProvider.GetRequiredService<ISettingsProviderService>()!;
        DirectoryManager = scopedServiceProvider.GetRequiredService<IDirectoryManagerService>()!;
        WebDriverManager = scopedServiceProvider.GetRequiredService<ITestWebDriverManager>()!;
        TestReporter = scopedServiceProvider.GetRequiredService<ITestReporterService>()!;
        VisualTester = scopedServiceProvider.GetRequiredService<IVisualTestService>()!;

        TestLogger = PageObjectLoggerFactory.CreateLogger(TestName);

        string? targetBrowserCiEnv = Environment.GetEnvironmentVariable("TARGET_BROWSER_CI");
        TestLogger.LogInformation("CI Environment Variable TARGET_BROWSER_CI: '{TargetBrowserCiEnv}'", targetBrowserCiEnv ?? "Not Set");
        TestLogger.LogInformation("NUnit Test Fixture is configured for BrowserType: '{BrowserType}'", BrowserType);

        if (!string.IsNullOrEmpty(targetBrowserCiEnv))
        {
            if (Enum.TryParse(targetBrowserCiEnv, true, out BrowserType ciBrowserType))
            {
                if (BrowserType != ciBrowserType)
                {
                    string skipMessage = $"Skipping test fixture: Fixture is for '{BrowserType}', but CI job is targeting '{ciBrowserType}'.";
                    TestLogger.LogWarning(
                        "Skipping test fixture: Fixture is for '{BrowserType}', but CI job is targeting '{CiBrowserType}'.",
                        BrowserType,
                        ciBrowserType
                    );
                    Assert.Ignore(skipMessage);

                    return;
                }
                TestLogger.LogInformation("CI Target Browser '{CiBrowserType}' matches Test Fixture Browser '{FixtureBrowserType}'. Proceeding with test setup.", ciBrowserType, BrowserType);
            }
            else
            {
                TestLogger.LogWarning(
                    "Could not parse TARGET_BROWSER_CI environment variable '{TargetBrowserCiEnvVar}' to a known BrowserType. Running test as per fixture: {FixtureBrowserType}",
                    targetBrowserCiEnv,
                    BrowserType
                );
            }
        }
        else
        {
            TestLogger.LogInformation("TARGET_BROWSER_CI environment variable not set (likely a local run). Running test as per fixture: {FixtureBrowserType}", BrowserType);
        }

        CorrelationId = Guid.NewGuid().ToString("N")[..12];
        string testLogFileKey = TestName;

        var scopeProperties = new Dictionary<string, object>
        {
            ["CorrelationId"] = CorrelationId,
            ["TestClassName"] = TestName,
            ["TestMethodName"] = TestContext.CurrentContext.Test.MethodName ?? "UnknownMethod",
            ["BrowserType"] = BrowserType.ToString(),
            ["TestLogFileKey"] = testLogFileKey
        };
        _loggingScope = TestLogger.BeginScope(scopeProperties);

        TestLogger.LogInformation("BaseTest SetUp started for test: {TestClass}", TestName);
        TestLogger.LogDebug("Resolved core services. CorrelationId: {CorrelationId}", CorrelationId);

        BrowserSettings = SettingsProvider.GetBrowserSettings(BrowserType);
        TestLogger.LogDebug("Browser settings loaded: Headless={Headless}, Timeout={Timeout}s", BrowserSettings.Headless, BrowserSettings.TimeoutSeconds);


        DirectoryManager.EnsureBaseDirectoriesExist();

        CurrentTestScreenshotDirectory = DirectoryManager.GetAndEnsureTestScreenshotDirectory(TestName);
        TestLogger.LogInformation("Screenshot directory prepared: {ScreenshotDir}", CurrentTestScreenshotDirectory);

        string? baseMethodName = TestContext.CurrentContext.Test.MethodName ?? "UnknownMethod";
        string browserNameString = BrowserType.ToString();
        string allureDisplayName = $"{baseMethodName} ({browserNameString})";

        TestLogger.LogDebug("Initializing test report via TestReporter. AllureDisplayName: {AllureDisplayName}", allureDisplayName);
        TestReporter.InitializeTestReport(allureDisplayName, browserNameString, CorrelationId);
        TestLogger.LogInformation("Test report initialized for Allure.");

        TestLogger.LogDebug("Initializing WebDriver via WebDriverManager for {Browser}.", BrowserType);
        WebDriverManager.InitializeDriver(BrowserType, TestName, CorrelationId);
        TestLogger.LogInformation("WebDriver initialization requested for {Browser}.", BrowserType);

        TestLogger.LogInformation("BaseTest SetUp completed for {TestClass}.", TestName);
    }

    /// <summary>
    /// Performs cleanup operations after each test. This includes:
    /// <list type="bullet">
    ///   <item><description>Finalizing the test report (including taking screenshots on failure).</description></item>
    ///   <item><description>Quitting the WebDriver instance to close the browser.</description></item>
    ///   <item><description>Disposing the logging and DI scopes for the test.</description></item>
    /// </list>
    /// </summary>
    [TearDown]
    public void Cleanup()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Skipped &&
            TestContext.CurrentContext.Result.Message != null &&
            TestContext.CurrentContext.Result.Message.Contains("Skipping test fixture:"))
        {
            TestLogger.LogInformation("BaseTest Cleanup (NUnit TearDown) skipped due to Assert.Ignore in SetUp for test: {TestFullName}", TestContext.CurrentContext.Test.FullName);
            _loggingScope?.Dispose();
            _testScope?.Dispose();

            return;
        }

        TestLogger.LogInformation("BaseTest Cleanup (NUnit TearDown) started for test: {TestFullName}", TestContext.CurrentContext.Test.FullName);

        IWebDriver? driverForActions = null;
        if (WebDriverManager != null && WebDriverManager.IsDriverActive)
        {
            try
            {
                driverForActions = WebDriverManager.GetDriver();
                TestLogger.LogDebug("WebDriver instance retrieved for cleanup actions.");
            }
            catch (InvalidOperationException ex)
            {
                TestLogger.LogWarning(
                    ex,
                    "Failed to get WebDriver instance during Cleanup for test {TestFullName}. Screenshot/reporting might be affected.",
                    TestContext.CurrentContext.Test.FullName
                );
            }
        }
        else
        {
            TestLogger.LogWarning("WebDriver was not active at the start of Cleanup for {TestFullName}.", TestContext.CurrentContext.Test.FullName);
        }

        if (TestReporter != null)
        {
            TestLogger.LogDebug("Finalizing test report via TestReporter.");

            TestReporter.FinalizeTestReport(
                TestContext.CurrentContext,
                driverForActions,
                BrowserType,
                CurrentTestScreenshotDirectory,
                CorrelationId
            );

            TestLogger.LogInformation(
                "Test report finalized. Test Outcome: {OutcomeStatus} - {OutcomeLabel}",
                TestContext.CurrentContext.Result.Outcome.Status,
                TestContext.CurrentContext.Result.Outcome.Label
            );
        }
        else
        {
            TestLogger.LogWarning("TestReporter was null during Cleanup. Report finalization skipped for {TestFullName}.", TestContext.CurrentContext.Test.FullName);
        }

        try
        {
            if (WebDriverManager != null && WebDriverManager.IsDriverActive)
            {
                TestLogger.LogDebug("Quitting WebDriver via WebDriverManager in TearDown.");
                WebDriverManager.QuitDriver();
                TestLogger.LogInformation("WebDriver quit successfully in TearDown for {TestName}.", TestName);
            }
        }
        catch (Exception ex)
        {
            TestLogger.LogError(ex, "Exception during WebDriverManager.QuitDriver in TearDown for test {TestName}.", TestName);
        }

        TestLogger.LogInformation("BaseTest Cleanup (NUnit TearDown) completed for {TestFullName}.", TestContext.CurrentContext.Test.FullName);

        _loggingScope?.Dispose();
        _loggingScope = null;

        _testScope?.Dispose();
        _testScope = null;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources,
    /// conforming to the <see cref="IDisposable"/> interface.
    /// This is the primary method for resource cleanup and is called by the NUnit framework
    /// or test runner at the end of the fixture's lifecycle.
    /// </summary>
    /// <remarks>
    /// This implementation follows the standard dispose pattern.
    /// It calls the protected virtual <see cref="Dispose(bool)"/> method
    /// and suppresses finalization.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources. It ensures that all disposable services resolved
    /// for the test scope are disposed. The WebDriver instance itself is quit in the `[TearDown]` method.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        string currentTestNameForDispose = TestName ?? "UnknownTest_Dispose";

        if (_disposed)
        {
            TestLogger.LogDebug("BaseTest for '{EffectiveTestName}' already disposed (disposing: {IsDisposing}). Skipping.", currentTestNameForDispose, disposing);
            return;
        }

        if (disposing)
        {
            TestLogger.LogDebug("Disposing WebDriverManager instance for {EffectiveTestName}.", currentTestNameForDispose);
            (WebDriverManager as IDisposable)?.Dispose();

            TestLogger.LogDebug("Disposing TestReporter instance for {EffectiveTestName}.", currentTestNameForDispose);
            (TestReporter as IDisposable)?.Dispose();

            TestLogger.LogDebug("Disposing ConfigProvider instance for {EffectiveTestName}.", currentTestNameForDispose);
            (SettingsProvider as IDisposable)?.Dispose();

            TestLogger.LogDebug("Disposing DirectoryManager instance for {EffectiveTestName}.", currentTestNameForDispose);
            (DirectoryManager as IDisposable)?.Dispose();

            TestLogger.LogDebug("Disposing VisualTester instance for {EffectiveTestName}.", currentTestNameForDispose);
            (VisualTester as IDisposable)?.Dispose();

            TestLogger.LogDebug("Disposing RetryService instance for {EffectiveTestName}.", currentTestNameForDispose);
            (RetryService as IDisposable)?.Dispose();

            TestLogger.LogInformation("Managed services disposed for {EffectiveTestName}.", currentTestNameForDispose);
        }

        if (_loggingScope != null)
        {
            TestLogger.LogDebug("Disposing logging scope for {EffectiveTestName}.", currentTestNameForDispose);
            _loggingScope.Dispose();
            _loggingScope = null;

            Serilog.Log.Debug("BaseTest logging scope explicitly disposed for {EffectiveTestName}.", currentTestNameForDispose);
        }

        if (_testScope != null)
        {
            TestLogger.LogWarning("Test-specific DI scope was not null in BaseTest.Dispose(true). Disposing now. Test: {TestName}", TestName);
            _testScope.Dispose();
            _testScope = null;
        }

        _disposed = true;
        TestLogger.LogInformation("BaseTest Dispose({IsDisposing}) completed for {EffectiveTestName}.", disposing, currentTestNameForDispose);
    }
}
