namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specifically for the Google Chrome browser,
/// inheriting common Chromium-based settings from <see cref="ChromiumBasedSettings"/>.
/// </summary>
/// <remarks>
/// This class is typically bound to a configuration section (e.g., "ChromeBrowserOptions" in appsettings.json)
/// and provides strongly-typed access to configurations for Chrome.
/// While it currently does not introduce properties beyond those in <see cref="ChromiumBasedSettings"/> ([user_input_previous_message_with_filename_ChromiumBasedSettings.cs]),
/// it serves as a dedicated type for Chrome configurations and can be extended with
/// Chrome-exclusive settings in the future (e.g., specific Chrome extension paths, mobile emulation settings).
/// These settings, along with inherited ones, are used by the <c>ChromeDriverFactoryService</c>
/// to initialize <c>ChromeOptions</c> and the <c>ChromeDriver</c> instance.
/// Configuration can be environment-specific, especially in CI/CD setups.
/// </remarks>
public class ChromeSettings : ChromiumBasedSettings
{
}
