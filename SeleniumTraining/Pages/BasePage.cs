namespace SeleniumTraining.Pages;

/// <summary>
/// Initializes a new instance of the <see cref="BasePage"/> abstract class.
/// Sets up WebDriver, WebDriverWait, logging, settings, retry service, and performs
/// initial page load and critical element visibility checks.
/// </summary>
/// <param name="driver">The <see cref="IWebDriver"/> instance for browser interaction. Must not be null.</param>
/// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Must not be null.</param>
/// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Must not be null.</param>
/// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Must not be null.</param>
/// <exception cref="ArgumentNullException">Thrown if <paramref name="driver"/>, <paramref name="loggerFactory"/>, <paramref name="settingsProvider"/>, or <paramref name="retryService"/> is null.</exception>
/// <exception cref="WebDriverTimeoutException">Thrown if <see cref="WaitForPageLoad"/>, <see cref="EnsureCriticalElementsAreDisplayed"/>, or the additional readiness checks time out.</exception>
/// <exception cref="Exception">Thrown for other unexpected errors during initialization.</exception>
public abstract class BasePage
{
    /// <summary>
    /// Gets the <see cref="IWebDriver"/> instance associated with this page object.
    /// Used for all browser interactions.
    /// </summary>
    /// <value>The WebDriver instance.</value>
    protected IWebDriver Driver { get; }

    /// <summary>
    /// Gets the <see cref="WebDriverWait"/> instance configured for this page object,
    /// used for explicit waits for element conditions.
    /// </summary>
    /// <value>The WebDriverWait instance.</value>
    protected WebDriverWait Wait { get; }

    /// <summary>
    /// Gets the <see cref="ILoggerFactory"/> instance, used primarily for creating loggers
    /// for components or services instantiated by this page object.
    /// </summary>
    /// <value>The logger factory.</value>
    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance specific to this page object's concrete type.
    /// Used for logging messages related to this page's operations and state.
    /// </summary>
    /// <value>The logger for this page object.</value>
    protected ILogger PageLogger { get; }

    /// <summary>
    /// Gets the <see cref="IRetryService"/> instance, used for executing actions
    /// or functions with retry policies to handle transient failures.
    /// </summary>
    /// <value>The retry service.</value>
    protected IRetryService Retry { get; }

    /// <summary>
    /// Gets an enumerable collection of <see cref="By"/> locators for elements that are considered
    /// critical for this page to be fully loaded and operational.
    /// Derived classes must implement this property to define their critical elements.
    /// </summary>
    /// <value>An enumerable of critical element locators.</value>
    /// <remarks>
    /// The <see cref="EnsureCriticalElementsAreDisplayed"/> method will wait for all elements
    /// identified by these locators to become visible during page initialization.
    /// The type is currently <see cref="IEnumerable"/> for flexibility, but often contains <see cref="By"/> locators.
    /// </remarks>
    protected abstract IEnumerable<By> CriticalElementsToEnsureVisible { get; }

    /// <summary>
    /// Gets the <see cref="ISettingsProviderService"/> instance, used to access
    /// application and framework settings.
    /// </summary>
    /// <value>The settings provider service.</value>
    protected ISettingsProviderService PageSettingsProvider { get; }

    /// <summary>
    /// Gets the <see cref="TestFrameworkSettings"/> containing common framework configurations,
    /// such as element highlighting preferences and default timeouts.
    /// </summary>
    /// <value>The test framework settings.</value>
    protected TestFrameworkSettings FrameworkSettings { get; }

    /// <summary>
    /// Gets the name of this page object class, typically used for logging and reporting.
    /// It is derived from the concrete page object's type name.
    /// </summary>
    /// <value>The name of the page object class.</value>
    protected string PageName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasePage"/> abstract class.
    /// Sets up WebDriver, WebDriverWait, logging, settings, and retry service.
    /// Does NOT perform page load validation; call <see cref="AssertPageIsLoaded"/> for that purpose.
    /// </summary>
    /// <remarks>
    /// This constructor is responsible for initializing all the essential services and properties for the page object.
    /// All page load and readiness checks have been moved to the <see cref="AssertPageIsLoaded"/> method
    /// to make verification an explicit step in the test flow.
    /// </remarks>
    /// <param name="driver">The <see cref="IWebDriver"/> instance for browser interaction. Must not be null.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Must not be null.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Must not be null.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Must not be null.</param>
    protected BasePage(
        IWebDriver driver,
        ILoggerFactory loggerFactory,
        ISettingsProviderService settingsProvider,
        IRetryService retryService
    )
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        Retry = retryService ?? throw new ArgumentNullException(nameof(retryService));
        PageLogger = LoggerFactory.CreateLogger(GetType());
        PageName = GetType().Name;

