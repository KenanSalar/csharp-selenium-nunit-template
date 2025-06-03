using System.Diagnostics;
using System.Text;

namespace SeleniumTraining.Utils;

/// <summary>
/// A utility class for measuring the execution time of operations, logging the duration,
/// and optionally attaching performance data to Allure reports.
/// Implements <see cref="IDisposable"/> to ensure the timer is stopped and logged if not explicitly done.
/// </summary>
/// <remarks>
/// This timer starts automatically upon instantiation. The duration can be logged and attached
/// to Allure reports via the <see cref="StopAndLog(bool, long?)"/> method.
/// If the timer instance is disposed (e.g., via a <c>using</c> statement) and was still running,
/// <see cref="StopAndLog(bool, long?)"/> will be called automatically with <c>attachToAllure</c> set to false.
/// It supports adding custom properties to the log scope for richer contextual information.
/// This is particularly useful for performance monitoring in automated tests, including those run in CI/CD pipelines.
/// </remarks>
public class PerformanceTimer : IDisposable
{
    private readonly string _operationName;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;
    private readonly Microsoft.Extensions.Logging.LogLevel _logLevel;
    private readonly Dictionary<string, object>? _logProperties;

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
    /// Initializes a new instance of the <see cref="PerformanceTimer"/> class and starts the timer.
    /// </summary>
    /// <param name="operationName">A descriptive name for the operation being timed (e.g., "PageLoad_HomePage", "LoginAttempt"). Must not be null.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance to use for logging the duration. Must not be null.</param>
    /// <param name="logLevel">The <see cref="Microsoft.Extensions.Logging.LogLevel"/> at which the duration message will be logged. Defaults to <see cref="Microsoft.Extensions.Logging.LogLevel.Information"/>.</param>
    /// <param name="additionalLogProperties">Optional. A dictionary of additional properties to include in the log scope when the duration is logged.
    /// Useful for adding context like test name, browser type, etc.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="operationName"/> or <paramref name="logger"/> is null.</exception>
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

    /// <summary>
    /// Stops the timer if it is currently running, logs the duration, and optionally attaches
    /// the performance data to the Allure report.
    /// </summary>
    /// <param name="attachToAllure">If <c>true</c>, attempts to attach the performance data as a text file to the Allure report. Defaults to <c>false</c>.</param>
    /// <param name="expectedMaxMilliseconds">Optional. If provided, the Allure attachment will include a status (PASS/FAIL)
    /// indicating whether the operation completed within this expected maximum duration.
    /// This also influences logging if the threshold is exceeded.</param>
    /// <remarks>
    /// If the timer is already stopped, this method does nothing further.
    /// The log message includes the operation name, duration in milliseconds, and duration in seconds.
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
    /// Logs the duration of the timed operation using the configured logger and log level.
    /// Includes any additional log properties specified during construction in the log scope.
    /// </summary>
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

    /// <summary>
    /// Attaches the performance data (operation name, duration, optional expected max and status)
    /// as a text file to the Allure report.
    /// </summary>
    /// <param name="expectedMaxMilliseconds">Optional. If provided, indicates the maximum expected duration in milliseconds,
    /// used to determine a PASS/FAIL status in the attachment.</param>
    /// <remarks>
    /// Exceptions during the Allure attachment process are caught and logged,
    /// preventing them from disrupting the test flow.
    /// The attachment filename includes the operation name for easy identification.
    /// </remarks>
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

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// Ensures that if the timer is still running when disposed, its duration is logged.
    /// </summary>
    /// <remarks>
    /// If the timer's <see cref="Stopwatch"/> is still running at the time of disposal (e.g., if <see cref="StopAndLog"/>
    /// was not explicitly called), this method calls <see cref="StopAndLog(bool, long?)"/>
    /// with <c>attachToAllure</c> set to <c>false</c> to ensure the duration is logged.
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
