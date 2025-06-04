using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Codeuctivity.ImageSharpCompare;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SeleniumTraining.Core.Services;

/// <summary>
/// Service responsible for performing visual regression testing by comparing UI screenshots
/// against approved baseline images.
/// </summary>
/// <remarks>
/// This service implements <see cref="IVisualTestService"/> and encapsulates the entire visual testing workflow:
/// <list type="bullet">
///   <item><description>Screenshot capture (full page, element, or region).</description></item>
///   <item><description>Path management for baseline, actual, and difference images using <see cref="IDirectoryManagerService"/>.</description></item>
///   <item><description>Image comparison using <c>ImageSharpCompare</c> library.</description></item>
///   <item><description>Baseline image management (creation if missing and configured, warning on creation).</description></item>
///   <item><description>Assertion of visual match based on a configurable pixel difference tolerance.</description></item>
/// </list>
/// It retrieves visual test settings (like baseline directory, auto-creation flags, default tolerance)
/// via <see cref="ISettingsProviderService"/>. This service is critical for detecting unintended UI changes,
/// especially in CI/CD environments ([3]) where visual consistency is paramount.
/// Inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class VisualTestService : BaseService, IVisualTestService
{
    private readonly IDirectoryManagerService _directoryManager;
    private readonly ITestWebDriverManager _webDriverManager;
    private readonly VisualTestSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="VisualTestService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/>.</param>
    /// <param name="webDriver">The current <see cref="IWebDriver"/> instance to use for taking screenshots. Must not be null.</param>
    /// <param name="directoryManager">Service for managing directory paths for visual test artifacts. Must not be null.</param>
    /// <param name="settingsProvider">Service for accessing application settings, including <see cref="VisualTestSettings"/>. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the required service parameters are null.</exception>
    /// <remarks>
    /// The constructor retrieves <see cref="VisualTestSettings"/> and <see cref="TestFrameworkSettings"/>
    /// using the provided <paramref name="settingsProvider"/>.
    /// The <paramref name="webDriver"/> parameter needs careful consideration regarding its lifecycle and how it's provided,
    /// especially in parallel testing scenarios (e.g., it should be the thread-specific driver).
    /// </remarks>
    public VisualTestService(
        ILoggerFactory loggerFactory,
        IDirectoryManagerService directoryManager,
        ITestWebDriverManager webDriverManager,
        IOptions<VisualTestSettings> visualTestSettings
    )
        : base(loggerFactory)
    {
        _directoryManager = directoryManager;
        _webDriverManager = webDriverManager;
        _settings = visualTestSettings.Value;
        ServiceLogger.LogInformation(
            "VisualTestService initialized. Baseline dir: {BaselineDir}, AutoCreate: {AutoCreate}",
            _settings.BaselineDirectory,
            _settings.AutoCreateBaselineIfMissing
        );
    }

    /// <inheritdoc cref="IVisualTestService.AssertVisualMatch(string, string, BrowserType, IWebElement?, Rectangle?, double?)" />
    /// <remarks>
    /// This implementation performs the following steps:
    /// <list type="number">
    ///   <item><description>Prepares file paths for baseline, actual, and difference images using <see cref="PrepareFilePaths"/>.</description></item>
    ///   <item><description>Captures the "actual" screenshot using <see cref="CaptureAndSaveActualImage"/>.</description></item>
    ///   <item><description>Checks for the existence of the baseline image. If it's missing and <see cref="VisualTestSettings.AutoCreateBaselineIfMissing"/> is true,
    ///   it copies the actual image as the new baseline and logs a warning (if configured).</description></item>
    ///   <item><description>If the baseline exists, it performs an image comparison using <c>ImageSharpCompare.CalcDiff</c>.</description></item>
    ///   <item><description>Saves a difference image if mismatches are found.</description></item>
    ///   <item><description>Asserts that the percentage of different pixels is less than or equal to the specified or default <paramref name="tolerancePercent"/>
    ///   using <c>Shouldly</c> assertions.</description></item>
    /// </list>
    /// Thorough logging is performed at each stage, including paths, comparison results, and any errors.
    /// Exceptions during image processing or comparison are caught, logged, and may result in test assertion failures.
    /// </remarks>
    public void AssertVisualMatch(
        string baselineIdentifier,
        string testName,
        BrowserType browserType,
        IWebElement? elementToCapture = null,
        Rectangle? cropArea = null,
        double? tolerancePercent = null
    )
    {
        double effectiveTolerance = tolerancePercent ?? _settings.DefaultComparisonTolerancePercent;
        (string baselineImagePath, string actualImagePath, string diffImagePath) = PrepareFilePaths(baselineIdentifier, testName, browserType.ToString());

        ServiceLogger.LogInformation(
            "Performing visual assertion for ID '{BaselineID}' in test '{Test}'. Baseline: '{BaselinePath}', Tolerance: {Tolerance}%",
            baselineIdentifier, testName, baselineImagePath, effectiveTolerance);

        try
        {
            CaptureAndSaveActualImage(actualImagePath, elementToCapture, cropArea);
            AllureApi.AddAttachment($"Actual - {baselineIdentifier}", "image/png", actualImagePath);

            if (File.Exists(baselineImagePath))
            {
                AllureApi.AddAttachment($"Baseline - {baselineIdentifier}", "image/png", baselineImagePath);
                ServiceLogger.LogDebug("Baseline image exists: {BaselinePath}. Comparing...", baselineImagePath);

                ICompareResult? comparisonResult = null;
                try
                {
                    comparisonResult = ImageSharpCompare.CalcDiff(actualImagePath, baselineImagePath, ResizeOption.DontResize);
                }
                catch (ImageSharpCompareException imgEx) when (imgEx.Message.Contains("Size of images differ."))
                {
                    ServiceLogger.LogError(imgEx, "Image size mismatch during visual comparison. Actual: {ActualPath}, Baseline: {BaselinePath}. Ensure baseline generation environment matches test execution environment.", actualImagePath, baselineImagePath);
                    Assert.Fail($"Visual comparison failed due to image size mismatch. Details: {imgEx.Message}");

                    return;
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError(ex, "Error during image comparison between {ActualPath} and {BaselinePath}", actualImagePath, baselineImagePath);
                    Assert.Fail($"Visual comparison failed due to an error: {ex.Message}");

                    return;
                }

                ServiceLogger.LogInformation(
                    "Comparison for '{BaselineID}': Pixel Error={ErrorPercentage}%, Absolute Error={AbsoluteErrorPixels}",
                    baselineIdentifier,
                    comparisonResult.PixelErrorPercentage,
                    comparisonResult.AbsoluteError
                );

                if (comparisonResult.PixelErrorPercentage > effectiveTolerance)
                {
                    ServiceLogger.LogWarning(
                        "Visual mismatch detected for '{BaselineID}'. Error: {ErrorPercentage}%, Tolerance: {Tolerance}%",
                        baselineIdentifier,
                        comparisonResult.PixelErrorPercentage,
                        effectiveTolerance
                    );
                    using (Image diffImage = ImageSharpCompare.CalcDiffMaskImage(actualImagePath, baselineImagePath))
                    {
                        diffImage.Save(diffImagePath);
                    }
                    AllureApi.AddAttachment($"Difference - {baselineIdentifier}", "image/png", diffImagePath);

                    comparisonResult.PixelErrorPercentage.ShouldBeLessThanOrEqualTo(
                        effectiveTolerance,
                        $"Visual mismatch for '{baselineIdentifier}'. " +
                        "Pixel error {comparisonResult.PixelErrorPercentage:F3}% exceeded tolerance {effectiveTolerance:F3}%. See attached difference image."
                    );
                }
                else
                {
                    ServiceLogger.LogInformation(
                        "Visual match successful for '{BaselineID}'. Error: {ErrorPercentage}% within tolerance {Tolerance}%",
                        baselineIdentifier,
                        comparisonResult.PixelErrorPercentage,
                        effectiveTolerance
                    );
                }
            }
            else
            {
                HandleMissingBaseline(baselineImagePath, actualImagePath, baselineIdentifier);
            }
        }
        catch (ShouldAssertException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ServiceLogger.LogError(ex, "Unexpected error during visual assertion for ID '{BaselineID}'", baselineIdentifier);
            // Save actual image even on error for debugging
            if (!File.Exists(actualImagePath) && ex is not ShouldAssertException)
            {
                // Attempt to save actual image if capture failed mid-way but driver is available
            }
            Assert.Fail($"Visual assertion for '{baselineIdentifier}' failed with an unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Captures a screenshot of the current UI state (full page, element, or specified region)
    /// and saves it to the provided file path.
    /// </summary>
    /// <param name="actualImagePath">The full path where the captured "actual" image will be saved.</param>
    /// <param name="elementToCapture">Optional. If provided, only this <see cref="IWebElement"/> is captured.</param>
    /// <param name="cropArea">Optional. If provided (and <paramref name="elementToCapture"/> is null),
    /// the full-page screenshot is cropped to this <see cref="Rectangle"/>.</param>
    /// <exception cref="ArgumentException">Thrown if both <paramref name="elementToCapture"/> and <paramref name="cropArea"/> are specified, as they are mutually exclusive for primary capture focus.</exception>
    /// <exception cref="WebDriverException">Thrown if the WebDriver instance cannot take screenshots or if an element is not interactable for screenshotting.</exception>
    /// <exception cref="InvalidOperationException">Thrown if cropping dimensions are invalid (e.g., outside image bounds).</exception>
    /// <remarks>
    /// If <paramref name="elementToCapture"/> is provided, its location and size are used to crop the screenshot.
    /// If <paramref name="cropArea"/> is provided, a full-page screenshot is taken first, then cropped.
    /// If neither is provided, a full-page screenshot is saved.
    /// The method ensures the directory for <paramref name="actualImagePath"/> exists.
    /// Any errors during screenshot capture or processing are logged.
    /// </remarks>
    private void CaptureAndSaveActualImage(string actualImagePath, IWebElement? elementToCapture, Rectangle? cropArea)
    {
        Screenshot screenshot = ((ITakesScreenshot)_webDriverManager.GetDriver()).GetScreenshot();

        using var image = Image.Load(screenshot.AsByteArray);

        ServiceLogger.LogInformation(
            "[VISUAL_DEBUG_CI] Actual image dimensions BEFORE crop: Width={Width}, Height={Height}. Path: {Path}",
            image.Width,
            image.Height,
            actualImagePath
        );

        if (elementToCapture != null)
        {
            System.Drawing.Point location = elementToCapture.Location;
            System.Drawing.Size size = elementToCapture.Size;
            var elementRect = new Rectangle(location.X, location.Y, size.Width, size.Height);

            elementRect.Intersect(image.Bounds);
            if (elementRect.Width <= 0 || elementRect.Height <= 0)
            {
                throw new InvalidOperationException(
                    $"Element for visual comparison '{elementToCapture.TagName}' resulted in an invalid crop area (zero or negative width/height) after intersection. " +
                    "Element Rect: {location.X},{location.Y} {size.Width}x{size.Height}. Image Bounds: {image.Width}x{image.Height}"
                );
            }
            image.Mutate(x => x.Crop(elementRect));
            ServiceLogger.LogDebug("Cropped actual image to element bounds: {Rect}", elementRect);
        }
        else if (cropArea.HasValue)
        {
            Rectangle validCropArea = cropArea.Value;
            validCropArea.Intersect(image.Bounds);
            if (validCropArea.Width <= 0 || validCropArea.Height <= 0)
            {
                throw new InvalidOperationException(
                    $"Specified cropArea for visual comparison resulted in an invalid crop area after intersection. " +
                    "CropArea: {cropArea.Value}. Image Bounds: {image.Width}x{image.Height}"
                );
            }
            image.Mutate(x => x.Crop(validCropArea));
            ServiceLogger.LogDebug("Cropped actual image to specified Rectangle: {Rect}", validCropArea);
        }

        image.Save(actualImagePath);
        ServiceLogger.LogInformation("Actual image saved: {ActualPath}", actualImagePath);
    }

    /// <summary>
    /// Handles the scenario where a baseline image is missing for a visual test.
    /// If auto-creation of baselines is enabled in settings, the actual image is copied as the new baseline.
    /// A warning is logged if baselines are automatically created (if configured).
    /// If auto-creation is disabled, an exception is thrown.
    /// </summary>
    /// <param name="baselineImagePath">The expected path of the missing baseline image.</param>
    /// <param name="actualImagePath">The path of the captured "actual" image, which will be used as the new baseline if auto-creation is enabled.</param>
    /// <param name="baselineIdentifier">The unique identifier for this visual checkpoint, used in logging.</param>
    /// <exception cref="FileNotFoundException">Thrown if auto-creation of baselines is disabled and the baseline image is missing.</exception>
    /// <remarks>
    /// This method centralizes the logic for managing missing baselines, adhering to the
    /// <see cref="VisualTestSettings.AutoCreateBaselineIfMissing"/> and
    /// <see cref="VisualTestSettings.WarnOnAutomaticBaselineCreation"/> settings.
    /// Ensures the directory for the baseline image exists before copying.
    /// </remarks>
    private void HandleMissingBaseline(string baselineImagePath, string actualImagePath, string baselineIdentifier)
    {
        ServiceLogger.LogWarning("Baseline image not found: {BaselinePath}", baselineImagePath);
        if (_settings.AutoCreateBaselineIfMissing)
        {
            File.Copy(actualImagePath, baselineImagePath, overwrite: true);
            ServiceLogger.LogInformation("New baseline image automatically created: {BaselinePath}", baselineImagePath);
            AllureApi.AddAttachment($"Baseline (NEW) - {baselineIdentifier}", "image/png", baselineImagePath);

            if (_settings.WarnOnAutomaticBaselineCreation)
            {
                Assert.Warn($"New visual baseline created for '{baselineIdentifier}' at '{baselineImagePath}'. Review and commit if correct. Test marked as Warning.");
            }
        }
        else
        {
            Assert.Fail($"Visual baseline missing for '{baselineIdentifier}' at '{baselineImagePath}' and auto-creation is disabled.");
        }
    }

    /// <summary>
    /// Prepares the fully qualified file paths for the baseline, actual, and difference images
    /// for a given visual test checkpoint.
    /// </summary>
    /// <param name="baselineIdentifier">A unique string identifying the visual checkpoint.</param>
    /// <param name="testName">The name of the test class, used for sub-folder organization under the browser-specific baseline directory.</param>
    /// <param name="browserName">The name of the browser, used for the top-level sub-folder within the baseline directory.</param>
    /// <returns>A tuple containing the absolute paths for the baseline image, actual image, and difference image.</returns>
    /// <remarks>
    /// Baseline images are stored under: <c>[ProjectRoot]/[VisualTestSettings.BaselineDirectory]/[browserName]/[testName]/</c>.
    /// Actual and difference images are stored under: <c>[TestOutput]/Screenshots/[testName]/VisualActuals/[browserName]/</c> and <c>VisualDiffs/[browserName]/</c> respectively.
    /// The method ensures that the directories for actual and diff images are created.
    /// </remarks>
    private (string BaselineImagePath, string ActualImagePath, string DiffImagePath) PrepareFilePaths(string baselineIdentifier, string testName, string browserName)
    {
        string baselineRoot = Path.Combine(_directoryManager.ProjectRootDirectory, _settings.BaselineDirectory);
        string baselineBrowserPath = Path.Combine(baselineRoot, browserName);
        string baselineTestPath = Path.Combine(baselineBrowserPath, SanitizeFilename(testName));
        string baselineImagePath = Path.Combine(baselineTestPath, $"{SanitizeFilename(baselineIdentifier)}.png");

        string testClassScreenshotsDir = _directoryManager.GetAndEnsureTestScreenshotDirectory(SanitizeFilename(testName));

        string actualImageDir = Path.Combine(testClassScreenshotsDir, "VisualActuals", browserName);
        string actualImagePath = Path.Combine(actualImageDir, $"{SanitizeFilename(baselineIdentifier)}_actual_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.png");
        string diffImageDir = Path.Combine(testClassScreenshotsDir, "VisualDiffs", browserName);
        string diffImagePath = Path.Combine(diffImageDir, $"{SanitizeFilename(baselineIdentifier)}_diff_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.png");

        _ = Directory.CreateDirectory(baselineTestPath); ;
        _ = Directory.CreateDirectory(actualImageDir);
        _ = Directory.CreateDirectory(diffImageDir);

        ServiceLogger.LogDebug("Prepared paths: Baseline='{BaselineP}', Actual='{ActualP}', Diff='{DiffP}'", baselineImagePath, actualImagePath, diffImagePath);
        return (baselineImagePath, actualImagePath, diffImagePath);
    }

    /// <summary>
    /// Sanitizes a given string to be suitable for use as a filename by replacing
    /// invalid file name characters and patterns with underscores.
    /// </summary>
    /// <param name="name">The input string to sanitize for use as a filename.</param>
    /// <returns>A sanitized string where invalid characters and patterns (like trailing dots or only invalid characters)
    /// have been replaced with underscores. Returns an empty string if the input <paramref name="name"/> is null or empty.</returns>
    /// <remarks>
    /// This method uses regular expressions to perform the sanitization.
    /// It first identifies all characters considered invalid for filenames on the current system
    /// using <see cref="Path.GetInvalidFileNameChars()"/>.
    /// Then, it constructs a regular expression to match sequences of these invalid characters
    /// or patterns like one or more invalid characters followed by trailing dots (e.g., "file..", "aux.txt").
    /// All such matches are replaced with a single underscore ("_").
    /// For example, "my:file/name?.txt" might become "my_file_name__txt".
    /// If the input name is null or empty, an empty string is returned directly without processing.
    /// </remarks>
    /// <example>
    /// <code>
    /// string originalName = "Test: Case* One?";
    /// string sanitized = SanitizeFilename(originalName); // sanitized might be "Test_ Case_ One_"
    ///
    /// string anotherName = "report..final.txt";
    /// string sanitized2 = SanitizeFilename(anotherName); // sanitized2 might be "report_final_txt" (depending on exact regex behavior with multiple dots)
    /// </code>
    /// </example>
    private static string SanitizeFilename(string name)
    {
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(CultureInfo.InvariantCulture, @"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return Regex.Replace(name, invalidRegStr, "_");
    }
}

