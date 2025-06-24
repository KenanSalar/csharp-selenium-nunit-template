namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specific to the Mozilla Firefox browser as an immutable record,
/// inheriting common browser settings from <see cref="BaseBrowserSettings"/>.
/// </summary>
/// <remarks>
/// This record is typically bound to the "FirefoxBrowserOptions" configuration section (e.g., in appsettings.json)
/// and provides strongly-typed, immutable access to configurations for Firefox.
/// It includes properties for the headless argument (<see cref="FirefoxHeadlessArgument"/>) and a list of
/// custom arguments (<see cref="FirefoxArguments"/>) passed to GeckoDriver on startup.
/// </remarks>
public record FirefoxSettings : BaseBrowserSettings
{
    /// <summary>
    /// Gets the command-line argument used to launch the Firefox browser in headless mode.
    /// </summary>
    /// <value>
    /// The headless mode argument string.
    /// </value>
    [Required(AllowEmptyStrings = false, ErrorMessage = "FirefoxHeadlessArgument is required for Firefox.")]
    public string? FirefoxHeadlessArgument { get; init; } = "--headless";

    /// <summary>
    /// Gets a list of custom command-line arguments to be passed to the
    /// Firefox browser when it is launched by WebDriver (via GeckoDriver).
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/>, where each string is a command-line argument.
    /// </value>
    public List<string> FirefoxArguments { get; init; } = [];

    /// <summary>
    /// Gets a dictionary of profile preferences to apply to the Firefox browser session.
    /// Keys are preference names (e.g., "signon.rememberSignons"), and values are the preference values.
    /// </summary>
    /// <value>A dictionary of Firefox profile preferences.</value>
    public Dictionary<string, object> FirefoxProfilePreferences { get; init; } = [];
}
