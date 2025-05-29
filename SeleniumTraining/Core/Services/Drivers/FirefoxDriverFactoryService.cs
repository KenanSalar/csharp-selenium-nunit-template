using OpenQA.Selenium.Firefox;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

public class FirefoxDriverFactoryService : DriverFactoryServiceBase, IBrowserDriverFactoryService // Inherit from DriverFactoryServiceBase
{
    public BrowserType Type => BrowserType.Firefox;
    private static readonly Version _minimumSupportedVersion = new("110.0");

    public FirefoxDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(FirefoxDriverFactoryService), Type);
    }

    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not FirefoxSettings settings)
        {
            var ex = new ArgumentException($"Invalid settings type provided. Expected {nameof(FirefoxSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
            Logger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(FirefoxDriverFactoryService));
            throw ex;
        }

        Logger.LogInformation(
            "Creating FirefoxDriver (GeckoDriver). Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if applicable).",
            settings.Headless, settings.WindowWidth ?? -1, settings.WindowHeight ?? -1);

        Logger.LogDebug("Attempting to set up GeckoDriver using WebDriverManager (FirefoxConfig).");
        try
        {
            _ = new DriverManager().SetUpDriver(new FirefoxConfig());
            Logger.LogInformation("WebDriverManager successfully completed GeckoDriver setup (FirefoxConfig).");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "WebDriverManager failed to set up GeckoDriver (FirefoxConfig).");
            throw;
        }

        FirefoxOptions firefoxOptions = options as FirefoxOptions ?? new FirefoxOptions();
        Logger.LogDebug("Initialized FirefoxOptions. Base options type: {OptionsBaseType}",
            firefoxOptions.GetType().BaseType?.Name ?? firefoxOptions.GetType().Name);

        var appliedOptionsForLog = new List<string>();

        if (settings.Headless)
        {
            string headlessArg = !string.IsNullOrEmpty(settings.FirefoxHeadlessArgument)
                ? settings.FirefoxHeadlessArgument
                : "-headless";
            firefoxOptions.AddArgument(headlessArg);
            appliedOptionsForLog.Add(headlessArg);
            Logger.LogDebug("Applied headless argument for {BrowserType}: '{HeadlessArgument}'", Type, headlessArg);
        }

        if (settings.WindowWidth.HasValue && settings.WindowHeight.HasValue)
        {
            bool sizeAlreadyInCustomArgs = settings.FirefoxArguments?.Any(arg => arg.StartsWith("--width=", StringComparison.Ordinal) || arg.StartsWith("--height=", StringComparison.Ordinal)) ?? false;
            if (!sizeAlreadyInCustomArgs)
            {
                string widthArg = $"--width={settings.WindowWidth.Value}";
                string heightArg = $"--height={settings.WindowHeight.Value}";
                firefoxOptions.AddArgument(widthArg);
                firefoxOptions.AddArgument(heightArg);
                appliedOptionsForLog.AddRange([widthArg, heightArg]);
                Logger.LogDebug("Applied direct window size arguments: {WidthArg}, {HeightArg}", widthArg, heightArg);
            }
            else
            {
                 Logger.LogDebug("Window size arguments (--width/--height) found in custom FirefoxArguments. Skipping direct application.");
            }
        }

        if (settings.FirefoxArguments != null && settings.FirefoxArguments.Count != 0)
        {
            Logger.LogDebug("Applying {ArgCount} Firefox arguments from configuration settings.", settings.FirefoxArguments.Count);
            foreach (string arg in settings.FirefoxArguments)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    firefoxOptions.AddArgument(arg);
                    appliedOptionsForLog.Add(arg);
                    Logger.LogTrace("Applied Firefox argument from settings: '{FirefoxArgument}'", arg);
                }
            }
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            Logger.LogWarning("LeaveBrowserOpenAfterTest=true is set, but FirefoxDriver does not have a direct 'LeaveBrowserRunning' equivalent. Manual teardown control would be needed.");
        }

        Logger.LogInformation(
            "FirefoxOptions configured. Effective arguments: [{EffectiveArgs}]",
            string.Join(", ", appliedOptionsForLog.Distinct()));

        Logger.LogDebug("Attempting to instantiate new FirefoxDriver with configured options.");
        FirefoxDriver driver;
        try
        {
            driver = new FirefoxDriver(firefoxOptions);
            Logger.LogInformation("FirefoxDriver instance created successfully. Driver hash: {DriverHashCode}", driver.GetHashCode());

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
