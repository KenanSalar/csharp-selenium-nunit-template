using System.Diagnostics;
using System.Text;

namespace SeleniumTraining.Utils;

public class PerformanceTimer : IDisposable
{
    private readonly string _operationName;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;
    private readonly Microsoft.Extensions.Logging.LogLevel _logLevel;
    private readonly Dictionary<string, object>? _logProperties;

    public TimeSpan Elapsed => _stopwatch.Elapsed;
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    public PerformanceTimer(
        string operationName,
        ILogger logger,
        Microsoft.Extensions.Logging.LogLevel logLevel = Microsoft.Extensions.Logging.LogLevel.Information,
        Dictionary<string, object>? additionalLogProperties = null
    )
    {
        _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logLevel = logLevel;
        _logProperties = additionalLogProperties;
        _stopwatch = Stopwatch.StartNew();
    }

    public void StopAndLog(bool attachToAllure = false, long? expectedMaxMilliseconds = null)
    {
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
        }
        LogDuration();

        if (attachToAllure)
        {
            AttachPerformanceToAllure(expectedMaxMilliseconds);
        }
    }

    private void LogDuration()
    {
        var scopeProperties = new Dictionary<string, object>
        {
            { "OperationName", _operationName },
            { "DurationMilliseconds", ElapsedMilliseconds },
            { "DurationSeconds", Elapsed.TotalSeconds }
        };

        if (_logProperties != null)
        {
            foreach (KeyValuePair<string, object> prop in _logProperties)
            {
                scopeProperties[prop.Key] = prop.Value;
            }
        }

        using (_logger.BeginScope(scopeProperties))
        {
            _logger.Log(
                _logLevel,
                "Performance: {OperationName} completed in {ElapsedMilliseconds} ms ({ElapsedSeconds:F3} s)",
                _operationName,
                ElapsedMilliseconds,
                Elapsed.TotalSeconds
            );
        }
    }

    private void AttachPerformanceToAllure(long? expectedMaxMilliseconds = null)
    {
        try
        {
            string status = "Actual";
            if (expectedMaxMilliseconds.HasValue)
            {
                status = ElapsedMilliseconds <= expectedMaxMilliseconds.Value ? "PASS" : "FAIL (Exceeded Threshold)";
            }

            string content = $"Operation: {_operationName}\nDuration: {ElapsedMilliseconds} ms ({Elapsed.TotalSeconds:F3} s)";
            if (expectedMaxMilliseconds.HasValue)
            {
                content += $"\nExpected Max: {expectedMaxMilliseconds.Value} ms\nStatus: {status}";
            }

            AllureApi.AddAttachment(
                $"{_operationName} - Performance",
                "text/plain",
                Encoding.UTF8.GetBytes(content),
                ".txt"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach performance data to Allure for operation: {OperationName}", _operationName);
        }
    }

    public void Dispose()
    {
        if (_stopwatch.IsRunning)
        {
            StopAndLog(attachToAllure: false);
        }
        GC.SuppressFinalize(this);
    }
}
