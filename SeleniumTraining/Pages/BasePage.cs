namespace SeleniumTraining.Pages;

public abstract class BasePage
{
    protected IWebDriver Driver { get; }
    protected WebDriverWait Wait { get; }
    protected ILoggerFactory LoggerFactory { get; }
    protected ILogger Logger { get; }
    protected string PageName { get; }

    protected abstract IEnumerable<By> CriticalElementsToEnsureVisible { get; }

    protected BasePage(IWebDriver driver, ILoggerFactory loggerFactory, int defaultTimeoutSeconds = 5)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        Logger = LoggerFactory.CreateLogger(GetType());
        PageName = GetType().Name;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(defaultTimeoutSeconds));

        Logger.LogInformation("Initializing {PageName}. Default explicit wait timeout: {DefaultTimeoutSeconds}s.", PageName, defaultTimeoutSeconds);

        try
        {
            Logger.LogDebug("Attempting to wait for {PageName} to load fully.", PageName);
            WaitForPageLoad();

            Logger.LogDebug("Attempting to ensure critical elements are visible on {PageName}.", PageName);
            EnsureCriticalElementsAreDisplayed();

            Logger.LogInformation("{PageName} initialized successfully.", PageName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize {PageName} due to an error during page load or critical element validation.", PageName);
            throw;
        }
    }

    [AllureStep("Wait for page load")]
    protected virtual void WaitForPageLoad()
    {
        try
        {
            Logger.LogDebug(
                "Waiting for {PageName} document.readyState to be 'complete'. Timeout: {TimeoutSeconds}s.",
                PageName, Wait.Timeout.TotalSeconds
            );
            bool pageLoaded = Wait.Until(d =>
                ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.Equals("complete") ?? false);

            if (pageLoaded)
                Logger.LogInformation("{PageName} document.readyState is 'complete'.", PageName);
            else
                Logger.LogWarning("{PageName} document.readyState check returned false, but Wait.Until did not time out. State may be indeterminate.", PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            Logger.LogError(
                ex,
                "{PageName} timed out waiting for document.readyState to be 'complete'. Timeout: {TimeoutSeconds}s.",
                PageName, Wait.Timeout.TotalSeconds
            );
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred while checking document.readyState for {PageName}.", PageName);
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
            Logger.LogTrace("No critical elements defined for {PageName} to ensure visibility.", PageName);
            return;
        }

        Logger.LogDebug("Checking visibility of {LocatorCount} critical element(s)/group(s) on {PageName}.", locators.Count(), PageName);

        try
        {
            Wait.EnsureElementsAreVisible(Logger, pageName, locators);
            Logger.LogInformation("All {LocatorCount} specified critical element(s)/group(s) on {PageName} are visible.", locators.Count(), PageName);
        }
        catch (WebDriverTimeoutException ex)
        {
            Logger.LogError(
                ex,
                "Failed to ensure all critical elements were visible on {PageName}. Timeout: {TimeoutSeconds}s.",
                PageName, Wait.Timeout.TotalSeconds
            );
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An unexpected error occurred while ensuring critical elements are visible on {PageName}.", PageName);
            throw;
        }
    }
}
