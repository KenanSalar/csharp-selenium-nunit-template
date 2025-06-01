namespace SeleniumTraining.Pages.SauceDemo.Components;

public abstract class BasePageComponent
{
    protected IWebElement RootElement { get; }
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }
    protected ILogger ComponentLogger { get; }
    protected ILoggerFactory LoggerFactory { get; }

    protected BasePageComponent(IWebElement rootElement, IWebDriver driver, ILoggerFactory loggerFactory, int defaultWaitSeconds = 5)
    {
        RootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        ComponentLogger = LoggerFactory.CreateLogger(GetType());
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(defaultWaitSeconds));
    }

    protected IWebElement FindElement(By locator)
    {
        ComponentLogger.LogDebug("Component {ComponentName} finding sub-element: {Locator}", GetType().Name, locator);
        return RootElement.FindElement(locator);
    }
}
