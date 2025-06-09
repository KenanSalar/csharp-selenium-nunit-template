namespace SeleniumTraining;

/// <summary>
/// Provides global setup and teardown logic that runs once for the entire test assembly.
/// This class is decorated with <see cref="SetUpFixtureAttribute"/>, indicating to NUnit
/// that it contains one-time setup and teardown methods for the assembly.
/// </summary>
/// <remarks>
/// The primary responsibilities of this class are:
/// <list type="bullet">
///   <item><description>Initializing the application's <see cref="TestHost"/> (configuration, logging, DI services)
///   before any tests run, via <see cref="GlobalSetupBeforeAllTests"/>.</description></item>
///   <item><description>Ensuring that base directories required for test artifacts (e.g., logs, screenshots)
///   are created at the start of the test run.</description></item>
///   <item><description>Tearing down the <see cref="TestHost"/> (flushing logs, disposing services)
///   after all tests in the assembly have completed, via <see cref="GlobalTeardownAfterAllTests"/>.</description></item>
/// </list>
/// This centralized setup and teardown is critical for preparing the test environment and ensuring
/// proper cleanup, especially in automated CI/CD pipelines where resource management is important.
/// </remarks>
[SetUpFixture]
public class AssemblyInitialize
{
    /// <summary>
    /// Performs global setup operations once before any tests in the assembly are executed.
    /// This method is decorated with <see cref="OneTimeSetUpAttribute"/>.
    /// </summary>
    /// <remarks>
    /// Key actions performed:
    /// <list type="bullet">
    ///   <item><description>Calls <see cref="TestHost.Initialize()"/> to set up application configuration,
    ///   Serilog logging, and the Dependency Injection container.</description></item>
    ///   <item><description>Resolves the <see cref="IDirectoryManagerService"/> from the initialized <see cref="TestHost.Services"/>.</description></item>
    ///   <item><description>Calls <see cref="IDirectoryManagerService.EnsureBaseDirectoriesExist()"/> to create
    ///   standard output directories for logs, screenshots, etc., if they don't already exist.</description></item>
    /// </list>
    /// If <see cref="TestHost.Initialize()"/> fails (e.g., due to configuration errors), it typically throws an exception
    /// which will prevent the test run from proceeding.
    /// </remarks>
    [OneTimeSetUp]
    public void GlobalSetupBeforeAllTests()
    {
        TestHost.Initialize();

        IDirectoryManagerService directoryManager = TestHost.Services.GetRequiredService<IDirectoryManagerService>();
        directoryManager.EnsureBaseDirectoriesExist();
    }

    /// <summary>
    /// Performs global teardown operations once after all tests in the assembly have completed.
    /// This method is decorated with <see cref="OneTimeTearDownAttribute"/>.
    /// </summary>
    /// <remarks>
    /// The primary action performed is calling <see cref="TestHost.TearDown()"/>.
    /// This typically includes:
    /// <list type="bullet">
    ///   <item><description>Flushing any buffered Serilog log messages to their sinks (e.g., files).</description></item>
    ///   <item><description>Disposing the root <see cref="IServiceProvider"/> and any disposable services it manages.</description></item>
    /// </list>
    /// This ensures that resources are properly released at the end of the test run.
    /// </remarks>
    [OneTimeTearDown]
    public void GlobalTeardownAfterAllTests()
    {
        TestHost.TearDown();
    }
}
