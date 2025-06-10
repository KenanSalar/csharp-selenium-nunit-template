using OpenQA.Selenium.Chromium;

namespace SeleniumTraining.Core.Services.Drivers;

public abstract class ChromiumDriverFactoryServiceBase : DriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public abstract BrowserType Type { get; }
    protected abstract Version MinimumSupportedVersion { get; }

    protected ChromiumDriverFactoryServiceBase(ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        
    }

    public abstract IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null);

    protected TDriver CreateDriverInstanceWithChecks<TDriver, TOptions>(
        TOptions driverOptions,
        Func<TOptions, TDriver> driverFactory
    )
        where TDriver : ChromiumDriver
        where TOptions : ChromiumOptions
    {
        ServiceLogger.LogDebug("Attempting to instantiate new {DriverType} with configured options.", typeof(TDriver).Name);
        TDriver driver;
        try
        {
            driver = driverFactory(driverOptions);
            ServiceLogger.LogInformation(
                "{BrowserType} WebDriver ({DriverType}) instance created successfully. Driver hash: {DriverHashCode}",
                Type, typeof(TDriver).Name, driver.GetHashCode()
            );
            PerformVersionCheck(driver, Type.ToString(), MinimumSupportedVersion);
            return driver;
        }
        catch (Exception ex)
        {
            LogAndThrowWebDriverCreationError(ex, Type, driverOptions, $"While creating {Type} driver.");
            throw;
        }
    }

    protected TChromiumOptions ConfigureCommonChromiumOptions<TChromiumOptions>(
        ChromiumBasedSettings settings,
        DriverOptions? baseOptions,
        out List<string> appliedOptionsForLog
    )
        where TChromiumOptions : ChromiumOptions, new()
    {
        appliedOptionsForLog = [];
        TChromiumOptions chromiumOptions = baseOptions as TChromiumOptions ?? new TChromiumOptions();
        ServiceLogger.LogDebug("Initialized {OptionsType} for {BrowserType}.", typeof(TChromiumOptions).Name, Type);

        string windowSizeArgument = GetWindowSizeArgumentInternal(settings);
        if (!string.IsNullOrEmpty(windowSizeArgument))
        {
            chromiumOptions.AddArgument(windowSizeArgument);
            appliedOptionsForLog.Add(windowSizeArgument);
        }

        if (settings.Headless && !string.IsNullOrEmpty(settings.ChromeHeadlessArgument))
        {
            chromiumOptions.AddArgument(settings.ChromeHeadlessArgument);
            appliedOptionsForLog.Add(settings.ChromeHeadlessArgument);
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            chromiumOptions.AddAdditionalOption("detach", true);
            ServiceLogger.LogWarning("DEBUGGING: {BrowserType} browser will be left running after the test.", Type);
        }

        if (settings.ChromeArguments != null && settings.ChromeArguments.Count != 0)
        {
            ServiceLogger.LogDebug("Applying {ArgCount} custom Chrome arguments from settings.", settings.ChromeArguments.Count);
            foreach (string arg in settings.ChromeArguments)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    chromiumOptions.AddArgument(arg);
                    appliedOptionsForLog.Add(arg);
                }
            }
        }

        return chromiumOptions;
    }

    /// <summary>
    /// Generates the window size command-line argument string (e.g., "--window-size=1920,1080")
    /// based on the width and height specified in the settings.
    /// </summary>
    /// <param name="settings">The <see cref="BaseBrowserSettings"/> (or derived type like <see cref="ChromiumBasedSettings"/>)
    /// containing <see cref="BaseBrowserSettings.WindowWidth"/> and <see cref="BaseBrowserSettings.WindowHeight"/>.</param>
    /// <returns>The formatted window size argument string, or an empty string if width or height is not specified.</returns>
    protected static string GetWindowSizeArgumentInternal(BaseBrowserSettings settings)
    {
        return settings.WindowWidth.HasValue && settings.WindowHeight.HasValue
            ? $"--window-size={settings.WindowWidth.Value},{settings.WindowHeight.Value}"
            : string.Empty;
    }
}
