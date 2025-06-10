using System.Drawing;

namespace SeleniumTraining.Core.Services.Drivers;

/// <summary>
/// Service responsible for orchestrating the initialization of WebDriver instances.
/// It coordinates with browser factory services and settings providers to create
/// and configure drivers based on the specified browser type and test context.
/// </summary>
/// <remarks>
/// This service implements <see cref="IDriverInitializationService"/> and utilizes
/// an <see cref="IBrowserFactoryManagerService"/> to delegate the actual browser driver creation
/// and an <see cref="ISettingsProviderService"/> to fetch browser-specific configurations.
/// It also handles initial driver configurations like implicit waits and logs the process.
/// This class inherits from <see cref="BaseService"/> for common logging capabilities.
/// </remarks>
public class DriverInitializationService : BaseService, IDriverInitializationService
{
    private readonly IBrowserFactoryManagerService _browserFactory;
    private readonly ISettingsProviderService _settingsProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DriverInitializationService"/> class.
    /// </summary>
    /// <param name="browserFactory">The browser factory manager service, used to create browser-specific WebDriver instances. Must not be null.</param>
    /// <param name="settingsProvider">The settings provider service, used to retrieve browser configurations. Must not be null.</param>
    /// <param name="loggerFactory">The logger factory, passed to the base <see cref="BaseService"/> for creating loggers. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="browserFactory"/>, <paramref name="settingsProvider"/>, or <paramref name="loggerFactory"/> is null.</exception>
    public DriverInitializationService(IBrowserFactoryManagerService browserFactory, ISettingsProviderService settingsProvider, ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _browserFactory = browserFactory ?? throw new ArgumentNullException(nameof(browserFactory));
        _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        ServiceLogger.LogInformation("{ServiceName} initialized.", nameof(DriverInitializationService));
    }

    /// <inheritdoc cref="IDriverInitializationService.InitializeDriver(BrowserType, string, string)" />
    /// <remarks>
    /// This implementation first retrieves browser-specific settings using the <see cref="ISettingsProviderService"/>.
    /// Then, it invokes the <see cref="IBrowserFactoryManagerService"/> to create the actual WebDriver instance.
    /// After successful creation, it applies the configured implicit wait timeout.
    /// Detailed logging is performed throughout the process, including on failure, where it attempts to quit
    /// any partially initialized driver.
    /// </remarks>
    public IWebDriver InitializeDriver(BrowserType browserType, string testName, string correlationId)
    {
        var logProps = new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["TestClassName"] = testName,
            ["BrowserType"] = browserType.ToString()
        };

        using (ServiceLogger.BeginScope(logProps!))
        {
            ServiceLogger.LogInformation("Attempting to initialize WebDriver for test: {TestName}, browser: {BrowserType}", testName, browserType);
            IWebDriver? driver = null;
            try
            {
                ServiceLogger.LogDebug("Retrieving browser-specific settings for {BrowserType}.", browserType);
                BaseBrowserSettings browserSettings = _settingsProvider.GetBrowserSettings(browserType);
                ServiceLogger.LogDebug("Successfully retrieved browser-specific settings for {BrowserType}.", browserType);

                driver = _browserFactory.UseBrowserDriver(browserType, browserSettings);

                if (driver != null)
                {
                    Size initialSize = driver.Manage().Window.Size;
                    ServiceLogger.LogInformation(
                       "WebDriver initialized successfully. Initial window size: {WindowWidth}x{WindowHeight}. Headless: {IsHeadless}. Explicit wait timeout will be {ExplicitWaitTimeout}s.",
                       initialSize.Width,
                       initialSize.Height,
                       browserSettings.Headless,
                       browserSettings.TimeoutSeconds
                    );
                    
                    return driver;
                }
                else
                {
                    ServiceLogger.LogError("WebDriver initialization failed: BrowserFactory returned null for {BrowserType} during test {TestName}.", browserType, testName);
                    throw new WebDriverException($"WebDriver failed to initialize (factory returned null) for {browserType} in test {testName}.");
                }
            }
            catch (Exception ex)
            {
                ServiceLogger.LogError(ex, "Exception during WebDriver initialization for test {TestName}, browser {BrowserType}.", testName, browserType);
                if (driver != null)
                {
                    ServiceLogger.LogWarning("Attempting to quit partially initialized WebDriver for test {TestName} due to an error.", testName);
                    try
                    {
                        driver.Quit();
                        ServiceLogger.LogInformation("Successfully quit partially initialized WebDriver for test {TestName}.", testName);
                    }
                    catch (Exception quitEx)
                    {
                        ServiceLogger.LogError(quitEx, "Failed to quit partially initialized WebDriver for test {TestName}.", testName);
                    }
                }
                throw;
            }
        }
    }
}
