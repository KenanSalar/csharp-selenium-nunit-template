namespace SeleniumTraining.Pages;

public abstract class BasePage
{
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }
    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger PageLogger { get; }
    protected string PageName { get; }

    protected abstract IEnumerable<By> CriticalElementsToEnsureVisible { get; }
    protected ISettingsProviderService PageSettingsProvider { get; }
    protected TestFrameworkSettings FrameworkSettings { get; }

    protected BasePage(IWebDriver driver, ILoggerFactory loggerFactory, ISettingsProviderService settingsProvider, int defaultTimeoutSeconds = 5)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        PageLogger = LoggerFactory.CreateLogger(GetType());
        PageName = GetType().Name;

        ArgumentNullException.ThrowIfNull(settingsProvider);
        PageSettingsProvider = settingsProvider;
        FrameworkSettings = PageSettingsProvider.GetSettings<TestFrameworkSettings>("TestFrameworkSettings");

        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(defaultTimeoutSeconds));

        PageLogger.LogInformation(
            "Initializing {PageName}. Default explicit wait timeout: {DefaultTimeoutSeconds}s. HighlightOnInteraction: {HighlightSetting}",
            PageName,
            defaultTimeoutSeconds,
            FrameworkSettings.HighlightElementsOnInteraction
        );

        var pageLoadTimer = new PerformanceTimer(
            $"PageLoad_{PageName}",
            PageLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            new Dictionary<string, object> { { "PageType", PageName } }
        );
        bool initializationSuccessful = false;

        try
        {
            PageLogger.LogDebug("Attempting to wait for {PageName} to load fully.", PageName);
            WaitForPageLoad();

            PageLogger.LogDebug("Attempting to ensure critical elements are visible on {PageName}.", PageName);
            EnsureCriticalElementsAreDisplayed();

            PageLogger.LogInformation("{PageName} initialized successfully.", PageName);
            initializationSuccessful = true;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "Failed to initialize {PageName} due to an error during page load or critical element validation.", PageName);
            throw;
        }
        finally
        {
            long expectedPageLoadTimeMs = 3000;
            pageLoadTimer.StopAndLog(
                attachToAllure: true,
                expectedMaxMilliseconds: initializationSuccessful
                    ? expectedPageLoadTimeMs
                    : null
            );
        }
    }

    [AllureStep("Wait for page load")]
    protected virtual void WaitForPageLoad()
    {
        try
        {
            PageLogger.LogDebug(
                "Waiting for {PageName} document.readyState to be 'complete'. Timeout: {TimeoutSeconds}s.",
                PageName,
                Wait.Timeout.TotalSeconds
            );
            bool pageLoaded = Wait.Until(d =>
                ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.Equals("complete") ?? false);

            if (pageLoaded)
                PageLogger.LogInformation("{PageName} document.readyState is 'complete'.", PageName);
            else
                PageLogger.LogWarning("{PageName} document.readyState check returned false, but Wait.Until did not time out. State may be indeterminate.", PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(
                ex,
                "{PageName} timed out waiting for document.readyState to be 'complete'. Timeout: {TimeoutSeconds}s.",
                PageName,
                Wait.Timeout.TotalSeconds
            );
            throw;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred while checking document.readyState for {PageName}.", PageName);
            throw;
        }
    }

    [AllureStep("Ensuring critical page elements are visible for {PageName}")]
    private void EnsureCriticalElementsAreDisplayed()
    {
        string pageName = GetType().Name;
        IEnumerable<By> locators = CriticalElementsToEnsureVisible;

        if (locators?.Any() != true)
        {
            PageLogger.LogTrace("No critical elements defined for {PageName} to ensure visibility.", PageName);
            return;
        }

        PageLogger.LogDebug("Checking visibility of {LocatorCount} critical element(s)/group(s) on {PageName}.", locators.Count(), PageName);

        try
        {
            Wait.EnsureElementsAreVisible(PageLogger, pageName, locators);
            PageLogger.LogInformation("All {LocatorCount} specified critical element(s)/group(s) on {PageName} are visible.", locators.Count(), PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(
                ex,
                "Failed to ensure all critical elements were visible on {PageName}. Timeout: {TimeoutSeconds}s.",
                PageName, Wait.Timeout.TotalSeconds
            );
            throw;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred while ensuring critical elements are visible on {PageName}.", PageName);
            throw;
        }
    }

    protected IWebElement HighlightIfEnabled(IWebElement element)
    {
        if (FrameworkSettings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(Driver, PageLogger, FrameworkSettings.HighlightDurationMs);

        return element;
    }

    protected IWebElement HighlightIfEnabled(By locator)
    {
        IWebElement element = Driver.FindElement(locator);

        if (FrameworkSettings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(Driver, PageLogger, FrameworkSettings.HighlightDurationMs);

        return element;
    }
}
