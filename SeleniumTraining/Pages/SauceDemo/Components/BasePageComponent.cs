namespace SeleniumTraining.Pages.SauceDemo.Components;

/// <summary>
/// Provides a foundational abstract class for Page Components within the Selenium test automation framework.
/// Page Components represent reusable, isolated parts of a web page (e.g., a product card, a search widget).
/// This base class encapsulates common functionalities such as a scoped root element context,
/// WebDriver access, logging, settings access, and retry mechanisms.
/// </summary>
/// <remarks>
/// Derived Page Component classes are initialized with a specific <see cref="RootElement"/>
/// which scopes all their interactions to that part of the DOM, promoting encapsulation and reusability.
/// It provides helper methods like <see cref="FindElement(By)"/> to locate sub-elements relative
/// to the component's <see cref="RootElement"/>. All element lookups are performed "live" against the DOM
/// with each call to ensure element references are always fresh, which avoids stale element issues by design.
/// This approach prioritizes reliability and simplicity.
/// Access to shared services like <see cref="IRetryService"/> and
/// <see cref="TestFrameworkSettings"/> is also provided.
/// </remarks>
public abstract class BasePageComponent
{
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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="rootElement"/>, <paramref name="driver"/>, <paramref name="loggerFactory"/>, <paramref name="settingsProvider"/>, or <paramref name="retryService"/> is null.</exception>
    protected BasePageComponent(
        IWebElement rootElement,
        IWebDriver driver,
        ILoggerFactory loggerFactory,
        ISettingsProviderService settingsProvider,
        IRetryService retryService
    )
    {
        RootElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        ComponentLogger = LoggerFactory.CreateLogger(GetType());
        Retry = retryService;

        ArgumentNullException.ThrowIfNull(settingsProvider);
        FrameworkSettings = settingsProvider.GetSettings<TestFrameworkSettings>("TestFrameworkSettings");

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(FrameworkSettings.DefaultExplicitWaitSeconds));
    }

    /// <summary>
    /// Finds a sub-element within this component, starting the search from the component's <see cref="RootElement"/>.
    /// This method performs a live search on the DOM every time it is called to ensure the element reference is fresh.
    /// </summary>
    /// <param name="locator">The <see cref="By"/> locator strategy to find the sub-element.</param>
    /// <returns>The freshly located <see cref="IWebElement"/> representing the found sub-element.</returns>
    /// <exception cref="NoSuchElementException">Thrown if no element is found using the provided <paramref name="locator"/> within the component's scope at the time of the call.</exception>
    /// <remarks>
    /// This method scopes the search to within the component's defined <see cref="RootElement"/>,
    /// promoting encapsulation. By performing a direct `FindElement` call without caching,
    /// it guarantees that the returned element is current, thus preventing `StaleElementReferenceException`
    /// issues that can arise from interacting with a previously located element after a DOM update.
    /// </remarks>
    protected IWebElement FindElement(By locator)
    {
        ComponentLogger.LogTrace("Component {ComponentName} attempting to find sub-element: {Locator}", GetType().Name, locator);

        return RootElement.FindElement(locator);
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
}
