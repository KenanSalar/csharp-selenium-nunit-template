using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Factory service specifically for creating and configuring <see cref="FirefoxDriver"/> (GeckoDriver) instances.
/// </summary>
/// <remarks>
/// This service handles the Firefox-specific setup for GeckoDriver setup,
/// configuring <see cref="FirefoxOptions"/> with common and Firefox-specific settings,
/// and instantiating the <see cref="FirefoxDriver"/>. It implements <see cref="IBrowserDriverFactoryService"/>
/// and inherits common factory functionalities from <see cref="DriverFactoryServiceBase"/>.
/// </remarks>
public class FirefoxDriverFactoryService : DriverFactoryServiceBase, IBrowserDriverFactoryService
{
    /// <summary>
    /// Gets the browser type this factory is responsible for, which is always <see cref="BrowserType.Firefox"/>.
    /// </summary>
    /// <inheritdoc cref="IBrowserDriverFactoryService.Type" />
    public BrowserType Type => BrowserType.Firefox;

    /// <summary>
    /// The minimum supported version for the Mozilla Firefox browser handled by this factory.
    /// </summary>
    private static readonly Version _minimumSupportedVersion = new("110.0");

    /// <summary>
    /// Initializes a new instance of the <see cref="FirefoxDriverFactoryService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The factory used to create loggers, passed to the base class.</param>
    public FirefoxDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        ServiceLogger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(FirefoxDriverFactoryService), Type);
    }

    /// <summary>
    /// Creates and configures a WebDriver instance for Firefox, supporting both local and remote (Selenium Grid) execution.
    /// </summary>
    /// <remarks>
    /// This method orchestrates the creation of a Firefox WebDriver by following these steps:
    /// <list type="number">
    ///   <item><description>Validates that the provided settings are of type <see cref="FirefoxSettings"/>.</description></item>
    ///   <item><description>Configures <see cref="FirefoxOptions"/> with common settings (headless, window size, etc.) and any custom arguments or profile preferences.</description></item>
    ///   <item><description>Checks if a <see cref="BaseBrowserSettings.SeleniumGridUrl"/> is specified in the settings.</description></item>
    ///   <item><description><b>If a Grid URL is present</b>, it creates a <see cref="RemoteWebDriver"/> instance pointing to the grid.</description></item>
    ///   <item><description><b>If no Grid URL is present</b>, it uses <see cref="WebDriverManager"/> to set up the local `geckodriver.exe` and then creates a local <see cref="FirefoxDriver"/> instance.</description></item>
    ///   <item><description>Performs a browser version check against the minimum supported version.</description></item>
    /// </list>
    /// </remarks>
    /// <param name="settingsBase">The browser settings to use for driver creation. Must be of type <see cref="FirefoxSettings"/>.</param>
    /// <param name="options">Optional additional driver options to apply. Currently not used in Firefox implementation.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="settingsBase"/> is not of type <see cref="FirefoxSettings"/>.</exception>
    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not FirefoxSettings settings)
        {
            var ex = new ArgumentException($"Invalid settings type provided. Expected {nameof(FirefoxSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
            ServiceLogger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(FirefoxDriverFactoryService));
            throw ex;
        }

        ServiceLogger.LogInformation(
            "Creating FirefoxDriver (GeckoDriver). Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if applicable).",
            settings.Headless,
            settings.WindowWidth ?? -1,
            settings.WindowHeight ?? -1
        );

        FirefoxOptions firefoxOptions = options as FirefoxOptions ?? new FirefoxOptions();
        ServiceLogger.LogDebug("Initialized FirefoxOptions. Base options type: {OptionsBaseType}",
            firefoxOptions.GetType().BaseType?.Name ?? firefoxOptions.GetType().Name);

        var appliedOptionsForLog = new List<string>();

        if (settings.Headless)
        {
            string headlessArg = !string.IsNullOrEmpty(settings.FirefoxHeadlessArgument)
                ? settings.FirefoxHeadlessArgument
                : "-headless";
            firefoxOptions.AddArgument(headlessArg);
            appliedOptionsForLog.Add(headlessArg);
            ServiceLogger.LogDebug("Applied headless argument for {BrowserType}: '{HeadlessArgument}'", Type, headlessArg);
        }

        if (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
        {
            bool sizeAlreadyInCustomArgs = settings.FirefoxArguments?.Any(arg =>
                arg.StartsWith("--width=",
                StringComparison.Ordinal) || arg.StartsWith("--height=",
                StringComparison.Ordinal)
            ) ?? false;

            if (!sizeAlreadyInCustomArgs)
            {
                string widthArg = $"--width={settings.WindowWidth.Value}";
                string heightArg = $"--height={settings.WindowHeight.Value}";
                firefoxOptions.AddArgument(widthArg);
                firefoxOptions.AddArgument(heightArg);
                appliedOptionsForLog.AddRange([widthArg, heightArg]);
                ServiceLogger.LogDebug("Applied direct window size arguments: {WidthArg}, {HeightArg}", widthArg, heightArg);
            }
            else
            {
                ServiceLogger.LogDebug("Window size arguments (--width/--height) found in custom FirefoxArguments. Skipping direct application.");
            }
        }

        if (settings.FirefoxArguments != null && settings.FirefoxArguments.Count != 0)
        {
            ServiceLogger.LogDebug("Applying {ArgCount} Firefox arguments from settings.", settings.FirefoxArguments.Count);
            foreach (string arg in settings.FirefoxArguments)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    firefoxOptions.AddArgument(arg);
                    appliedOptionsForLog.Add(arg);
                }
            }
        }

        if (settings.FirefoxProfilePreferences != null && settings.FirefoxProfilePreferences.Count != 0)
        {
            ServiceLogger.LogDebug("Applying {PrefCount} Firefox profile preferences.", settings.FirefoxProfilePreferences.Count);
            foreach (KeyValuePair<string, object> pref in settings.FirefoxProfilePreferences)
            {
                try
                {
                    string key = pref.Key;
                    string? stringValue = pref.Value?.ToString();

                    if (stringValue is null)
                    {
                        ServiceLogger.LogWarning("Skipping Firefox profile preference '{PrefKey}' because its value is null.", key);
                        continue;
                    }

                    if (bool.TryParse(stringValue, out bool boolResult))
                    {
                        firefoxOptions.SetPreference(key, boolResult);
                    }
                    else if (int.TryParse(stringValue, out int intResult))
                    {
                        firefoxOptions.SetPreference(key, intResult);
                    }
                    else
                    {
                        firefoxOptions.SetPreference(key, stringValue);
                    }

                    appliedOptionsForLog.Add($"pref:{key}={stringValue}");
                }
                catch (Exception ex)
                {
                    ServiceLogger.LogError(ex, "Failed to apply Firefox preference '{PrefKey}' with value '{PrefValue}'.", pref.Key, pref.Value);
                }
            }
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            ServiceLogger.LogWarning("LeaveBrowserOpenAfterTest=true is set, but FirefoxDriver does not have a direct 'LeaveBrowserRunning' equivalent. Manual teardown control would be needed.");
        }

        ServiceLogger.LogInformation(
            "FirefoxOptions configured. Effective arguments: [{EffectiveArgs}]",
            string.Join(", ", appliedOptionsForLog.Distinct()));

        if (string.IsNullOrEmpty(settings.SeleniumGridUrl))
        {
            ServiceLogger.LogInformation("Creating local FirefoxDriver instance. Selenium Manager will ensure the driver is available.");

            var localDriver = new FirefoxDriver(firefoxOptions);
            PerformVersionCheck(localDriver, Type.ToString(), _minimumSupportedVersion);

            return localDriver;
        }

        ServiceLogger.LogInformation("Creating RemoteWebDriver instance for Firefox Grid at {GridUrl}", settings.SeleniumGridUrl);

        var remoteDriver = new RemoteWebDriver(new Uri(settings.SeleniumGridUrl), firefoxOptions);
        PerformVersionCheck(remoteDriver, Type.ToString(), _minimumSupportedVersion);

        return remoteDriver;
    }
}
