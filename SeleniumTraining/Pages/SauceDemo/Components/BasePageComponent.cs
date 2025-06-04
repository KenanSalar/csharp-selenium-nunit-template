namespace SeleniumTraining.Pages.SauceDemo.Components;

/// <summary>
/// Provides a foundational abstract class for Page Components within the Selenium test automation framework.
/// Page Components represent reusable, isolated parts of a web page (e.g., a product card, a search widget).
/// This base class encapsulates common functionalities such as a root element context,
/// WebDriver and WebDriverWait instances, logging, settings access, retry mechanisms, and
/// an element caching strategy to improve performance by reducing redundant element lookups.
/// </summary>
/// <remarks>
/// Derived Page Component classes are typically initialized with a specific <see cref="RootElement"/>
/// which scopes their interactions to that part of the DOM. This promotes encapsulation and reusability.
/// It provides helper methods like <see cref="FindElement(By)"/> to locate sub-elements relative
/// to the component's <see cref="RootElement"/>, which now includes caching with stale element checks.
/// It also offers <see cref="HighlightIfEnabled(IWebElement)"/> for debugging.
/// Access to shared services like <see cref="IRetryService"/> and
/// <see cref="TestFrameworkSettings"/> is also provided. The element cache (<see cref="_elementCache"/>)
/// is instance-based and stores elements found within this component.
/// </remarks>
public abstract class BasePageComponent
{
    /// <summary>
    /// Instance-level cache for storing <see cref="IWebElement"/> instances found within this component,
    /// keyed by their <see cref="By"/> locator. This helps to reduce redundant DOM lookups.
    /// </summary>
    private readonly Dictionary<By, IWebElement> _elementCache = [];

    /// <summary>
    /// Gets the root <see cref="IWebElement"/> that defines the scope of this page component.
    /// All element searches within the component are typically relative to this root.
    /// </summary>
    /// <value>The root web element for this component.</value>
    protected IWebElement RootElement { get; }

    /// <summary>
    /// Gets the <see cref="IWebDriver"/> instance associated with this component,
    /// typically inherited from the parent page object.
    /// </summary>
    /// <value>The WebDriver instance.</value>
    protected IWebDriver Driver { get; }

    /// <summary>
    /// Gets the <see cref="WebDriverWait"/> instance configured for this component,
    /// used for explicit waits for element conditions within the component's scope.
    /// </summary>
    /// <value>The WebDriverWait instance.</value>
    protected WebDriverWait Wait { get; }

    /// <summary>
    /// Gets the <see cref="ILoggerFactory"/> instance, used primarily for creating loggers
    /// for any further sub-components or services instantiated by this component.
    /// </summary>
    /// <value>The logger factory.</value>
    protected ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Gets the <see cref="ILogger"/> instance specific to this page component's concrete type.
    /// Used for logging messages related to this component's operations and state.
    /// </summary>
    /// <value>The logger for this page component.</value>
    protected ILogger ComponentLogger { get; }

    /// <summary>
    /// Gets the <see cref="TestFrameworkSettings"/> containing common framework configurations,
    /// such as element highlighting preferences.
    /// </summary>
    /// <value>The test framework settings.</value>
    protected TestFrameworkSettings FrameworkSettings { get; }

