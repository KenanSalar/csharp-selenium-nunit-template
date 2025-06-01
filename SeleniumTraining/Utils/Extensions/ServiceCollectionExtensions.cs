using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SeleniumTraining.Utils.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services = services.AddSingleton(configuration)
            .AddLogging(loggingBuilder =>
                loggingBuilder.ClearProviders()
                .AddSerilog(dispose: true)
            );

        _ = services.AddApplicationOptions(configuration);

        services = services.AddSingleton<ISettingsProviderService, SettingsProviderService>()
            .AddSingleton<IDirectoryManagerService, DirectoryManagerService>()
            .AddSingleton<IBrowserFactoryManagerService, BrowserFactoryManagerService>()
            .AddSingleton<IBrowserDriverFactoryService, ChromeDriverFactoryService>()
            // .AddSingleton<IBrowserDriverFactoryService, BraveDriverFactoryService>()
            .AddSingleton<IBrowserDriverFactoryService, FirefoxDriverFactoryService>()
            .AddTransient<IThreadLocalDriverStorageService, ThreadLocalDriverStorageService>()
            .AddTransient<IDriverInitializationService, DriverInitializationService>()
            .AddTransient<IDriverLifecycleService, DriverLifecycleService>()
            .AddScoped<ITestWebDriverManager, TestWebDriverManager>()
            .AddTransient<ITestReporterService, TestReporterService>()
            .AddScoped<IVisualTestService, VisualTestService>();

        return services;
    }

    private static IServiceCollection AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddOptions<ChromeSettings>()
            .Bind(configuration.GetSection("ChromeBrowserOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<FirefoxSettings>()
            .Bind(configuration.GetSection("FirefoxBrowserOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        _ = services.AddOptions<BraveSettings>()
            .Bind(configuration.GetSection("BraveBrowserOptions"))
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

        return services;
    }
}
