using Microsoft.Extensions.Options;
using Serilog;

namespace SeleniumTraining.Core;

public static class TestHost
{
    private static IServiceProvider? _serviceProvider;
    private static IConfiguration? _configuration;

    public static IServiceProvider Services =>
        _serviceProvider ?? throw new InvalidOperationException("TestHost not initialized. Call Initialize() in your test assembly setup.");

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
