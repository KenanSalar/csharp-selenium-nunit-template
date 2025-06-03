using SixLabors.ImageSharp;

namespace SeleniumTraining.Core.Services.Contracts;

/// <summary>
/// Defines the contract for a service that performs visual regression testing by comparing
/// UI elements or full pages against approved baseline images.
/// </summary>
/// <remarks>
/// This service encapsulates the logic for capturing screenshots, comparing them with baselines,
/// managing baseline images (creation/update), and reporting visual differences.
/// It helps in detecting unintended visual changes in the application's UI.
/// Implementations might use image comparison libraries like ImageSharpCompare.
/// </remarks>
public interface IVisualTestService
{
    /// <summary>
    /// Asserts that the current visual appearance of a UI element or page matches a pre-defined baseline image.
    /// If a baseline image does not exist and auto-creation is enabled, a new baseline will be created.
    /// </summary>
    /// <param name="baselineIdentifier">A unique string that identifies this specific visual checkpoint (e.g., "LoginPage_Header", "ProductDetails_MainImage").
    /// This identifier is used to name the baseline, actual, and diff image files.</param>
    /// <param name="testName">The name of the current test class (e.g., from TestContext or BaseTest.TestName),
    /// used for organizing baseline images into subfolders.</param>
    /// <param name="browserType">The <see cref="BrowserType"/> on which the test is running, used for browser-specific baselines.</param>
    /// <param name="elementToCapture">Optional. If provided, only this specific <see cref="IWebElement"/> will be captured for comparison.
    /// If null, and <paramref name="cropArea"/> is also null, a full-page screenshot is taken.</param>
    /// <param name="cropArea">Optional. If provided, the screenshot will be cropped to this <see cref="Rectangle"/> area.
    /// This is an alternative to <paramref name="elementToCapture"/> for capturing specific regions. If both are null, a full-page screenshot is taken.</param>
    /// <param name="tolerancePercent">Optional. The acceptable percentage of pixel difference between the actual image and the baseline.
    /// If null, a default tolerance configured in the service implementation or settings will be used.</param>
    /// <exception cref="ShouldAssertException">Thrown by Shouldly if the visual comparison fails (i.e., pixel difference exceeds tolerance).</exception>
    /// <exception cref="FileNotFoundException">May be thrown if a baseline image is expected but not found and auto-creation is disabled.</exception>
    /// <exception cref="InvalidOperationException">May be thrown for issues like invalid crop areas or if the WebDriver instance cannot take screenshots.</exception>
    /// <remarks>
    /// The method handles the following:
    /// <list type="bullet">
    ///   <item><description>Capturing the current screenshot (full page, element, or region).</description></item>
    ///   <item><description>Locating the corresponding baseline image based on identifier, test name, and browser type.</description></item>
    ///   <item><description>Comparing the actual image with the baseline using an image comparison library.</description></item>
    ///   <item><description>Generating and saving a difference image if a mismatch is detected.</description></item>
    ///   <item><description>Asserting the visual match based on the configured tolerance.</description></item>
    ///   <item><description>Optionally creating a new baseline if one is missing and settings permit.</description></item>
    /// </list>
    /// Ensure that baseline images are stored in a version-controlled location, typically specified in the service's configuration.
    /// </remarks>
    public void AssertVisualMatch(
        string baselineIdentifier,
        string testName,
        BrowserType browserType,
        IWebElement? elementToCapture = null,
        Rectangle? cropArea = null,
        double? tolerancePercent = null
    );
}
