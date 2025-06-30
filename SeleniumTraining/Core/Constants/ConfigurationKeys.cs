namespace SeleniumTraining.Core.Constants;

/// <summary>
/// Provides constant string values for configuration section keys
/// to avoid "magic strings" and ensure consistency when accessing settings.
/// </summary>
public static class ConfigurationKeys
{
    // Top-level keys in the appsettings.json files
    public const string TestFrameworkSettings = "TestFrameworkSettings";
    public const string RetryPolicySettings = "RetryPolicySettings";
    public const string VisualTestSettings = "VisualTestSettings";
    public const string SeleniumGrid = "SeleniumGrid";
    public const string SauceDemo = "SauceDemo";

    // Browser-specific settings sections
    public const string ChromeBrowserOptions = "ChromeBrowserOptions";
    public const string EdgeBrowserOptions = "EdgeBrowserOptions";
    public const string FirefoxBrowserOptions = "FirefoxBrowserOptions";
}
