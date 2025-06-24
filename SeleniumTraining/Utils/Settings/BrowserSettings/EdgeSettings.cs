namespace SeleniumTraining.Utils.Settings.BrowserSettings;

/// <summary>
/// Defines settings specifically for the Microsoft Edge browser as an immutable record,
/// inheriting common settings from <see cref="ChromiumBasedSettings"/>.
/// </summary>
/// <remarks>
/// This record is bound to the "EdgeBrowserOptions" configuration section. It serves as a dedicated,
/// strongly-typed container for Edge-specific settings, ensuring configuration is immutable once loaded.
/// </remarks>
public record EdgeSettings : ChromiumBasedSettings
{
}
