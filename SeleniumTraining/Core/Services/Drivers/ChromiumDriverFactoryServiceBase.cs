using OpenQA.Selenium.Chrome;

namespace SeleniumTraining.Core.Services.Drivers;

public abstract class ChromiumDriverFactoryServiceBase : DriverFactoryServiceBase
{
    protected abstract BrowserType ConcreteBrowserType { get; }
    protected abstract Version MinimumSupportedVersion { get; }

    protected ChromiumDriverFactoryServiceBase(ILoggerFactory loggerFactory)
        : base(loggerFactory) { }

    protected virtual ChromeOptions ConfigureCommonChromeOptions(
        ChromiumBasedSettings settings,
        DriverOptions? baseOptions,
        out List<string> appliedOptionsForLog
    )
    {
        appliedOptionsForLog = [];
        ChromeOptions chromeOptions = baseOptions as ChromeOptions ?? new ChromeOptions();
        ServiceLogger.LogDebug("Initialized ChromeOptions for {BrowserType}. Base options type: {OptionsBaseType}",
            ConcreteBrowserType, chromeOptions.GetType().BaseType?.Name ?? chromeOptions.GetType().Name);

        string windowSizeArgument = GetWindowSizeArgumentInternal(settings);
        if (!string.IsNullOrEmpty(windowSizeArgument))
        {
            chromeOptions.AddArgument(windowSizeArgument);
            appliedOptionsForLog.Add(windowSizeArgument);
            ServiceLogger.LogDebug("Applied window size argument for {BrowserType}: '{WindowSizeArgument}'", ConcreteBrowserType, windowSizeArgument);
        }

        if (settings.Headless && !string.IsNullOrEmpty(settings.ChromeHeadlessArgument))
        {
            chromeOptions.AddArgument(settings.ChromeHeadlessArgument);
            appliedOptionsForLog.Add(settings.ChromeHeadlessArgument);
            ServiceLogger.LogDebug("Applied headless argument for {BrowserType}: '{HeadlessArgument}'", ConcreteBrowserType, settings.ChromeHeadlessArgument);
        }

        if (settings.LeaveBrowserOpenAfterTest)
        {
            chromeOptions.LeaveBrowserRunning = true;
            ServiceLogger.LogWarning("DEBUGGING: {BrowserType} browser will be left running after the test due to LeaveBrowserOpenAfterTest=true setting.", ConcreteBrowserType);
        }

        if (settings.ChromeArguments != null && settings.ChromeArguments.Count != 0)
        {
            ServiceLogger.LogDebug(
                "Applying {ArgCount} custom Chrome arguments from configuration settings for {BrowserType}.",
                settings.ChromeArguments.Count,
                ConcreteBrowserType
            );
            foreach (string arg in settings.ChromeArguments)
            {
                if (!string.IsNullOrWhiteSpace(arg))
                {
                    chromeOptions.AddArgument(arg);
                    appliedOptionsForLog.Add(arg);
                    ServiceLogger.LogTrace("Applied Chrome argument from settings for {BrowserType}: '{ChromeArgument}'", ConcreteBrowserType, arg);
                }
            }
        }
        return chromeOptions;
    }

    protected ChromeDriver CreateDriverInstanceWithChecks(ChromeOptions chromeOptions)
    {
        ServiceLogger.LogDebug("Attempting to instantiate new ChromeDriver (for {BrowserType}) with configured options.", ConcreteBrowserType);
        ChromeDriver driver;
        try
        {
            driver = new ChromeDriver(chromeOptions);

            ServiceLogger.LogInformation(
                "{BrowserType} WebDriver (via ChromeDriver) instance created successfully. Driver hash: {DriverHashCode}",
                ConcreteBrowserType,
                driver.GetHashCode()
            );

            PerformVersionCheck(driver, ConcreteBrowserType.ToString(), MinimumSupportedVersion);
            return driver;
        }
        catch (Exception ex)
        {
            LogAndThrowWebDriverCreationError(ex, ConcreteBrowserType, chromeOptions, $"While creating {ConcreteBrowserType} driver.");
            throw;
        }
    }

    protected static string GetWindowSizeArgumentInternal(BaseBrowserSettings settings)
    {
        return settings.WindowWidth.HasValue && settings.WindowHeight.HasValue
            ? $"--window-size={settings.WindowWidth.Value},{settings.WindowHeight.Value}"
            : string.Empty;
    }
}
