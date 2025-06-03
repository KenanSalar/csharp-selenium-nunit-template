namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service responsible for capturing and saving screenshots from a WebDriver instance.
/// </summary>
/// <remarks>
/// Implementations of this interface will handle the technical details of interacting with the WebDriver
/// to get screenshot data and persisting it to the file system. This service decouples screenshot
/// functionality from other services that might need to consume or trigger screenshots (e.g., reporting services).
/// </remarks>
public interface IScreenshotService
{
    /// <summary>
    /// Captures a screenshot from the provided WebDriver instance and saves it to the specified directory with the given file name.
    /// </summary>
    /// <param name="driver">The <see cref="IWebDriver"/> instance from which to take the screenshot. Must implement <see cref="ITakesScreenshot"/>.</param>
    /// <param name="directory">The directory where the screenshot image file will be saved. This directory will be created if it doesn't exist.</param>
    /// <param name="fileNameWithoutExtension">The desired file name for the screenshot. The ".png" extension will be appended automatically.</param>
    /// <returns>The full path to the saved screenshot file if successful; otherwise, <c>null</c> if capture or saving fails (e.g., driver doesn't support screenshots, I/O errors).</returns>
    /// <remarks>
    /// This method is responsible for:
    /// <list type="bullet">
    ///   <item><description>Verifying that the driver can take screenshots.</description></item>
    ///   <item><description>Capturing the screenshot data.</description></item>
    ///   <item><description>Ensuring the target directory exists.</description></item>
    ///   <item><description>Saving the screenshot as a PNG file.</description></item>
    /// </list>
    /// Any exceptions during the process should be handled internally by the implementation, typically resulting in a <c>null</c> return value and logged errors.
    /// </remarks>
    /// <exception cref="ArgumentNullException">May be thrown by implementations if <paramref name="driver"/>, <paramref name="directory"/>, or <paramref name="fileNameWithoutExtension"/> is null or invalid, though implementations are encouraged to handle gracefully and return null.</exception>
    public string? CaptureAndSaveScreenshot(IWebDriver driver, string directory, string fileNameWithoutExtension);
}
