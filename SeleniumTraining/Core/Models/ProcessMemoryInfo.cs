namespace SeleniumTraining.Core.Models;

/// <summary>
/// Represents memory usage information for a process at a point in time.
/// </summary>
/// <remarks>
/// This record is used to encapsulate key memory metrics such as Working Set,
/// Private Memory, and Paged Memory, typically in Megabytes.
/// </remarks>
public record ProcessMemoryInfo
{
    /// <summary>
    /// Gets the Working Set of the process in Megabytes (MB).
    /// The working set is the set of memory pages recently touched by the threads in the process.
    /// It includes both shared and private data.
    /// </summary>
    /// <value>The working set size in megabytes.</value>
    public double WorkingSetMB { get; init; }

    /// <summary>
    /// Gets the Private Memory Size of the process in Megabytes (MB).
    /// This is the portion of memory that is exclusively used by the process and not shared with other processes.
    /// </summary>
    /// <value>The private memory size in megabytes.</value>
    public double PrivateMemoryMB { get; init; }

    /// <summary>
    /// Gets the Paged Memory Size of the process in Megabytes (MB).
    /// This is the amount of memory that the process has committed that could be written to the system's paging file(s).
    /// </summary>
    /// <value>The paged memory size in megabytes.</value>
    public double PagedMemoryMB { get; init; }

    /// <summary>
    /// Returns a string representation of the memory information.
    /// </summary>
    /// <returns>A string detailing working set, private memory, and paged memory in MB.</returns>
    public override string ToString()
    {
        return $"WorkingSet: {WorkingSetMB:F2} MB, PrivateMemory: {PrivateMemoryMB:F2} MB, PagedMemory: {PagedMemoryMB:F2} MB";
    }
}
