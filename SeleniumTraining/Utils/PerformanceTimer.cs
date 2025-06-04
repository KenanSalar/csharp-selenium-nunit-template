using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SeleniumTraining.Utils;

/// <summary>
/// A utility class for measuring the execution time of operations, logging the duration,
/// and optionally attaching performance and memory usage data to Allure reports.
/// Implements <see cref="IDisposable"/> to ensure the timer is stopped and logged if not explicitly done.
/// </summary>
/// <remarks>
/// This timer starts automatically upon instantiation. If an <see cref="IResourceMonitorService"/>
/// is provided, it also captures initial memory usage. The duration and memory usage (start, end, delta)
/// can be logged and attached to Allure reports via the <see cref="StopAndLog(bool, long?)"/> method.
/// If the timer instance is disposed (e.g., via a <c>using</c> statement) and was still running,
/// <see cref="StopAndLog(bool, long?)"/> will be called automatically with <c>attachToAllure</c> set to false.
/// It supports adding custom properties to the log scope for richer contextual information.
/// </remarks>
public class PerformanceTimer : IDisposable
{
    private readonly string _operationName;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;
    private readonly Microsoft.Extensions.Logging.LogLevel _logLevel;
    private readonly Dictionary<string, object>? _logProperties;

    /// <summary>
    /// Stores the memory information captured at the start of the timing operation. Null if monitoring is disabled or fails.
    /// </summary>
    private ProcessMemoryInfo? _startMemoryInfo;

    /// <summary>
    /// The resource monitoring service used to capture memory usage. Null if not provided.
    /// </summary>
    private readonly IResourceMonitorService? _resourceMonitor;

