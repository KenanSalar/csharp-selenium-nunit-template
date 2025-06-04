namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service that monitors system and process resources,
/// particularly the memory usage of the current application process.
/// </summary>
public interface IResourceMonitorService
{
    /// <summary>
    /// Gets the current memory usage information for the executing process.
    /// </summary>
    /// <returns>A <see cref="ProcessMemoryInfo"/> object containing current memory metrics
    /// (WorkingSet, PrivateMemory, PagedMemory) in Megabytes.</returns>
    public ProcessMemoryInfo GetCurrentProcessMemoryUsage();
}
