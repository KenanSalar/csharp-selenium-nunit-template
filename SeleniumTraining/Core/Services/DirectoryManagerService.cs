namespace SeleniumTraining.Core.Services;

/// <summary>
/// Manages and provides standardized directory paths for test execution artifacts.
/// It determines paths for logs, screenshots, and general test outputs, and ensures
/// these directories exist on the file system.
/// </summary>
/// <remarks>
/// This service implements <see cref="IDirectoryManagerService"/> ([2]) and calculates paths relative
/// to the test assembly and project root. It is crucial for organizing test artifacts
/// in a consistent manner, especially for CI/CD pipelines ([3]) and local debugging.
/// It inherits from <see cref="BaseService"/> for common logging capabilities.
/// The logic for determining the project root involves searching upwards from the
/// test assembly directory for a <c>.csproj</c> file.
/// </remarks>
public class DirectoryManagerService : BaseService, IDirectoryManagerService
{
    /// <inheritdoc cref="IDirectoryManagerService.TestAssemblyRootDirectory" />
    public string TestAssemblyRootDirectory { get; } = string.Empty; // /bin/Debug/net9.0/

    /// <inheritdoc cref="IDirectoryManagerService.BaseTestOutputDirectory" />
    public string BaseTestOutputDirectory { get; } = string.Empty; // /TestOutput/

    /// <inheritdoc cref="IDirectoryManagerService.BaseScreenshotDirectoryRoot" />
    public string BaseScreenshotDirectoryRoot { get; } = string.Empty; // /TestOutput/Screenshots/

    /// <inheritdoc cref="IDirectoryManagerService.BaseLogDirectoryRoot" />
    public string BaseLogDirectoryRoot { get; } = string.Empty; // TestOutput/Logs/

    /// <inheritdoc cref="IDirectoryManagerService.ProjectRootDirectory" />
    public string ProjectRootDirectory { get; } = string.Empty; // SeleniumTraining/

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryManagerService"/> class.
    /// During initialization, it determines the test assembly root, project root,
    /// and derives other base output directories. It also ensures these base directories exist.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    /// <remarks>
    /// The project root directory is determined by traversing up from the test assembly's location
    /// until a directory containing a <c>.csproj</c> file is found. If not found, it defaults
    /// to the test assembly root, and a warning is logged.
    /// After determining all paths, <see cref="EnsureBaseDirectoriesExist"/> is called.
    /// </remarks>
    public DirectoryManagerService(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initializing directory paths.", nameof(DirectoryManagerService));

        TestAssemblyRootDirectory = AppContext.BaseDirectory;
        ServiceLogger.LogDebug("Determined Test Assembly Root Directory: {TestAssemblyRootDir}", TestAssemblyRootDirectory);

        string? currentDirectory = TestAssemblyRootDirectory;
        while (currentDirectory != null)
        {
            if (Directory.GetFiles(currentDirectory, "*.csproj").Length != 0)
            {
                ProjectRootDirectory = currentDirectory;
                break;
            }

            DirectoryInfo? parentDir = Directory.GetParent(currentDirectory);

            if (parentDir == null)
            {
                ProjectRootDirectory = TestAssemblyRootDirectory;
                ServiceLogger.LogWarning("Could not determine project root by finding .csproj. Falling back to TestAssemblyRootDirectory: {FallbackDir}", ProjectRootDirectory);
                break;
            }
            currentDirectory = parentDir.FullName;
        }
        ServiceLogger.LogDebug("Determined Project Root Directory: {ProjectRootDir}", ProjectRootDirectory);

        BaseTestOutputDirectory = Path.Combine(TestAssemblyRootDirectory, "TestOutput");
        ServiceLogger.LogDebug("Set Base Test Output Directory: {BaseTestOutputDir}", BaseTestOutputDirectory);

        BaseScreenshotDirectoryRoot = Path.Combine(BaseTestOutputDirectory, "Screenshots");
        ServiceLogger.LogDebug("Set Base Screenshot Root Directory: {BaseScreenshotRootDir}", BaseScreenshotDirectoryRoot);

        BaseLogDirectoryRoot = Path.Combine(BaseTestOutputDirectory, "Logs");
        ServiceLogger.LogDebug("Set Base Log Root Directory: {BaseLogRootDir}", BaseLogDirectoryRoot);

        EnsureBaseDirectoriesExist();
    }

