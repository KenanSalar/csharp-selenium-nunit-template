namespace SeleniumTraining.Core.Services;

/// <summary>
/// Provides access to application settings and configurations by interacting
/// with the <see cref="IConfiguration"/> system.
/// </summary>
/// <remarks>
/// This service implements <see cref="ISettingsProviderService"/> ([2]) and is responsible for
/// retrieving strongly-typed settings objects from configuration sources (e.g., appsettings.json).
/// It handles browser-specific settings and generic configuration section retrieval.
/// An important feature is the automatic override of 'Headless' mode to true when a CI environment
/// is detected (via the "CI" environment variable), ensuring tests run headlessly in CI/CD pipelines ([3]).
/// This class inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class SettingsProviderService : BaseService, ISettingsProviderService
{
    /// <inheritdoc cref="ISettingsProviderService.Configuration" />
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsProviderService"/> class.
    /// </summary>
    /// <param name="configuration">The root <see cref="IConfiguration"/> instance, typically injected via DI. Must not be null.</param>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="configuration"/> or <paramref name="loggerFactory"/> is null.</exception>
    /// <remarks>
    /// Upon construction, this service stores the provided <see cref="IConfiguration"/> instance.
    /// It also logs information about the registered configuration providers if the <paramref name="configuration"/>
    /// instance is an <see cref="IConfigurationRoot"/>, which can be useful for debugging configuration issues.
    /// </remarks>
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

    /// <inheritdoc cref="ISettingsProviderService.GetBrowserSettings(BrowserType)" />
    /// <remarks>
    /// This implementation maps the <paramref name="browserType"/> to a specific configuration section name
    /// (e.g., "ChromeBrowserOptions", "FirefoxBrowserOptions"). It then attempts to retrieve and bind
    /// this section to the corresponding settings class (e.g., <see cref="ChromeSettings"/>, <see cref="FirefoxSettings"/>).
    /// Crucially, if a CI environment is detected (via the "CI" environment variable being "true"),
    /// it will override the <c>Headless</c> property of the retrieved settings to <c>true</c>,
    /// ensuring tests run headlessly in CI/CD pipelines. This behavior is logged.
    /// </remarks>
    [AllureStep("Retrieving browser settings for {browserType}")]
    public BaseBrowserSettings GetBrowserSettings(BrowserType browserType)
    {
        string sectionName = browserType switch
        {
            BrowserType.Chrome => "ChromeBrowserOptions",
            BrowserType.Firefox => "FirefoxBrowserOptions",
            // BrowserType.Brave => "BraveBrowserOptions",
            _ => throw new NotSupportedException($"Browser type '{browserType}' is not supported for specific settings.")
        };

        ServiceLogger.LogDebug("Attempting to retrieve settings from section '{SettingsSectionName}' for {BrowserType}.", sectionName, browserType);

        BaseBrowserSettings? settings = browserType switch
        {
            BrowserType.Chrome => Configuration.GetSection(sectionName).Get<ChromeSettings>(),
            BrowserType.Firefox => Configuration.GetSection(sectionName).Get<FirefoxSettings>(),
            // BrowserType.Brave => Configuration.GetSection(sectionName).Get<BraveSettings>(),
            _ => null
        };

        if (settings == null)
        {
            ServiceLogger.LogError("'{SettingsSectionName}' section not found or could not be bound for {BrowserType}.", sectionName, browserType);
            throw new InvalidOperationException($"'{sectionName}' not found or could not be bound for {browserType} in configuration.");
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

    /// <inheritdoc cref="ISettingsProviderService.GetSettings{TClassSite}(string)" />
    /// <remarks>
    /// This implementation uses <see cref="IConfiguration.GetSection(string)"/> to retrieve the specified
    /// configuration section. It then checks if the section exists. If it does, it attempts to bind
    /// the section to an instance of <typeparamref name="TClassSite"/> using <c>section.Get&lt;TClassSite&gt;()</c>.
    /// Appropriate exceptions are thrown if the section is not found or if binding fails.
    /// Detailed logging of the process is performed.
    /// </remarks>
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
