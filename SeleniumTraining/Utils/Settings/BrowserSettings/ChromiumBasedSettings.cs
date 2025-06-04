namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specific to Chromium-based browsers (e.g., Chrome, Edge),
/// extending the common <see cref="BaseBrowserSettings"/>.
/// </summary>
/// <remarks>
/// This class is typically bound to a configuration section for a specific Chromium browser
/// (e.g., "ChromeBrowserOptions", "EdgeBrowserOptions" in appsettings.json).
/// It includes properties for the specific command-line argument used to launch in headless mode
/// (<see cref="ChromeHeadlessArgument"/>) and a list of additional custom arguments
/// (<see cref="ChromeArguments"/>) to be passed to the browser instance on startup.
/// These settings allow for fine-grained control over how Chromium browsers are launched and behave,
/// which can be important for specialized testing scenarios or CI/CD environments .
/// The <see cref="RequiredAttribute"/> ensures that critical settings like the headless argument are provided.
/// </remarks>
public class ChromiumBasedSettings : BaseBrowserSettings
{
    /// <summary>
    /// Gets or sets the command-line argument used to launch Chromium-based browsers in headless mode.
    /// </summary>
    /// <value>
    /// The headless mode argument string. Defaults to "--headless=new".
    /// This property is marked as required.
    /// </value>
    /// <remarks>
    /// Different versions or configurations of Chromium might use different headless arguments
    /// (e.g., "--headless", "--headless=new"). This setting allows specifying the correct one.
    /// The <see cref="RequiredAttribute"/> ensures this value is provided in the configuration.
    /// </remarks>
    [Required(AllowEmptyStrings = false, ErrorMessage = "ChromeHeadlessArgument is required for Chromeium based browsers.")]
    public string? ChromeHeadlessArgument { get; set; } = "--headless=new";

    /// <summary>
    /// Gets or sets a list of custom command-line arguments to be passed to the
    /// Chromium-based browser when it is launched by WebDriver.
    /// </summary>
    /// <value>
    /// A <see cref="List{T}"/> of <see cref="string"/>, where each string is a command-line argument.
    /// Defaults to an empty list.
    /// </value>
    /// <remarks>
    /// This allows for advanced configuration of the browser, such as enabling experimental features,
    /// setting proxy servers, disabling GPU acceleration, or specifying user data directories.
    /// For example: <c>"--disable-gpu"</c>, <c>"--window-size=1920,1080"</c>.
    /// Arguments should be provided in the format expected by the browser executable.
    /// </remarks>
    public List<string> ChromeArguments { get; set; } = [];
}