    /// <inheritdoc cref="IDirectoryManagerService.EnsureBaseDirectoriesExist()" />
    /// <remarks>
    /// This implementation iterates through the <see cref="BaseTestOutputDirectory"/>,
    /// <see cref="BaseScreenshotDirectoryRoot"/>, and <see cref="BaseLogDirectoryRoot"/>,
    /// calling a private helper method <see cref="TryCreateDirectory"/> for each to ensure its existence.
    /// Exceptions during directory creation are logged and re-thrown by the helper method.
    /// </remarks>
    public void EnsureBaseDirectoriesExist()
    {
        ServiceLogger.LogInformation("Ensuring base output directories exist.");
        ServiceLogger.LogDebug(
            "Base directories to ensure: TestOutput={TestOutputDir}, Screenshots={ScreenshotDir}, Logs={LogDir}",
            BaseTestOutputDirectory,
            BaseScreenshotDirectoryRoot,
            BaseLogDirectoryRoot
        );

        TryCreateDirectory(BaseTestOutputDirectory, "Base Test Output");
        TryCreateDirectory(BaseScreenshotDirectoryRoot, "Base Screenshot");
        TryCreateDirectory(BaseLogDirectoryRoot, "Base Log");

        ServiceLogger.LogInformation("Base output directories ensured (created if they didn't exist).");
    }

    /// <inheritdoc cref="IDirectoryManagerService.GetAndEnsureTestScreenshotDirectory(string)" />
    /// <remarks>
    /// This implementation validates that <paramref name="testSpecificFolderName"/> is not null or whitespace.
    /// It then combines this folder name with <see cref="BaseScreenshotDirectoryRoot"/> to form the full path
    /// and uses the private helper <see cref="TryCreateDirectory"/> to ensure this specific directory exists.
    /// Exceptions during directory creation are logged and re-thrown by the helper method.
    /// </remarks>
    public string GetAndEnsureTestScreenshotDirectory(string testSpecificFolderName)
    {
        if (string.IsNullOrWhiteSpace(testSpecificFolderName))
        {
            ServiceLogger.LogError("Invalid test-specific folder name provided (null or whitespace). Cannot create screenshot directory.");
            throw new ArgumentException("Test-specific folder name cannot be null or whitespace.", nameof(testSpecificFolderName));
        }

        string specificScreenshotDir = Path.Combine(BaseScreenshotDirectoryRoot, testSpecificFolderName);
        ServiceLogger.LogInformation(
            "Ensuring test-specific screenshot directory exists for test: '{TestSpecificName}', Path: {SpecificScreenshotDirPath}",
            testSpecificFolderName,
            specificScreenshotDir
        );

        TryCreateDirectory(specificScreenshotDir, $"Test-specific screenshot folder '{testSpecificFolderName}'");

        return specificScreenshotDir;
    }

    /// <summary>
    /// Attempts to create the specified directory if it does not already exist.
    /// Logs the outcome of the operation, including any exceptions encountered.
    /// </summary>
    /// <param name="path">The absolute path of the directory to create.</param>
    /// <param name="directoryDescription">A user-friendly description of the directory's purpose, used for logging (e.g., "Base Test Output", "Test-specific screenshot folder 'MyTest'").</param>
    /// <exception cref="UnauthorizedAccessException">Re-thrown if access is denied during directory creation.</exception>
    /// <exception cref="IOException">Re-thrown if an I/O error occurs during directory creation.</exception>
    /// <exception cref="Exception">Re-thrown for any other unexpected errors during directory creation.</exception>
    /// <remarks>
    /// This helper method centralizes the directory creation logic and error handling for the service.
    /// It checks for directory existence before attempting creation to avoid unnecessary operations or exceptions.
    /// </remarks>
    private void TryCreateDirectory(string path, string directoryDescription)
    {
        ServiceLogger.LogDebug("Attempting to create directory: {DirectoryDescription} at path: {DirectoryPath}", directoryDescription, path);
        try
        {
            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
                ServiceLogger.LogInformation("Directory successfully created: {DirectoryDescription} at path: {DirectoryPath}", directoryDescription, path);
            }
            else
            {
                ServiceLogger.LogDebug("Directory already exists: {DirectoryDescription} at path: {DirectoryPath}", directoryDescription, path);
            }
        }
        catch (UnauthorizedAccessException uaEx)
        {
            ServiceLogger.LogError(
                uaEx,
                "Access denied. Failed to create directory: {DirectoryDescription} at path: {DirectoryPath}. Check permissions.",
                directoryDescription,
                path
            );
            throw;
        }
        catch (IOException ioEx)
        {
            ServiceLogger.LogError(
                ioEx,
                "An IO error occurred. Failed to create directory: {DirectoryDescription} at path: {DirectoryPath}.",
                directoryDescription,
                path
            );
            throw;
        }
        catch (Exception ex)
        {
            ServiceLogger.LogError(
                ex,
                "An unexpected error occurred. Failed to create directory: {DirectoryDescription} at path: {DirectoryPath}.",
                directoryDescription,
                path
            );
            throw;
        }
    }
}
