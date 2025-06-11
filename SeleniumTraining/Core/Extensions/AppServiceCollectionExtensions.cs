using Serilog;

namespace SeleniumTraining.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to centralize
/// the registration of application services and configuration options for Dependency Injection (DI).
/// </summary>
/// <remarks>
/// This class helps in organizing and standardizing the DI setup.
/// The <see cref="AddApplicationServices"/> method is the primary entry point for registering
/// all necessary services, including logging, configuration options, and various application-specific services
/// with their appropriate lifetimes (singleton, scoped, transient).
/// The <see cref="AddApplicationOptions"/> method, called internally, handles the binding and validation
/// of strongly-typed configuration settings.
/// This structured approach is beneficial for maintaining a clean and testable application,
/// especially in CI/CD environments where consistent setup is key.
/// </remarks>
public static class AppServiceCollectionExtensions
{
    /// <summary>
    /// Registers core application services and logging with the specified <see cref="IServiceCollection"/>.
    /// This method is intended to be called during the DI container setup (e.g., in <c>TestHost.Initialize</c>).
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application's <see cref="IConfiguration"/>, used for logging setup
    /// and for binding configuration options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for fluent chaining of registrations.</returns>
    /// <remarks>
    /// This method performs the following registrations:
    /// <list type="bullet">
    ///   <item><description>Registers <see cref="IConfiguration"/> as a singleton.</description></item>
    ///   <item><description>Configures logging using <see cref="SerilogServiceCollectionExtensions.AddSerilog(ILoggingBuilder, Serilog.ILogger, bool)"/>, clearing existing providers and enabling Serilog with disposal.</description></item>
    ///   <item><description>Calls <see cref="AddApplicationOptions"/> to register and validate strongly-typed configuration settings.</description></item>
    ///   <item><description>Registers various application services with their respective lifetimes:</description>
    ///     <list type="bullet">
    ///       <item><description><see cref="IDirectoryManagerService"/> as Singleton (<see cref="DirectoryManagerService"/>)</description></item>
    ///       <item><description><see cref="ISettingsProviderService"/> as Singleton (<see cref="SettingsProviderService"/>)</description></item>
    ///       <item><description><see cref="IRetryService"/> as Singleton (<see cref="RetryService"/>)</description></item>
    ///       <item><description><see cref="IVisualTestService"/> as Singleton (<see cref="VisualTestService"/>)</description></item>
    ///       <item><description><see cref="IBrowserFactoryManagerService"/> as Singleton (<see cref="BrowserFactoryManagerService"/>)</description></item>
    ///       <item><description><see cref="ChromeDriverFactoryService"/> as Transient (implements <see cref="IBrowserDriverFactoryService"/>)</description></item>
    ///       <item><description><see cref="FirefoxDriverFactoryService"/> as Transient (implements <see cref="IBrowserDriverFactoryService"/>)</description></item>
    ///       <item><description><c>EdgeDriverFactoryService</c> as Transient (commented out in the original code)</description></item>
    ///       <item><description><see cref="IDriverInitializationService"/> as Scoped (<see cref="DriverInitializationService"/>)</description></item>
    ///       <item><description><see cref="IDriverLifecycleService"/> as Transient (<see cref="DriverLifecycleService"/>)</description></item>
    ///       <item><description><see cref="ITestWebDriverManager"/> as Scoped (<see cref="TestWebDriverManager"/>)</description></item>
    ///       <item><description><see cref="ITestReporterService"/> as Singleton (<see cref="TestReporterService"/>)</description></item>
    ///       <item><description><see cref="IThreadLocalDriverStorageService"/> as Singleton (<see cref="ThreadLocalDriverStorageService"/>)</description></item>
    ///     </list>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configuration"/> is null (though <paramref name="services"/> as 'this' parameter won't be null).</exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services = services.AddSingleton(configuration)
            .AddLogging(loggingBuilder =>
                loggingBuilder.ClearProviders()
                .AddSerilog(dispose: true)
            )
            .AddApplicationOptions(configuration)
            .AddSingleton<ISettingsProviderService, SettingsProviderService>()
            .AddSingleton<IDirectoryManagerService, DirectoryManagerService>()
            .AddSingleton<IBrowserFactoryManagerService, BrowserFactoryManagerService>()
            .AddSingleton<IBrowserDriverFactoryService, ChromeDriverFactoryService>()
            .AddSingleton<IBrowserDriverFactoryService, EdgeDriverFactoryService>()
            .AddSingleton<IBrowserDriverFactoryService, FirefoxDriverFactoryService>()
            .AddSingleton<IThreadLocalDriverStorageService, ThreadLocalDriverStorageService>()
            .AddTransient<IDriverInitializationService, DriverInitializationService>()
            .AddTransient<IDriverLifecycleService, DriverLifecycleService>()
            .AddScoped<ITestWebDriverManager, TestWebDriverManager>()
            .AddTransient<ITestReporterService, TestReporterService>()
            .AddTransient<IScreenshotService, ScreenshotService>()
            .AddTransient<IResourceMonitorService, ResourceMonitorService>()
            .AddScoped<IVisualTestService, VisualTestService>()
            .AddSingleton<IRetryService, RetryService>();

        return services;
    }

    /// <summary>
    /// Registers and configures strongly-typed options classes from the application's configuration.
    /// It binds specific configuration sections to their corresponding settings classes and enables
    /// data annotation validation and validation on startup.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the options configurations to.</param>
    /// <param name="configuration">The application's <see cref="IConfiguration"/>, used to retrieve configuration sections.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance, allowing for fluent chaining.</returns>
    /// <remarks>
    /// This private helper method is called by <see cref="AddApplicationServices"/>.
    /// It configures options for:
    /// <list type="bullet">
    ///   <item><description><see cref="ChromeBrowserOptions"/> from section "ChromeBrowserOptions".</description></item>
    ///   <item><description><see cref="FirefoxBrowserOptions"/> from section "FirefoxBrowserOptions".</description></item>
    ///   <item><description><see cref="SauceDemoSettings"/> from section "SauceDemo".</description></item>
    ///   <item><description><see cref="VisualTestSettings"/> from section "VisualTestSettings".</description></item>
    /// </list>
    /// For each options class, <c>ValidateDataAnnotations()</c> and <c>ValidateOnStart()</c> are called
    /// to ensure that configuration is valid when the application or DI container starts,
    /// catching configuration errors early.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configuration"/> is null (though <paramref name="services"/> as 'this' parameter won't be null).</exception>
    /// <exception cref="OptionsValidationException">May be thrown at application startup (during <c>BuildServiceProvider</c>) if any options validation fails.</exception>
    private static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddOptions<ChromeSettings>()
            .Bind(configuration.GetSection("ChromeBrowserOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<EdgeSettings>()
            .Bind(configuration.GetSection("EdgeBrowserOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<FirefoxSettings>()
            .Bind(configuration.GetSection("FirefoxBrowserOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<SauceDemoSettings>()
            .Bind(configuration.GetSection("SauceDemo"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<VisualTestSettings>()
            .Bind(configuration.GetSection("VisualTestSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<RetryPolicySettings>()
            .Bind(configuration.GetSection("RetryPolicySettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
