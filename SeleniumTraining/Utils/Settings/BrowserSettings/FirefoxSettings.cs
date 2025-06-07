namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specific to the Mozilla Firefox browser,
/// inheriting common browser settings from <see cref="BaseBrowserSettings"/>.
/// </summary>
/// <remarks>
/// This class is typically bound to a configuration section (e.g., "FirefoxBrowserOptions" in appsettings.json)
/// and provides strongly-typed access to configurations for Firefox.
/// It includes properties for the specific command-line argument used to launch in headless mode
/// (<see cref="FirefoxHeadlessArgument"/>) and a list of additional custom arguments
/// (<see cref="FirefoxArguments"/>) to be passed to the Firefox instance (via GeckoDriver) on startup.
/// These settings allow for fine-grained control over how Firefox is launched and behaves,
/// which can be important for specialized testing scenarios or CI/CD environments.
/// The <see cref="RequiredAttribute"/> ensures that critical settings like the headless argument are provided.
/// </remarks>
public class FirefoxSettings : BaseBrowserSettings
{
    /// <summary>
    /// Gets or sets the command-line argument used to launch the Firefox browser in headless mode.
    /// </summary>
    /// <value>
    /// The headless mode argument string. Defaults to "--headless".
    /// This property is marked as required.
    /// </value>
    /// <remarks>
    /// Firefox has used various arguments for headless mode over time (e.g., "-headless", "--headless").
    /// This setting allows specifying the correct one for the targeted Firefox version and GeckoDriver.
    /// The <see cref="RequiredAttribute"/> ensures this value is provided in the configuration.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "FirefoxHeadlessArgument is required for Firefox.")]
    public string? FirefoxHeadlessArgument { get; set; } = "--headless";

    /// <summary>
    /// Gets or sets a list of custom command-line arguments to be passed to the
    /// Firefox browser when it is launched by WebDriver (via GeckoDriver).
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/>, where each string is a command-line argument.
    /// Defaults to an empty list.
    /// </value>
    /// <remarks>
    /// This allows for advanced configuration of the browser, such as setting preferences,
    /// specifying profiles, or controlling other browser behaviors.
    /// For example: <c>"-profile /path/to/profile"</c>, <c>"-width 1920"</c>, <c>"-height 1080"</c>.
    /// Arguments should be provided in the format expected by Firefox or GeckoDriver.
    /// </remarks>
    public List<string> FirefoxArguments { get; set; } = [];

    /// <summary>
    /// Gets or sets a dictionary of profile preferences to apply to the Firefox browser session.
    /// Keys are preference names (e.g., "signon.rememberSignons"), and values are the preference values.
    /// </summary>
    /// <value>A dictionary of Firefox profile preferences. Defaults to an empty dictionary.</value>
    /// <remarks>
    /// This allows for fine-grained control over browser features, such as disabling the password manager,
    /// web notifications, or setting download behaviors.
    /// Example: {"signon.rememberSignons": false}
    /// </remarks>
    public Dictionary<string, object> FirefoxProfilePreferences { get; set; } = [];
}
