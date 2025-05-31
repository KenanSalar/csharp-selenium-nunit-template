namespace SeleniumTraining.Core.Services;

public class SettingsProviderService : BaseService, ISettingsProviderService
{
    public IConfiguration Configuration { get; }

    public SettingsProviderService(IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        Logger.LogInformation("{ServiceName} initialized, using IConfiguration instance provided via DI.", nameof(SettingsProviderService));

        if (Configuration is IConfigurationRoot configurationRoot)
        {
            Logger.LogDebug("Listing configuration providers for the injected IConfiguration instance in {ServiceName}:", nameof(SettingsProviderService));

            int providerCount = 0;
            foreach (IConfigurationProvider provider in configurationRoot.Providers)
            {
                providerCount++;
                Logger.LogDebug("  Provider {ProviderNum}: {ProviderType}", providerCount, provider.GetType().Name);
            }
            if (providerCount == 0)
            {
                Logger.LogWarning("No configuration providers found in the injected IConfiguration instance for {ServiceName}.", nameof(SettingsProviderService));
            }
        }
        else
        {
            Logger.LogDebug("Injected IConfiguration in {ServiceName} is not an IConfigurationRoot, cannot list providers.", nameof(SettingsProviderService));
        }
    }

    [AllureStep("Retrieving browser settings")]
    public BaseBrowserSettings GetBrowserSettings(BrowserType browserType)
    {
        string sectionName = browserType switch
        {
            BrowserType.Chrome => "ChromeBrowserOptions",
            BrowserType.Firefox => "FirefoxBrowserOptions",
            // BrowserType.Brave => "BraveBrowserOptions",
            _ => throw new NotSupportedException($"Browser type '{browserType}' is not supported for specific settings.")
        };

        Logger.LogDebug("Attempting to retrieve settings from section '{SettingsSectionName}' for {BrowserType}.", sectionName, browserType);

        BaseBrowserSettings? settings = browserType switch
        {
            BrowserType.Chrome => Configuration.GetSection(sectionName).Get<ChromeSettings>(),
            BrowserType.Firefox => Configuration.GetSection(sectionName).Get<FirefoxSettings>(),
            // BrowserType.Brave => Configuration.GetSection(sectionName).Get<BraveSettings>(),
            _ => null
        };

        if (settings == null)
        {
            Logger.LogError("'{SettingsSectionName}' section not found or could not be bound for {BrowserType}.", sectionName, browserType);
            throw new InvalidOperationException($"'{sectionName}' not found or could not be bound for {browserType} in configuration.");
        }

        string? ciEnvironmentVariable = Environment.GetEnvironmentVariable("CI");
        if (!string.IsNullOrEmpty(ciEnvironmentVariable) && ciEnvironmentVariable.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            if (!settings.Headless)
            {
                Logger.LogInformation("CI environment detected (CI={CIValue}). Overriding Headless mode to true, as configured settings had Headless=false.", ciEnvironmentVariable);
                settings.Headless = true;
            }
            else
            {
                Logger.LogDebug("CI environment detected, and Headless mode is already configured as true in loaded settings.");
            }
        }

        Logger.LogInformation(
            "Successfully retrieved and bound {SettingsSectionName}. Effective Headless: {IsHeadless}, Timeout: {TimeoutSeconds}s, Window: {Width}x{Height}",
            sectionName,
            settings.Headless,
            settings.TimeoutSeconds,
            settings.WindowWidth?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "Default",
            settings.WindowHeight?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "Default"
        );

        return settings;
    }

    [AllureStep("Retrieving settings for section: {sectionName}")]
    public TClassSite GetSettings<TClassSite>(string sectionName) where TClassSite : class
    {
        Logger.LogDebug(
            "Attempting to retrieve settings for section '{ConfigSectionName}' and bind to type {TargetTypeName}.",
            sectionName,
            typeof(TClassSite).Name
        );

        IConfigurationSection settingsSection = Configuration.GetSection(sectionName);
        if (!settingsSection.Exists())
        {
            Logger.LogError("Configuration section '{ConfigSectionName}' not found.", sectionName);
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found.");
        }

        TClassSite? settings = settingsSection.Get<TClassSite>();
        if (settings == null)
        {
            Logger.LogError(
                "Failed to bind configuration section '{ConfigSectionName}' to type {TargetTypeName}.",
                sectionName,
                typeof(TClassSite).Name
            );
            throw new InvalidOperationException($"Failed to bind configuration section '{sectionName}' to type '{typeof(TClassSite).Name}'.");
        }

        Logger.LogInformation(
            "Successfully retrieved and bound settings from section '{ConfigSectionName}' to type {TargetTypeName}.",
            sectionName,
            typeof(TClassSite).Name
        );

        return settings;
    }
}
