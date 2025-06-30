namespace SeleniumTraining.Utils.Settings.TestFeatures;

/// <summary>
/// Defines settings specifically for the visual regression testing feature of the framework as an immutable record.
/// These settings control aspects like baseline image management, comparison tolerances, and logging behavior.
/// </summary>
/// <remarks>
/// This record is typically bound to the "VisualTestSettings" configuration section (e.g., in appsettings.json).
/// As a record with `init`-only properties, its configuration is fixed upon loading.
/// Proper configuration is essential for effective visual testing, allowing tests to detect
/// unintended UI changes while accommodating acceptable levels of variance.
/// Data annotations like <see cref="RequiredAttribute"/> and <see cref="RangeAttribute"/> are used
/// for validating these settings when they are loaded.
/// </remarks>
public record VisualTestSettings
{
    /// <summary>
    /// Gets the root directory for storing visual baseline images.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "BaselineDirectory for visual tests is required.")]
    public string BaselineDirectory { get; init; } = "ProjectVisualBaselines";

    /// <summary>
    /// Gets a value indicating whether a new baseline image should be automatically
    /// created and saved if one is missing during a visual assertion.
    /// </summary>
    /// <value>
    /// <c>true</c> to automatically create missing baselines; otherwise, <c>false</c>.
    /// </value>
    public bool AutoCreateBaselineIfMissing { get; init; } = true;

    /// <summary>
    /// Gets the default tolerance percentage allowed for pixel differences
    /// when comparing an actual image against a baseline image.
    /// A value of 0 means an exact match is required.
    /// </summary>
    /// <value>
    /// The default comparison tolerance as a percentage (e.g., 0.20 for 0.20%).
    /// </value>
    [System.ComponentModel.DataAnnotations.Range(0, 100, ErrorMessage = "DefaultComparisonTolerancePercent must be between 0 and 100.")]
    public double DefaultComparisonTolerancePercent { get; init; } = 0.20;

    /// <summary>
    /// Gets a value indicating whether a warning message should be logged
    /// when a baseline image is automatically created because it was missing.
    /// </summary>
    /// <value>
    /// <c>true</c> to log a warning upon automatic baseline creation; otherwise, <c>false</c>.
    /// </value>
    public bool WarnOnAutomaticBaselineCreation { get; init; } = true;
}