    /// <summary>
    /// Gets the total elapsed time measured by the current instance.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> representing the elapsed time.</value>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Gets the total elapsed time measured by the current instance, in milliseconds.
    /// </summary>
    /// <value>A <see langword="long"/> representing the elapsed time in milliseconds.</value>
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;


    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceTimer"/> class, starts the timer,
    /// and optionally captures initial memory usage.
    /// </summary>
    /// <param name="operationName">A descriptive name for the operation being timed (e.g., "PageLoad_HomePage", "LoginAttempt"). Must not be null.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance to use for logging the duration. Must not be null.</param>
    /// <param name="logLevel">The <see cref="Microsoft.Extensions.Logging.LogLevel"/> at which the duration message will be logged. Defaults to <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>.</param>
    /// <param name="additionalLogProperties">Optional. A dictionary of additional properties to include in the log scope when the duration is logged.
    /// Useful for adding context like test name, browser type, etc.</param>
    /// <param name="resourceMonitor">Optional. An instance of <see cref="IResourceMonitorService"/> to enable memory usage monitoring alongside performance timing.
    /// If provided, initial memory usage is captured upon construction.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="operationName"/> or <paramref name="logger"/> is null.</exception>
    public PerformanceTimer(
        string operationName,
        ILogger logger,
        Microsoft.Extensions.Logging.LogLevel logLevel = Microsoft.Extensions.Logging.LogLevel.Information,
        Dictionary<string, object>? additionalLogProperties = null,
        IResourceMonitorService? resourceMonitor = null
    )
    {
        _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logLevel = logLevel;
        _logProperties = additionalLogProperties;
        _resourceMonitor = resourceMonitor;

        if (_resourceMonitor != null)
        {
            _startMemoryInfo = _resourceMonitor.GetCurrentProcessMemoryUsage();
            _logger.LogTrace("PerformanceTimer '{OperationName}': Start Memory: {StartMemory}", _operationName, _startMemoryInfo);
        }

        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops the timer if it is currently running, logs the duration (and memory usage if monitored),
    /// and optionally attaches the performance and memory data to the Allure report.
    /// </summary>
    /// <param name="attachToAllure">If <c>true</c>, attempts to attach the performance and memory data as a text file to the Allure report. Defaults to <c>false</c>.</param>
    /// <param name="expectedMaxMilliseconds">Optional. If provided, the Allure attachment will include a status (PASS/FAIL)
    /// indicating whether the operation completed within this expected maximum duration.
    /// This also influences logging if the threshold is exceeded.</param>
    /// <remarks>
    /// If the timer is already stopped, this method does nothing further.
    /// The log message includes the operation name, duration, and memory usage details if available.
    /// Any errors during Allure attachment are logged but do not cause this method to throw an exception.
    /// </remarks>
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

    /// <summary>
    /// Logs the duration of the timed operation and memory usage (if monitored)
    /// using the configured logger and log level.
    /// Includes any additional log properties specified during construction in the log scope.
    /// </summary>
    private void LogDuration()
    {
        ProcessMemoryInfo? endMemoryInfo = null;
        if (_resourceMonitor != null)
        {
            endMemoryInfo = _resourceMonitor.GetCurrentProcessMemoryUsage();
        }

        var scopeProperties = new Dictionary<string, object>
        {
            { "OperationName", _operationName },
            { "DurationMilliseconds", ElapsedMilliseconds },
            { "DurationSeconds", Elapsed.TotalSeconds }
        };

        if (_startMemoryInfo != null)
        {
            scopeProperties["StartMemory_WorkingSetMB"] = _startMemoryInfo.WorkingSetMB;
            scopeProperties["StartMemory_PrivateMB"] = _startMemoryInfo.PrivateMemoryMB;
        }
        if (endMemoryInfo != null)
        {
            scopeProperties["EndMemory_WorkingSetMB"] = endMemoryInfo.WorkingSetMB;
            scopeProperties["EndMemory_PrivateMB"] = endMemoryInfo.PrivateMemoryMB;
            if (_startMemoryInfo != null)
            {
                scopeProperties["DeltaMemory_WorkingSetMB"] = endMemoryInfo.WorkingSetMB - _startMemoryInfo.WorkingSetMB;
                scopeProperties["DeltaMemory_PrivateMB"] = endMemoryInfo.PrivateMemoryMB - _startMemoryInfo.PrivateMemoryMB;
            }
        }

        if (_logProperties != null)
        {
            foreach (KeyValuePair<string, object> prop in _logProperties)
            {
                scopeProperties[prop.Key] = prop.Value;
            }
        }

        using (_logger.BeginScope(scopeProperties))
        {
            string memoryLogPart = "";
            if (_startMemoryInfo != null && endMemoryInfo != null)
            {
                memoryLogPart = $", StartMem: {_startMemoryInfo.WorkingSetMB:F2}MB WS, " +
                    "EndMem: {endMemoryInfo.WorkingSetMB:F2}MB WS (Delta: {(endMemoryInfo.WorkingSetMB - _startMemoryInfo.WorkingSetMB):F2}MB WS)";
            }
            else if (endMemoryInfo != null)
            {
                memoryLogPart = $", EndMem: {endMemoryInfo.WorkingSetMB:F2}MB WS";
            }

            _logger.Log(
                _logLevel,
                "Performance: {OperationName} completed in {ElapsedMilliseconds} ms ({ElapsedSeconds:F3} s){MemoryLog}",
                _operationName,
                ElapsedMilliseconds,
                Elapsed.TotalSeconds,
                memoryLogPart
            );
        }
    }

    /// <summary>
    /// Attaches the performance data (operation name, duration, optional expected max and status)
    /// and memory usage data (start, end, delta if available) as a text file to the Allure report.
    /// </summary>
    /// <param name="expectedMaxMilliseconds">Optional. If provided, indicates the maximum expected duration in milliseconds,
    /// used to determine a PASS/FAIL status in the attachment for the performance timing.</param>
    /// <remarks>
    /// Exceptions during the Allure attachment process are caught and logged,
    /// preventing them from disrupting the test flow.
    /// The attachment filename includes the operation name for easy identification.
    /// End memory usage is captured at the point this method is called if not already available.
    /// </remarks>
    private void AttachPerformanceToAllure(long? expectedMaxMilliseconds = null)
    {
        ProcessMemoryInfo? endMemoryInfo = null;
        if (_resourceMonitor != null && _stopwatch.IsRunning)
        {
            _startMemoryInfo ??= _resourceMonitor.GetCurrentProcessMemoryUsage();

            endMemoryInfo = _resourceMonitor.GetCurrentProcessMemoryUsage();
        }
        else if (_resourceMonitor != null)
        { 
            endMemoryInfo = _resourceMonitor.GetCurrentProcessMemoryUsage();
        }

        try
        {
            string status = "Actual";
            if (expectedMaxMilliseconds.HasValue)
            {
                status = ElapsedMilliseconds <= expectedMaxMilliseconds.Value ? "PASS" : "FAIL (Exceeded Threshold)";
            }

            var contentBuilder = new StringBuilder();
            contentBuilder = contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"Operation: {_operationName}")
                .AppendLine(CultureInfo.InvariantCulture, $"Duration: {ElapsedMilliseconds} ms ({Elapsed.TotalSeconds:F3} s)");

            if (expectedMaxMilliseconds.HasValue)
            {
                contentBuilder = contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"Expected Max: {expectedMaxMilliseconds.Value} ms")
                    .AppendLine(CultureInfo.InvariantCulture, $"Status: {status}");
            }

            if (_startMemoryInfo != null)
            {
                contentBuilder = contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Memory: {_startMemoryInfo}");
            }
            if (endMemoryInfo != null)
            {
                contentBuilder = contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"End Memory: {endMemoryInfo}");
                if (_startMemoryInfo != null)
                {
                    contentBuilder = contentBuilder.AppendLine(CultureInfo.InvariantCulture, $"Delta WorkingSet: {endMemoryInfo.WorkingSetMB - _startMemoryInfo.WorkingSetMB:F2} MB")
                        .AppendLine(CultureInfo.InvariantCulture, $"Delta PrivateMemory: {endMemoryInfo.PrivateMemoryMB - _startMemoryInfo.PrivateMemoryMB:F2} MB");
                }
            }

            AllureApi.AddAttachment(
            $"{_operationName} - Performance & Memory",
            "text/plain",
            Encoding.UTF8.GetBytes(contentBuilder.ToString()),
            ".txt"
        );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to attach performance data to Allure for operation: {OperationName}", _operationName);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// Ensures that if the timer is still running when disposed, its duration and memory usage (if monitored) are logged.
    /// </summary>
    /// <remarks>
    /// If the timer's <see cref="Stopwatch"/> is still running at the time of disposal (e.g., if <see cref="StopAndLog(bool, long?)"/>
    /// was not explicitly called), this method calls <see cref="StopAndLog(bool, long?)"/>
    /// with <c>attachToAllure</c> set to <c>false</c> to ensure the duration and memory data are logged.
    /// This is crucial for scenarios using <c>using</c> statements with the timer.
    /// </remarks>
    public void Dispose()
    {
        if (_stopwatch.IsRunning)
        {
            StopAndLog(attachToAllure: false);
        }
        GC.SuppressFinalize(this);
    }
}