    /// <summary>
    /// Gets the <see cref="IRetryService"/> instance, used for executing actions
    /// or functions with retry policies to handle transient failures within the component.
    /// </summary>
    /// <value>The retry service.</value>
    protected IRetryService Retry { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="BasePageComponent"/> abstract class.
    /// Sets up the root element, WebDriver, WebDriverWait, logging, settings, and retry service.
    /// </summary>
    /// <param name="rootElement">The root <see cref="IWebElement"/> for this component. Must not be null.</param>
    /// <param name="driver">The <see cref="IWebDriver"/> instance. Must not be null.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> for creating loggers. Must not be null.</param>
    /// <param name="settingsProvider">The <see cref="ISettingsProviderService"/> for accessing configurations. Must not be null.</param>
    /// <param name="retryService">The <see cref="IRetryService"/> for executing operations with retry logic. Must not be null.</param>
    /// <param name="defaultWaitSeconds">The default timeout in seconds for the <see cref="WebDriverWait"/> instance specific to this component. Defaults to 5 seconds.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rootElement"/>, <paramref name="driver"/>, <paramref name="loggerFactory"/>, <paramref name="settingsProvider"/>, or <paramref name="retryService"/> is null.</exception>
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

    /// <summary>
    /// Finds a sub-element within this component, starting the search from the component's <see cref="RootElement"/>.
    /// This method incorporates an element caching strategy: if an element for the given <paramref name="locator"/>
    /// is found in the cache and is not stale, it is returned directly. Otherwise, the element is located
    /// from the <see cref="RootElement"/>, stored in the cache, and then returned.
    /// </summary>
    /// <param name="locator">The <see cref="By"/> locator strategy to find the sub-element.</param>
    /// <returns>The <see cref="IWebElement"/> representing the found sub-element.</returns>
    /// <exception cref="NoSuchElementException">Thrown if no element is found using the provided <paramref name="locator"/> within the component's scope after a cache miss or if the cached element was stale and re-fetch failed.</exception>
    /// <remarks>
    /// This method scopes the search to within the component's defined <see cref="RootElement"/>,
    /// promoting encapsulation. The caching mechanism helps reduce redundant DOM lookups for frequently accessed elements.
    /// Stale element checks are performed on cached elements before returning them.
    /// The search and caching operations are logged.
    /// </remarks>
    protected IWebElement FindElement(By locator)
    {
        ComponentLogger.LogTrace("Component {ComponentName} attempting to find sub-element: {Locator}", GetType().Name, locator);

        if (_elementCache.TryGetValue(locator, out IWebElement? cachedElement))
        {
            try
            {
                _ = cachedElement.Enabled;
                ComponentLogger.LogTrace("Returning element for locator '{Locator}' from component cache.", locator);
                return cachedElement;
            }
            catch (StaleElementReferenceException)
            {
                ComponentLogger.LogDebug("Cached element for locator '{Locator}' in component '{ComponentName}' was stale. Removing from cache.", locator, GetType().Name);
                _ = _elementCache.Remove(locator);
            }
            catch (Exception ex)
            {
                ComponentLogger.LogWarning(ex, "Unexpected error checking staleness of cached element for locator '{Locator}' in component '{ComponentName}'. Will re-fetch.", locator, GetType().Name);
                _ = _elementCache.Remove(locator);
            }
        }

        ComponentLogger.LogTrace("Element for locator '{Locator}' not in component cache or was stale. Finding new element.", locator);
        IWebElement newElement = RootElement.FindElement(locator);
        _elementCache[locator] = newElement; // Add or update in cache
        ComponentLogger.LogDebug("Found and cached new element for locator '{Locator}' in component '{ComponentName}'.", locator, GetType().Name);
        return newElement;
    }

    /// <summary>
    /// Highlights the specified web element if element highlighting is enabled in the framework settings.
    /// </summary>
    /// <param name="element">The <see cref="IWebElement"/> to highlight. This element should typically be a sub-element of this component.</param>
    /// <returns>The same web element that was passed in, allowing for fluent chaining of operations.</returns>
    /// <remarks>
    /// This method utilizes an extension method <c>HighlightElement</c> to perform the visual highlighting.
    /// The duration of the highlight is configured via <see cref="TestFrameworkSettings.HighlightDurationMs"/>.
    /// If highlighting is disabled, the method returns the element without modification.
    /// Uses the component-specific <see cref="ComponentLogger"/> for logging.
    /// </remarks>
    protected IWebElement HighlightIfEnabled(IWebElement element)
    {
        if (FrameworkSettings.HighlightElementsOnInteraction)
            _ = element.HighlightElement(Driver, ComponentLogger, FrameworkSettings.HighlightDurationMs);

        return element;
    }

    /// <summary>
    /// Clears the internal element cache for this component instance.
    /// This should be called if actions performed within the component are known to
    /// drastically alter its internal DOM structure, potentially making all cached elements stale.
    /// </summary>
    protected void ClearComponentElementCache()
    {
        ComponentLogger.LogDebug("Clearing element cache for component {ComponentName}.", GetType().Name);
        _elementCache.Clear();
    }
}
