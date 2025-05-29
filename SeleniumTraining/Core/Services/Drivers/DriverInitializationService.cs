using System.Drawing;

namespace SeleniumTraining.Core.Services.Drivers;

public class DriverInitializationService : BaseService, IDriverInitializationService
{
    private readonly IBrowserFactoryManagerService _browserFactory;
    private readonly ISettingsProviderService _settingsProvider;

    public DriverInitializationService(IBrowserFactoryManagerService browserFactory, ISettingsProviderService settingsProvider, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));
        _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        Logger.LogInformation("{ServiceName} initialized.", nameof(DriverInitializationService));
    }

    public IWebDriver InitializeDriver(BrowserType browserType, string testName, string correlationId)
    {
        var logProps = new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["TestClassName"] = testName,
            ["BrowserType"] = browserType.ToString()
        };

        using (Logger.BeginScope(logProps!))
        {
            Logger.LogInformation("Attempting to initialize WebDriver for test: {TestName}, browser: {BrowserType}", testName, browserType);
            IWebDriver? driver = null;
            try
            {
                Logger.LogDebug("Retrieving browser-specific settings for {BrowserType}.", browserType);
                BaseBrowserSettings browserSettings = _settingsProvider.GetBrowserSettings(browserType);
                Logger.LogDebug("Successfully retrieved browser-specific settings for {BrowserType}.", browserType);

                driver = _browserFactory.UseBrowserDriver(browserType, browserSettings);

                if (driver != null)
                {
                    Size initialSize = driver.Manage().Window.Size;
                    Logger.LogInformation(
                        "WebDriver initialized successfully. Initial window size: {WindowWidth}x{WindowHeight}. Headless: {IsHeadless}. ImplicitWait: {ImplicitWaitSeconds}s",
                        initialSize.Width, initialSize.Height, browserSettings.Headless, browserSettings.TimeoutSeconds);
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(browserSettings.TimeoutSeconds);
                    return driver;
                }
                else
                {
                    Logger.LogError("WebDriver initialization failed: BrowserFactory returned null for {BrowserType} during test {TestName}.", browserType, testName);
                    throw new WebDriverException($"WebDriver failed to initialize (factory returned null) for {browserType} in test {testName}.");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception during WebDriver initialization for test {TestName}, browser {BrowserType}.", testName, browserType);
                if (driver != null)
                {
                    Logger.LogWarning("Attempting to quit partially initialized WebDriver for test {TestName} due to an error.", testName);
                    try
                    {
                        driver.Quit();
                        Logger.LogInformation("Successfully quit partially initialized WebDriver for test {TestName}.", testName);
                    }
                    catch (Exception quitEx)
                    {
                        Logger.LogError(quitEx, "Failed to quit partially initialized WebDriver for test {TestName}.", testName);
                    }
                }
                throw;
            }
        }
    }
}
