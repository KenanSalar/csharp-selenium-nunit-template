namespace SeleniumTraining.Core;

/// <summary>
/// Provides a lean base class for all UI tests, encapsulating DI scope management
/// and delegation of test lifecycle events to an <see cref="ITestLifecycleManager"/>.
/// </summary>
/// <remarks>
/// This abstract class is designed to be inherited by concrete test fixture classes.
/// Its primary responsibilities are:
/// <list type="bullet">
///   <item><description>Creating a new dependency injection scope for each test.</description></item>
///   <item><description>Resolving the <see cref="ITestLifecycleManager"/> and a few other core services needed directly by tests.</description></item>
///   <item><description>Hooking into NUnit's <c>[SetUp]</c> and <c>[TearDown]</c> attributes to delegate the orchestration of test setup and cleanup.</description></item>
///   <item><description>Providing common properties like Loggers and Settings providers to derived test classes.</description></item>
/// </list>
/// </remarks>
[AllureNUnit]
public abstract class BaseTest : IDisposable
{
    private IServiceScope? _testScope;
    private IDisposable? _loggingScope;

    /// <summary>
    /// Gets the orchestrator for the current test's lifecycle, which manages
    /// driver initialization, reporting, and cleanup.
    /// </summary>
    protected ITestLifecycleManager LifecycleManager { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance specifically for logging messages from the current test class.
    /// </summary>
    protected ILogger TestLogger { get; private set; } = null!;

    /// <summary>
    /// Gets the service for accessing application and framework settings.
    /// </summary>
    protected ISettingsProviderService SettingsProvider { get; private set; } = null!;

    /// <summary>
    /// Gets the service for executing actions with retry policies.
    /// </summary>
    protected IRetryService RetryService { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="ILoggerFactory"/> instance used for creating loggers for page objects.
    /// </summary>
    protected ILoggerFactory PageObjectLoggerFactory { get; private set; } = null!;

    /// <summary>
    /// Gets the service for monitoring process resource usage, such as memory.
    /// </summary>
    protected IResourceMonitorService ResourceMonitor { get; private set; } = null!;

    /// <summary>
    /// Gets the name of the current test class.
    /// </summary>
    protected string TestName { get; }

    /// <summary>
    /// Gets the <see cref="BrowserType"/> for which this test fixture is configured to run.
    /// </summary>
    protected BrowserType BrowserType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTest"/> class.
    /// </summary>
    /// <param name="browserType">The <see cref="BrowserType"/> on which the tests in this fixture are intended to run.</param>
    protected BaseTest(BrowserType browserType)
    {
        BrowserType = browserType;
        TestName = GetType().Name;
    }

    /// <summary>
    /// Performs setup operations before each test by creating a DI scope and
    /// delegating the core setup orchestration to the <see cref="ITestLifecycleManager"/>.
    /// </summary>
    [SetUp]
    public virtual void SetUp()
    {
        _testScope = TestHost.Services.CreateScope();
        IServiceProvider scopedServiceProvider = _testScope.ServiceProvider;

        // Resolve only what BaseTest and derived tests absolutely need to access directly
        PageObjectLoggerFactory = scopedServiceProvider.GetRequiredService<ILoggerFactory>();
        TestLogger = PageObjectLoggerFactory.CreateLogger(TestName);
        SettingsProvider = scopedServiceProvider.GetRequiredService<ISettingsProviderService>();
        RetryService = scopedServiceProvider.GetRequiredService<IRetryService>();
        LifecycleManager = scopedServiceProvider.GetRequiredService<ITestLifecycleManager>();
        ResourceMonitor = scopedServiceProvider.GetRequiredService<IResourceMonitorService>();

        // Set up a logging scope with context
        string correlationId = Guid.NewGuid().ToString("N")[..12];
        var scopeProperties = new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TestClassName"] = TestName,
            ["TestMethodName"] = TestContext.CurrentContext.Test.MethodName ?? "UnknownMethod",
            ["BrowserType"] = BrowserType.ToString(),
            ["TestLogFileKey"] = TestName
        };
        _loggingScope = TestLogger.BeginScope(scopeProperties);

        TestLogger.LogInformation("BaseTest SetUp started for: {TestName}", TestName);

        // DELEGATE all setup orchestration to the lifecycle manager
        LifecycleManager.InitializeTestScope(TestName, TestContext.CurrentContext, BrowserType);

        TestLogger.LogInformation("BaseTest SetUp completed for: {TestName}", TestName);
    }

    /// <summary>
    /// Performs cleanup operations after each test by delegating to the
    /// <see cref="ITestLifecycleManager"/> and then disposing the DI scope.
    /// </summary>
    [TearDown]
    public void Cleanup()
    {
        // Skip finalization if the test was ignored during setup
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Skipped)
        {
            TestLogger.LogInformation("BaseTest Cleanup skipped due to Assert.Ignore in SetUp.");
            return;
        }

        TestLogger.LogInformation("BaseTest Cleanup started for: {TestFullName}", TestContext.CurrentContext.Test.FullName);

        try
        {
            // DELEGATE all teardown orchestration to the lifecycle manager
            LifecycleManager.FinalizeTestScope(TestContext.CurrentContext);
            TestLogger.LogInformation("Test Lifecycle finalized successfully.");
        }
        catch (Exception ex)
        {
            TestLogger.LogError(ex, "An unexpected error occurred during test finalization.");
        }
        finally
        {
            // Dispose logging and DI scopes
            _loggingScope?.Dispose();
            _testScope?.Dispose();
            TestLogger.LogInformation("BaseTest Cleanup completed.");
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _loggingScope?.Dispose();
        _testScope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
