using OpenQA.Selenium.Firefox;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Factory service specifically for creating and configuring <see cref="FirefoxDriver"/> (GeckoDriver) instances.
/// </summary>
/// <remarks>
/// This service handles the Firefox-specific setup, including invoking WebDriverManager for GeckoDriver setup,
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
    /// Creates and returns a new <see cref="FirefoxDriver"/> instance configured with the provided settings and options.
    /// </summary>
    /// <remarks>
    /// This method ensures that the provided <paramref name="settingsBase"/> are of type <see cref="FirefoxSettings"/>.
    /// It uses <see cref="WebDriverManager"/> to set up GeckoDriver.
    /// It then configures <see cref="FirefoxOptions"/> by applying headless mode, window size,
    /// custom arguments, and profile preferences from settings before instantiating the <see cref="FirefoxDriver"/>.
    /// </remarks>
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

        ServiceLogger.LogDebug("Attempting to set up GeckoDriver using WebDriverManager (FirefoxConfig).");
        try
        {
            _ = new DriverManager().SetUpDriver(new FirefoxConfig());
            ServiceLogger.LogInformation("WebDriverManager successfully completed GeckoDriver setup (FirefoxConfig).");
        }
        catch (Exception ex)
        {
            ServiceLogger.LogError(ex, "WebDriverManager failed to set up GeckoDriver (FirefoxConfig).");
            throw;
        }

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
                    if (pref.Value is bool boolValue)
                    {
                        firefoxOptions.SetPreference(pref.Key, boolValue);
                    }
                    else if (pref.Value is int intValue)
                    {
                        firefoxOptions.SetPreference(pref.Key, intValue);
                    }
                    else
                    {
                        string stringValue = pref.Value?.ToString() ?? string.Empty;
                        firefoxOptions.SetPreference(pref.Key, stringValue);
                    }
                    appliedOptionsForLog.Add($"pref:{pref.Key}={pref.Value}");
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

        ServiceLogger.LogDebug("Attempting to instantiate new FirefoxDriver with configured options.");
        FirefoxDriver driver;
        try
        {
            driver = new FirefoxDriver(firefoxOptions);
            ServiceLogger.LogInformation("FirefoxDriver instance created successfully. Driver hash: {DriverHashCode}", driver.GetHashCode());

            PerformVersionCheck(driver, Type.ToString(), _minimumSupportedVersion); // From DriverFactoryServiceBase
            return driver;
        }
        catch (Exception ex)
        {
            LogAndThrowWebDriverCreationError(ex, Type, firefoxOptions, "While creating Firefox driver.");
            if (ex is UnsupportedBrowserVersionException) throw;
            throw;
        }
    }
}
