namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specific to Chromium-based browsers (e.g., Chrome, Edge) as an immutable record,
/// extending the common <see cref="BaseBrowserSettings"/>.
/// </summary>
/// <remarks>
/// This record is bound to a configuration section for a specific Chromium browser (e.g., "ChromeBrowserOptions").
/// As a record with `init`-only properties, its state is fixed after loading, which prevents accidental changes.
/// It includes properties for the headless mode argument (<see cref="ChromeHeadlessArgument"/>)
/// and a list of custom arguments (<see cref="ChromeArguments"/>) to be passed to the browser on startup.
/// </remarks>
public record ChromiumBasedSettings : BaseBrowserSettings
{
    /// <summary>
    /// Gets the command-line argument used to launch Chromium-based browsers in headless mode.
    /// </summary>
    /// <value>
    /// The headless mode argument string.
    /// </value>
    [Required(AllowEmptyStrings = false, ErrorMessage = "ChromeHeadlessArgument is required for Chromeium based browsers.")]
    public string? ChromeHeadlessArgument { get; init; } = "--headless=new";

    /// <summary>
    /// Gets a list of custom command-line arguments to be passed to the
    /// Chromium-based browser when it is launched by WebDriver.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/>, where each string is a command-line argument.
    /// </value>
    public List<string> ChromeArguments { get; init; } = [];

    /// <summary>
    /// Gets a dictionary of user profile preferences to apply to the browser session.
    /// Keys are preference names (e.g., "credentials_enable_service"), and values are the preference values.
    /// </summary>
    /// <value>A dictionary of user profile preferences.</value>
    public Dictionary<string, object> UserProfilePreferences { get; init; } = [];
}
