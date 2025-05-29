namespace SeleniumTraining.Core.Services.Contracts;

public interface IDirectoryManagerService
{
    public string TestAssemblyRootDirectory { get; }
    public string BaseTestOutputDirectory { get; }
    public string BaseScreenshotDirectoryRoot { get; }
    public string BaseLogDirectoryRoot { get; }
    public void EnsureBaseDirectoriesExist();
    public string GetAndEnsureTestScreenshotDirectory(string testSpecificFolderName);
}
