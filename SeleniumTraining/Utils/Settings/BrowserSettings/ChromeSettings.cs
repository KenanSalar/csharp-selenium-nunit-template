namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specifically for the Google Chrome browser as an immutable record,
/// inheriting common settings from <see cref="ChromiumBasedSettings"/>.
/// </summary>
/// <remarks>
/// This record is typically bound to the "ChromeBrowserOptions" configuration section (e.g., in appsettings.json)
/// and provides strongly-typed, immutable access to configurations for Chrome.
/// It serves as a dedicated type for Chrome configurations and can be extended with
/// Chrome-exclusive settings in the future. The settings are used by the <c>ChromeDriverFactoryService</c>
/// to initialize <c>ChromeOptions</c> and the <c>ChromeDriver</c> instance.
/// </remarks>
public record ChromeSettings : ChromiumBasedSettings
{
}
