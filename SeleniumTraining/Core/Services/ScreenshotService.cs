namespace SeleniumTraining.Core.Services;

/// <summary>
/// Provides an implementation of <see cref="IScreenshotService"/> for capturing and saving screenshots
/// from WebDriver instances.
/// </summary>
/// <remarks>
/// This service inherits from <see cref="BaseService"/> to leverage common logging functionalities.
/// It handles the conversion of WebDriver's screenshot data into a PNG file and saves it to the specified location.
/// Error handling is included to manage issues during screenshot capture or file I/O, with errors being logged.
/// </remarks>
public class ScreenshotService : BaseService, IScreenshotService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScreenshotService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="loggerFactory"/> is null.</exception>
    public ScreenshotService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(ScreenshotService));
    }

    /// <inheritdoc cref="IScreenshotService.CaptureAndSaveScreenshot(IWebDriver, string, string)"/>
    /// <remarks>
    /// This implementation first checks if the provided <paramref name="driver"/> can take screenshots.
    /// It then constructs the full file path, ensures the target directory exists,
    /// captures the screenshot, and saves it as a PNG file.
    /// Exceptions encountered during these operations are logged, and the method returns <c>null</c> in case of failure.
    /// </remarks>
    public string? CaptureAndSaveScreenshot(IWebDriver driver, string directory, string fileNameWithoutExtension)
    {
        if (driver is not ITakesScreenshot screenshotDriver)
        {
            ServiceLogger.LogWarning("WebDriver instance does not support ITakesScreenshot. Cannot capture screenshot.");
            return null;
        }

        string filePath = Path.Combine(directory, $"{fileNameWithoutExtension}.png");

        try
        {
            ServiceLogger.LogDebug("Attempting to capture screenshot. Target path: {FilePath}", filePath);
            Screenshot screenshot = screenshotDriver.GetScreenshot();

            string? targetDirectoryPath = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(targetDirectoryPath))
            {
                ServiceLogger.LogError("Could not determine directory from path {FilePath}. Screenshot saving aborted.", filePath);
                return null;
            }
            _ = Directory.CreateDirectory(targetDirectoryPath);

            screenshot.SaveAsFile(filePath);
            ServiceLogger.LogInformation("Screenshot successfully saved to {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            ServiceLogger.LogError(ex, "Failed to capture or save screenshot to {FilePath}.", filePath);
            return null;
        }
    }
}

