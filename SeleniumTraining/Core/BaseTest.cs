namespace SeleniumTraining.Core;

[AllureNUnit]
public abstract class BaseTest : IDisposable
{
    private bool _disposed;
    private IServiceScope? _testScope;

    protected BrowserType BrowserType { get; }
    protected BaseBrowserSettings BrowserSettings { get; private set; } = null!;

    // Dependency Injection
    protected ILoggerFactory PageObjectLoggerFactory { get; private set; } = null!;
    protected ISettingsProviderService SettingsProvider { get; private set; } = null!;
    protected IDirectoryManagerService DirectoryManager { get; private set; } = null!;
    protected ITestWebDriverManager WebDriverManager { get; private set; } = null!;
    protected ITestReporterService TestReporter { get; private set; } = null!;
    protected IVisualTestService VisualTester { get; private set; } = null!;
    public ILogger Logger { get; protected set; } = null!;

    protected string CurrentTestScreenshotDirectory { get; private set; } = string.Empty;
    protected string TestName { get; }

    protected string CorrelationId { get; private set; } = string.Empty;
    private IDisposable? _loggingScope;

    protected BaseTest(BrowserType browserType)
    {
        BrowserType = browserType;
        TestName = GetType().Name;
    }

    [SetUp]
    public virtual void SetUp()
    {
        _testScope = TestHost.Services.CreateScope();
        IServiceProvider scopedServiceProvider = _testScope.ServiceProvider;

        ILoggerFactory loggerFactoryForBase = scopedServiceProvider.GetRequiredService<ILoggerFactory>()!;
        Logger = loggerFactoryForBase.CreateLogger(TestName);

        string? targetBrowserCiEnv = Environment.GetEnvironmentVariable("TARGET_BROWSER_CI");
        Logger.LogInformation("CI Environment Variable TARGET_BROWSER_CI: '{TargetBrowserCiEnv}'", targetBrowserCiEnv ?? "Not Set");
        Logger.LogInformation("NUnit Test Fixture is configured for BrowserType: '{BrowserType}'", BrowserType);

        if (!string.IsNullOrEmpty(targetBrowserCiEnv))
        {
            if (Enum.TryParse(targetBrowserCiEnv, true, out BrowserType ciBrowserType))
            {
                if (BrowserType != ciBrowserType)
                {
                    string skipMessage = $"Skipping test fixture: Fixture is for '{BrowserType}', but CI job is targeting '{ciBrowserType}'.";
                    Logger.LogWarning(
                        "Skipping test fixture: Fixture is for '{BrowserType}', but CI job is targeting '{CiBrowserType}'.",
                        BrowserType,
                        ciBrowserType
                    );
                    Assert.Ignore(skipMessage);

                    return;
                }
                Logger.LogInformation("CI Target Browser '{CiBrowserType}' matches Test Fixture Browser '{FixtureBrowserType}'. Proceeding with test setup.", ciBrowserType, BrowserType);
            }
            else
            {
                Logger.LogWarning(
                    "Could not parse TARGET_BROWSER_CI environment variable '{TargetBrowserCiEnvVar}' to a known BrowserType. Running test as per fixture: {FixtureBrowserType}",
                    targetBrowserCiEnv,
                    BrowserType
                );

            }
        }
        else
        {
            Logger.LogInformation("TARGET_BROWSER_CI environment variable not set (likely a local run). Running test as per fixture: {FixtureBrowserType}", BrowserType);
        }

        SettingsProvider = scopedServiceProvider.GetRequiredService<ISettingsProviderService>()!;
        DirectoryManager = scopedServiceProvider.GetRequiredService<IDirectoryManagerService>()!;
        PageObjectLoggerFactory = scopedServiceProvider.GetRequiredService<ILoggerFactory>()!;
        WebDriverManager = scopedServiceProvider.GetRequiredService<ITestWebDriverManager>()!;
        TestReporter = scopedServiceProvider.GetRequiredService<ITestReporterService>()!;
        VisualTester = scopedServiceProvider.GetRequiredService<IVisualTestService>()!;

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
        _loggingScope = Logger.BeginScope(scopeProperties);

        Logger.LogInformation("BaseTest SetUp started for test: {TestClass}", TestName);
        Logger.LogDebug("Resolved core services. CorrelationId: {CorrelationId}", CorrelationId);

        BrowserSettings = SettingsProvider.GetBrowserSettings(BrowserType);
        Logger.LogDebug("Browser settings loaded: Headless={Headless}, Timeout={Timeout}s", BrowserSettings.Headless, BrowserSettings.TimeoutSeconds);


        DirectoryManager.EnsureBaseDirectoriesExist();
        CurrentTestScreenshotDirectory = DirectoryManager.GetAndEnsureTestScreenshotDirectory(TestContext.CurrentContext.Test.Name);
        Logger.LogInformation("Screenshot directory prepared: {ScreenshotDir}", CurrentTestScreenshotDirectory);

        string? baseMethodName = TestContext.CurrentContext.Test.MethodName ?? "UnknownMethod";
        string browserName = BrowserType.ToString();
        string allureDisplayName = $"{baseMethodName} ({browserName})";

        Logger.LogDebug("Initializing test report via TestReporter. AllureDisplayName: {AllureDisplayName}", allureDisplayName);
        TestReporter.InitializeTestReport(allureDisplayName, browserName, CorrelationId);
        Logger.LogInformation("Test report initialized for Allure.");

        Logger.LogDebug("Initializing WebDriver via WebDriverManager for {Browser}.", BrowserType);
        WebDriverManager.InitializeDriver(BrowserType, TestName, CorrelationId);
        Logger.LogInformation("WebDriver initialization requested for {Browser}.", BrowserType);

        Logger.LogInformation("BaseTest SetUp completed for {TestClass}.", TestName);
    }

