namespace SeleniumTraining.Utils.Settings.TestFeatures;

/// <summary>
/// Defines settings specifically for the visual regression testing feature of the framework.
/// These settings control aspects like baseline image management, comparison tolerances, and logging behavior.
/// </summary>
/// <remarks>
/// This class is typically bound to a configuration section (e.g., "VisualTestSettings" in appsettings.json)
/// and provides strongly-typed access to these configurations.
/// Proper configuration is essential for effective visual testing, allowing tests to detect
/// unintended UI changes while accommodating acceptable levels of variance.
/// In CI/CD environments, these settings might influence whether baselines are automatically
/// updated or if tests with visual differences fail the build.
/// Data annotations like <see cref="RequiredAttribute"/> and <see cref="RangeAttribute"/> are used
/// for validating these settings when they are loaded.
/// </remarks>
public class VisualTestSettings
{
    /// <summary>
    /// Defines settings specifically for the visual regression testing feature of the framework.
    /// These settings control aspects like baseline image management, comparison tolerances, and logging behavior.
    /// </summary>
    /// <remarks>
    /// This class is typically bound to a configuration section (e.g., "VisualTestSettings" in appsettings.json)
    /// and provides strongly-typed access to these configurations.
    /// Proper configuration is essential for effective visual testing, allowing tests to detect
    /// unintended UI changes while accommodating acceptable levels of variance.
    /// In CI/CD environments, these settings might influence whether baselines are automatically
    /// updated or if tests with visual differences fail the build.
    /// Data annotations like <see cref="RequiredAttribute"/> and <see cref="RangeAttribute"/> are used
    /// for validating these settings when they are loaded.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "BaselineDirectory for visual tests is required.")]
    public string BaselineDirectory { get; set; } = "ProjectVisualBaselines";

    /// <summary>
    /// Gets or sets a value indicating whether a new baseline image should be automatically
    /// created and saved if one is missing during a visual assertion.
    /// </summary>
    /// <value>
    /// <c>true</c> to automatically create missing baselines; otherwise, <c>false</c>.
    /// Defaults to <c>true</c>.
    /// </value>
    /// <remarks>
    /// When set to <c>true</c>, the first run of a visual test (or a run after a baseline is deleted)
    /// will establish the current UI state as the baseline. If set to <c>false</c>, a missing baseline
    /// will likely result in a test failure or an error.
    /// This behavior might be overridden or controlled differently in CI environments.
    /// </remarks>
    public bool AutoCreateBaselineIfMissing { get; set; } = true;

    /// <summary>
    /// Gets or sets the default tolerance percentage allowed for pixel differences
    /// when comparing an actual image against a baseline image.
    /// A value of 0 means an exact match is required.
    /// </summary>
    /// <value>
    /// The default comparison tolerance as a percentage (e.g., 0.20 for 0.20%).
    /// Must be between 0 and 100, inclusive. Defaults to 0.20.
    /// </value>
    /// <remarks>
    /// This global tolerance can be overridden on a per-assertion basis if the visual testing
    /// service supports it. Setting an appropriate tolerance helps to avoid false positives
    /// due to minor rendering differences (e.g., anti-aliasing) while still catching significant changes.
    /// The <see cref="RangeAttribute"/> ensures this value is within a valid range.
    /// </remarks>
    [System.ComponentModel.DataAnnotations.Range(0, 100, ErrorMessage = "DefaultComparisonTolerancePercent must be between 0 and 100.")]
    public double DefaultComparisonTolerancePercent { get; set; } = 0.20;

    /// <summary>
    /// Gets or sets a value indicating whether a warning message should be logged
    /// when a baseline image is automatically created because it was missing.
    /// </summary>
    /// <value>
    /// <c>true</c> to log a warning upon automatic baseline creation; otherwise, <c>false</c>.
    /// Defaults to <c>true</c>.
    /// </value>
    /// <remarks>
    /// This setting helps to alert users that new baselines have been established,
    /// prompting them to review and approve these new baselines.
    /// This is particularly useful during initial test development or when UI changes are expected.
    /// </remarks>
    public bool WarnOnAutomaticBaselineCreation { get; set; } = true;
}
