using Microsoft.Extensions.Options;
using Serilog;

namespace SeleniumTraining.Core;

/// <summary>
/// Manages the application's host environment for test execution, including configuration,
/// logging (Serilog), and Dependency Injection (DI) services.
/// This is a static class, typically initialized once per test assembly run.
/// </summary>
/// <remarks>
/// The <see cref="Initialize"/> method must be called before any tests access services,
/// usually in a global setup fixture (e.g., NUnit's `[OneTimeSetUp]`).
/// It builds the application configuration (from sources like appsettings.json),
/// configures Serilog, and sets up the DI container by registering application services.
/// It performs options validation during DI setup to catch configuration errors early.
/// The <see cref="TearDown"/> method should be called after all tests have run
/// (e.g., NUnit's `[OneTimeTearDown]`) to flush logs and dispose of the service provider.
/// Access to the configured <see cref="IServiceProvider"/> is through the static <see cref="Services"/> property.
/// </remarks>
public static class TestHost
{
    private static IServiceProvider? _serviceProvider;
    private static IConfiguration? _configuration;

    /// <summary>
    /// Gets the configured <see cref="IServiceProvider"/> for accessing registered DI services.
    /// </summary>
    /// <value>The application's root <see cref="IServiceProvider"/>.</value>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this property is accessed before <see cref="Initialize"/> has been successfully called.
    /// </exception>
    /// <remarks>
    /// This property provides the entry point for resolving services required by tests or test infrastructure.
    /// Ensure <see cref="Initialize"/> is called in a global test setup to make this available.
    /// </remarks>
    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("TestHost not initialized. Call Initialize() in your test assembly setup.");

    /// <summary>
    /// Initializes the TestHost, setting up configuration, Serilog logging, and the Dependency Injection (DI) container.
    /// This method should be called once at the beginning of the test run (e.g., in a global test assembly setup).
    /// </summary>
    /// <remarks>
    /// The initialization process involves:
    /// <list type="bullet">
    ///   <item><description>Building the application configuration using an extension method (<c>BuildApplicationConfiguration</c>).</description></item>
    ///   <item><description>Configuring Serilog using the built configuration (<c>ConfigureApplicationSerilog</c>).</description></item>
    ///   <item><description>Creating an <see cref="IServiceCollection"/> and registering application services (<c>AddApplicationServices</c>).</description></item>
    ///   <item><description>Building the <see cref="IServiceProvider"/> with scope validation enabled.</description></item>
    ///   <item><description>Performing options validation via the DI container. If validation fails, a fatal error is logged, and the exception is re-thrown, typically terminating the test run.</description></item>
    /// </list>
    /// If <see cref="Initialize"/> is called multiple times, subsequent calls will have no effect if already initialized.
    /// Fatal errors during DI setup or options validation are logged and will cause the application to terminate.
    /// </remarks>
    /// <exception cref="OptionsValidationException">Re-thrown if configuration options validation fails during service provider build. The test run will typically terminate.</exception>
    /// <exception cref="Exception">Re-thrown for any other unexpected fatal error during the DI service provider build. The test run will typically terminate.</exception>
    public static void Initialize()
    {
        if (_serviceProvider != null)
            return;

        _configuration = new ConfigurationBuilder().BuildApplicationConfiguration(typeof(TestHost));

        SerilogConfigurationExtensions.ConfigureApplicationSerilog(_configuration);

        Log.Information("TestHost: Configuration built and Serilog initialized. Starting DI setup.");

        IServiceCollection services = new ServiceCollection();

        _ = services.AddApplicationServices(_configuration);

        try
        {
            _serviceProvider = services.BuildServiceProvider(validateScopes: true);

            Microsoft.Extensions.Logging.ILogger initLogger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("TestHost");
            initLogger.LogInformation("TestHost: DI Service Provider built successfully. Options validated.");
        }
        catch (OptionsValidationException ex)
        {
            Log.Fatal(ex, "TestHost: Configuration validation failed during ServiceProvider build. Application will terminate.");
            foreach (string failure in ex.Failures)
            {
                Log.Fatal("Validation Failure: {FailureMessage}", failure);
            }
            throw;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "TestHost: An unexpected error occurred during DI ServiceProvider build. Application will terminate.");
            throw;
        }
    }

    /// <summary>
    /// Tears down the TestHost, performing necessary cleanup operations such as flushing Serilog logs
    /// and disposing of the <see cref="IServiceProvider"/> if it implements <see cref="IDisposable"/>.
    /// This method should be called once at the end of the test run (e.g., in a global test assembly teardown).
    /// </summary>
    /// <remarks>
    /// After teardown, the <see cref="Services"/> property will no longer be accessible, and
    /// internal references to the service provider and configuration are cleared.
    /// </remarks>
    public static void TearDown()
    {
        Log.Information("TestHost TearDown called. Shutting down Serilog.");
        Log.CloseAndFlush();

        if (_serviceProvider is IDisposable disposableProvider)
            disposableProvider.Dispose();

        _serviceProvider = null;
        _configuration = null;
    }
}
