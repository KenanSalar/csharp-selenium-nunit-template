namespace SeleniumTraining.Utils.Settings;

/// <summary>
/// Defines common settings for the test automation framework as an immutable record,
/// particularly those controlling UI interaction behaviors like element highlighting.
/// </summary>
/// <remarks>
/// This record is typically bound to the "TestFrameworkSettings" configuration section (e.g., in appsettings.json).
/// As an immutable record, its configuration is fixed upon loading, ensuring consistent behavior throughout the test run.
/// These settings can influence how tests behave, for example, by enabling visual cues during execution
/// for debugging purposes.
/// </remarks>
public record TestFrameworkSettings
{
    /// <summary>
    /// Gets a value indicating whether web elements should be visually highlighted
    /// (e.g., by changing their border or background color) when interacted with by Selenium commands.
    /// </summary>
    /// <value>
    /// <c>true</c> if elements should be highlighted upon interaction; otherwise, <c>false</c>.
    /// </value>
    public bool HighlightElementsOnInteraction { get; init; }

    /// <summary>
    /// Gets the duration, in milliseconds, for which an element remains highlighted
    /// after an interaction, if <see cref="HighlightElementsOnInteraction"/> is enabled.
    /// </summary>
    /// <value>
    /// The duration of the highlight in milliseconds.
    /// </value>
    public int HighlightDurationMs { get; init; } = 200;

    /// <summary>
    /// Gets the default timeout in seconds for explicit waits (e.g., for WebDriverWait).
    /// </summary>
    /// <value>
    /// The default timeout duration in seconds.
    /// </value>
    public int DefaultExplicitWaitSeconds { get; init; } = 10;
}
