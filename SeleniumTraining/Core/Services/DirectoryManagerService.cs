namespace SeleniumTraining.Core.Services;

public class DirectoryManagerService : BaseService, IDirectoryManagerService
{
    public string TestAssemblyRootDirectory { get; } = string.Empty;
    public string BaseTestOutputDirectory { get; } = string.Empty; // /TestOutput/
    public string BaseScreenshotDirectoryRoot { get; } = string.Empty; // /TestOutput/Screenshots/
    public string BaseLogDirectoryRoot { get; } = string.Empty; // TestOutput/Logs/

    public DirectoryManagerService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{ServiceName} initializing directory paths.", nameof(DirectoryManagerService));

        TestAssemblyRootDirectory = AppContext.BaseDirectory;
        Logger.LogDebug("Determined Test Assembly Root Directory: {TestAssemblyRootDir}", TestAssemblyRootDirectory);

        BaseTestOutputDirectory = Path.Combine(TestAssemblyRootDirectory, "TestOutput");
        Logger.LogDebug("Set Base Test Output Directory: {BaseTestOutputDir}", BaseTestOutputDirectory);

        BaseScreenshotDirectoryRoot = Path.Combine(BaseTestOutputDirectory, "Screenshots");
        Logger.LogDebug("Set Base Screenshot Root Directory: {BaseScreenshotRootDir}", BaseScreenshotDirectoryRoot);

        BaseLogDirectoryRoot = Path.Combine(BaseTestOutputDirectory, "Logs");
        Logger.LogDebug("Set Base Log Root Directory: {BaseLogRootDir}", BaseLogDirectoryRoot);
    }

    public void EnsureBaseDirectoriesExist()
    {
        Logger.LogInformation("Ensuring base output directories exist.");
        Logger.LogDebug(
            "Base directories to ensure: TestOutput={TestOutputDir}, Screenshots={ScreenshotDir}, Logs={LogDir}",
            BaseTestOutputDirectory,
            BaseScreenshotDirectoryRoot,
            BaseLogDirectoryRoot
        );

        TryCreateDirectory(BaseTestOutputDirectory, "Base Test Output");
        TryCreateDirectory(BaseScreenshotDirectoryRoot, "Base Screenshot");
        TryCreateDirectory(BaseLogDirectoryRoot, "Base Log");

        Logger.LogInformation("Base output directories ensured (created if they didn't exist).");
    }

    public string GetAndEnsureTestScreenshotDirectory(string testSpecificFolderName)
    {
        if (string.IsNullOrWhiteSpace(testSpecificFolderName))
        {
            Logger.LogError("Invalid test-specific folder name provided (null or whitespace). Cannot create screenshot directory.");
            throw new ArgumentException("Test-specific folder name cannot be null or whitespace.", nameof(testSpecificFolderName));
        }

        string specificScreenshotDir = Path.Combine(BaseScreenshotDirectoryRoot, testSpecificFolderName);
        Logger.LogInformation(
            "Ensuring test-specific screenshot directory exists for test: '{TestSpecificName}', Path: {SpecificScreenshotDirPath}",
            testSpecificFolderName,
            specificScreenshotDir
        );

        TryCreateDirectory(specificScreenshotDir, $"Test-specific screenshot folder '{testSpecificFolderName}'");

        return specificScreenshotDir;
    }

    private void TryCreateDirectory(string path, string directoryDescription)
    {
        Logger.LogDebug("Attempting to create directory: {DirectoryDescription} at path: {DirectoryPath}", directoryDescription, path);
        try
        {
            if (!Directory.Exists(path))
            {
                _ = Directory.CreateDirectory(path);
                Logger.LogInformation("Directory successfully created: {DirectoryDescription} at path: {DirectoryPath}", directoryDescription, path);
            }
            else
            {
                Logger.LogDebug("Directory already exists: {DirectoryDescription} at path: {DirectoryPath}", directoryDescription, path);
            }
        }
        catch (UnauthorizedAccessException uaEx)
        {
            Logger.LogError(
                uaEx,
                "Access denied. Failed to create directory: {DirectoryDescription} at path: {DirectoryPath}. Check permissions.",
                directoryDescription,
                path
            );
            throw;
        }
        catch (IOException ioEx)
        {
            Logger.LogError(
                ioEx,
                "An IO error occurred. Failed to create directory: {DirectoryDescription} at path: {DirectoryPath}.",
                directoryDescription,
                path
            );
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An unexpected error occurred. Failed to create directory: {DirectoryDescription} at path: {DirectoryPath}.",
                directoryDescription,
                path
            );
            throw;
        }
    }
}
