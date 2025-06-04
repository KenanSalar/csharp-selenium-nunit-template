using System.Diagnostics;

namespace SeleniumTraining.Core.Services;

/// <summary>
/// Service for monitoring system and process resources, primarily focusing on memory usage
/// of the current test execution process. Implements <see cref="IResourceMonitorService"/>.
/// </summary>
/// <remarks>
/// This service utilizes <see cref="Process.GetCurrentProcess()"/> to retrieve
/// memory metrics like Working Set, Private Memory, and Paged Memory.
/// It inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class ResourceMonitorService : BaseService, IResourceMonitorService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceMonitorService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/>.</param>
    public ResourceMonitorService(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(ResourceMonitorService));
    }

    /// <inheritdoc cref="IResourceMonitorService.GetCurrentProcessMemoryUsage" />
    /// <remarks>
    /// This implementation retrieves memory information for the current process.
    /// It calls <see cref="Process.Refresh()"/> to ensure the retrieved values are up-to-date
    /// before accessing memory properties like <see cref="Process.WorkingSet64"/>,
    /// <see cref="Process.PrivateMemorySize64"/>, and <see cref="Process.PagedMemorySize64"/>.
    /// The values are converted to Megabytes.
    /// </remarks>
    public ProcessMemoryInfo GetCurrentProcessMemoryUsage()
    {
        var currentProcess = Process.GetCurrentProcess();
        currentProcess.Refresh();

        double workingSetMB = currentProcess.WorkingSet64 / (1024.0 * 1024.0);
        double privateMemoryMB = currentProcess.PrivateMemorySize64 / (1024.0 * 1024.0);
        double pagedMemoryMB = currentProcess.PagedMemorySize64 / (1024.0 * 1024.0);

        var memoryInfo = new ProcessMemoryInfo
        {
            WorkingSetMB = workingSetMB,
            PrivateMemoryMB = privateMemoryMB,
            PagedMemoryMB = pagedMemoryMB
        };

        ServiceLogger.LogDebug("Current process memory usage: {MemoryInfo}", memoryInfo.ToString());

        return memoryInfo;
    }
}
