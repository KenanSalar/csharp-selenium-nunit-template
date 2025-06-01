using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Codeuctivity.ImageSharpCompare;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SeleniumTraining.Core.Services;

public class VisualTestService : BaseService, IVisualTestService
{
    private readonly IDirectoryManagerService _directoryManager;
    private readonly ITestWebDriverManager _webDriverManager;
    private readonly VisualTestSettings _settings;

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
                    comparisonResult = ImageSharpCompare.CalcDiff(actualImagePath, baselineImagePath);
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

        _ = Directory.CreateDirectory(baselineTestPath);;
        _ = Directory.CreateDirectory(actualImageDir);
        _ = Directory.CreateDirectory(diffImageDir);

        ServiceLogger.LogDebug("Prepared paths: Baseline='{BaselineP}', Actual='{ActualP}', Diff='{DiffP}'", baselineImagePath, actualImagePath, diffImagePath);
        return (baselineImagePath, actualImagePath, diffImagePath);
    }

    private static string SanitizeFilename(string name)
    {
        string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(CultureInfo.InvariantCulture, @"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return Regex.Replace(name, invalidRegStr, "_");
    }
}

