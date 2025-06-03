using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace SeleniumTraining.Utils.Extensions;

/// <summary>
/// Provides extension methods for configuring Serilog logging for the application.
/// </summary>
/// <remarks>
/// This class centralizes the setup of Serilog, enabling consistent logging practices.
/// It primarily offers the <see cref="ConfigureApplicationSerilog"/> method to initialize
/// Serilog based on an <see cref="IConfiguration"/> instance, which typically includes
/// settings from appsettings.json and other sources.
/// A key feature is the dynamic routing of log events to different files based on a
/// "TestLogFileKey" property in the log context, facilitating organized logs per test class or context.
/// </remarks>
public static class SerilogConfigurationExtensions
{
    /// <summary>
    /// Configures and initializes the global Serilog logger (<see cref="Log.Logger"/>)
    /// based on the provided <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="configuration">The application's <see cref="IConfiguration"/> instance,
    /// which Serilog will read its settings from (e.g., minimum levels, specific sink configurations).</param>
    /// <remarks>
    /// This method sets up Serilog with the following features:
    /// <list type="bullet">
    ///   <item><description>Reads base configuration from the provided <paramref name="configuration"/> object.</description></item>
    ///   <item><description>Enriches log events from the current <see cref="Serilog.Context.LogContext"/>.</description></item>
    ///   <item><description>Uses <c>WriteTo.Map()</c> to dynamically route log events to different files.
    ///   The routing key is determined by the "TestLogFileKey" property in the log event's properties.
    ///   If "TestLogFileKey" is not present, logs are routed to a "GlobalOrOrphanedLogs" file.</description></item>
    ///   <item><description>Log files are written in <see cref="CompactJsonFormatter"/> format.</description></item>
    ///   <item><description>Log file paths are constructed as: <c>TestOutput/Logs/{SafeClassNamePart}/{SafeClassNamePart}-{yyyy-MM-dd}.json</c>,
    ///   where <c>SafeClassNamePart</c> is derived from the "TestLogFileKey".</description></item>
    ///   <item><description>Retains up to 7 log files per sink (per key).</description></item>
    /// </list>
    /// After configuration, a confirmation message "Application Serilog initialized via extension method." is logged.
    /// This setup is crucial for structured and context-aware logging, especially in test automation scenarios
    /// where logs from different tests or components need to be segregated.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Typically not thrown by this method directly for <paramref name="configuration"/> as <c>ReadFrom.Configuration</c> might handle nulls, but it's good practice to ensure it's not null before calling.</exception>
    public static void ConfigureApplicationSerilog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Map(
                logEvent =>
                {
                    LogEventPropertyValue? testLogFileKeyProperty = logEvent.Properties.GetValueOrDefault("TestLogFileKey");
                    return testLogFileKeyProperty?.ToString()?.Trim('"') ?? "GlobalOrOrphanedLogs";
                },
                (key, sinkConfiguration) =>
                {
                    string classNamePart = key.Contains('.') ? key[(key.LastIndexOf('.') + 1)..] : key;
                    string safeNamePart = string.Join("_", classNamePart.Split(Path.GetInvalidFileNameChars()));

                    if (string.IsNullOrWhiteSpace(safeNamePart))
                    {
                        safeNamePart = "InvalidContextName";
                    }

                    string dateSuffix = DateTime.Now.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                    _ = sinkConfiguration.File(
                        formatter: new CompactJsonFormatter(),
                        path: Path.Combine("TestOutput", "Logs", safeNamePart, $"{safeNamePart}-{dateSuffix}.json"),
                        retainedFileCountLimit: 7
                        // outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{Properties:j}{NewLine}{Exception}",
                        // formatProvider: System.Globalization.CultureInfo.InvariantCulture
                    );
                },
                sinkMapCountLimit: 100
            )
            .CreateLogger();

        Log.Information("Application Serilog initialized via extension method.");
    }
}