    [TearDown]
    public void Cleanup()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Skipped &&
            TestContext.CurrentContext.Result.Message.Contains("Skipping test fixture:"))
        {
            Logger.LogInformation("BaseTest Cleanup (NUnit TearDown) skipped due to Assert.Ignore in SetUp for test: {TestFullName}", TestContext.CurrentContext.Test.FullName);
            _loggingScope?.Dispose();
            _testScope?.Dispose();

            return;
        }

        Logger.LogInformation("BaseTest Cleanup (NUnit TearDown) started for test: {TestFullName}", TestContext.CurrentContext.Test.FullName);

        IWebDriver? driverForActions = null;
        if (WebDriverManager != null && WebDriverManager.IsDriverActive)
        {
            try
            {
                driverForActions = WebDriverManager.GetDriver();
                Logger.LogDebug("WebDriver instance retrieved for cleanup actions.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.LogWarning(
                    ex,
                    "Failed to get WebDriver instance during Cleanup for test {TestFullName}. Screenshot/reporting might be affected.",
                    TestContext.CurrentContext.Test.FullName
                );
            }
        }
        else
        {
            Logger.LogWarning("WebDriver was not active at the start of Cleanup for {TestFullName}.", TestContext.CurrentContext.Test.FullName);
        }

        if (TestReporter != null)
        {
            Logger.LogDebug("Finalizing test report via TestReporter.");

            TestReporter.FinalizeTestReport(
                TestContext.CurrentContext,
                driverForActions,
                BrowserType,
                CurrentTestScreenshotDirectory,
                CorrelationId
            );

            Logger.LogInformation(
                "Test report finalized. Test Outcome: {OutcomeStatus} - {OutcomeLabel}",
                TestContext.CurrentContext.Result.Outcome.Status,
                TestContext.CurrentContext.Result.Outcome.Label
            );
        }
        else
        {
            Logger.LogWarning("TestReporter was null during Cleanup. Report finalization skipped for {TestFullName}.", TestContext.CurrentContext.Test.FullName);
        }

        Logger.LogInformation("BaseTest Cleanup (NUnit TearDown) completed for {TestFullName}.", TestContext.CurrentContext.Test.FullName);

        _loggingScope?.Dispose();
        _loggingScope = null;

        _testScope?.Dispose();
        _testScope = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        string currentTestNameForDispose = TestName ?? "UnknownTest_Dispose";

        if (_disposed)
        {
            Logger.LogDebug("BaseTest for '{EffectiveTestName}' already disposed (disposing: {IsDisposing}). Skipping.", currentTestNameForDispose, disposing);
            return;
        }

        if (disposing)
        {
            Logger.LogDebug("Attempting to quit WebDriver for {EffectiveTestName} via WebDriverManager.", currentTestNameForDispose);
            try
            {
                WebDriverManager.QuitDriver();
                Logger.LogInformation("WebDriver quit command issued for {EffectiveTestName}.", currentTestNameForDispose);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception during WebDriverManager.QuitDriver for test {EffectiveTestName}.", currentTestNameForDispose);
            }

            Logger.LogDebug("Disposing WebDriverManager instance for {EffectiveTestName}.", currentTestNameForDispose);
            (WebDriverManager as IDisposable)?.Dispose();

            Logger.LogDebug("Disposing TestReporter instance for {EffectiveTestName}.", currentTestNameForDispose);
            (TestReporter as IDisposable)?.Dispose();

            Logger.LogDebug("Disposing ConfigProvider instance for {EffectiveTestName}.", currentTestNameForDispose);
            (SettingsProvider as IDisposable)?.Dispose();

            Logger.LogDebug("Disposing DirectoryManager instance for {EffectiveTestName}.", currentTestNameForDispose);
            (DirectoryManager as IDisposable)?.Dispose();

            Logger.LogInformation("Managed services disposed for {EffectiveTestName}.", currentTestNameForDispose);
        }

        if (_loggingScope != null)
        {
            Logger.LogDebug("Disposing logging scope for {EffectiveTestName}.", currentTestNameForDispose);
            _loggingScope.Dispose();
            _loggingScope = null;

            Serilog.Log.Debug("BaseTest logging scope explicitly disposed for {EffectiveTestName}.", currentTestNameForDispose);
        }

        if (_testScope != null)
        {
            Logger.LogWarning("Test-specific DI scope was not null in BaseTest.Dispose(true). Disposing now. Test: {TestName}", TestName);
            _testScope.Dispose();
            _testScope = null;
        }

        _disposed = true;
        Logger.LogInformation("BaseTest Dispose({IsDisposing}) completed for {EffectiveTestName}.", disposing, currentTestNameForDispose);
    }
}
