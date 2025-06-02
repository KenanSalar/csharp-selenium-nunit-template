namespace SeleniumTraining.Pages.SauceDemo.Components;

public abstract class BasePageComponent
{
    protected IWebElement RootElement { get; }
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }
    protected ILogger ComponentLogger { get; }
    protected ILoggerFactory LoggerFactory { get; }
    protected TestFrameworkSettings FrameworkSettings { get; }
    protected IRetryService Retry { get; private set; }

    protected BasePageComponent(
        IWebElement rootElement,
        IWebDriver driver,
        ILoggerFactory loggerFactory,
        ISettingsProviderService settingsProvider,
        IRetryService retryService,
        int defaultWaitSeconds = 5
    )
    {
        RootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        ComponentLogger = LoggerFactory.CreateLogger(GetType());
        Retry = retryService;

        ArgumentNullException.ThrowIfNull(settingsProvider);
        FrameworkSettings = settingsProvider.GetSettings<TestFrameworkSettings>("TestFrameworkSettings");

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(defaultWaitSeconds));
    }

    protected IWebElement FindElement(By locator)
    {
        ComponentLogger.LogDebug("Component {ComponentName} finding sub-element: {Locator}", GetType().Name, locator);
        return RootElement.FindElement(locator);
    }

    protected IWebElement HighlightIfEnabled(IWebElement element)
    {
        if (FrameworkSettings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(Driver, ComponentLogger, FrameworkSettings.HighlightDurationMs);

        return element;
    }
}
