namespace SeleniumTraining.Core.Services;

/// <summary>
/// Provides access to application settings and configurations by interacting
/// with the <see cref="IConfiguration"/> system.
/// </summary>
/// <remarks>
/// This service implements <see cref="ISettingsProviderService"/> and is responsible for
/// retrieving strongly-typed settings objects from configuration sources (e.g., appsettings.json).
/// It handles browser-specific settings and generic configuration section retrieval.
/// An important feature is the automatic override of 'Headless' mode to true when a CI environment
/// is detected, ensuring tests run headlessly in CI/CD pipelines.
/// </remarks>
public class SettingsProviderService : BaseService, ISettingsProviderService
{
    /// <inheritdoc cref="ISettingsProviderService.Configuration" />
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsProviderService"/> class.
    /// </summary>
    /// <param name="configuration">The root <see cref="IConfiguration"/> instance, typically injected via DI.</param>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers.</param>
    public SettingsProviderService(IConfiguration configuration, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        ServiceLogger.LogInformation("{ServiceName} initialized, using IConfiguration instance provided via DI.", nameof(SettingsProviderService));

        if (Configuration is IConfigurationRoot configurationRoot)
        {
            ServiceLogger.LogDebug("Listing configuration providers for the injected IConfiguration instance in {ServiceName}:", nameof(SettingsProviderService));

            int providerCount = 0;
            foreach (IConfigurationProvider provider in configurationRoot.Providers)
            {
                providerCount++;
                ServiceLogger.LogDebug("  Provider {ProviderNum}: {ProviderType}", providerCount, provider.GetType().Name);
            }
            if (providerCount == 0)
            {
                ServiceLogger.LogWarning("No configuration providers found in the injected IConfiguration instance for {ServiceName}.", nameof(SettingsProviderService));
            }
        }
        else
        {
            ServiceLogger.LogDebug("Injected IConfiguration in {ServiceName} is not an IConfigurationRoot, cannot list providers.", nameof(SettingsProviderService));
        }
    }

    /// <summary>
    /// Retrieves browser-specific settings based on the provided <see cref="BrowserType"/>.
    /// </summary>
    /// <param name="browserType">The type of browser for which to retrieve settings.</param>
    /// <returns>A <see cref="BaseBrowserSettings"/> object containing the configuration for the specified browser.</returns>
    /// <exception cref="NotSupportedException">Thrown if the provided <paramref name="browserType"/> is not configured in the service.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the configuration section for the browser settings is not found or is invalid.</exception>
    /// <remarks>
    /// This implementation maps the <paramref name="browserType"/> to a specific configuration section name
    /// (e.g., "ChromeBrowserOptions", "EdgeBrowserOptions") and binds the settings to the correct type.
    /// It also automatically applies Selenium Grid and CI-specific settings.
    /// </remarks>
    [AllureStep("Retrieving browser settings for {browserType}")]
    public BaseBrowserSettings GetBrowserSettings(BrowserType browserType)
    {
        string sectionName = browserType switch
        {
            BrowserType.Chrome => "ChromeBrowserOptions",
            BrowserType.Edge => "EdgeBrowserOptions",
            BrowserType.Firefox => "FirefoxBrowserOptions",
            // BrowserType.Brave => "BraveBrowserOptions",
            _ => throw new NotSupportedException($"Browser type '{browserType}' is not supported for specific settings.")
        };

        ServiceLogger.LogDebug("Attempting to retrieve settings from section '{SettingsSectionName}' for {BrowserType}.", sectionName, browserType);

        BaseBrowserSettings? settings = browserType switch
        {
            BrowserType.Chrome => Configuration.GetSection(sectionName).Get<ChromeSettings>(),
            BrowserType.Edge => Configuration.GetSection(sectionName).Get<EdgeSettings>(),
            BrowserType.Firefox => Configuration.GetSection(sectionName).Get<FirefoxSettings>(),
            // BrowserType.Brave => Configuration.GetSection(sectionName).Get<BraveSettings>(),
            _ => null
        };

        if (settings == null)
        {
            ServiceLogger.LogError("'{SettingsSectionName}' section not found or could not be bound for {BrowserType}.", sectionName, browserType);
            throw new InvalidOperationException($"'{sectionName}' not found or could not be bound for {browserType} in configuration.");
        }

        try
        {
            SeleniumGridSettings gridSettings = GetSettings<SeleniumGridSettings>("SeleniumGrid");
            if (gridSettings != null && gridSettings.Enabled)
            {
                ServiceLogger.LogInformation("Selenium Grid is enabled. Setting remote URL to: {GridUrl}", gridSettings.Url);
                settings.SeleniumGridUrl = gridSettings.Url;
            }
        }
        catch (Exception ex)
        {
            ServiceLogger.LogWarning(ex, "Could not load SeleniumGrid settings. Assuming Grid is disabled.");
        }

        string? ciEnvironmentVariable = Environment.GetEnvironmentVariable("CI");
        if (!string.IsNullOrEmpty(ciEnvironmentVariable) && ciEnvironmentVariable.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            if (!settings.Headless)
            {
                ServiceLogger.LogInformation("CI environment detected (CI={CIValue}). Overriding Headless mode to true, as configured settings had Headless=false.", ciEnvironmentVariable);
                settings.Headless = true;
            }
            else
            {
                ServiceLogger.LogDebug("CI environment detected, and Headless mode is already configured as true in loaded settings.");
            }
        }

        ServiceLogger.LogInformation(
            "Successfully retrieved and bound {SettingsSectionName}. Effective Headless: {IsHeadless}, Timeout: {TimeoutSeconds}s, Window: {Width}x{Height}",
            sectionName,
            settings.Headless,
            settings.TimeoutSeconds,
            settings.WindowWidth?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "Default",
            settings.WindowHeight?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "Default"
        );

        return settings;
    }

    /// <summary>
    /// Retrieves and deserializes a configuration section into an instance of the specified class <typeparamref name="TClassSite"/>.
    /// </summary>
    /// <typeparam name="TClassSite">The type of the class to deserialize the configuration section into.</typeparam>
    /// <param name="sectionName">The name (key) of the configuration section to retrieve.</param>
    /// <returns>An instance of <typeparamref name="TClassSite"/> populated with values from the configuration.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the section is not found or cannot be bound to the specified type.</exception>
    [AllureStep("Retrieving settings for section: {sectionName}")]
    public TClassSite GetSettings<TClassSite>(string sectionName) where TClassSite : class
    {
        ServiceLogger.LogDebug(
            "Attempting to retrieve settings for section '{ConfigSectionName}' and bind to type {TargetTypeName}.",
            sectionName,
            typeof(TClassSite).Name
        );

        IConfigurationSection settingsSection = Configuration.GetSection(sectionName);
        if (!settingsSection.Exists())
        {
            ServiceLogger.LogError("Configuration section '{ConfigSectionName}' not found.", sectionName);
            throw new InvalidOperationException($"Configuration section '{sectionName}' not found.");
        }

        TClassSite? settings = settingsSection.Get<TClassSite>();
        if (settings == null)
        {
            ServiceLogger.LogError(
                "Failed to bind configuration section '{ConfigSectionName}' to type {TargetTypeName}.",
                sectionName,
                typeof(TClassSite).Name
            );
            throw new InvalidOperationException($"Failed to bind configuration section '{sectionName}' to type '{typeof(TClassSite).Name}'.");
        }

        ServiceLogger.LogInformation(
            "Successfully retrieved and bound settings from section '{ConfigSectionName}' to type {TargetTypeName}.",
            sectionName,
            typeof(TClassSite).Name
        );

        return settings;
    }
}
