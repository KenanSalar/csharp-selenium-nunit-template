namespace SeleniumTraining.Core.Services;

public class DirectoryManagerService : BaseService, IDirectoryManagerService
{
    public string TestAssemblyRootDirectory { get; } = string.Empty;
    public string BaseTestOutputDirectory { get; } = string.Empty; // /TestOutput/
    public string BaseScreenshotDirectoryRoot { get; } = string.Empty; // /TestOutput/Screenshots/
    public string BaseLogDirectoryRoot { get; } = string.Empty; // TestOutput/Logs/

    public DirectoryManagerService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initializing directory paths.", nameof(DirectoryManagerService));

        TestAssemblyRootDirectory = AppContext.BaseDirectory;
        ServiceLogger.LogDebug("Determined Test Assembly Root Directory: {TestAssemblyRootDir}", TestAssemblyRootDirectory);

        BaseTestOutputDirectory = Path.Combine(TestAssemblyRootDirectory, "TestOutput");
        ServiceLogger.LogDebug("Set Base Test Output Directory: {BaseTestOutputDir}", BaseTestOutputDirectory);

        BaseScreenshotDirectoryRoot = Path.Combine(BaseTestOutputDirectory, "Screenshots");
        ServiceLogger.LogDebug("Set Base Screenshot Root Directory: {BaseScreenshotRootDir}", BaseScreenshotDirectoryRoot);

        BaseLogDirectoryRoot = Path.Combine(BaseTestOutputDirectory, "Logs");
        ServiceLogger.LogDebug("Set Base Log Root Directory: {BaseLogRootDir}", BaseLogDirectoryRoot);
    }

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
