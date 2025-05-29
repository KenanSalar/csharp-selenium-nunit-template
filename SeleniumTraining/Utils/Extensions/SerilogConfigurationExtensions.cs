using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace SeleniumTraining.Utils.Extensions;

public static class SerilogConfigurationExtensions
{
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
