namespace SeleniumTraining.Utils.Settings;

/// <summary>
/// Defines common settings for the test automation framework,
/// particularly those controlling UI interaction behaviors like element highlighting.
/// </summary>
/// <remarks>
/// This class is typically bound to a configuration section (e.g., "TestFrameworkSettings" in appsettings.json)
/// and provides strongly-typed access to these settings.
/// These settings can influence how tests behave, for example, by enabling visual cues during execution
/// for debugging purposes. In CI/CD environments ([user_input_previous_message_with_filename_programming.ci_cd]), some of these settings (like highlighting) might be
/// configured differently (e.g., disabled to save resources or avoid issues in headless browsers).
/// </remarks>
public class TestFrameworkSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether web elements should be visually highlighted
    /// (e.g., by changing their border or background color) when interacted with by Selenium commands.
    /// </summary>
    /// <value>
    /// <c>true</c> if elements should be highlighted upon interaction; otherwise, <c>false</c>.
    /// The default value depends on the configuration source (e.g., appsettings.json).
    /// </value>
    /// <remarks>
    /// This setting is primarily used for debugging purposes to visually track which elements
    /// the automation script is interacting with. It might be disabled in CI/CD runs for performance.
    /// </remarks>
    public bool HighlightElementsOnInteraction { get; set; }

    /// <summary>
    /// Gets or sets the duration, in milliseconds, for which an element remains highlighted
    /// after an interaction, if <see cref="HighlightElementsOnInteraction"/> is enabled.
    /// </summary>
    /// <value>
    /// The duration of the highlight in milliseconds. Defaults to 250ms if not otherwise specified
    /// in the configuration.
    /// </value>
    /// <remarks>
    /// A short duration provides a visual cue without significantly slowing down test execution.
    /// </remarks>
    public int HighlightDurationMs { get; set; } = 200;

    /// <summary>
    /// Gets or sets the default timeout in seconds for explicit waits (e.g., for WebDriverWait).
    /// </summary>
    /// <value>
    /// The default timeout duration in seconds. Defaults to 10 seconds if not otherwise specified
    /// in the configuration.
    /// </value>
    /// <remarks>
    /// This central setting is used to initialize all <see cref="WebDriverWait"/> instances,
    /// such as in the <see cref="BasePage"/> constructor, ensuring consistent wait times
    /// across the framework. Making this configurable allows for easy adjustment of the framework's
    /// patience based on environment or application performance.
    /// </remarks>
    public int DefaultExplicitWaitSeconds { get; set; } = 10;
}