        ArgumentNullException.ThrowIfNull(settingsProvider);
        PageSettingsProvider = settingsProvider;
        FrameworkSettings = PageSettingsProvider.GetSettings<TestFrameworkSettings>("TestFrameworkSettings");

        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(FrameworkSettings.DefaultExplicitWaitSeconds));

        PageLogger.LogInformation(
            "Instantiated {PageName}. Page-load validation will be performed by AssertPageIsLoaded().",
            PageName
        );
    }

    /// <summary>
    /// Asserts that the page is fully loaded by waiting for the document to be ready
    /// and ensuring all critical elements are visible. This method should be called
    /// immediately after instantiating a page object.
    /// </summary>
    /// <returns>The current page instance for fluent chaining.</returns>
    /// <exception cref="WebDriverTimeoutException">Thrown if the page or its critical elements do not load within the configured timeout.</exception>
    [AllureStep("Asserting that page '{PageName}' is loaded and ready")]
    public virtual BasePage AssertPageIsLoaded()
    {
        var pageLoadTimer = new PerformanceTimer(
            $"PageLoad_{PageName}",
            PageLogger,
            Microsoft.Extensions.Logging.LogLevel.Information,
            new Dictionary<string, object> { { "PageType", PageName } }
        );
        bool initializationSuccessful = false;

        try
        {
            PageLogger.LogDebug("Attempting to wait for {PageName} to load (document.readyState).", PageName);
            WaitForPageLoad();

            PageLogger.LogDebug("Attempting to ensure critical elements are visible on {PageName}.", PageName);
            EnsureCriticalElementsAreDisplayed();

            if (DefinesAdditionalBaseReadinessConditions())
            {
                PageLogger.LogDebug("Attempting to wait for additional base readiness conditions on {PageName} using AllOf.", PageName);
                bool additionalConditionsMet = Wait.Until(CustomExpectedConditions.AllOf(
                    PageLogger,
                    GetAdditionalBaseReadinessConditions().ToArray()
                ));

                if (!additionalConditionsMet)
                {
                    PageLogger.LogWarning("Additional base readiness conditions for {PageName} were not met (AllOf returned false).", PageName);
                }
                PageLogger.LogInformation("Additional base readiness conditions met for {PageName}.", PageName);
            }

            PageLogger.LogInformation("{PageName} fully loaded and validated successfully.", PageName);
            initializationSuccessful = true;
        }
        catch (WebDriverTimeoutException ex)
        {
            PageLogger.LogError(ex, "{PageName} timed out during page load validation. Timeout: {TimeoutSeconds}s.", PageName, Wait.Timeout.TotalSeconds);
            throw;
        }
        catch (Exception ex)
        {
            PageLogger.LogError(ex, "An unexpected error occurred during page load validation for {PageName}.", PageName);
            throw;
        }
        finally
        {
            const long expectedPageLoadTimeMs = 3000;
            pageLoadTimer.StopAndLog(
                attachToAllure: true,
                expectedMaxMilliseconds: initializationSuccessful
                    ? expectedPageLoadTimeMs
                    : null
            );
        }

        return this;
    }

    /// <summary>
    /// Waits for the current page's document readyState to be 'complete'.
    /// This is a fundamental check to ensure the basic HTML structure of the page has loaded.
    /// </summary>
    /// <remarks>
    /// This method uses <see cref="CustomExpectedConditions.DocumentIsReady()"/> with the page's
    /// <see cref="Wait"/> instance. Exceptions during this process, including timeouts, are logged and re-thrown.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if the document does not reach the 'complete' state within the configured timeout.</exception>
    /// <exception cref="Exception">Thrown for other unexpected errors during the readyState check.</exception>
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

            bool pageLoaded = Wait.Until(CustomExpectedConditions.DocumentIsReady());

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

    /// <summary>
    /// Ensures that all critical elements defined by <see cref="CriticalElementsToEnsureVisible"/>
    /// for the current page are visible. This method is called during page initialization.
    /// </summary>
    /// <remarks>
    /// It uses the <see cref="WaitExtensions.EnsureElementsAreVisible(WebDriverWait, ILogger, string, IEnumerable)"/>
    /// extension method to check each locator. If no critical elements are defined, the method returns early.
    /// Exceptions, including timeouts if any element is not visible within the configured wait period, are logged and re-thrown.
    /// </remarks>
    /// <exception cref="WebDriverTimeoutException">Thrown if any critical element is not visible within the timeout.</exception>
    /// <exception cref="Exception">Thrown for other unexpected errors during the visibility check.</exception>
    [AllureStep("Ensuring critical page elements are visible.")]
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

    /// <summary>
    /// Highlights the specified web element if element highlighting is enabled in the framework settings.
    /// </summary>
    /// <param name="element">The <see cref="IWebElement"/> to highlight.</param>
    /// <returns>The same web element that was passed in, allowing for fluent chaining of operations.</returns>
    /// <remarks>
    /// This method utilizes an extension method <c>HighlightElement</c> to perform the visual highlighting.
    /// The duration of the highlight is configured via <see cref="TestFrameworkSettings.HighlightDurationMs"/>.
    /// If highlighting is disabled, the method returns the element without modification.
    /// </remarks>
    protected IWebElement HighlightIfEnabled(IWebElement element)
    {
        if (FrameworkSettings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(Driver, PageLogger, FrameworkSettings.HighlightDurationMs);

        return element;
    }

    /// <summary>
    /// Finds an element by the given locator using the page-level cache and then highlights it
    /// if element highlighting is enabled in the framework settings.
    /// </summary>
    /// <param name="locator">The <see cref="By"/> locator used to find the web element via <see cref="FindElementOnPage(By)"/>.</param>
    /// <returns>The found and potentially highlighted <see cref="IWebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown by <see cref="FindElementOnPage(By)"/> if no element is found using the provided <paramref name="locator"/>.</exception>
    /// <remarks>
    /// This method first finds the element using <see cref="FindElementOnPage(By)"/> which incorporates caching,
    /// and then calls the overloaded <see cref="HighlightIfEnabled(IWebElement)"/> method.
    /// </remarks>
    protected IWebElement HighlightIfEnabled(By locator)
    {
        IWebElement element = FindElementOnPage(locator);

        if (FrameworkSettings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(Driver, PageLogger, FrameworkSettings.HighlightDurationMs);

        return element;
    }

    /// <summary>
    /// Determines if the derived page defines additional base readiness conditions beyond
    /// document ready state and critical element visibility.
    /// Derived pages can override this method to return <c>true</c> if they provide conditions
    /// through <see cref="GetAdditionalBaseReadinessConditions"/>.
    /// </summary>
    /// <returns><c>false</c> by default; derived pages should override to <c>true</c> if they have additional conditions.</returns>
    protected virtual bool DefinesAdditionalBaseReadinessConditions()
    {
        return false;
    }

    /// <summary>
    /// Provides a list of additional custom wait conditions (as <see cref="Func{IWebDriver, Boolean}"/>)
    /// to be checked during page initialization using a composite <see cref="CustomExpectedConditions.AllOf"/> wait.
    /// This method is only invoked if <see cref="DefinesAdditionalBaseReadinessConditions"/> returns <c>true</c>.
    /// </summary>
    /// <returns>An enumerable of functions, each representing a boolean wait condition.
    /// Returns an empty enumerable by default.</returns>
    /// <remarks>
    /// Derived pages should override this method to yield their specific additional readiness conditions.
    /// Each function should take an <see cref="IWebDriver"/> and return <c>true</c> if the condition is met,
    /// <c>false</c> otherwise (or if an element is not yet present, which should be handled within the function).
    /// </remarks>
    protected virtual IEnumerable<Func<IWebDriver, bool>> GetAdditionalBaseReadinessConditions()
    {
        yield break;
    }

    /// <summary>
    /// Finds an element on the page by performing a live search against the DOM using the WebDriver instance.
    /// This ensures the element reference is always current.
    /// </summary>
    /// <param name="locator">The <see cref="By"/> locator strategy to find the element.</param>
    /// <returns>The freshly located <see cref="IWebElement"/>.</returns>
    /// <exception cref="NoSuchElementException">Thrown if the element is not found on the page at the time of the call.</exception>
    /// <remarks>
    /// This method provides a centralized way to find page-level elements. By performing a direct
    /// `Driver.FindElement(locator)` call every time, it avoids the risks associated with element caching,
    /// such as `StaleElementReferenceException`, making the framework more reliable and easier to debug.
    /// </remarks>
    protected IWebElement FindElementOnPage(By locator)
    {
        PageLogger.LogTrace("Page {PageName} attempting to find element: {Locator}", PageName, locator);

        return Driver.FindElement(locator);
    }
}
