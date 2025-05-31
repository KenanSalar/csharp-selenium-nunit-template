using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace SeleniumTraining.Core.Services.Drivers;

public class ChromeDriverFactoryService : ChromiumDriverFactoryServiceBase, IBrowserDriverFactoryService
{
    public BrowserType Type => BrowserType.Chrome;
    protected override BrowserType ConcreteBrowserType => BrowserType.Chrome;
    protected override Version MinimumSupportedVersion { get; } = new("110.0");

    public ChromeDriverFactoryService(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger.LogInformation("{FactoryName} initialized for {BrowserType}.", nameof(ChromeDriverFactoryService), Type);
    }

    public IWebDriver CreateDriver(BaseBrowserSettings settingsBase, DriverOptions? options = null)
    {
        if (settingsBase is not ChromeSettings settings)
        {
            var ex = new ArgumentException($"Invalid settings type provided. Expected {nameof(ChromeSettings)}, got {settingsBase.GetType().Name}.", nameof(settingsBase));
            Logger.LogError(ex, "Settings type mismatch in {FactoryName}.", nameof(ChromeDriverFactoryService));
            throw ex;
        }

        Logger.LogInformation(
            "Creating {BrowserType} WebDriver. Requested settings - Headless: {IsHeadless}, WindowSize: {WindowWidth}x{WindowHeight} (if specified).",
            Type,
            settings.Headless,
            settings.WindowWidth ?? -1,
            settings.WindowHeight ?? -1
        );

        Logger.LogDebug("Attempting to set up ChromeDriver using WebDriverManager (ChromeConfig) for {BrowserType}.", Type);
        try
        {
            _ = new DriverManager().SetUpDriver(new ChromeConfig());
            Logger.LogInformation("WebDriverManager successfully completed ChromeDriver setup (ChromeConfig) for {BrowserType}.", Type);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "WebDriverManager failed to set up ChromeDriver (ChromeConfig) for {BrowserType}.", Type);
            throw;
        }

        ChromeOptions chromeOptions = ConfigureCommonChromeOptions(
            settings,
            options,
            out List<string> appliedOptionsForLog
        );

        Logger.LogInformation(
            "ChromeOptions configured for {BrowserType}. Effective arguments: [{EffectiveArgs}]",
            Type, string.Join(", ", appliedOptionsForLog.Distinct()));

        return CreateDriverInstanceWithChecks(chromeOptions);
    }
}
